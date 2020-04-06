﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using XRTK.Definitions.Devices;
using XRTK.Definitions.Utilities;
using XRTK.Interfaces.InputSystem;

namespace XRTK.Providers.Controllers.UnityInput
{
    /// <summary>
    /// Xbox Controller using Unity Input System
    /// </summary>
    public class XboxController : GenericJoystickController
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="trackingState"></param>
        /// <param name="controllerHandedness"></param>
        /// <param name="inputSource"></param>
        /// <param name="interactions"></param>
        public XboxController(TrackingState trackingState, Handedness controllerHandedness, IMixedRealityInputSource inputSource = null, MixedRealityInteractionMapping[] interactions = null)
            : base(trackingState, controllerHandedness, inputSource, interactions)
        {
        }

        /// <summary>
        /// Default interactions for Xbox Controller using Unity Input System.
        /// </summary>
        public override MixedRealityInteractionMapping[] DefaultInteractions => new[]
        {
            new MixedRealityInteractionMapping("Left Thumbstick", AxisType.DualAxis, DeviceInputType.ThumbStick, ControllerMappingLibrary.AXIS_1, ControllerMappingLibrary.AXIS_2, false, true),
            new MixedRealityInteractionMapping("Left Thumbstick Click", AxisType.Digital, DeviceInputType.ButtonPress,KeyCode.JoystickButton8),
            new MixedRealityInteractionMapping("Right Thumbstick", AxisType.DualAxis, DeviceInputType.ThumbStick, ControllerMappingLibrary.AXIS_4, ControllerMappingLibrary.AXIS_5, false, true),
            new MixedRealityInteractionMapping("Right Thumbstick Click", AxisType.Digital, DeviceInputType.ButtonPress,KeyCode.JoystickButton9),
            new MixedRealityInteractionMapping("D-Pad", AxisType.DualAxis, DeviceInputType.DirectionalPad, ControllerMappingLibrary.AXIS_6, ControllerMappingLibrary.AXIS_7, false, true),
            new MixedRealityInteractionMapping("Shared Trigger", AxisType.SingleAxis, DeviceInputType.Trigger, ControllerMappingLibrary.AXIS_3),
            new MixedRealityInteractionMapping("Left Trigger", AxisType.SingleAxis, DeviceInputType.Trigger, ControllerMappingLibrary.AXIS_9),
            new MixedRealityInteractionMapping("Right Trigger", AxisType.SingleAxis, DeviceInputType.Trigger, ControllerMappingLibrary.AXIS_10),
            new MixedRealityInteractionMapping("View", AxisType.Digital, DeviceInputType.ButtonPress,KeyCode.JoystickButton6),
            new MixedRealityInteractionMapping("Menu", AxisType.Digital, DeviceInputType.ButtonPress,KeyCode.JoystickButton7),
            new MixedRealityInteractionMapping("Left Bumper", AxisType.Digital, DeviceInputType.ButtonPress,KeyCode.JoystickButton4),
            new MixedRealityInteractionMapping("Right Bumper", AxisType.Digital, DeviceInputType.ButtonPress,KeyCode.JoystickButton5),
            new MixedRealityInteractionMapping("A", AxisType.Digital, DeviceInputType.ButtonPress,KeyCode.JoystickButton0),
            new MixedRealityInteractionMapping("B", AxisType.Digital, DeviceInputType.ButtonPress,KeyCode.JoystickButton1),
            new MixedRealityInteractionMapping("X", AxisType.Digital, DeviceInputType.ButtonPress,KeyCode.JoystickButton2),
            new MixedRealityInteractionMapping("Y", AxisType.Digital, DeviceInputType.ButtonPress,KeyCode.JoystickButton3),
        };

        /// <inheritdoc />
        public override void SetupDefaultInteractions(Handedness controllerHandedness)
        {
            AssignControllerMappings(DefaultInteractions);
        }
    }
}