using NHM.Common;
using System;
using Microsoft.WindowsAPICodePack.Shell;
using System.Collections.Generic;

namespace NhmPackager
{
    internal static class VersionInfoHelpers
    {
        // Disposing ShellFile under NET6 throws an error hence the static fiasco
        static List<ShellFile> _files = new List<ShellFile>();

        internal static (string nsisFileTemplate, string version) GenerateVariableTemplate(string path)
        {
            ShellFile openFilePath(string path)
            {
                var file = ShellFile.FromFilePath(path);
                _files.Add(file);
                return file;
            }
            var file = openFilePath(path);

            string VERSION = file.Properties.System.FileVersion.Value;
            string BASE_NAME = file.Properties.System.FileDescription.Value;
            string COMPANY_NAME = file.Properties.System.Company.Value;
            string APP_DESCRIPTION = "NiceHash Miner is a simple to use mining tool";
            string COPYRIGHT = file.Properties.System.Copyright.Value;

            string BASE_BRAND_NAME = "NiceHash Miner";
            string TRADEMARK = "NICEHASH ®";

            string APP_ID = "com.nicehash.nhm";
            string APP_GUID_ = APP_GUID.GUID;

            string NSIS_GENERATED_FILE_TEMPLATE =
            "########################################\n" +
            "## This is generated file\n" +
            $"## Date: {DateTime.Now}\n\n" +

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

            return (NSIS_GENERATED_FILE_TEMPLATE, VERSION);
        }
    }
}
