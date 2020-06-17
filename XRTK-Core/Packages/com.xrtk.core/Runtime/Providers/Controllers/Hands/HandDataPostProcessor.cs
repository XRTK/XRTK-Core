﻿// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using UnityEngine;
using XRTK.Definitions.Controllers.Hands;
using XRTK.Definitions.Devices;
using XRTK.Definitions.Utilities;
using XRTK.Services;
using XRTK.Utilities;

namespace XRTK.Providers.Controllers.Hands
{
    /// <summary>
    /// The hand data post processor updates <see cref="HandData"/> provided
    /// by a platform and enriches it with potentially missing information.
    /// </summary>
    public sealed class HandDataPostProcessor
    {
        /// <summary>
        /// Creates a new instance of the hand data post processor.
        /// </summary>
        /// <param name="trackedPoses">Pose recognizer instance to use for pose recognition.</param>
        public HandDataPostProcessor(IReadOnlyList<HandControllerPoseProfile> trackedPoses)
        {
            TrackedPoseProcessor = new HandTrackedPoseProcessor(trackedPoses);
            GripPostProcessor = new HandGripPostProcessor();
        }

        private const float IS_POINTING_DOTP_THRESHOLD = .1f;
        private const float TWO_CENTIMETER_SQUARE_MAGNITUDE = 0.0004f;
        private const float FIVE_CENTIMETER_SQUARE_MAGNITUDE = 0.0025f;
        private const float PINCH_STRENGTH_DISTANCE = FIVE_CENTIMETER_SQUARE_MAGNITUDE - TWO_CENTIMETER_SQUARE_MAGNITUDE;

        /// <summary>
        /// Processor instance used for pose recognition.
        /// </summary>
        private HandTrackedPoseProcessor TrackedPoseProcessor { get; }

        /// <summary>
        /// Grip post processor instance used for grip estimation.
        /// </summary>
        private HandGripPostProcessor GripPostProcessor { get; }

        /// <summary>
        /// Is <see cref="HandData.PointerPose"/> provided by the platform?
        /// </summary>
        public bool PlatformProvidesPointerPose { get; set; }

        /// <summary>
        /// Is <see cref="HandData.IsPinching"/> provided by the platform?
        /// </summary>
        public bool PlatformProvidesIsPinching { get; set; }

        /// <summary>
        /// Is <see cref="HandData.PinchStrength"/> provided by the platform?
        /// </summary>
        public bool PlatformProvidesPinchStrength { get; set; }

        /// <summary>
        /// Is <see cref="HandData.IsPointing"/> provided by the platform?
        /// </summary>
        public bool PlatformProvidesIsPointing { get; set; }

        /// <summary>
        /// Finalizes the hand data retrieved from platform APIs by adding
        /// any information the platform could not provide.
        /// </summary>
        /// <param name="handedness">The handedness of the data provided.</param>
        /// <param name="handData">The hand data retrieved from platform conversion.</param>
        public HandData PostProcess(Handedness handedness, HandData handData)
        {
            handData = UpdateIsPinchingAndStrength(handData);
            handData = UpdateIsPointing(handData);
            handData = UpdatePointerPose(handData);
            handData = GripPostProcessor.Process(handData);
            handData = TrackedPoseProcessor.Process(handedness, handData);

            return handData;
        }

