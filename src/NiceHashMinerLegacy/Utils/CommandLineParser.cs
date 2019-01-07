using NiceHashMinerLegacy.Common.Enums;

namespace NiceHashMiner.Utils
{
    internal class CommandLineParser
    {
        // keep it simple only two parameters for now
        public readonly bool IsLang;

        public readonly LanguageType LangValue = 0;

        public CommandLineParser(string[] argv)
        {
            if (ParseCommandLine(argv, "-config", out var tmpString))
            {
                Helpers.ConsolePrint("CommandLineParser", "-config parameter has been depreciated, run setting from GUI");
            }
            if (ParseCommandLine(argv, "-lang", out tmpString))
            {
                IsLang = true;
                // if parsing fails set to default
                if (int.TryParse(tmpString, out var tmp))
                {
                    LangValue = (LanguageType) tmp;
                }
                else
                {
                    LangValue = LanguageType.En;
                }
            }
        }

        private static bool ParseCommandLine(string[] argv, string find, out string value)
        {
            value = "";

            for (var i = 0; i < argv.Length; i++)
            {
                if (argv[i].Equals(find))
                {
                    if ((i + 1) < argv.Length && argv[i + 1].Trim()[0] != '-')
                    {
                        value = argv[i + 1];
                    }

                    return true;
                }
            }

            return false;
        }
    }
}
