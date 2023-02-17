rem echo off

xcopy /s /i /f /y sign\* Release

pause

cd installer

NhmPackager.exe

pause
