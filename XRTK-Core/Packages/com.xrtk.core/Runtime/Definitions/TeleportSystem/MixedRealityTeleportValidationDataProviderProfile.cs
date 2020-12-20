﻿// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using XRTK.Definitions.Utilities;
using XRTK.Services.Teleportation;

namespace XRTK.Definitions.TeleportSystem
{
    /// <summary>
    /// Configuration profile for the <see cref="MixedRealityTeleportValidationDataProvider"/>.
    /// </summary>
    [CreateAssetMenu(menuName = "Mixed Reality Toolkit/Teleport System/Teleport Validation Data Provider Profile", fileName = "MixedRealityTeleportValidationDataProviderProfile", order = (int)CreateProfileMenuItemIndices.Input)]
    public class MixedRealityTeleportValidationDataProviderProfile : BaseMixedRealityProfile
    {
        [SerializeField]
        [Tooltip("Layers that are considered 'valid' for teleportation.")]
        private LayerMask validLayers = UnityEngine.Physics.DefaultRaycastLayers;

        /// <summary>
        /// Layers that are considered 'valid' for teleportation.
        /// </summary>
        public LayerMask ValidLayers
        {
            get => validLayers;
            internal set => validLayers = value;
        }

        [SerializeField]
        [Tooltip("Layers that are considered 'invalid' for teleportation.")]
        private LayerMask invalidLayers = UnityEngine.Physics.IgnoreRaycastLayer;

        /// <summary>
        /// Layers that are considered 'invalid' for teleportation.
        /// </summary>
        public LayerMask InvalidLayers
        {
            get => invalidLayers;
            internal set => invalidLayers = value;
        }

        [SerializeField]
        [Range(0f, 1f)]
        [Tooltip("The up direction threshold to use when determining if a surface is 'flat' enough to teleport to.")]
        private float upDirectionThreshold = 0.2f;

        /// <summary>
        /// The up direction threshold to use when determining if a surface is 'flat' enough to teleport to.
        /// </summary>
        public float UpDirectionThreshold
        {
            get => upDirectionThreshold;
            internal set => upDirectionThreshold = value;
        }

        [SerializeField]
        [Min(.1f)]
        [Tooltip("The maximum distance from the player a teleport location can be away.")]
        private float maxDistance = 10f;

        /// <summary>
        /// The maximum distance from the player a teleport location can be away.
        /// </summary>
        public float MaxDistance
        {
            get => maxDistance;
            internal set => maxDistance = value;
        }
    }
}
