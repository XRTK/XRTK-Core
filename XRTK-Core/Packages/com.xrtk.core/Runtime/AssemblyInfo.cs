// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

#define XRTK

using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyVersion("0.3.0")]
[assembly: AssemblyTitle("com.xrtk.core")]
[assembly: AssemblyCompany("XRTK")]
[assembly: AssemblyCopyright("Copyright (c) XRTK. All rights reserved.")]

// Note: these are the names of the assembly definitions themselves, not necessarily the actual namespace the class is in.
[assembly: InternalsVisibleTo("XRTK.Editor")]
[assembly: InternalsVisibleTo("XRTK.Tests")]
[assembly: InternalsVisibleTo("XRTK.WindowsMixedReality")]
[assembly: InternalsVisibleTo("XRTK.WindowsMixedReality.Player")]
[assembly: InternalsVisibleTo("XRTK.Oculus")]
[assembly: InternalsVisibleTo("XRTK.Oculus.Player")]
[assembly: InternalsVisibleTo("XRTK.Lumin")]
[assembly: InternalsVisibleTo("XRTK.Lumin.Player")]
