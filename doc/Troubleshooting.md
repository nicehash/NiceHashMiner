Check the below fixes first, then open an [Issue](https://github.com/NiceHash/NiceHashMinerLegacy/issues) if you still need help (also search for your issue to see if it has been posted before). Provide log files.

* [Missing/corrupt .dll errors](#dllerrors)
  * [OpenCL.dll](#opencl)
  * [msvcp\<version>.dll/msvcr\<version>.dll/vcruntime\<version>](#msvcp)
  * [api-ms-win-crt-runtime-|1-1-0.dll](#crt)
* [Benchmark won't complete](#bench)
* [NiceHashMiner is making Windows laggy](#lag)
* [Freezing/Display driver crashing while mining](#ddcrash)
* [Discrepancy between NiceHashMiner reported hashrates and NiceHash online stats](#profitdisc) 
* [No supported devices](#nosupportdev)
* [NiceHash Miner Legacy or included miners are being flagged and/or removed by anti-virus software](#nicehash-miner-legacy-or-included-miners-are-being-flagged-by-anti-virus)

## <a name="dllerrors"></a> Missing .dll errors

Dll files are used for programs to access external libraries. In general, common dlls can be found either in the same directory as the program that uses it, or in a specific system directory in your Windows install. Both of these will work, however dlls in your Windows install can be run by any program without cluttering their program directory.

With this, there are two ways to fix a missing dll error for common dlls:

* Get a copy of the .dll and put it in the program directory

* Find an installer that can install it to your system

### <a name="opencl"></a> Missing OpenCL.dll

This dll allows programs to access OpenCL, which is generally used for AMD mining. Almost every AMD miner program will need access to it, and NHML uses it during the detection of AMD devices. 

OpenCL.dll should be installed to your system almost always, since it is installed with your drivers. However in some circumstances (incomplete driver install, Windows Update) this may no longer work. 

The first step would be to reinstall your graphics drivers. If that still doesn't work, you can find a copy of OpenCL.dll in the `\bin\ethminer` directory. From there you can copy it to the directory of the program that is giving you the error.

### <a name="msvcp"></a> Missing msvcr\<version>.dll, msvcp\<version>.dll, or vcruntime\<version>.dll

These dlls are necessary for programs written in Visual C++. Many miner programs as well as several auxiliary programs NHML uses are written in VC++ and require these dlls. You will find a copy of each dll needed for NHML and its auxiliary programs included in the NHML directory. However, some miner programs may still need them and you can copy to their directory as needed. You may also choose to install them globally, through the MSVC++ redistributable packages:

* If `<version> = 120` [VC++ 2013](https://www.microsoft.com/en-ca/download/details.aspx?id=40784)

* If `<version> = 140` Run the VC++ 2015 installer in the `bin` folder, `vc_redist.x64.exe`

* If `<version> = 141` [VC++ 2017](https://go.microsoft.com/fwlink/?LinkId=746572)

### <a name="crt"></a> Missing api-ms-win-crt-runtime-|1-1-0.dll

The program that is trying to launch depends on the Universal C Runtime, which is included in Windows 10 and available from Windows Update for older versions. You can try running Windows Update, or manually downloading the package from [Microsoft](https://support.microsoft.com/en-us/help/2999226/update-for-universal-c-runtime-in-windows).

## <a name="bench"></a> Benchmark won't complete

First, try running the benchmark on "Precise". If it still doesn't complete, [manually benchmark](./Advanced/ManualBenchmarking.md) the troublesome algorithm.

## <a name="lag"></a> NiceHash Miner Legacy is making Windows laggy

On a dedicated mining box, if the processor and motherboard support integrated graphics, simply connect the monitor to that instead. Otherwise, you could try decreasing the intensity for the miner that is causing problems, and accepting that it will run at a slightly slower speed. Google the file name of the miner and find a command line option to do with 'intensity'. Add this to the extra launch options for this miner, probably using a smaller number until it works better.
* example 1: For claymore dual miners something like "-ethi 3" will probably help.
* example 2: sgminer's algorithms usually support an "--intensity ##" or "--xintensity ##" setting.

## <a name="ddcrash"></a> Freezing/display driver crashing while mining

Try disabling overclock if you have one. If not, this is usually caused by a conflict of the miner and your drivers, try opening an [Issue](https://github.com/NiceHash/NiceHashMinerLegacy/issues).

## <a name="profitdisc"></a> Discrepancy between NiceHash Miner Legacy reported hashrates and NiceHash online stats

These discrepancies are normal and can be caused by fluctuations in the NiceHash network. The important value to check on the NiceHash online stats is the profit that is averaged over the last hour (in the box with projected monthly/weekly/daily profits). The fluctuations are more or less noise and can go up and down, canceling each other out over time.

If your displayed profit is much lower than expected, make sure you aren't getting rejected shares which can be caused by overclocks/poor network connection.

[Official comment](https://www.reddit.com/r/NiceHash/comments/6in4aw/mining_the_speed_on_the_web_is_not_the_same_as/)

## <a name="nosupportdev"></a> No supported devices

You will get a message stating there are no supported devices found if NHML is not able to access any devices on your system. The first thing to check is whether your devices are supported:

* CPUs: must support 64-bit and [AES instruction set](https://en.wikipedia.org/wiki/AES_instruction_set#Intel_and_AMD_x86_architecture). You can look up your specific CPU to make sure it supports this.

* NVIDIA: Cuda Compute Capability 2.1 or higher. This includes *most* GPUs since the GeForce 600 series (and some older ones).

* AMD: Any GPU with OpenCL support (note older GPUs, especially those with little VRAM, may perform poorly).

Also old drivers may not show up in NHML. Try with the latest drivers to see if detection works.

If you are certain your device is supported and it is connected and properly to your computer:

**Users with a Maxwell 2.0 GPU only, meaning:**

* For desktop: all GTX 900 series cards (note this does not include the GT card)

* For mobile: all GTX 900m series cards 965m and up

These GPUs have a setting in the NVIDIA Control Panel (NVCP) called "Optimize for computer performance". This mode can cause NHML to not be able to detect the GPU. Make sure this value is off in global settings. If you need the setting on for a specific program, you can disable it globally and add a program specific rule to enable it.

If you are still unable to get your device to show up, please open an issue *and attach your log file*.

## NiceHash Miner Legacy or included miners are being flagged by Anti-Virus

Many miner programs are flagged by AV software because they are included in actual viruses. These viruses install the programs on inexperienced peoples' computers, and mine on them unknowingly.

Because NHML downloads many miner files, most of which are flagged, it too gets flagged as a virus for the same reason as above. The AV companies do not have a reliable way to discern if NHML is downloading these files with the user's permission (as is the case), or if it is a virus downloading them to secretly mine on the computer.

As a result of this, users should be prepared for NHML to be flagged and/or removed by their AV software, and know how to set up exceptions. If you do some research around the internet, you will find much information on why most miner software is flagged by AVs. It is something that much of the mining community has learned to live with, since the virus makers that ruin it for the rest of us likely won't stop including common miners in their Trojans.

### What if I do not feel safe with making AV exceptions?

Download the NHML source code and compile it yourself. There are instructions on how to do so [here](./ProjectBuilding/Development.md). This way you know exactly what is running on your computer. Unfortunately you would also have to compile all of the miner programs individually as well, since they are what is being flagged first usually. You would also have to forego 3rd party miners, since these are closed source.