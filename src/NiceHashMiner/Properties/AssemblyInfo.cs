using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
#if TESTNET || TESTNETDEV || PRODUCTION_NEW // NEW PRODUCTION
[assembly: AssemblyTitle("NiceHashMiner")]
[assembly: AssemblyDescription("NiceHashMiner is simple to use mining tool")]
[assembly: AssemblyProduct("NiceHashMiner")]
#else  // OLD PRODUCTION
[assembly: AssemblyTitle("NiceHashMinerLegacy")]
[assembly: AssemblyDescription("NiceHashMinerLegacy is simple to use mining tool")]
[assembly: AssemblyProduct("NiceHashMinerLegacy")]
#endif
[assembly: AssemblyCompany("H-BIT, d.o.o")]
[assembly: AssemblyCopyright("H-BIT, d.o.o ©  2019")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

[assembly: InternalsVisibleTo("NiceHashMiner.Tests")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("14c0b1c1-14f7-4302-8253-a7c8c46c02f4")]

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

#if TESTNET || TESTNETDEV || PRODUCTION_NEW // NEW PRODUCTION
[assembly: AssemblyVersion("1.9.2.8")]
[assembly: AssemblyFileVersion("1.9.2.8")]
#else  // OLD PRODUCTION 
[assembly: AssemblyVersion("1.9.1.8")]
[assembly: AssemblyFileVersion("1.9.1.8")]
#endif
