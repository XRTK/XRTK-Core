// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using UnityEngine;
using XRTK.Definitions.CameraSystem;
using XRTK.Extensions;
using XRTK.Interfaces.CameraSystem;
using XRTK.Services;
using XRTK.Utilities;

namespace XRTK.Providers.CameraSystem
{
    /// <summary>
    /// Base class for all <see cref="IMixedRealityCameraDataProvider"/>s can inherit from.
    /// </summary>
    [System.Runtime.InteropServices.Guid("EA4C0C19-E533-4AE8-91A2-6998CB8905BB")]
    public class BaseCameraDataProvider : BaseDataProvider, IMixedRealityCameraDataProvider
    {
        /// <inheritdoc />
        public BaseCameraDataProvider(string name, uint priority, BaseMixedRealityCameraDataProviderProfile profile, IMixedRealityCameraSystem parentService)
            : base(name, priority, profile, parentService)
        {
            cameraSystem = parentService;

            if (profile.IsNull())
            {
                profile = MixedRealityToolkit.TryGetSystemProfile<IMixedRealityCameraSystem, MixedRealityCameraSystemProfile>(out var cameraSystemProfile)
                    ? cameraSystemProfile.GlobalCameraProfile
                    : throw new ArgumentException($"Unable to get a valid {nameof(MixedRealityCameraSystemProfile)}!");
            }

            if (profile.CameraRigType?.Type == null)
            {
                throw new Exception($"{nameof(profile.CameraRigType)} cannot be null!");
            }

            isCameraPersistent = profile.IsCameraPersistent;
            cameraRigType = profile.CameraRigType.Type;
            DefaultHeadHeight = profile.DefaultHeadHeight;

            nearClipPlaneOpaqueDisplay = profile.NearClipPlaneOpaqueDisplay;
            cameraClearFlagsOpaqueDisplay = profile.CameraClearFlagsOpaqueDisplay;
            backgroundColorOpaqueDisplay = profile.BackgroundColorOpaqueDisplay;
            opaqueQualityLevel = profile.OpaqueQualityLevel;

            nearClipPlaneTransparentDisplay = profile.NearClipPlaneTransparentDisplay;
            cameraClearFlagsTransparentDisplay = profile.CameraClearFlagsTransparentDisplay;
            backgroundColorTransparentDisplay = profile.BackgroundColorTransparentDisplay;
            transparentQualityLevel = profile.TransparentQualityLevel;

            bodyAdjustmentAngle = profile.BodyAdjustmentAngle;
            bodyAdjustmentSpeed = profile.BodyAdjustmentSpeed;
        }

        /// <summary>
        /// The fallback value if the <see cref="DefaultHeadHeight"/> is zero.
        /// </summary>
        private const float BodyHeightFallback = 1.6f;

        private readonly IMixedRealityCameraSystem cameraSystem = null;

        private readonly Type cameraRigType;

        private readonly bool isCameraPersistent;

        private readonly int opaqueQualityLevel;
        private readonly int transparentQualityLevel;

        private readonly float nearClipPlaneOpaqueDisplay;
        private readonly float nearClipPlaneTransparentDisplay;

        private readonly Color backgroundColorOpaqueDisplay;
        private readonly Color backgroundColorTransparentDisplay;

        private readonly CameraClearFlags cameraClearFlagsOpaqueDisplay;
        private readonly CameraClearFlags cameraClearFlagsTransparentDisplay;

        private readonly float bodyAdjustmentSpeed;
        private readonly double bodyAdjustmentAngle;

        private bool cameraOpaqueLastFrame;

        /// <inheritdoc />
        public virtual bool IsOpaque
        {
            get
            {
#if UNITY_2020_1_OR_NEWER
                return XRDeviceUtilities.IsDisplayOpaque;
#else
                return true;
#endif // UNITY_2020_1_OR_NEWER
            }
        }

        /// <inheritdoc />
        public virtual bool IsStereoscopic => CameraRig.PlayerCamera.stereoEnabled;

