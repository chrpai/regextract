using System;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Deployment.WindowsInstaller;
using RegExtract.Enums;

namespace RegExtract.Services
{
    class RegistryExtractor : IDisposable   
    {
        Database _database;
        public RegistryExtractor(string msiPath)
        {
            _database = new Database(msiPath, DatabaseOpenMode.ReadOnly);

        }

        public void Dispose()
        {
            if(_database != null)
            {
                _database.Dispose();
                _database = null;
            }
        }

        public void Extract(string wxiPath)
        {
            string previousRegistryKey = string.Empty;
            XNamespace ns = "http://schemas.microsoft.com/wix/2006/wi";
            XDocument document = new XDocument( new XElement(ns + "Wix", new XElement(ns + "Include")));
            XElement includeElement = document.Descendants(ns + "Include").First();

            string selectComment = "SELECT `Root`, `Key`, `Name`, `Value` FROM `Registry` ORDER BY `Root`, `Key`";
            try
            {
                using (View view = _database.OpenView(selectComment))
                {
                    view.Execute();
                    foreach (var record in view)
                    {
                        RegistryRootType root = (RegistryRootType)Enum.Parse(typeof(RegistryRootType), record.GetInteger(1).ToString());
                        string key = record.GetString(2);
                        string name = record.GetString(3);
                        string value = record.GetString(4);

                        XElement currentRegistryKeyElement = null;

                        string currentRegistryKey = $"{root.ToString()}|{key}";
                        if (!currentRegistryKey.Equals(previousRegistryKey))
                        {
                            if(string.IsNullOrEmpty(value))
                            {

                                currentRegistryKeyElement = new XElement(ns + "RegistryKey", new XAttribute("Root", root), new XAttribute("Key", key));
                                switch (name)
                                {
                                    case "*":
                                        currentRegistryKeyElement.Add(new XAttribute("Action", "createAndRemoveOnUninstall"));
                                        break;
                                    case "+":
                                        currentRegistryKeyElement.Add(new XAttribute("Action", "create"));
                                        break;
                                    case "-":
                                        currentRegistryKeyElement.Add(new XAttribute("ForceDeleteOnUninstall", "yes"));
                                        break;
                                }
                            }
                            else
                            {
                                currentRegistryKeyElement = new XElement(ns + "RegistryKey", new XAttribute("Root", root), new XAttribute("Key", key));
                                currentRegistryKeyElement.Add(
                                    new XElement(
                                        ns + "RegistryValue",
                                            new XAttribute("Root", root.ToString()),
                                            new XAttribute("Key", key),
                                            new XAttribute("Value", value)
                                            )
                                            );
                            }
                            includeElement.Add(currentRegistryKeyElement);
                            previousRegistryKey = currentRegistryKey;

                        }
                    }
                }
            }
            catch(Exception ex)
            {

            }
            Console.WriteLine(document);
        }
    }
}
