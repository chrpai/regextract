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

            string selectComment = "SELECT `Registry`, `Root`, `Key`, `Name`, `Value` FROM `Registry` ORDER BY `Root`, `Key`";
            try
            {
                using (View view = _database.OpenView(selectComment))
                {
                    view.Execute();
                    foreach (var record in view)
                    {
                        string registry = record.GetString(1);
                        RegistryRootType root = (RegistryRootType)Enum.Parse(typeof(RegistryRootType), record.GetInteger(2).ToString());
                        string key = record.GetString(3);
                        string name = record.GetString(4);
                        string value = record.GetString(5);

                        XElement currentRegistryKeyElement = null;

                        string currentRegistryKey = $"{root.ToString()}|{key}";
                        if (!currentRegistryKey.Equals(previousRegistryKey))
                        {
                            currentRegistryKeyElement = new XElement(ns + "RegistryKey", new XAttribute("Id", registry), new XAttribute("Root", root), new XAttribute("Key", key));
                            if (string.IsNullOrEmpty(value))
                            {

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
                                char prefix = name[0];
                                string dataType = string.Empty;
                                switch (prefix)
                                {
                                    case '#':
                                        dataType = "integer";
                                        break;
                                    default:
                                        dataType = "string";
                                        break;
                                }

                                XElement currentRegistryValueElement = new XElement(
                                                     ns + "RegistryValue",
                                                         new XAttribute("Id", registry),
                                                         new XAttribute("Name", name),
                                                         new XAttribute("Value", value),
                                                         new XAttribute("Type", dataType)
                                                         );

                                currentRegistryKeyElement.Add(currentRegistryValueElement);

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
