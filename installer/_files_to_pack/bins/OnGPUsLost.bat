:: This script contains two option how you can "repair" lost GPU
:: Default option (option 1) will restart your entire rig, you can also uncomment "NirCMD" to make a screenshot befor reboot (you need to place NirCMD in NHML directory under "NirCMD\nircmd.exe")
:: Second option will use nvidiaInspector to restart display drivers, you need to place "NvidiaInspector" in NHML directory under "NV_Inspector\nvidiaInspector.exe", this will set GPU profile (OC etc.) to default so it will also select MSI Afterburner profile
:: Option 1 is used by default, if you whant to use Option 2 you need to change line "set OPTION=1" to "set OPTION=2"
:: Remember that Option 2 requires "nvidiaInspector" and MSIAfterburner, you can tune Option 2 to your needs (look for comments for more clues) 
@echo off
::Select desire option here
set OPTION=1 
IF %OPTION%==2 GOTO OPT2

:OPT1
:: Option 1: restart RIG
echo %DATE% %TIME% >> logs\GPU_Lost.txt
::NirCMD\nircmd.exe savescreenshot "logs\GPU_Lost_%date:/=-%-%time::=-%.jpg"
::timeout 4
shutdown -r -f -t 0
exit

:OPT2
:: Option 2: close NHML, restart display drivers, start NHML, nvidiaInspector is required in NHML directory
echo %DATE% %TIME% Lost GPU >> logs\GPU_Lost.txt
::NirCMD\nircmd.exe savescreenshot "logs\GPU_Lost_%date:/=-%-%time::=-%.jpg"
::timeout 2
taskkill /IM NiceHashMiner.exe
timeout 3
NV_Inspector\nvidiaInspector.exe -restartDisplayDriver
:: It is possible that this timeout should be increased when more GPUs are present
timeout 12
:: Save your custom config for all GPUs under same profile number, if you use default config for your GPUs this step is not required
"c:\Program Files (x86)\MSI Afterburner\MSIAfterburner.exe" -Profile2
echo %DATE% %TIME% Profile 2 set >> logs\GPU_Lost.txt
timeout 4
start NiceHashMiner.exe
