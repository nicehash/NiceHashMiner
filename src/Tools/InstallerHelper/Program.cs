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
        static string GenerateVariableTemplate()
        {
            string VERSION = "1.9.2.11";

            string APP_ID = "com.nicehash.nhm";
            string APP_GUID = "8abad8e2-b957-48ed-92ba-4339c2a40e78";

            string BASE_NAME = "NiceHashMiner";
            string BASE_BRAND_NAME = "NiceHash Miner";
            string COMPANY_NAME = "H-BIT d.o.o.";
            string APP_DESCRIPTION = BASE_NAME + " - Client for NiceHash.com";

            string TRADEMARK = "NICEHASH ®";
            string COPYRIGHT = "Copyright © 2018 H-BIT d.o.o.";

            string NSIS_GENERATED_FILE_TEMPLATE =
            "########################################\n" +
            "## This is generated file\n" +
            "## Date: REPLACE_DATE\n\n" +

            "; Product and version\n" +
            "!define BASE_NAME \"REPLACE_BASE_NAME\"\n" +
            "!define BASE_BRAND_NAME \"REPLACE_BASE_BRAND_NAME\"\n" +
            "!define VERSION \"REPLACE_VERSION\"\n\n" +

            "!define COMPANY_NAME \"REPLACE_COMPANY_NAME\"\n" +
            "!define APP_ID \"REPLACE_APP_ID\"\n" +
            "; TODO check if this needs to be always re-generated(UUID-v5 is used in electron-builder)\n" +
            "; TODO this is uuid4 and check if we need to generate this\n" +
            "!define APP_GUID \"REPLACE_APP_GUID\"\n" +
            "!define APP_DESCRIPTION \"REPLACE_APP_DESCRIPTION\"\n\n" +

            "!macro addVersionInfo\n" +
            ";--------------------------------\n" +
            ";Version Information\n\n" +

             "VIProductVersion \"${VERSION}\"\n" +
             "VIAddVersionKey \"ProductName\" \"${PRODUCT_NAME}\"\n" +
             "; VIAddVersionKey \"Comments\" \"Do we need this?\"\n" +
             "VIAddVersionKey \"CompanyName\" \"${COMPANY_NAME}\"\n" +
             "VIAddVersionKey \"LegalTrademarks\" \"REPLACE_TRADEMARK\"\n" +
             "VIAddVersionKey \"LegalCopyright\"  \"REPLACE_COPYRIGHT\"\n" +
             "VIAddVersionKey \"FileDescription\" \"${APP_DESCRIPTION}\"\n" +
              "VIAddVersionKey \"FileVersion\" \"${VERSION}\"\n\n" +

            ";--------------------------------\n" +
            "!macroend\n";

            var finalString = NSIS_GENERATED_FILE_TEMPLATE;
            finalString = finalString.Replace("REPLACE_DATE", DateTime.Now.ToString());
            finalString = finalString.Replace("REPLACE_BASE_NAME", BASE_NAME);
            finalString = finalString.Replace("REPLACE_BASE_BRAND_NAME", BASE_BRAND_NAME);
            finalString = finalString.Replace("REPLACE_VERSION", VERSION);
            finalString = finalString.Replace("REPLACE_COMPANY_NAME", COMPANY_NAME);
            finalString = finalString.Replace("REPLACE_APP_ID", APP_ID);
            finalString = finalString.Replace("REPLACE_APP_GUID", APP_GUID);
            finalString = finalString.Replace("REPLACE_APP_DESCRIPTION", APP_DESCRIPTION);
            finalString = finalString.Replace("REPLACE_TRADEMARK", TRADEMARK);
            finalString = finalString.Replace("REPLACE_COPYRIGHT", COPYRIGHT);
            return finalString;
        }


        static void Main(string[] args)
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

            var generatedTemplate = GenerateVariableTemplate();
            File.WriteAllText(Path.Combine(pluginPackagesFolder, "packageDefsGenerated.nsh"), generatedTemplate);
        }
    }
}