        /// <inheritdoc />
        public virtual bool HeadHeightIsManagedByDevice => XRDeviceUtilities.IsDevicePresent;

        /// <inheritdoc />
        public IMixedRealityCameraRig CameraRig { get; private set; }

        /// <inheritdoc />
        public float DefaultHeadHeight { get; }

        private float headHeight;

        /// <inheritdoc />
        public virtual float HeadHeight
        {
            get => headHeight;
            set
            {
                if (value.Equals(headHeight))
                {
                    return;
                }

                headHeight = value;
                CameraRig.CameraPoseDriver.originPose = new Pose(new Vector3(0f, headHeight, 0f), Quaternion.identity);
            }
        }

        #region IMixedRealitySerivce Implementation

        /// <inheritdoc />
        public override uint Priority => 0;

        /// <inheritdoc />
        public override void Initialize()
        {
            base.Initialize();

            if (CameraRig == null)
            {
                // TODO Currently we always get the main camera. Should we provide a tag to search for alts?
                CameraRig = CameraCache.Main.gameObject.EnsureComponent(cameraRigType) as IMixedRealityCameraRig;
                Debug.Assert(CameraRig != null);
            }

            ApplySettingsForDefaultHeadHeight();
            cameraOpaqueLastFrame = IsOpaque;

            if (IsOpaque)
            {
                ApplySettingsForOpaqueDisplay();
            }
            else
            {
                ApplySettingsForTransparentDisplay();
            }

            cameraSystem.RegisterCameraDataProvider(this);
        }

        /// <inheritdoc />
        public override void Enable()
        {
            base.Enable();

            if (Application.isPlaying &&
                isCameraPersistent)
            {
                CameraRig.PlayerCamera.transform.root.DontDestroyOnLoad();
            }

            ApplySettingsForDefaultHeadHeight();
        }

        /// <inheritdoc />
        public override void Update()
        {
            base.Update();

            if (!Application.isPlaying) { return; }

            if (cameraOpaqueLastFrame != IsOpaque)
            {
                cameraOpaqueLastFrame = IsOpaque;

                ApplySettingsForDefaultHeadHeight();

                if (IsOpaque)
                {
                    ApplySettingsForOpaqueDisplay();
                }
                else
                {
                    ApplySettingsForTransparentDisplay();
                }
            }
        }

        /// <inheritdoc />
        public override void LateUpdate()
        {
            base.LateUpdate();

            if (!Application.isPlaying) { return; }

            SyncRigTransforms();
        }

        /// <inheritdoc />
        public override void Disable()
        {
            base.Disable();

            if (CameraRig == null ||
                CameraRig.GameObject.IsNull())
            {
                return;
            }

            ResetRigTransforms();

            if (!CameraRig.PlayerCamera.IsNull() &&
                !CameraRig.PlayerCamera.transform.IsNull())
            {
                var cameraTransform = CameraRig.PlayerCamera.transform;
                cameraTransform.SetParent(null);
                cameraTransform.position = Vector3.one;
                cameraTransform.rotation = Quaternion.identity;
            }

            if (CameraRig.PlayspaceTransform != null)
            {
                CameraRig.PlayspaceTransform.gameObject.Destroy();
            }

            if (CameraRig is Component component &&
                component is IMixedRealityCameraRig)
            {
                component.Destroy();
            }
        }

        /// <inheritdoc />
        public override void Destroy()
        {
            base.Destroy();

            cameraSystem.UnRegisterCameraDataProvider(this);
        }

        #endregion IMixedRealitySerivce Implementation

