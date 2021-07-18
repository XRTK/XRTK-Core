// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using XRTK.Interfaces;

namespace XRTK.Editor.BuildPipeline
{
    /// <summary>
    /// The Build Info defines common properties for a build.
    /// </summary>
    public interface IBuildInfo
    {
        /// <summary>
        /// Should the build auto increment the build version number?
        /// </summary>
        /// <remarks>
        /// If <see cref="Version"/> is assigned then this flag is ignored.
        /// </remarks>
        bool AutoIncrement { get; set; }

        /// <summary>
        /// The build Bundle Identifier (i.e. 'com.xrtk.core')
        /// </summary>
        string BundleIdentifier { get; set; }

        /// <summary>
        /// The build version number
        /// </summary>
        /// <remarks>
        /// If set will override <see cref="AutoIncrement"/>
        /// </remarks>
        Version Version { get; set; }

        /// <summary>
        /// The version code (usually a single integer for platforms like iOS, Android, and Magic Leap)
        /// </summary>
        int? VersionCode { get; set; }

        /// <summary>
        /// Is this build being issued from the command line?
        /// </summary>
        bool IsCommandLine { get; }

        /// <summary>
        /// The directory to put the final build output relative to the project root.
        /// </summary>
        /// <remarks>
        /// Defaults to "<see cref="Application.dataPath"/>/Builds/Platform Target/"
        /// </remarks>
        string OutputDirectory { get; set; }

        /// <summary>
        /// The directory containing the full path to the <see cref="OutputDirectory"/>.
        /// </summary>
        string AbsoluteOutputDirectory { get; }

        /// <summary>
        /// The full output path for the final build.
        /// </summary>
        /// <remarks>May include <see cref="ExecutableFileExtension"/>.</remarks>
        string FullOutputPath { get; }

        /// <summary>
        /// The executable file extension for this build.
        /// </summary>
        /// <remarks>If a directory is expected then keep as default.</remarks>
        string ExecutableFileExtension { get; }

        /// <summary>
        /// The list of scenes to include in the build.
        /// </summary>
        IEnumerable<EditorBuildSettingsScene> Scenes { get; set; }

        /// <summary>
        /// Build options to include in the Unity player build pipeline.
        /// </summary>
        BuildOptions BuildOptions { get; set; }

        /// <summary>
        /// The build target.
        /// </summary>
        BuildTarget BuildTarget { get; }

        /// <summary>
        /// The <see cref="IMixedRealityPlatform"/> to build to.
        /// </summary>
        IMixedRealityPlatform BuildPlatform { get; }

        /// <summary>
        /// Optional parameter to set the player's <see cref="ColorSpace"/>
        /// </summary>
        ColorSpace? ColorSpace { get; set; }

        /// <summary>
        /// The symbols associated with this build.
        /// </summary>
        string BuildSymbols { get; set; }

        /// <summary>
        /// The build configuration (i.e. debug, release, or master)
        /// </summary>
        string Configuration { get; set; }

        /// <summary>
        /// Parses command line args via <see cref="Environment.GetCommandLineArgs"/>
        /// </summary>
        /// <remarks>
        /// Supported arguments:<para/>
        /// -autoIncrement<para/>
        /// -versionName "0.1.0"<para/>
        /// -versionCode "1"<para/>
        /// -bundleIdentifier "com.xrtk.core"<para/>
        /// -sceneList (CSV format)<para/>
        /// -sceneListFile (CSV format)<para/>
        /// -buildOutputDirectory <para/>
        /// -colorSpace (see <see cref="UnityEngine.ColorSpace"/>)<para/>
        /// -buildArchitecture (x86, x64, ARM, ARM64)<para/>
        /// -buildConfiguration (debug, master, release)<para/>
        /// </remarks>
        void ParseCommandLineArgs();

        /// <summary>
        /// Should the executable be installed on <see cref="OnPostProcessBuild"/>?
        /// </summary>
        bool Install { get; set; }

        /// <summary>
        ///   <para>Implement this function to receive a callback before the build is started.</para>
        /// </summary>
        /// <param name="report">A report containing information about the build, such as its target platform and output path.</param>
        void OnPreProcessBuild(BuildReport report);

        /// <summary>
        ///   <para>Implement this function to receive a callback after the build is complete.</para>
        /// </summary>
        /// <param name="report">A BuildReport containing information about the build, such as the target platform and output path.</param>
        void OnPostProcessBuild(BuildReport report);
    }
}
