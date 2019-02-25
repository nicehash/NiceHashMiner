#pragma once

#include "OpenCLDevice.h"

class JSONHelpers {
public:
	static std::string GetPlatformDevicesJsonString(std::vector<JsonLog> &platforms, std::string statusStr, std::string errorStr);
	static std::string GetPlatformDevicesJsonStringPretty(std::vector<JsonLog> &platforms, std::string statusStr, std::string errorStr);
};
