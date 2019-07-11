using System;

namespace RigIDPrinter
{
    class Program
    {
        static void Main(string[] args)
        {
            var rigId = NHM.UUID.UUID.GetDeviceB64UUID(true);
            Console.WriteLine($"RIG_ID: {rigId}");
        }
    }
}