        /// <summary>
        /// Depending on whether there is an XR device connected,
        /// moves the camera to the setting from the camera profile.
        /// </summary>
        protected virtual void ApplySettingsForDefaultHeadHeight()
        {
            // We need to check whether the application is playing or not here.
            // Since this code is executed even when not in play mode, we want
            // to definitely apply the head height configured in the editor, when
            // not in play mode. It helps with working in the editor and visualizing
            // the user's perspective. When running though, we need to make sure we do
            // not interfere with any platform provided head pose tracking.
            if (!Application.isPlaying || !HeadHeightIsManagedByDevice)
            {
                HeadHeight = DefaultHeadHeight;
            }
            // If we are running and the device/platform provides the head pose,
            // we need to make sure to reset any applied head height while in edit mode.
            else if (Application.isPlaying && HeadHeightIsManagedByDevice)
            {
                HeadHeight = 0f;
            }

            ResetRigTransforms();
            SyncRigTransforms();
        }

        /// <summary>
        /// Applies opaque settings from camera profile.
        /// </summary>
        protected virtual void ApplySettingsForOpaqueDisplay()
        {
            CameraRig.PlayerCamera.clearFlags = cameraClearFlagsOpaqueDisplay;
            CameraRig.PlayerCamera.nearClipPlane = nearClipPlaneOpaqueDisplay;
            CameraRig.PlayerCamera.backgroundColor = backgroundColorOpaqueDisplay;
            QualitySettings.SetQualityLevel(opaqueQualityLevel, false);
        }

        /// <summary>
        /// Applies transparent settings from camera profile.
        /// </summary>
        protected virtual void ApplySettingsForTransparentDisplay()
        {
            CameraRig.PlayerCamera.clearFlags = cameraClearFlagsTransparentDisplay;
            CameraRig.PlayerCamera.backgroundColor = backgroundColorTransparentDisplay;
            CameraRig.PlayerCamera.nearClipPlane = nearClipPlaneTransparentDisplay;
            QualitySettings.SetQualityLevel(transparentQualityLevel, false);
        }

        /// <summary>
        /// Resets the <see cref="IMixedRealityCameraRig.PlayspaceTransform"/>, <see cref="IMixedRealityCameraRig.CameraTransform"/>,
        /// and <see cref="IMixedRealityCameraRig.BodyTransform"/> poses.
        /// </summary>
        protected virtual void ResetRigTransforms()
        {
            CameraRig.PlayspaceTransform.position = Vector3.zero;
            CameraRig.PlayspaceTransform.rotation = Quaternion.identity;

            // If the camera is a 2d camera then we can adjust the camera's height to match the head height.
            CameraRig.CameraTransform.position = IsStereoscopic ? Vector3.zero : new Vector3(0f, HeadHeight, 0f);

            CameraRig.CameraTransform.rotation = Quaternion.identity;
            CameraRig.BodyTransform.position = Vector3.zero;
            CameraRig.BodyTransform.rotation = Quaternion.identity;
        }

        /// <summary>
        /// Called each <see cref="LateUpdate"/> to the sync the <see cref="IMixedRealityCameraRig.PlayspaceTransform"/>,
        /// <see cref="IMixedRealityCameraRig.CameraTransform"/>, and <see cref="IMixedRealityCameraRig.BodyTransform"/> poses.
        /// </summary>
        protected virtual void SyncRigTransforms()
        {
            var cameraLocalPosition = CameraRig.CameraTransform.localPosition;
            var bodyLocalPosition = CameraRig.BodyTransform.localPosition;

            bodyLocalPosition.x = cameraLocalPosition.x;

            bodyLocalPosition.y = HeadHeight > 0f
                ? cameraLocalPosition.y - HeadHeight
                : cameraLocalPosition.y - BodyHeightFallback;

            bodyLocalPosition.z = cameraLocalPosition.z;

            CameraRig.BodyTransform.localPosition = bodyLocalPosition;

            var bodyRotation = CameraRig.BodyTransform.rotation;
            var headRotation = CameraRig.CameraTransform.rotation;
            var currentAngle = Mathf.Abs(Quaternion.Angle(bodyRotation, headRotation));

            if (currentAngle > bodyAdjustmentAngle)
            {
                CameraRig.BodyTransform.rotation = Quaternion.Slerp(bodyRotation, headRotation, Time.deltaTime * bodyAdjustmentSpeed);
            }
        }
    }
}
