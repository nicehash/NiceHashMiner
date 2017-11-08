using NiceHashMiner.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace NiceHashMiner.Utils
{
    class CommandLineParser
    {
        // keep it simple only two parameters for now
        readonly public bool IsLang = false;
        readonly public string LangValue = "en";

        public CommandLineParser(string[] argv)
        {
            string tmpString;
            if(ParseCommandLine(argv, "-config", out tmpString)) {
                Helpers.ConsolePrint("CommandLineParser", "-config parameter has been depreciated, run setting from GUI");
            }
            if (ParseCommandLine(argv, "-lang", out tmpString)) {
                IsLang = true;
                // if parsing fails set to default
                if (System.IO.File.Exists(Environment.CurrentDirectory + "langs/" + tmpString))
                {
                    LangValue = tmpString;
                } else {
                    LangValue = "en";
                }
            }
        }

        private bool ParseCommandLine(string[] argv, string find, out string value)
        {
            value = "";

            for (int i = 0; i < argv.Length; i++)
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
