#pragma once

#include <map>
#include <string>
#include <cstdint>

#include <nvml.h>

#include "CudaDevice.h"

#ifdef WIN32
#define USE_DYNAMIC_LIB_LOAD 1
#else
#define USE_DYNAMIC_LIB_LOAD 0
#endif

class nvidia_nvml_helper
{
private:
	nvidia_nvml_helper(); // no instances
public:
	static void SafeNVMLInit();
	// set UUID, VendorID and VendorName
	static void SetCudaDeviceAttributes(const char *pciBusID, CudaDevice &cudaDevice);
	static void SafeNVMLShutdown();

	static std::string GetDriverVersionSafe();

private:
	// helpers
	static std::map<std::uint16_t, std::string> _VENDOR_NAMES;
	static std::uint16_t getVendorId(nvmlPciInfo_t &nvmlPciInfo);
	static std::string getVendorString(nvmlPciInfo_t &nvmlPciInfo);
};

