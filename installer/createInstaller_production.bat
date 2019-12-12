if exist .\nhmpacker\_files_to_pack (
    echo "Folder already exists, deleting."
    rmdir /s /q .\nhmpacker\_files_to_pack
) else (
    mkdir .\nhmpacker\_files_to_pack
)

copy /y .\build_settings\build_settings_production.json .\..\Release\build_settings.json

mkdir .\nhmpacker\_files_to_pack\assets
xcopy /s /i nsis_template .\nhmpacker\_files_to_pack\nsis
xcopy /s /i .\..\Release .\nhmpacker\_files_to_pack\bins
copy .\nhm_windows_x.y.z.r-template\EULA.* .\nhmpacker\_files_to_pack\assets
copy .\createSha256Sums.bat .\nhmpacker\_files_to_pack\assets

rmdir /s /q .\..\src\Tools\MinerPluginsPacker\bin\Release\miner_plugins
rmdir /s /q .\..\src\Tools\MinerPluginsPacker\bin\Release\plugins_packages
.\..\src\Tools\MinerPluginsPacker\bin\Release\MinerPluginsPacker.exe .\..\src\Miners
xcopy /s /i /y .\..\src\Tools\MinerPluginsPacker\bin\Release\miner_plugins .\nhmpacker\_files_to_pack\assets\miner_plugins
xcopy /s /i /y .\..\src\Tools\MinerPluginsPacker\bin\Release\plugins_packages .\nhmpacker\_files_to_pack\assets\plugins_packages

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
copy .\createSha256Sums.bat %filename%

xcopy /s /i /y .\..\src\Tools\MinerPluginsPacker\bin\Release\miner_plugins .\%filename%\miner_plugins
xcopy /s /i /y .\..\src\Tools\MinerPluginsPacker\bin\Release\plugins_packages .\%filename%\plugins_packages

.\nhmpacker\bins_bundle\7z\7z.exe a -tzip %filename%.zip .\%filename%\*

copy .\nhmpacker\nhm_*.exe .\

rmdir /s /q .\nhmpacker\_files_to_pack
rmdir /s /q %filename%
del .\nhmpacker\nhm_*.exe

if exist .\%filename%_release_files (
    echo "Root folder already exists, deleting."
    rmdir /s /q .\%filename%_release_files
)

if exist .\%filename%_release_files\%filename%_release_files (
    echo "Root folder already exists, deleting."
    rmdir /s /q .\%filename%_release_files\%filename%_release_files
)

mkdir .\%filename%_release_files\%filename%_production_release_files
copy nhm_windows_*.* .\%filename%_release_files\%filename%_production_release_files\
del nhm_windows_*.exe
del nhm_windows_*.7z
del nhm_windows_*.zip

mkdir .\%filename%_release_files\plugins
xcopy /s /i /y .\..\src\Tools\MinerPluginsPacker\bin\Release\miner_plugins .\%filename%_release_files\plugins\miner_plugins
xcopy /s /i /y .\..\src\Tools\MinerPluginsPacker\bin\Release\plugins_packages .\%filename%_release_files\plugins\plugins_packages

copy .\createSha256Sums.bat .\%filename%_release_files\createSha256Sums.bat