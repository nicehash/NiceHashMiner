#include "AMDOpenCLDeviceDetection.h"
#include <Windows.h>

extern "C"
{
    __declspec(dllexport) char* __cdecl _GetOpenCLDevices(bool prettyString)
    {
        static std::string ret;
        AMDOpenCLDeviceDetection detection;
        if (detection.QueryDevices()) {
            if (prettyString) {
                ret = detection.GetDevicesJsonStringPretty();
            }
            else {
                ret = detection.GetDevicesJsonString();
            }
        }
        else {
            ret = detection.GetErrorString();
        }
        return (char*)ret.c_str();
    }
}

BOOL WINAPI DllMain(
    HINSTANCE hinstDLL,  // handle to DLL module
    DWORD fdwReason,     // reason for calling function
    LPVOID lpReserved)  // reserved
{
    // Perform actions based on the reason for calling.
    switch (fdwReason)
    {
    case DLL_PROCESS_ATTACH:
        // Initialize once for each new process.
        // Return FALSE to fail DLL load.
        break;

    case DLL_THREAD_ATTACH:
        // Do thread-specific initialization.
        break;

    case DLL_THREAD_DETACH:
        // Do thread-specific cleanup.
        break;

    case DLL_PROCESS_DETACH:
        // Perform any necessary cleanup.
        break;
    }
    return TRUE;  // Successful DLL_PROCESS_ATTACH.
}
