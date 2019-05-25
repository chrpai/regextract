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
            string msiPath = @"..\..\..\..\Test\Test.msi";

            try
            {
                using (RegistryExtractor registryExtractor = new RegistryExtractor(msiPath))
                {
                    registryExtractor.Extract(@"..\..\..\..\Test\test.wxi");
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Console.WriteLine("PAK");
            Console.Read();
        }
    }
}
