﻿// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using UnityEngine;
using XRTK.Interfaces;

namespace XRTK.Definitions.Platforms
{
    /// <summary>
    /// Used by the XRTK to signal that the feature is only available when the current built target matches the platform target.
    /// </summary>
    public sealed class CurrentBuildTargetPlatform : BasePlatform
    {
        /// <inheritdoc />
        public override bool IsAvailable => Application.isEditor;

        /// <summary>
        /// Checks to see if the current build target is available for the list of provided platform.
        /// </summary>
        /// <param name="platforms"></param>
        /// <returns>True, if any build target is active.</returns>
        public static bool IsBuildTargetActive(List<IMixedRealityPlatform> platforms)
        {
            var isBuildTargetActive = false;
            var isEditorPlatformActive = false;
            var isEditorPlatformPresent = false;

            foreach (var platform in platforms)
            {
                switch (platform)
                {
                    case AllPlatforms _:
                        return true;
                    case CurrentBuildTargetPlatform _:
                        isEditorPlatformPresent = true;
                        isEditorPlatformActive = platform.IsAvailable;
                        break;
                    default:
                        isBuildTargetActive |= platform.IsBuildTargetAvailable;
                        break;
                }
            }

            return !isEditorPlatformPresent || isBuildTargetActive && isEditorPlatformActive;
        }
    }
}