using NHM.Common;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace NhmPackager
{
    internal static class VersionInfoHelpers
    {

        public static Tuple<string, string, string> GenerateVariableTemplate(string path)
        {
            byte[] assemblyBytes = File.ReadAllBytes(path);
            var assembly = Assembly.Load(assemblyBytes);
            var assemblyData = assembly.CustomAttributes;

            string VERSION = assemblyData.Where(data => data.AttributeType == typeof(AssemblyFileVersionAttribute)).FirstOrDefault()?.ConstructorArguments.FirstOrDefault().Value.ToString(); ;


            string BASE_NAME = assemblyData.Where(data => data.AttributeType == typeof(AssemblyTitleAttribute)).FirstOrDefault()?.ConstructorArguments.FirstOrDefault().Value.ToString(); ;
            string COMPANY_NAME = assemblyData.Where(data => data.AttributeType == typeof(AssemblyCompanyAttribute)).FirstOrDefault()?.ConstructorArguments.FirstOrDefault().Value.ToString();
            string APP_DESCRIPTION = assemblyData.Where(data => data.AttributeType == typeof(AssemblyDescriptionAttribute)).FirstOrDefault()?.ConstructorArguments.FirstOrDefault().Value.ToString(); ;
            string COPYRIGHT = assemblyData.Where(data => data.AttributeType == typeof(AssemblyCopyrightAttribute)).FirstOrDefault()?.ConstructorArguments.FirstOrDefault().Value.ToString(); ;

            string BASE_BRAND_NAME = "NiceHash Miner";
            string TRADEMARK = "NICEHASH ®";

            string APP_ID = "com.nicehash.nhm";
            string APP_GUID_ = APP_GUID.GUID;

            string BuildTag = "";
            if (BASE_NAME.Contains("TESTNETDEV"))
            {
                BuildTag = "_TESTNETDEV";
            }
            else if (BASE_NAME.Contains("TESTNET"))
            {
                BuildTag = "_TESTNET";
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
            $"!define APP_GUID \"{APP_GUID_}\"\n" +
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

            return Tuple.Create(NSIS_GENERATED_FILE_TEMPLATE, VERSION, BuildTag);
        }


    }
}