        /// <summary>
        /// Updates <see cref="HandData.IsPinching"/> and <see cref="HandData.PinchStrength"/>
        /// if the platform did not provide it.
        /// </summary>
        /// <param name="handData">The hand data to update <see cref="HandData.IsPinching"/> and <see cref="HandData.PinchStrength"/> for.</param>
        private HandData UpdateIsPinchingAndStrength(HandData handData)
        {
            if (handData.TrackingState == TrackingState.Tracked)
            {
                var thumbTipPose = handData.Joints[(int)TrackedHandJoint.ThumbTip];
                var indexTipPose = handData.Joints[(int)TrackedHandJoint.IndexTip];

                if (!PlatformProvidesIsPinching)
                {
                    handData.IsPinching = (thumbTipPose.Position - indexTipPose.Position).sqrMagnitude < TWO_CENTIMETER_SQUARE_MAGNITUDE;
                }

                if (!PlatformProvidesPinchStrength)
                {
                    var distanceSquareMagnitude = (thumbTipPose.Position - indexTipPose.Position).sqrMagnitude - TWO_CENTIMETER_SQUARE_MAGNITUDE;
                    handData.PinchStrength = 1 - Mathf.Clamp(distanceSquareMagnitude / PINCH_STRENGTH_DISTANCE, 0f, 1f);
                }
            }
            else
            {
                handData.IsPinching = false;
                handData.PinchStrength = 0f;
            }

            return handData;
        }

        /// <summary>
        /// Updates <see cref="HandData.IsPointing"/> if the platform did not provide it.
        /// </summary>
        /// <param name="handData">The hand data to update <see cref="HandData.IsPointing"/> for.</param>
        private HandData UpdateIsPointing(HandData handData)
        {
            if (handData.TrackingState == TrackingState.Tracked && !PlatformProvidesIsPointing)
            {
                var playspaceTransform = MixedRealityToolkit.CameraSystem.MainCameraRig.PlayspaceTransform;
                var localPalmPose = handData.Joints[(int)TrackedHandJoint.Palm];
                var worldPalmPose = new MixedRealityPose()
                {
                    Position = handData.RootPose.Position + handData.RootPose.Rotation * localPalmPose.Position,
                    Rotation = playspaceTransform.rotation * handData.RootPose.Rotation * localPalmPose.Rotation
                };

                var cameraTransform = MixedRealityToolkit.CameraSystem != null
                ? MixedRealityToolkit.CameraSystem.MainCameraRig.PlayerCamera.transform
                : CameraCache.Main.transform;

                // We check if the palm forward is roughly in line with the camera lookAt.
                var projectedPalmUp = Vector3.ProjectOnPlane(-worldPalmPose.Up, cameraTransform.up);
                handData.IsPointing = Vector3.Dot(cameraTransform.forward, projectedPalmUp) > IS_POINTING_DOTP_THRESHOLD;
            }
            else
            {
                handData.IsPointing = false;
            }

            return handData;
        }

        /// <summary>
        /// Updates <see cref="HandData.PointerPose"/> if the platform did not provide it.
        /// </summary>
        /// <param name="handData">The hand data to update <see cref="HandData.PointerPose"/> for.</param>
        private HandData UpdatePointerPose(HandData handData)
        {
            if (handData.TrackingState == TrackingState.Tracked && !PlatformProvidesPointerPose)
            {
                var palmPose = handData.Joints[(int)TrackedHandJoint.Palm];
                palmPose.Rotation = Quaternion.Inverse(palmPose.Rotation) * palmPose.Rotation;

                var thumbProximalPose = handData.Joints[(int)TrackedHandJoint.ThumbProximal];
                var indexDistalPose = handData.Joints[(int)TrackedHandJoint.IndexDistal];
                var pointerPosition = handData.RootPose.Position + Vector3.Lerp(thumbProximalPose.Position, indexDistalPose.Position, .5f);
                var pointerEndPosition = pointerPosition + palmPose.Forward * 10f;

                var camera = MixedRealityToolkit.CameraSystem != null
                    ? MixedRealityToolkit.CameraSystem.MainCameraRig.PlayerCamera
                    : CameraCache.Main;

                var pointerDirection = pointerEndPosition - pointerPosition;
                var pointerRotation = Quaternion.LookRotation(pointerDirection, camera.transform.up) * handData.RootPose.Rotation;

                pointerRotation = camera.transform.rotation * pointerRotation;

                handData.PointerPose = new MixedRealityPose(pointerPosition, pointerRotation);
            }

            return handData;
        }
    }
}