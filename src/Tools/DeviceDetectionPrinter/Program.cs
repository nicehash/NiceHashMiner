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
                Console.WriteLine("Error usage [ocl|cuda] [-nvmlFallback] [-p] [-n]");
                return;
            }
            var typeStr = args.FirstOrDefault() ?? "";
            var detectionType = GetDetectionType(typeStr.ToLower());
            if (detectionType != DetectionType.CUDA && detectionType != DetectionType.OPEN_CL)
            {
                Console.WriteLine("Error usage [ocl|cuda] [-nvmlFallback] [-p] [-n]");
                return;
            }
            var nvmlFallback = args.Contains("-nvmlFallback");
            var isPretty = args.Contains("-p");
            var isNoNewline = args.Contains("-n");
            string printString = null;

            if (detectionType == DetectionType.OPEN_CL)
            {
                printString = DeviceDetection.GetOpenCLDevices(isPretty);
            } else if(detectionType == DetectionType.CUDA)
            {
                printString = DeviceDetection.GetCUDADevices(isPretty, nvmlFallback);
            }

            Console.Write(printString);
            if (!isNoNewline) Console.WriteLine();
        }
    }
}
