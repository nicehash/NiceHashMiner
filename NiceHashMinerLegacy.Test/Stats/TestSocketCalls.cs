using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiceHashMinerLegacy.Tests.Stats
{
    internal class TestSocketCalls
    {
        public const string Data = @"{
  ""method"": ""essentials"",
  ""params"": [
    [
      ""3.0.0.1"",
      ""http://miner-test.nicehash.com:8880/downloader/installers/nhm_windows_3.0.0.1.exe"",
      ""1.9.1.5"",
      ""https://github.com/nicehash/NiceHashMinerLegacy/releases/download/1.9.0.5/nhm_windows_1.9.0.5.zip""
    ],
    [
      ""1.5.10a"",
      ""https://miner.nicehash.com/windows/bins/excavator_v1.5.10a_Win64.zip""
    ],
    [
      [
        0,
        ""NVIDIA GTX 750 Ti""
      ],
      [
        1,
        ""NVIDIA GeForce 930 MX""
      ],
      [
        2,
        ""NVIDIA GTX 960""
      ],
      [
        3,
        ""NVIDIA GTX 970""
      ],
      [
        4,
        ""NVIDIA GTX 980""
      ],
      [
        5,
        ""NVIDIA GTX 980 Ti""
      ],
      [
        6,
        ""NVIDIA GTX 1050""
      ],
      [
        7,
        ""NVIDIA GTX 1050 Ti""
      ],
      [
        8,
        ""NVIDIA GTX 1060 3GB""
      ],
      [
        9,
        ""NVIDIA GTX 1060 6GB""
      ],
      [
        10,
        ""NVIDIA GTX 1070""
      ],
      [
        11,
        ""NVIDIA GTX 1070 Ti""
      ],
      [
        12,
        ""NVIDIA GTX 1080""
      ],
      [
        13,
        ""NVIDIA GTX 1080 Ti""
      ],
      [
        14,
        ""NVIDIA P106-100""
      ],
      [
        15,
        ""NVIDIA P104-100""
      ],
      [
        16,
        ""NVIDIA TITAN V""
      ],
      [
        17,
        ""NVIDIA TITAN XP""
      ]
    ],
    [
      [
        0,
        ""scrypt""
      ],
      [
        1,
        ""sha256""
      ],
      [
        2,
        ""scryptnf""
      ],
      [
        3,
        ""x11""
      ],
      [
        4,
        ""x13""
      ],
      [
        5,
        ""keccak""
      ],
      [
        6,
        ""x15""
      ],
      [
        7,
        ""nist5""
      ],
      [
        8,
        ""neoscrypt""
      ],
      [
        9,
        ""lyra2re""
      ],
      [
        10,
        ""whirlpoolx""
      ],
      [
        11,
        ""qubit""
      ],
      [
        12,
        ""quark""
      ],
      [
        13,
        ""axiom""
      ],
      [
        14,
        ""lyra2rev2""
      ],
      [
        15,
        ""scryptjanenf16""
      ],
      [
        16,
        ""blake256r8""
      ],
      [
        17,
        ""blake256r14""
      ],
      [
        18,
        ""blake256r8vni""
      ],
      [
        19,
        ""hodl""
      ],
      [
        20,
        ""daggerhashimoto""
      ],
      [
        21,
        ""decred""
      ],
      [
        22,
        ""cryptonight""
      ],
      [
        23,
        ""lbry""
      ],
      [
        24,
        ""equihash""
      ],
      [
        25,
        ""pascal""
      ],
      [
        26,
        ""x11ghost""
      ],
      [
        27,
        ""sia""
      ],
      [
        28,
        ""blake2s""
      ],
      [
        29,
        ""skunk""
      ],
      [
        30,
        ""cryptonightv7""
      ],
      [
        31,
        ""cryptonightheavy""
      ],
      [
        32,
        ""lyra2z""
      ],
      [
        33,
        ""x16r""
      ]
    ],
    [
      ""2.4.5"",
      ""https://miner.nicehash.com/windows/bins/xmr-stak_2.4.5_03.zip""
    ],
    [
      ""15.0.24210.00"",
      ""https://aka.ms/vs/15/release/VC_redist.x64.exe""
    ],
    [
      ""0.0.9.1"",
      ""https://miner.nicehash.com/windows/bins/nhmInstaller.exe""
    ]
  ]
}";
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
