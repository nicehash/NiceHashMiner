#pragma once

#define __CL_ENABLE_EXCEPTIONS
#define CL_USE_DEPRECATED_OPENCL_2_0_APIS
#define CL_VERSION_1_2
//#define CL_USE_DEPRECATED_OPENCL_1_1_APIS
#include "cl_ext.hpp"

#include <time.h>
#include <functional>
#include <map>

#include "OpenCLDevice.h"

class AMDOpenCLDeviceDetection {
public:
	AMDOpenCLDeviceDetection();
	~AMDOpenCLDeviceDetection();
		
	bool QueryDevices();
	void PrintDevicesJson();
	void PrintDevicesJsonDirty();

	std::string GetDevicesJsonString();
    std::string GetDevicesJsonStringPretty();
	std::string GetErrorString();

private:
	static std::vector<cl::Device> getDevices(std::vector<cl::Platform> const& _platforms, unsigned _platformId);
	static std::vector<cl::Platform> getPlatforms();

	std::vector<std::string> _platformNames;
	std::vector<JsonLog> _devicesPlatformsDevices;

	std::string _errorString = "";
	std::string _statusString = "OK"; // assume ok

	static std::string StringnNullTerminatorFix(const std::string& str);
};
