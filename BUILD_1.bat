
rmdir /s /q Release
rmdir /s /q sign

call "C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\Tools\VsDevCmd.bat"

msbuild.exe NiceHashMiner.sln -p:Configuration=Release -p:Platform=x64 -t:Rebuild

pause

mkdir sign
del /Q sign\*

del /S Release\*.pdb 

cd Release

..\fd . -u -t f --extension exe --extension dll -E OpenCL -E common -E BouncyCastle* -E Hardcodet* -E ManagedNvml* -E MegaApiClient* -E Microsoft* -E MyDownloader* -E Newtonsoft* -E System* -E SharpCompress* -E device_* -E log4net* -E websocket* -E zxing* --exec xcopy /f /s /i /y {} ..\sign\{//}\

pause
