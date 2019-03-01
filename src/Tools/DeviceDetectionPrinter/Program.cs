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
                Console.WriteLine("Error usage [ocl|cuda] [|p]");
                return;
            }

            var isPretty = args.Count() >= 2;
            if (args[0].ToLower() == "ocl")
            {
                Console.WriteLine(DeviceDetection.GetOpenCLDevices(isPretty));
            } else if(args[0].ToLower() == "cuda")
            {
                Console.WriteLine(DeviceDetection.GetCUDADevices(isPretty));
            } else
            {
                Console.WriteLine("Error usage [ocl|cuda] [|p]");
            }
        }
    }
}
