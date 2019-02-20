#include "JSONHelpers.h"


#include "json.hpp"
// for convenience
using json = nlohmann::json;

namespace amd_json {
	json getJsonDevicesArray(const std::vector<OpenCLDevice>& devs) {
		json retArr = json::array();
		for (const auto &dev : devs) {
			retArr.push_back({
				{ "DeviceID", dev.DeviceID },
				{ "AMD_BUS_ID", dev.AMD_BUS_ID },
				{ "_CL_DEVICE_NAME", dev._CL_DEVICE_NAME },
				{ "_CL_DEVICE_TYPE", dev._CL_DEVICE_TYPE },
				{ "_CL_DEVICE_GLOBAL_MEM_SIZE", dev._CL_DEVICE_GLOBAL_MEM_SIZE },
				{ "_CL_DEVICE_VENDOR", dev._CL_DEVICE_VENDOR },
				{ "_CL_DEVICE_VERSION", dev._CL_DEVICE_VERSION },
				{ "_CL_DRIVER_VERSION", dev._CL_DRIVER_VERSION }
			});
		}
		return retArr;
	}

    json getJsonPlatform(const JsonLog& p) {
        return json {
        	{ "PlatformName", p.PlatformName },
			{ "PlatformNum", p.PlatformNum },
			{ "Devices", getJsonDevicesArray(p.Devices) }
        };
    }

    json conver_to_json(const std::vector<JsonLog>& ps, std::string statusStr, std::string errorStr) {
    	json platformsJson = json::array();
		for (const auto &p : ps) {
			platformsJson.push_back(getJsonPlatform(p));
		}
		return json {
        	{ "Status", statusStr },
			{ "Platforms", platformsJson },
			{ "ErrorString", errorStr }
        };
    }
}

std::string JSONHelpers::GetPlatformDevicesJsonString(std::vector<JsonLog> &platforms, std::string statusStr, std::string errorStr) {
	const auto j = amd_json::conver_to_json(platforms, statusStr, errorStr);
	return j.dump();
}

std::string JSONHelpers::GetPlatformDevicesJsonStringPretty(std::vector<JsonLog> &platforms, std::string statusStr, std::string errorStr) {
	const auto j = amd_json::conver_to_json(platforms, statusStr, errorStr);
	return j.dump(4);
}
