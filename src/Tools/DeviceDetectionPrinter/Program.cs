using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceDetectionPrinter
{
    class Program
    {
        private enum DetectionType
        {
            UNKNOWN,
            CUDA,
            OPEN_CL
        }

        private static DetectionType GetDetectionType(string firstArg)
        {
            if (firstArg == "ocl") return DetectionType.OPEN_CL;

            if (firstArg == "cuda") return DetectionType.CUDA;

            return DetectionType.UNKNOWN;
        }

        static void Main(string[] args)
        {
            if (args.Count() < 1)
            {
                Console.WriteLine("Error usage [ocl|cuda] [-|p]");
                return;
            }
            var detectionType = GetDetectionType(args[0].ToLower());
            if (detectionType != DetectionType.CUDA && detectionType != DetectionType.OPEN_CL)
            {
                Console.WriteLine("Error usage [ocl|cuda] [-|p]");
                return;
            }

            var isPretty = args.Count() >= 2 && args[1] != "-";
            var isNoNewline = args.Count() >= 2 && args[1] == "-";
            string printString = null;

            if (detectionType == DetectionType.OPEN_CL)
            {
                printString = DeviceDetection.GetOpenCLDevices(isPretty);
            } else if(detectionType == DetectionType.CUDA)
            {
                printString = DeviceDetection.GetCUDADevices(isPretty);
            }

            Console.Write(printString);
            if (!isNoNewline) Console.WriteLine();
        }
    }
}
