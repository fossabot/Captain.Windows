﻿using System.Reflection;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyProduct("Captain")]
[assembly: AssemblyDescription("The minimal, extensible screen capturer.")]
[assembly: AssemblyCopyright("© 2017 sanlyx")]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("7495d12f-ca61-4548-8c8d-59d8feca51f0")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
[assembly: AssemblyVersion("0.6.0.0")]
[assembly: AssemblyFileVersion("0.6.0.0")]

[assembly: AssemblyMetadata("prerelease", "beta")]

#if DEBUG
[assembly: AssemblyMetadata("debug", "")]
[assembly: AssemblyTitle("Captain")]
[assembly: AssemblyCompany("sanlyx")]
[assembly: AssemblyTrademark("Captain")]
[assembly: ComVisible(false)]

#endif