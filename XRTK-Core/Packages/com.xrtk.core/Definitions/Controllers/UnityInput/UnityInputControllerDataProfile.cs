﻿// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using XRTK.Definitions.Utilities;

namespace XRTK.Definitions.Controllers.UnityInput.Profiles
{
    public class UnityInputControllerDataProfile : BaseMixedRealityControllerDataProviderProfile
    {
        public override ControllerDefinition[] GetDefaultControllerOptions()
        {
            return new[]
            {
                new ControllerDefinition(typeof(GenericJoystickController), Handedness.None, true),
                new ControllerDefinition(typeof(XboxController))
            };
        }
    }
}