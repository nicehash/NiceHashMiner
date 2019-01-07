using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiceHashMinerLegacy.Tests.Stats
{
    internal class TestSocketCalls
    {
        public const string Essentials =
            "{\"method\":\"essentials\",\"d\":[[0,\"1030\"],[1,\"1050\"],[2,\"1050 Ti\"],[3,\"1060 3GB\"],[4,\"1060 6GB\"],[5,\"1070\"],[6,\"1070 Ti\"],[7,\"1080\"],[8,\"1080 Ti\"],[9,\"P106-100\"],[10,\"P104-100\"],[11,\"TITAN V\"],[12,\"TITAN XP\"],[13,\"TITAN X\"]],\"a\":[[0,\"scrypt\"],[1,\"sha256\"],[2,\"scryptnf\"],[3,\"x11\"],[4,\"x13\"],[5,\"keccak\"],[6,\"x15\"],[7,\"nist5\"],[8,\"neoscrypt\"],[9,\"lyra2re\"],[10,\"whirlpoolx\"],[11,\"qubit\"],[12,\"quark\"],[13,\"axiom\"],[14,\"lyra2rev2\"],[15,\"scryptjanenf16\"],[16,\"blake256r8\"],[17,\"blake256r14\"],[18,\"blake256r8vni\"],[19,\"hodl\"],[20,\"daggerhashimoto\"],[21,\"decred\"],[22,\"cryptonight\"],[23,\"lbry\"],[24,\"equihash\"],[25,\"pascal\"],[26,\"x11ghost\"],[27,\"sia\"],[28,\"blake2s\"],[29,\"skunk\"],[30,\"cryptonightv7\"],[31,\"cryptonightheavy\"],[32,\"lyra2z\"],[33,\"x16r\"],[34,\"cryptonightv8\"]],\"l\":[[\"3.0.0.5\",\"https://miner-test.nicehash.com/files/nhm_windows_3.0.0.5_updater.exe\"],[\"1.9.1.2\",\"https://github.com/nicehash/NiceHashMinerLegacy/releases/download/1.9.1.2/nhm_windows_1.9.1.2.zip\"],[\"1.5.13a\",\"https://miner-test.nicehash.com/files/excavator_v1.5.13a_Win64.zip\"],[\"2.5.1\",\"https://miner-test.nicehash.com/files/xmr-stak_2.5.1.zip\"],[\"0.0.9.5\",\"https://miner-test.nicehash.com/files/nhmInstaller_0.0.9.5.exe\"],[\"1.5.13a\",\"no_link\"]],\"r\":[[\"v1.5.13a\",[[[\"neoscrypt\",[8],[0,1]],[\"lyra2rev2\",[14],[0,1]],[\"lyra2z\",[32],[0,1]],[\"x16r\",[33],[0,1]]],5,0,0],[[],5,0,3500000000]],[\"xmr-stak 2.5.1 9636018\",[[[\"cryptonight_v7\",[30],[1]],[\"cryptonight_heavy\",[31],[1]],[\"cryptonight_v8\",[34],[1]]],true,true]]]}";

        public const string InvalidWorkerSet =
            "{\r\n  \"method\": \"mining.set.worker\",\r\n  \"id\": 12,\r\n  \"worker\": \"thisisnotavalidnameitistoolong\"\r\n}";
        public const string ValidWorkerSet =
            "{\r\n  \"method\": \"mining.set.worker\",\r\n  \"id\": 12,\r\n  \"worker\": \"main\"\r\n}";

        public const string InvalidUserSet =
            "{\r\n  \"method\": \"mining.set.username\",\r\n  \"id\": 15,\r\n  \"username\": \"3KpWmp49Cdbswr23KhjagNbwqiwcFh8Br\"\r\n}";
        public const string ValidUserSet =
            "{\r\n  \"method\": \"mining.set.username\",\r\n  \"id\": 15,\r\n  \"username\": \"3KpWmp49Cdbswr23KhjagNbwqiwcFh8Br2\"\r\n}";

        public const string EnableAll =
            "{\r\n  \"method\": \"mining.enable\",\r\n  \"id\": 89,\r\n  \"device\": \"*\"\r\n}";
        public const string EnableOne =
            "{{\r\n  \"method\": \"mining.enable\",\r\n  \"id\": 89,\r\n  \"device\": \"{0}\"\r\n}}";
        public const string DisableAll =
            "{\r\n  \"method\": \"mining.disable\",\r\n  \"id\": 89,\r\n  \"device\": \"*\"\r\n}";
        public const string DisableOne =
            "{{\r\n  \"method\": \"mining.disable\",\r\n  \"id\": 89,\r\n  \"device\": \"{0}\"\r\n}}";
        public const string InvalidEnableOne =
            "{\r\n  \"method\": \"mining.enable\",\r\n  \"id\": 89,\r\n  \"device\": \"invaliduuid\"\r\n}";
        public const string InvalidDisableOne =
            "{\r\n  \"method\": \"mining.disable\",\r\n  \"id\": 89,\r\n  \"device\": \"invaliduuid\"\r\n}";
    }
}
