echo %DATE% %TIME% >> logs\GPU_Lost.txt
::NirCMD\nircmd.exe savescreenshot "logs\GPU_Lost_%date:/=-%-%time::=-%.jpg"
shutdown -r -f -t 0