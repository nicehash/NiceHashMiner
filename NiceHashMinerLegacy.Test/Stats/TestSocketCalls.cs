using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiceHashMinerLegacy.Tests.Stats
{
    internal class TestSocketCalls
    {
        public const string Data =
            "{\r\n  \"method\": \"essentials\",\r\n  \"params\":\r\n  [\r\n    [\"3.0.0.1\", \"1.9.1.2\"],\r\n    [\"1.5.5a\", \"https://github.com/nicehash/excavator/releases/download/v1.5.5a/excavator_v1.5.5a_NVIDIA_Win64.zip\"],\r\n    [\r\n      [0, \"NVIDIA GTX 1070 Ti\"], \r\n      [1, \"MSI GeForce GTX 1060 6GB\"],\r\n   ],\r\n    [\r\n      [0, \"scrypt\"],\r\n      [1, \"sha256\"],\r\n      [2, \"scryptnf\"],\r\n      [3, \"x11\"],\r\n      [4, \"x13\"],\r\n      [5, \"keccak\"],\r\n ]\r\n  ]\r\n}";

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
