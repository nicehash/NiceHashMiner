using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace InstallerHelper
{
    class Program
    {
        static Tuple<string, string> GenerateVariableTemplate(string path)
        {
            var assembly = Assembly.LoadFrom(path);
            var assemblyData = assembly.CustomAttributes;

            string VERSION = assemblyData.Where(data => data.AttributeType == typeof(AssemblyFileVersionAttribute)).FirstOrDefault()?.ConstructorArguments.FirstOrDefault().Value.ToString(); ;


            string BASE_NAME = assemblyData.Where(data => data.AttributeType == typeof(AssemblyTitleAttribute)).FirstOrDefault()?.ConstructorArguments.FirstOrDefault().Value.ToString(); ;
            string COMPANY_NAME = assemblyData.Where(data => data.AttributeType == typeof(AssemblyCompanyAttribute)).FirstOrDefault()?.ConstructorArguments.FirstOrDefault().Value.ToString();
            string APP_DESCRIPTION = assemblyData.Where(data => data.AttributeType == typeof(AssemblyDescriptionAttribute)).FirstOrDefault()?.ConstructorArguments.FirstOrDefault().Value.ToString(); ;
            string COPYRIGHT = assemblyData.Where(data => data.AttributeType == typeof(AssemblyCopyrightAttribute)).FirstOrDefault()?.ConstructorArguments.FirstOrDefault().Value.ToString(); ;

            string BASE_BRAND_NAME = "NiceHash Miner";
            string TRADEMARK = "NICEHASH ®";
            
            string APP_ID = "com.nicehash.nhm";
            string APP_GUID = "8abad8e2-b957-48ed-92ba-4339c2a40e78";

            if(BASE_NAME == "NiceHashMinerLegacy")
            {
                APP_ID = "com.nicehash.nhml";
                APP_GUID = "6722ab0a-4f1d-4703-8169-dede20aad630";
            }

            string NSIS_GENERATED_FILE_TEMPLATE =
            "########################################\n" +
            "## This is generated file\n" +
            $"## Date: {DateTime.Now.ToString()}\n\n" +

            "; Product and version\n" +
            $"!define BASE_NAME \"{BASE_NAME}\"\n" +
            $"!define BASE_BRAND_NAME \"{BASE_BRAND_NAME}\"\n" +
            $"!define VERSION \"{VERSION}\"\n\n" +

            $"!define COMPANY_NAME \"{COMPANY_NAME}\"\n" +
            $"!define APP_ID \"{APP_ID}\"\n" +
            "; TODO check if this needs to be always re-generated(UUID-v5 is used in electron-builder)\n" +
            "; TODO this is uuid4 and check if we need to generate this\n" +
            $"!define APP_GUID \"{APP_GUID}\"\n" +
            $"!define APP_DESCRIPTION \"{APP_DESCRIPTION}\"\n\n" +

            "!macro addVersionInfo\n" +
            ";--------------------------------\n" +
            ";Version Information\n\n" +

             "VIProductVersion \"${VERSION}\"\n" +
             "VIAddVersionKey \"ProductName\" \"${PRODUCT_NAME}\"\n" +
             "; VIAddVersionKey \"Comments\" \"Do we need this?\"\n" +
             "VIAddVersionKey \"CompanyName\" \"${COMPANY_NAME}\"\n" +
             $"VIAddVersionKey \"LegalTrademarks\" \"{TRADEMARK}\"\n" +
             $"VIAddVersionKey \"LegalCopyright\"  \"{COPYRIGHT}\"\n" +
             "VIAddVersionKey \"FileDescription\" \"${APP_DESCRIPTION}\"\n" +
             "VIAddVersionKey \"FileVersion\" \"${VERSION}\"\n\n" +

            ";--------------------------------\n" +
            "!macroend\n";

            return new Tuple<string, string>(NSIS_GENERATED_FILE_TEMPLATE, VERSION);
        }

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Please enter NiceHashMiner.exe path!");
            } else
            {
                var exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var pluginPackagesFolder = Path.Combine(exePath, "_files_to_pack");

                if (Directory.Exists(pluginPackagesFolder))
                {
                    Console.WriteLine("Deleting old installer files");
                    Directory.Delete(pluginPackagesFolder, true);
                }
                if (!Directory.Exists(pluginPackagesFolder))
                {
                    Directory.CreateDirectory(pluginPackagesFolder);
                }

                var (generatedTemplate, version) = GenerateVariableTemplate(args[0]);
                File.WriteAllText(Path.Combine(pluginPackagesFolder, "packageDefsGenerated.nsh"), generatedTemplate, new UTF8Encoding(true));
                File.WriteAllText(Path.Combine(pluginPackagesFolder, "version.txt"), version);
            }
        }
    }
}
