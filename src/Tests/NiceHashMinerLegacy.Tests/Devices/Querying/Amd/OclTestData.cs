using System.Collections.Generic;

namespace NiceHashMinerLegacy.Tests.Devices.Querying.Amd
{
    internal static class OclTestData
    {
        public const string TestData1 = @"
{
    ""ErrorString"": """",
    ""Platforms"": [
        {
            ""Devices"": [
                {
                    ""AMD_BUS_ID"": 13,
                    ""DeviceID"": 0,
                    ""_CL_DEVICE_GLOBAL_MEM_SIZE"": 4294967296,
                    ""_CL_DEVICE_NAME"": ""Baffin"",
                    ""_CL_DEVICE_TYPE"": ""GPU"",
                    ""_CL_DEVICE_VENDOR"": ""Advanced Micro Devices, Inc."",
                    ""_CL_DEVICE_VERSION"": ""OpenCL 2.0 AMD-APP (2527.10)"",
                    ""_CL_DRIVER_VERSION"": ""2527.10""
                },
                {
                    ""AMD_BUS_ID"": 8,
                    ""DeviceID"": 1,
                    ""_CL_DEVICE_GLOBAL_MEM_SIZE"": 4294967296,
                    ""_CL_DEVICE_NAME"": ""Baffin"",
                    ""_CL_DEVICE_TYPE"": ""GPU"",
                    ""_CL_DEVICE_VENDOR"": ""Advanced Micro Devices, Inc."",
                    ""_CL_DEVICE_VERSION"": ""OpenCL 2.0 AMD-APP (2527.10)"",
                    ""_CL_DRIVER_VERSION"": ""2527.10""
                },
                {
                    ""AMD_BUS_ID"": 7,
                    ""DeviceID"": 2,
                    ""_CL_DEVICE_GLOBAL_MEM_SIZE"": 8589934592,
                    ""_CL_DEVICE_NAME"": ""Ellesmere"",
                    ""_CL_DEVICE_TYPE"": ""GPU"",
                    ""_CL_DEVICE_VENDOR"": ""Advanced Micro Devices, Inc."",
                    ""_CL_DEVICE_VERSION"": ""OpenCL 2.0 AMD-APP (2527.10)"",
                    ""_CL_DRIVER_VERSION"": ""2527.10""
                },
                {
                    ""AMD_BUS_ID"": 12,
                    ""DeviceID"": 3,
                    ""_CL_DEVICE_GLOBAL_MEM_SIZE"": 8589934592,
                    ""_CL_DEVICE_NAME"": ""Ellesmere"",
                    ""_CL_DEVICE_TYPE"": ""GPU"",
                    ""_CL_DEVICE_VENDOR"": ""Advanced Micro Devices, Inc."",
                    ""_CL_DEVICE_VERSION"": ""OpenCL 2.0 AMD-APP (2527.10)"",
                    ""_CL_DRIVER_VERSION"": ""2527.10""
                },
                {
                    ""AMD_BUS_ID"": 11,
                    ""DeviceID"": 4,
                    ""_CL_DEVICE_GLOBAL_MEM_SIZE"": 8573157376,
                    ""_CL_DEVICE_NAME"": ""gfx900"",
                    ""_CL_DEVICE_TYPE"": ""GPU"",
                    ""_CL_DEVICE_VENDOR"": ""Advanced Micro Devices, Inc."",
                    ""_CL_DEVICE_VERSION"": ""OpenCL 2.0 AMD-APP (2527.10)"",
                    ""_CL_DRIVER_VERSION"": ""2527.10 (PAL,HSAIL)""
                }
            ],
            ""PlatformName"": ""AMD Accelerated Parallel Processing"",
            ""PlatformNum"": 0
        }
    ],
    ""Status"": ""OK""
}";

        public static List<int> TestData1BusIDs = new List<int>
        {
            13,
            8,
            7,
            12,
            11
        };
    }
}
