﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using XRTK.Definitions.Devices;
using XRTK.Definitions.Utilities;

namespace XRTK.DataProviders.Controllers.OpenVR
{
    [CreateAssetMenu(menuName = "Mixed Reality Toolkit/Mixed Reality Controller Mappings/Oculus Touch Controller Mapping Profile", fileName = "OculusTouchControllerMappingProfile")]
    public class OculusTouchControllerMappingProfile : BaseMixedRealityControllerMappingProfile
    {
        /// <inheritdoc />
        public override SupportedControllerType ControllerType => SupportedControllerType.OculusTouch;

        protected override void Awake()
        {
            if (!HasSetupDefaults)
            {
                ControllerMappings = new[]
                {
                    new MixedRealityControllerMapping("Oculus Touch Controller Left", typeof(OculusTouchController), Handedness.Left),
                    new MixedRealityControllerMapping("Oculus Touch Controller Right", typeof(OculusTouchController), Handedness.Right),
                };
            }

            base.Awake();
        }
    }
}