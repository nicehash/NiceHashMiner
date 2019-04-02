using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceDetectionPrinter
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Count() < 1)
            {
                Console.WriteLine("Error usage [ocl|cuda] [-|p]");
                return;
            }
            var isPretty = args.Count() >= 2 && args[1] != "-";
            var isNoNewline = args.Count() >= 2 && args[1] == "-";
            string printString = null;
            if (args[0].ToLower() == "ocl")
            {
                printString = DeviceDetection.GetOpenCLDevices(isPretty);
            } else if(args[0].ToLower() == "cuda")
            {
                printString = DeviceDetection.GetCUDADevices(isPretty);
            } else
            {
                Console.WriteLine("Error usage [ocl|cuda] [|p]");
                return;
            }

            Console.Write(printString);
            if (!isNoNewline) Console.WriteLine();
        }
    }
}
