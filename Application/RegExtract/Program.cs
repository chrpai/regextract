using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RegExtract.Services;
namespace RegExtract
{
    class Program
    {
        static void Main(string[] args)
        {
            string msiPath = @"C:\GitHub\IsWiX-4.14.4.0.msi";
            using (RegistryExtractor registryExtractor = new RegistryExtractor(msiPath))
            {
                registryExtractor.Extract(@"C:\github\registry.wxi");
            }

            Console.WriteLine("PAK");
            Console.Read();
        }
    }
}
