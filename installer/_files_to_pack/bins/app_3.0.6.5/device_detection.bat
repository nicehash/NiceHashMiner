echo "device_detection.exe cpu -p" > device_detection.txt
.\device_detection.exe cpu -p >> device_detection.txt
echo. >> device_detection.txt

echo "device_detection.exe cuda -p" >> device_detection.txt
.\device_detection.exe cuda -p >> device_detection.txt
echo. >> device_detection.txt

echo "device_detection.exe ocl -p" >> device_detection.txt
.\device_detection.exe ocl -p >> device_detection.txt
echo. >> device_detection.txt

echo "device_detection.exe all -p" >> device_detection.txt
.\device_detection.exe all -p >> device_detection.txt
echo. >> device_detection.txt

echo "copy .\OpenCL\OpenCL.dll .\OpenCL.dll" >> device_detection.txt
copy .\openCL\OpenCL.dll .\OpenCL.dll
echo. >> device_detection.txt

REM With OpenCL.dll
echo "device_detection.exe cpu -p" >> device_detection.txt
.\device_detection.exe cpu -p >> device_detection.txt
echo. >> device_detection.txt

echo "device_detection.exe cuda -p" >> device_detection.txt
.\device_detection.exe cuda -p >> device_detection.txt
echo. >> device_detection.txt

echo "device_detection.exe ocl -p" >> device_detection.txt
.\device_detection.exe ocl -p >> device_detection.txt
echo. >> device_detection.txt

echo "device_detection.exe all -p" >> device_detection.txt
.\device_detection.exe all -p >> device_detection.txt
echo. >> device_detection.txt

echo "del .\OpenCL.dll" >> device_detection.txt
del .\OpenCL.dll
