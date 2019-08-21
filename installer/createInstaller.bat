if exist .\nhmpacker\_files_to_pack (
    echo "Folder already exists, deleting."
    rmdir /s /q .\nhmpacker\_files_to_pack
) else (
    mkdir .\nhmpacker\_files_to_pack
)

mkdir .\nhmpacker\_files_to_pack\assets
xcopy /s /i nsis_template .\nhmpacker\_files_to_pack\nsis
xcopy /s /i .\..\Release .\nhmpacker\_files_to_pack\bins
copy .\nhm_windows_x.y.z.r-template\EULA.* .\nhmpacker\_files_to_pack\assets

.\..\src\Tools\InstallerHelper\bin\Release\InstallerHelper.exe %CD%\nhmpacker\_files_to_pack\bins\NiceHashMiner.exe
copy .\..\src\Tools\InstallerHelper\bin\Release\_files_to_pack\version.txt .\nhmpacker\_files_to_pack\version.txt
copy .\..\src\Tools\InstallerHelper\bin\Release\_files_to_pack\packageDefsGenerated.nsh .\nhmpacker\_files_to_pack\nsis\include_common\packageDefsGenerated.nsh

.\nhmpacker\nhmpacker.exe

for %%i in (.\nhmpacker\nhm_windows_1.*.exe) do (
    set "filename=%%~ni"  
)  

xcopy /s /i .\..\Release %filename%
copy .\nhm_windows_x.y.z.r-template\EULA.html %filename%
copy .\nhm_windows_x.y.z.r-template\EULA.rtf %filename% 
7z a -t7z %filename%.7z .\%filename%\*

copy .\nhmpacker\nhm_*.exe .\

rmdir /s /q .\nhmpacker\_files_to_pack
rmdir /s /q %filename%
del .\nhmpacker\nhm_*.exe

if exist %filename%_release_files (
    echo "Folder already exists, deleting."
    rmdir /s /q %filename%_release_files
)
mkdir %filename%_release_files
copy nhm_windows_*.* .\%filename%_release_files\
del nhm_windows_*.exe
del nhm_windows_*.7z