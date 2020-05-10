﻿// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using UnityEngine;
using XRTK.Definitions.Utilities;
using XRTK.Providers.Controllers.Hands;

namespace XRTK.Definitions.Controllers.Hands
{
    /// <summary>
    /// Provides additional configuration options for hand data providers.
    /// </summary>
    public abstract class BaseHandControllerDataProviderProfile : BaseMixedRealityControllerDataProviderProfile
    {
        [SerializeField]
        [Tooltip("Defines what kind of data should be aggregated for the hands rendering.")]
        private HandRenderingMode renderingMode = HandRenderingMode.Joints;

        /// <summary>
        /// Defines what kind of data should be aggregated for the hands rendering.
        /// </summary>
        public HandRenderingMode RenderingMode => renderingMode;

        [SerializeField]
        [Tooltip("If set, hands will be setup with colliders and a rigidbody to work with Unity's physics system.")]
        private bool handPhysicsEnabled = false;

        /// <summary>
        /// If set, hands will be setup with colliders and a rigidbody to work with Unity's physics system.
        /// </summary>
        public bool HandPhysicsEnabled => handPhysicsEnabled;

        [SerializeField]
        [Tooltip("If set, hand colliders will be setup as triggers.")]
        private bool useTriggers = false;

        /// <summary>
        /// If set, hand colliders will be setup as triggers.
        /// </summary>
        public bool UseTriggers => useTriggers;

        [SerializeField]
        [Tooltip("Set the bounds mode to use for calculating hand bounds.")]
        private HandBoundsMode boundsMode = HandBoundsMode.Hand;

        /// <summary>
        /// Set the bounds mode to use for calculating hand bounds.
        /// </summary>
        public HandBoundsMode BoundsMode => boundsMode;

        [SerializeField]
        [Tooltip("Tracked hand poses for pose detection.")]
        private List<HandControllerPoseDefinition> trackedPoses = new List<HandControllerPoseDefinition>();

        /// <summary>
        /// Tracked hand poses for pose detection.
        /// </summary>
        public IReadOnlyList<HandControllerPoseDefinition> TrackedPoses => trackedPoses;

        public override ControllerDefinition[] GetDefaultControllerOptions()
        {
            return new[]
            {
                new ControllerDefinition(typeof(MixedRealityHandController), Handedness.Left),
                new ControllerDefinition(typeof(MixedRealityHandController), Handedness.Right),
            };
        }
    }
}