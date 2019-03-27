echo "DeviceDetectionPrinter.exe cuda p" >> device_detection_test_output.txt
.\DeviceDetectionPrinter.exe cuda p >> device_detection_test_output.txt
echo. >> device_detection_test_output.txt

echo "DeviceDetectionPrinter.exe ocl p" >> device_detection_test_output.txt
.\DeviceDetectionPrinter.exe ocl p >> device_detection_test_output.txt
echo. >> device_detection_test_output.txt

echo "copy .\OpenCL\OpenCL.dll .\OpenCL.dll" >> device_detection_test_output.txt
copy .\openCL\OpenCL.dll .\OpenCL.dll
echo. >> device_detection_test_output.txt

echo "DeviceDetectionPrinter.exe cuda p" >> device_detection_test_output.txt
.\DeviceDetectionPrinter.exe cuda p >> device_detection_test_output.txt
echo. >> device_detection_test_output.txt

echo "DeviceDetectionPrinter.exe ocl p" >> device_detection_test_output.txt
.\DeviceDetectionPrinter.exe ocl p >> device_detection_test_output.txt
echo. >> device_detection_test_output.txt

echo "del .\OpenCL.dll" >> device_detection_test_output.txt
del .\OpenCL.dll