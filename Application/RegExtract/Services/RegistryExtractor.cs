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
                    XElement currentRegistryKeyElement = null;
                    foreach (var record in view)
                    {
                        string registry = record.GetString(1);
                        RegistryRootType root = (RegistryRootType)Enum.Parse(typeof(RegistryRootType), record.GetInteger(2).ToString());
                        string key = record.GetString(3);
                        string name = record.GetString(4);
                        string value = record.GetString(5);


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
                            includeElement.Add(currentRegistryKeyElement);
                            previousRegistryKey = currentRegistryKey;

                        }

                        //TODO: Implement Multistring @Action attribute (PREPEND, APPEND)
                        if (!string.IsNullOrEmpty(value))
                        {
                            string dataType = string.Empty;

                            if (value.Contains("[~]"))
                            {
                                dataType = "multiString";
                            }
                            else if (value.StartsWith("#%"))
                            {
                                value = value.Substring(2);
                                dataType = "expandable";
                            }
                            else if(value.StartsWith("#x"))
                            {
                                value = value.Substring(2);
                                dataType = "binary";
                            }
                            else if (value.StartsWith("#"))
                            {
                                value = value.Substring(1);
                                dataType = "integer";
                            }
                            else
                            {
                                dataType = "string";
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
