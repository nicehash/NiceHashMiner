using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.

#if TESTNET
[assembly: AssemblyTitle("NiceHash Miner TESTNET")]
[assembly: AssemblyDescription("NiceHash Miner is simple to use mining tool")]
[assembly: AssemblyProduct("NiceHash Miner TESTNET")]
#elif TESTNETDEV
[assembly: AssemblyTitle("NiceHash Miner TESTNETDEV")]
[assembly: AssemblyDescription("NiceHash Miner is simple to use mining tool")]
[assembly: AssemblyProduct("NiceHash Miner TESTNETDEV")]
#else
[assembly: AssemblyTitle("NiceHash Miner")]
[assembly: AssemblyDescription("NiceHash Miner is simple to use mining tool")]
[assembly: AssemblyProduct("NiceHash Miner")]
#endif

[assembly: AssemblyCompany("H-BIT, d.o.o.")]
[assembly: AssemblyCopyright("H-BIT, d.o.o. ©  2021")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("14c0b1c1-14f7-4302-8253-a7c8c46c02f4")]
//In order to begin building localizable applications, set
//<UICulture>CultureYouAreCodingWith</UICulture> in your .csproj file
//inside a <PropertyGroup>.  For example, if you are using US english
//in your source files, set the <UICulture> to en-US.  Then uncomment
//the NeutralResourceLanguage attribute below.  Update the "en-US" in
//the line below to match the UICulture setting in the project file.

//[assembly: NeutralResourcesLanguage("en-US", UltimateResourceFallbackLocation.Satellite)]


[assembly: ThemeInfo(
    ResourceDictionaryLocation.None, //where theme specific resource dictionaries are located
                                     //(used if a resource is not found in the page,
                                     // or application resource dictionaries)
    ResourceDictionaryLocation.SourceAssembly //where the generic resource dictionary is located
                                              //(used if a resource is not found in the page,
                                              // app, or any theme specific resource dictionaries)
)]


// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]

[assembly: AssemblyVersion("3.0.6.5")]
[assembly: AssemblyFileVersion("3.0.6.5")]
