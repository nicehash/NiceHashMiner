using System.Collections.Generic;

namespace NBMiner
{
    internal static class Shared
    {
        public static Dictionary<string, int> MappedCudaIds = new Dictionary<string, int>();

        public static string GetUUIDFromMinerID(int id)
        {
            foreach (var kvp in MappedCudaIds)
            {
                if (kvp.Value == id) return kvp.Key;
            }
            return null;
        }
    }
}
