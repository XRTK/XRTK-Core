﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using XRTK.Definitions.Devices;
using XRTK.Definitions.Utilities;

namespace XRTK.Providers.Controllers.OpenVR
{
    [CreateAssetMenu(menuName = "Mixed Reality Toolkit/Mixed Reality Controller Mappings/Vive Wand Controller Mapping Profile", fileName = "ViveWandControllerMappingProfile")]
    public class ViveWandControllerMappingProfile : BaseMixedRealityControllerMappingProfile
    {
        /// <inheritdoc />
        public override SupportedControllerType ControllerType => SupportedControllerType.ViveWand;

        protected override void Awake()
        {
            if (!HasSetupDefaults)
            {
                ControllerMappings = new[]
                {
                    new MixedRealityControllerMapping("Vive Wand Controller Left", typeof(ViveWandController), Handedness.Left),
                    new MixedRealityControllerMapping("Vive Wand Controller Right", typeof(ViveWandController), Handedness.Right),
                };
            }

            base.Awake();
        }
    }
}