#include "CudaDetection.h"

#include <iostream>
#include <stdexcept>

#include <sstream>

namespace cuda_json {
    json conver_to_json(const CudaDevice& d) {
        return json {
        	{ "DeviceID", d.DeviceID },
			{ "VendorName", d.VendorName },
			{ "DeviceName", d.DeviceName },
			{ "SM_major", d.SM_major },
			{ "SM_minor", d.SM_minor },
			{ "UUID", d.UUID },
			{ "DeviceGlobalMemory", d.DeviceGlobalMemory },
			{ "pciDeviceId", d.pciDeviceId },
			{ "pciSubSystemId", d.pciSubSystemId },
			{ "SMX", d.SMX },
			{ "VendorID", d.VendorID },
			{ "HasMonitorConnected", d.HasMonitorConnected },
            { "pciBusID", d.pciBusID },
        };
    }
}

#include "nvidia_nvml_helper.h"
#include "cuda_helper.h"

using namespace std;
using namespace cuda_json;

CudaDetection::CudaDetection() { }
CudaDetection::~CudaDetection() { }

#define PCI_BUS_LEN 64

bool CudaDetection::QueryDevices() {
	try {
		int device_count;
		CUDA_SAFE_CALL(cudaGetDeviceCount(&device_count));
		nvidia_nvml_helper::SafeNVMLInit(); // NVML_SAFE_CALL(nvmlInit());
		for (int i = 0; i < device_count; ++i) {
			CudaDevice cudaDevice;

			cudaDeviceProp props;
			CUDA_SAFE_CALL(cudaGetDeviceProperties(&props, i));
			char pciBusID[PCI_BUS_LEN];
			CUDA_SAFE_CALL(cudaDeviceGetPCIBusId(pciBusID, PCI_BUS_LEN, i));

			// init serial vendor stuff
			nvidia_nvml_helper::SetCudaDeviceAttributes(pciBusID, cudaDevice);

			// init device info
			cudaDevice.DeviceID = i;
			cudaDevice.pciBusID = props.pciBusID;
			//cudaDevice.VendorName = getVendorString(pciInfo);
			cudaDevice.DeviceName = props.name;
			cudaDevice.SM_major = props.major;
			cudaDevice.SM_minor = props.minor;
			//cudaDevice.UUID = uuid;
			cudaDevice.DeviceGlobalMemory = props.totalGlobalMem;
			//cudaDevice.pciDeviceId = pciInfo.pciDeviceId;
			//cudaDevice.pciSubSystemId = pciInfo.pciSubSystemId;
			cudaDevice.SMX = props.multiProcessorCount;
			//cudaDevice.VendorID = getVendorId(pciInfo);

			_cudaDevices.push_back(cudaDevice);
		}
		_driverVersionStr = nvidia_nvml_helper::GetDriverVersionSafe();
		nvidia_nvml_helper::SafeNVMLShutdown(); // NVML_SAFE_CALL(nvmlShutdown());
	}
	catch (runtime_error &err) {
		_errorString = err.what();
		//_errorMsgs.push_back(err.what());
		return false;
	}
	return true;
}

json CudaDetection::createJsonObj() {
	json j = {
		{"DriverVersion", _driverVersionStr},
		{"CudaDevices", json::array()},
		{"ErrorString", _errorString},
	};
	for (const auto &d : _cudaDevices) {
		j["CudaDevices"].push_back(conver_to_json(d));
	}
	return j;
}

void CudaDetection::PrintDevicesJson() {
	json j = createJsonObj();
	cout << j.dump(4);
}
// non human readable print
void CudaDetection::PrintDevicesJson_d() {
	json j = createJsonObj();
	cout << j.dump();
}

string CudaDetection::GetDevicesJsonString() {
	json j = createJsonObj();
	return j.dump();
}

string CudaDetection::GetErrorString() {
	return _errorString;
}

void CudaDetection::PrintDriverVersion() {
	cout << _driverVersionStr << endl;
}

std::string CudaDetection::GetDriverVersion() {
	return _driverVersionStr;
}
