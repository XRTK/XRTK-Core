// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace XRTK.Editor.Utilities
{
    internal class AssemblyDefinitionPreProcessor : AssetPostprocessor
    {
        [Serializable]
        private class PackageInfo
        {
            [SerializeField]
            private string name;

            public string Name => name;

            [SerializeField]
            private string version;

            public string Version => version;
        }

        private const string VersionRegexPattern = "\\[assembly: AssemblyVersion\\(\"(.*)\"\\)\\]";

        private void OnPreprocessAsset()
        {
            if (assetPath.Contains("package.json") && !Application.isBatchMode)
            {
                if (Path.GetFullPath(assetPath).Contains("PackageCache"))
                {
                    return;
                }

                var text = File.ReadAllText(assetPath);
                var packageJson = JsonUtility.FromJson<PackageInfo>(text);

                if (!packageJson.Name.Contains("com.xrtk"))
                {
                    return;
                }

                var newVersion = $"[assembly: AssemblyVersion(\"{packageJson.Version}\")]";
                var asmdefs = Directory.GetFiles(assetPath.Replace("package.json", string.Empty), "*.asmdef", SearchOption.AllDirectories);

                foreach (var assembly in asmdefs)
                {
                    var assemblyName = Path.GetFileNameWithoutExtension(assembly).ToLower();
                    var directory = Path.GetDirectoryName(assembly);
                    var assemblyInfoPath = $"{directory}/AssemblyInfo.cs";
                    var fileText = !File.Exists(assemblyInfoPath)
                        ? $@"// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Reflection;

[assembly: AssemblyVersion(""0.0.0"")]
[assembly: AssemblyTitle(""com.{assemblyName}"")]
[assembly: AssemblyCompany(""XRTK"")]
[assembly: AssemblyCopyright(""Copyright (c) XRTK. All rights reserved."")]
"
                        : File.ReadAllText(assemblyInfoPath);

                    if (!fileText.Contains("AssemblyVersion"))
                    {
                        fileText += "\nusing System.Reflection;\n\n[assembly: AssemblyVersion(\"0.0.0\")]\n";
                    }

                    if (!fileText.Contains("AssemblyTitle"))
                    {
                        fileText += $"[assembly: AssemblyTitle(\"com.{assemblyName}\")]\n";
                    }

                    File.WriteAllText(assemblyInfoPath, Regex.Replace(fileText, VersionRegexPattern, newVersion));
                }

                AssetDatabase.Refresh(ImportAssetOptions.Default);
            }
        }
    }
}
