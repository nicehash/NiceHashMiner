#pragma once

#include <vector>
#include <string>

#include "CudaDevice.h"

#include "json.hpp"
// for convenience
using json = nlohmann::json;

class CudaDetection
{
public:
	CudaDetection();
	~CudaDetection();

	bool QueryDevices();
	void PrintDevicesJson();
	void PrintDevicesJson_d();
	std::string GetDevicesJsonString();
	std::string GetErrorString();

	void PrintDriverVersion();
	std::string GetDriverVersion();

private:
	json createJsonObj();

	std::string _errorString = "";
	//std::vector<std::string> _errorMsgs;
	std::vector<CudaDevice> _cudaDevices;
	// driver version
	std::string _driverVersionStr = "";
};

