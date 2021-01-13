# NiceHash Miner

- [Introduction](#introduction)
- [What are the benefits?](#benefits)
- [Features](#features)
- [Requirements](#requirements)
- [How to get&run it?](#run)
- [Where is the profit coming from?](#profit)
- [How to run NiceHash Miner only when profitability is high enough?](#highprofit)
- [Additional options](#options)
- [Troubleshooting](#troubleshooting)
- [How to report bugs and issues?](#bugs)

# <a name="introduction"></a> Introduction

NiceHash Miner is a continuation of NiceHash Miner Legacy. This version is intended for beginners or experienced miners that want the quickest updates and highest hashrates, and are okay with customization of the antivirus. Please check out quick [guide](https://storage.googleapis.com/public-prod-data/marketing%2FNHM%20User%20Guide2.pdf) about NiceHash Miner, so that your experience will be even better.

<img src="Resources/NHM_screenshot.PNG" />

Please follow us on Twitter <a href="https://twitter.com/NiceHashMining" target="_blank">@NiceHashMining</a> or on our <a href="https://www.nicehash.com/blog/" target="_blank">Blog</a> for updates on new versions and other important information.

# <a name="benefits"></a> What are the benefits?

NiceHash Miner is an advanced auto-miner that supports the latest algorithms and miners. No need to go through tons of configuration files, various mining software versions, configuration tuning or cryptocurrency coins market analysis. **Auto-tuning for best performance and efficiency**, automatic selection and runtime **automatic switching to most profitable cryptocurrency algorithm** are all integrated into NiceHash Miner and will enable you seamless, joyful and **profitable mining experience**.

# <a name="features"></a> Features

- Easy one-click CPU mining for CPUs that support at least AES (only works on Windows x64).
- Easy one-click GPU mining for NVIDIA GPUs using microarchitecture (compute capability) SM 3.0+.
- Easy one-click GPU mining for AMD GPUs using any AMD GPU devices that supports OpenCL.
- Integrated support for Simple Multi-Algorithm. Always mine most profitable algorithm.
- Integrated benchmarking tool. Run it only once before you start mining and after every hardware/driver/software upgrade.
- Watch-feature - automatically restart miner if crashed or hanged.
- Display current rate and your balance in real time.
- Auto update notifications.
- Much more...

# <a name="requirements"></a> Requirements

- **Windows** 10 or newer operating system **64-bit**
- **[Note: Windows 10 is recommended and will provide you a much better user experience]**
- For CPU mining a modern CPU with AES support
- For AMD mining any AMD GPU with OpenCL support
- For NVIDIA mining any NVIDIA GPU with Compute capability (SM) 3.0 or newer
- **up-to-date patches** for OS
- **up-to-date drivers** for all GPUs
- **Reliable** internet connectivity
- For GPU Mining, **paging file size** of 60% of your total GPU VRAM memory
- Personal **Bitcoin wallet** (you can create one by [registering](https://www.nicehash.com/support/general-help/account/creating-a-new-account) on NiceHash page)

# <a name="run"></a> How to get&run it?

All you have to do is download zip package or installer exe from the [releases page](https://github.com/nicehash/NiceHashMiner/releases).
If you choose installer just run it and follow the instructions. In case of zip package extract it and run the miner.
After that enter your Bitcoin wallet address where you want to get your coins sent at - and you are ready to start mining and maximizing your profit.

<i>**Note**: Windows 10 with .NET Framework 4.8 or higher and Microsoft Visual C++ Redistributable 2015 are required. However, if you encounter any issues when starting application (application would fail to start or errors/warnings about missing DLL files are displayed) you should download and install <a href="https://dotnet.microsoft.com/download/dotnet-framework/thank-you/net48-web-installer" target="_blank">Microsoft **.NET Framework 4.8**</a> and <a href="https://www.microsoft.com/en-us/download/details.aspx?id=48145" target="_blank">Microsoft **Visual C++ Redistributable 2015 (vcredist_x64.exe)**</a> (after installation a reboot might be required).</i>

Detailed instructions:
- Download binaries from here: https://github.com/nicehash/NiceHashMiner/releases
1. Installer
    - Run installer file (nhm_windows_3.x.y.z.exe)
    - Follow the instructions
2. Zip archive
    - Extract zip archive
    - Run NiceHashMiner.exe
- Make sure you select your own personal Bitcoin wallet to receive payments, see **Bitcoin wallet guidelines and instructions** here: https://www.nicehash.com/support/general-help/wallet/how-to-use-nicehash-wallet.
- You will receive Bitcoin payments according to our payments schedule: https://www.nicehash.com/support/mining-help/earnings-and-payments/when-and-how-do-you-get-paid

# <a name="profit"></a> Where is the profit coming from?

As a back-end NiceHash Miner relies on the <a href="https://www.nicehash.com" target="_blank">NiceHash.com</a> service. By running NiceHash Miner you're essentially selling the hashing power of your CPUs & GPUs to hashing power buyers. Those are using the hashing power to mine various cryptocurrency coins and support decentralized blockchain networks - similar to cloud computing - only that by running NiceHash Miner you're actually being a provider for the cryptocurrency mining hashing power. You are being part of a global compute power network, **empowering decentralized digital currencies**.

# <a name="highprofit"></a> How to run NiceHash Miner only when profitability is high enough?
Profitability of mining can go up and down that may be unprofitable to mine especially places with high electricity cost. By using the "MinimumProfit" settings, NiceHashMiner will stop mining if the current profits are below the minimum amount (in USD). This will help you mine during "profitable" times only.

# <a name="options"></a> Additional options

Go to 'Settings' tab. Alternatively, you can manually modify \configs\General.json for general settings and \configs\device_settings_XXX.json (XXX is your device UUID) files for device benchmark settings (close NiceHash Miner first).

## General settings
Parameter | Range | Description
-----------------|----------|-------------------
ConfigFileVersion | Version | This is to identify which version of NiceHashMiner did the config file is made from.
BitcoinAddress | valid BTC address | The address that NiceHashMiner will mine to.
WorkerName | text | To identify the computer on NiceHash web UI.
RigGroup | text | To identify the group your rig belongs to.
ServiceLocation | number | Used to select the location of the mining server.
AutoStartMining | true or false | When set to true, NiceHashMiner will start mining on program start.
HideMiningWindows | true or false | When set to true, miner console windows will be hidden (not working for all miners).
MinimizeMiningWindows | true or false | When set to true, miner console windows will be minimized to tray (not working for all miners).
MinerAPIQueryInterval | number | Amount of time between each API call to get the latest stats from miner.
MinerRestartDelayMS | number | Amount of time to delay before trying to restart the miner.
ApiBindPortPoolStart | number | Set the starting value (default is 5100) for miners API ports. When a new miner is created it will use an avaliable API port starting from the ApiBindPortPoolStart and higher.
NVIDIAP0State | true or false | When set to true, NiceHashMiner would change all supported NVidia GPUs to P0 state. This will increase some performance on certain algorithms.
StartMiningWhenIdle | true or false | Automatically start mining when computer is idle and stop mining when computer is being used.
IdleCheckType | 0 or 1 | If 0 idle state will be set by InputTimeout; if set to 1 idle state will be set by SessionLock
MinIdleSeconds | number | When StartMiningWhenIdle is set to true and IdleCheckType is set to 0, MinIdleSeconds tells how many seconds computer has to be idle before mining starts.
IdleWhenNoInternetAccess | true or false | When set to true, idle state will be set to true on internet access loss.
DebugConsole | true or false | When set to true, it displays debug console.
LogToFile | true or false | Set it to true if you would like NiceHashMiner to log to a file.
LogMaxFileSize | number | The maximum size (in bytes) of the log file before roll over.
ShowDriverVersionWarning | true or false | Set to true if you would like to get a warning if less than ideal driver for mining is detected.
DisableWindowsErrorReporting | true or false | Set it to true if you would like to disable windows error reporting. This will allow NiceHashMiner to restart the miner in the case of the miner crashes.
ShowInternetConnectionWarning | true or false | Set to true if you would like to get a warning if the internet connection is not available.
MinimumProfit | number | If set to any value, NiceHashMiner will stop mining if the calculated profit falls below the set amount.
MineRegardlessOfProfit | true or false | When set to true, NiceHashMiner will mine regardless of your mining profitability.
UseIFTTT | true or false | When set to true, NiceHashMiner will use the API Key you provide to notify you when profitability has gone below the profitability you have configured.
IFTTTKey | text | IFTTT key for notifications.
Use3rdPartyMinersTOS | 0 or 4 | If set to 4, you agree with the 3rd Party Miners TOS
Hwid | text | Your hardware ID. Better left intact
AgreedWithTOS | 0 or 4 | If set to 4, you agree with the NiceHash Miner TOS
SwitchSmaTimeChangeSeconds | "lower": number, "upper": number | Set range for SMA check.
SwitchSmaTicksStable | "lower": number, "upper": number | Set range for stable algorithms ticks needed for switch.
SwitchSmaTicksUnstable | "lower": number, "upper": number | Set range for unstable algorithms ticks needed for switch.
KwhPrice | number | Set price for kWh, used for profit calculating.
SwitchProfitabilityThreshold | number | Threshold for algorithm switch. Lower this number is, faster switches will happen.
DisplayCurrency | valid 3 letter code | Converts to selected currency via http://fixer.io valid options are any supported via fixer.
TimeUnit | 0, 1, 2, 3 or 4 | Set 0 for Hour, 1 for Day, 2 for Week, 3 for Month and 4 for Year.
AutoScaleBTCValues | true or false | When set to true, BTC values will be scaled to mBTC if needed for better overview.
Language | valid 2 letter code | Language selection for NiceHashMiner GUI.
MinimizeToTray | true or false | When set to true, NiceHashMiner will minimize to the system tray.
DisplayPureProfit | true or false | When set to true, net profit will be shown instead of gross.
DisplayTheme | Light or Dark | Set GUI theme for NiceHash Miner.
ShowPowerColumns | true or false | When set to true, additional power usage column will be shown in device overview.
GUIWindowsAlwaysOnTop | true or false | NiceHash Miner window will always remain on top of other windows.
MainFormSize | "X, Y" | Set X and Y sizes for the NiceHash Miner window.
AllowMultipleInstances | true or false | When set to true, multiple NiceHash Miner instances will be allowed to run at the same time.
UseSmaCache | true or false | When set to true, received SMA values will be saved to file.
UseEthlargement | true or false | When set to true, EthlargementPill will be used when possible during mining.
ShowNotifications | {"NotificationID": true|false} | When set to false, specified notification won't be shown anymore in the future.
DisableVisualCRedistributableCheck | true or false | When set to true, NiceHash Miner won't try to download VC Redist file. 
RunScriptOnCUDA_GPU_Lost | true or false | When set to true, script will be run on detected GPU dead.
DisableDeviceStatusMonitoring | true or false | When set to true, device status monitoring (temp, fan speed, etc.) will be disabled.
DisableDevicePowerModeSettings | true or false | When set to true, NiceHash Miner won't be able to change your power mode settings.
ShowGPUPCIeBusIDs | true or false | When set to true, PCIe Bus IDs will be shown in the device overview.
AutoUpdateNiceHashMiner | true or false | When set to true, NiceHash Miner will be updated automatically on the new release.
AutoUpdateMinerPlugins | true or false | When set to true, miner plugins will be automatically updated on the new releases.

## Benchmark settings (per device)
Parameter | Range | Description
-----------------|----------|-------------------
DeviceUUID | text | Used for unique identification purposes in the config file (**DO NOT EDIT**)
DeviceName | text | Used for identification purposes in the config file (**DO NOT EDIT**)
Enabled | true or false | When set to false, device won't mine.
MinimumProfit | number | Minimum profit for device to mine.
TDPSettings | settings for tdp | Set tdp settings for specific device.
PluginAlgorithmSettings\Name | text | Name of a plugin (**DO NOT EDIT**)
PluginAlgorithmSettings\PluginUUID | text | UUID of a plugin (**DO NOT EDIT**)
PluginAlgorithmSettings\PluginVersion | number | Version of a plugin (**DO NOT EDIT**)
PluginAlgorithmSettings\AlgorithmIDs | text | Ids of plugin algorithm (**DO NOT EDIT**)
PluginAlgorithmSettings\Speeds | array[number] | Fine tune algorithm ratios by manually setting benchmark speeds for each algorithmID.
PluginAlgorithmSettings\ExtraLaunchParameters | text | Additional launch parameters when launching miner and this algorithm.
PluginAlgorithmSettings\Enabled | true or false | Set to false if you would like to skip & disable a particular algorithm. Benchmarking as well as actual mining will be disabled for this particular algorithm. That said, auto-switching will skip this algorithm when mining will be running.
PluginAlgorithmSettings\PowerUsage | number | Power usage recorded during benchmarking of this algorithm.

Examples:
--------
```
{
    "Name": "TRex",
    "PluginUUID": "03f80500-94ec-11ea-a64d-17be303ea466",
    "PluginVersion": "11.0",
    "AlgorithmIDs": "KAWPOW",
    "Speeds": [
      8841376.5
    ],
    "ExtraLaunchParameters": "--intensity 20",
    "Enabled": true,
    "PowerUsage": 113.81357777777778
}
```
# <a name="troubleshooting"></a> Troubleshooting

My NVIDIA video card(s) is/are not detected.
> Make sure to install latest official NVIDIA drivers from here: 
http://www.nvidia.com/Download/index.aspx
> Also check whether your card has Compute capability (version) 3.0+, check here: https://en.wikipedia.org/wiki/CUDA#GPUs_supported

My AMD video card(s) is/are not detected.
> Make sure to install latest official AMD drivers from here:
http://support.amd.com/en-us/download
> Also check whether your card supports OpenCL, check "OpenCL" column here: https://en.wikipedia.org/wiki/List_of_AMD_graphics_processing_units

When uninstalling or reinstalling display drivers we suggest you to use Guru 3D Display Driver Uninstaller. This utility will make sure you get a clean driver state. You can <a href="http://www.guru3d.com/files-details/display-driver-uninstaller-download.html" target="_blank">download it here</a>.
If you are using Windows 10, the AMD installer utility is sometimes unable to install drivers, therefore you have to install them manually. Download and run diver installer, let it extract the archive and then cancel it when prompted to execute actual install. After this go to Control Panel / System / Device Manager, Right click on your GPU, choose to Update driver manually and point to c:\AMD\[driver version] folder to finish the manual driver installation.

Google Chrome is blocking the download of the application
> Google Chrome falsely flags NiceHash Miner as a potentially unwanted program and thus prevents you from saving the program archive on your computer. This is an issue with Google Chrome and not with NiceHash Miner. Use another browser such as Firefox or Edge to download the file instead. Also, read 'My anti-virus is blocking the application' below.

My benchmarking results are not accurate
> Any kind of automation can only be done up to a particular level. We've spent significant effort to make benchmarking as good as possible, but it can't be made ideal. If you still see a deviation of actual mining speed from the one, calculated from benchmark, then you should manually enter these observed speed numbers from actual mining into config.json file or set them via the "Benchmark" tab.

Benchmarks on particular algorithms keep getting terminated on AMD GPUs
> In some particular combinations of Windows version + AMD Driver version benchmarks on some algorithms keep getting terminated. If the particular algorithm that is being terminated is shown on pause on the front page of NiceHash.com (No orders - mining unavailable), then this is normal expected behaviour. However, if benchmark is also terminated for active algorithms, then you have to try manual benchmarking.

# <a name="bugs"></a> How to report bugs and issues?

To report bugs and issues please use the GitHub issue reporting tool: https://github.com/nicehash/NiceHashMiner/issues. Any bugs and issues reports are very much appreciated since it helps us to improve NiceHash Miner. Thank you.
