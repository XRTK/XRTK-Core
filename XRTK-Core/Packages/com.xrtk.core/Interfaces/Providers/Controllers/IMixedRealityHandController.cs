﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using XRTK.Definitions.Utilities;

namespace XRTK.Interfaces.Providers.Controllers
{
    /// <summary>
    /// Hand definition, used to provide access to hand joints and other data.
    /// </summary>
    public interface IMixedRealityHandController : IMixedRealityController
    {
        /// <summary>
        /// Get the current pose of a hand joint.
        /// </summary>
        /// <remarks>
        /// Hand bones should be oriented along the Z-axis, with the Y-axis indicating the "up" direction,
        /// i.e. joints rotate primarily around the X-axis.
        /// </remarks>
        bool TryGetJoint(TrackedHandJoint joint, out MixedRealityPose pose);

        [Obsolete("Marked for review. Will have to reconsider this implementation once other issues are solved.")]
        bool IsInPointingPose { get; }
    }
}