// Copyright (c) XRTK. All rights reserved.
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
        [Range(.5f, 1f)]
        [Tooltip("Threshold in range [0.5, 1] that defines when a hand is considered to be grabing.")]
        private float gripThreshold = .8f;

        /// <summary>
        /// Threshold in range [0, 1] that defines when a hand is considered to be grabbing.
        /// </summary>
        public float GripThreshold => gripThreshold;

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
        private HandBoundsLOD boundsMode = HandBoundsLOD.Low;

        /// <summary>
        /// Set the bounds mode to use for calculating hand bounds.
        /// </summary>
        public HandBoundsLOD BoundsMode => boundsMode;

        [SerializeField]
        [Tooltip("Tracked hand poses for pose detection.")]
        private HandControllerPoseProfile[] trackedPoses = new HandControllerPoseProfile[0];

        /// <summary>
        /// Tracked hand poses for pose detection.
        /// </summary>
        public IReadOnlyList<HandControllerPoseProfile> TrackedPoses => trackedPoses;

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
