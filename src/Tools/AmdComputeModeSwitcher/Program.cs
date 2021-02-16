using Microsoft.Win32;
using System;
using System.Collections.Generic;

namespace AmdComputeModeSwitcher
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var localMachineKey = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Control\\Class\\{4d36e968-e325-11ce-bfc1-08002be10318}"))
            {
                var cardFolders = localMachineKey.GetSubKeyNames();
                var results = new Dictionary<string, string>();
                var notComputeMode = 0;
                int tmp;

                foreach (var cardFolder in cardFolders)
                    if (int.TryParse(cardFolder, out tmp))
                    {
                        RegistryKey cardRegistry = null;
                        try
                        {
                            cardRegistry = localMachineKey.OpenSubKey(cardFolder);
                        }
                        catch (Exception) { }
                        if (cardRegistry != null)
                        {
                            var KMD_EnableInternalLargePage = cardRegistry.GetValue("KMD_EnableInternalLargePage");
                            if (KMD_EnableInternalLargePage == null || KMD_EnableInternalLargePage.ToString() != "2")
                            {
                                notComputeMode++;
                                results.Add(cardFolder, "Not in compute mode");
                            }
                            else
                            {
                                results.Add(cardFolder, "Compute mode");
                            }
                        }
                    }

                var cardString = "All cards will be switched to " + (notComputeMode > 0 ? "compute" : "graphics") + " mode!\n";
                foreach (var result in results)
                {
                    cardString += "\n" + result.Key + ": " + result.Value;
                }
                Console.WriteLine(cardString);

                try
                {
                    results = new Dictionary<string, string>();

                    foreach (var cardFolder in cardFolders)
                        if (int.TryParse(cardFolder, out tmp))
                        {
                            RegistryKey cardRegistry = null;
                            try
                            {
                                cardRegistry = localMachineKey.OpenSubKey(cardFolder, true);
                            }
                            catch (Exception) { }
                            if (cardRegistry != null)
                            {
                                if (notComputeMode > 0)
                                {
                                    // Switch all to compute mode 
                                    try { cardRegistry.SetValue("KMD_EnableInternalLargePage", "2", RegistryValueKind.DWord); results.Add(cardFolder, "Success"); }
                                    catch (Exception ex) { results.Add(cardFolder, "Error: " + ex.Message); }
                                }
                                else
                                {
                                    // Switch all to graphics mode 
                                    try { cardRegistry.DeleteValue("KMD_EnableInternalLargePage"); results.Add(cardFolder, "Success"); }
                                    catch (Exception ex) { results.Add(cardFolder, "Error: " + ex.Message); }
                                }
                            }
                        }

                    cardString = "Switched successfully to " + (notComputeMode > 0 ? "compute" : "graphics") + " mode!\n";
                    foreach (var result in results)
                    {
                        cardString += "\n" + result.Key + ": " + result.Value;
                    }
                    Console.WriteLine("Switched successfully");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }
}
