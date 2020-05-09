﻿// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using XRTK.Definitions.Controllers.Hands;
using XRTK.Definitions.Controllers.Simulation.Hands;

namespace XRTK.Interfaces.Providers.Controllers.Hands
{
    public interface IMixedRealityHandControllerDataProvider : IMixedRealityControllerDataProvider
    {
        /// <summary>
        /// Should hand mesh data be read if the platform provides it?
        /// </summary>
        bool HandMeshingEnabled { get; }

        /// <summary>
        /// Are hand physics enabled?
        /// </summary>
        bool HandPhysicsEnabled { get; }

        /// <summary>
        /// Shall hand colliders be triggers?
        /// </summary>
        bool UseTriggers { get; }

        /// <summary>
        /// Gets the configured hand bounds mode to be used with hand physics.
        /// </summary>
        HandBoundsMode BoundsMode { get; }

        /// <summary>
        /// Gets the list of poses tracked by the data provider.
        /// </summary>
        IReadOnlyList<HandControllerPoseDefinition> TrackedPoses { get; }
    }
}