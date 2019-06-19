echo "cuda_device_detection.exe -p" > testOutput.txt
.\cuda_device_detection.exe -p >> testOutput.txt
echo. >> testOutput.txt

echo "cuda_device_detection.exe -p -nvmlFallback" >> testOutput.txt
.\cuda_device_detection.exe -p -nvmlFallback >> testOutput.txt
echo. >> testOutput.txt

echo "opencl_device_detection.exe -p" >> testOutput.txt
.\opencl_device_detection.exe -p >> testOutput.txt
echo. >> testOutput.txt

echo "DeviceDetectionPrinter.exe cuda -p" >> testOutput.txt
.\DeviceDetectionPrinter.exe cuda -p >> testOutput.txt
echo. >> testOutput.txt

echo "DeviceDetectionPrinter.exe cuda -p -nvmlFallback" >> testOutput.txt
.\DeviceDetectionPrinter.exe cuda -p -nvmlFallback >> testOutput.txt
echo. >> testOutput.txt

echo "DeviceDetectionPrinter.exe ocl -p" >> testOutput.txt
.\DeviceDetectionPrinter.exe ocl -p >> testOutput.txt
echo. >> testOutput.txt

echo "copy .\OpenCL\OpenCL.dll .\OpenCL.dll" >> testOutput.txt
copy .\openCL\OpenCL.dll .\OpenCL.dll
echo. >> testOutput.txt

echo "cuda_device_detection.exe -p" >> testOutput.txt
.\cuda_device_detection.exe -p >> testOutput.txt
echo. >> testOutput.txt

echo "opencl_device_detection.exe -p" >> testOutput.txt
.\opencl_device_detection.exe -p >> testOutput.txt
echo. >> testOutput.txt

echo "DeviceDetectionPrinter.exe cuda -p" >> testOutput.txt
.\DeviceDetectionPrinter.exe cuda -p >> testOutput.txt
echo. >> testOutput.txt

echo "DeviceDetectionPrinter.exe cuda -p -nvmlFallback" >> testOutput.txt
.\DeviceDetectionPrinter.exe cuda -p -nvmlFallback >> testOutput.txt
echo. >> testOutput.txt

echo "DeviceDetectionPrinter.exe ocl -p" >> testOutput.txt
.\DeviceDetectionPrinter.exe ocl -p >> testOutput.txt
echo. >> testOutput.txt

echo "del .\OpenCL.dll" >> testOutput.txt
del .\OpenCL.dll