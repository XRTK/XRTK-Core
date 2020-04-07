﻿// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using UnityEngine;
using XRTK.Definitions.Controllers;
using XRTK.Definitions.Devices;
using XRTK.Definitions.Utilities;
using XRTK.Interfaces.InputSystem;
using XRTK.Interfaces.Providers.Controllers;

namespace XRTK.Definitions.Controllers
{
    /// <summary>
    /// Base Controller class to inherit from for all controllers.
    /// </summary>
    public abstract class BaseController : IMixedRealityController
    {
        /// <summary>
        /// Creates a new instance of a controller.
        /// </summary>
        /// <param name="controllerDataProvider">The <see cref="IMixedRealityControllerDataProvider"/> this controller belongs to.</param>
        /// <param name="trackingState">The initial tracking state of this controller.</param>
        /// <param name="controllerHandedness">The controller's handedness.</param>
        /// <param name="inputSource">The <see cref="IMixedRealityInputSource"/> this controller belongs to.</param>
        /// <param name="interactions">The <see cref="MixedRealityInteractionMapping"/>s currently assigned to this controller.</param>
        protected BaseController(IMixedRealityControllerDataProvider controllerDataProvider, TrackingState trackingState, Handedness controllerHandedness, IMixedRealityInputSource inputSource = null, MixedRealityInteractionMapping[] interactions = null)
        {
            ControllerDataProvider = controllerDataProvider;
            TrackingState = trackingState;
            ControllerHandedness = controllerHandedness;
            InputSource = inputSource;
            Interactions = interactions;

            IsPositionAvailable = false;
            IsPositionApproximate = false;
            IsRotationAvailable = false;

            Enabled = true;
        }

        /// <summary>
        /// The default interactions for this controller.
        /// </summary>
        public virtual MixedRealityInteractionMapping[] DefaultInteractions { get; } = null;

        /// <summary>
        /// The Default Left Handed interactions for this controller.
        /// </summary>
        public virtual MixedRealityInteractionMapping[] DefaultLeftHandedInteractions { get; } = null;

        /// <summary>
        /// The Default Right Handed interactions for this controller.
        /// </summary>
        public virtual MixedRealityInteractionMapping[] DefaultRightHandedInteractions { get; } = null;

        #region IMixedRealityController Implementation

        /// <inheritdoc />
        public bool Enabled { get; set; }

        /// <inheritdoc />
        public IMixedRealityControllerDataProvider ControllerDataProvider { get; }

        /// <inheritdoc />
        public TrackingState TrackingState { get; protected set; }

        /// <inheritdoc />
        public Handedness ControllerHandedness { get; }

        /// <inheritdoc />
        public IMixedRealityInputSource InputSource { get; }

        /// <inheritdoc />
        public IMixedRealityControllerVisualizer Visualizer { get; private set; }

        /// <inheritdoc />
        public bool IsPositionAvailable { get; protected set; }

        /// <inheritdoc />
        public bool IsPositionApproximate { get; protected set; }

        /// <inheritdoc />
        public bool IsRotationAvailable { get; protected set; }

        /// <inheritdoc />
        public MixedRealityInteractionMapping[] Interactions { get; private set; } = null;

        /// <inheritdoc />
        public Vector3 AngularVelocity { get; } = Vector3.zero;

        /// <inheritdoc />
        public Vector3 Velocity { get; } = Vector3.zero;

        #endregion IMixedRealityController Implementation

        /// <summary>
        /// Updates the current readings for the controller.
        /// </summary>
        public virtual void UpdateController() { }

        /// <inheritdoc />
        public bool SetupConfiguration(Type controllerType)
        {
            if (controllerType == null)
            {
                Debug.LogError("controllerType cannot be null");
                return false;
            }

            // Have to test that a controller type has been registered in the profiles,
            // else it's Unity Input manager mappings will not have been setup by the inspector
            bool profileFound = false;

            // We can only enable controller profiles if mappings exist. Assign any known interaction mappings if found.
            //if (MixedRealityToolkit.Instance.ActiveProfile.InputSystemProfile.ControllerMappingProfiles.MixedRealityControllerMappings.GetControllerInteractionMapping(controllerType, ControllerHandedness, out MixedRealityControllerMapping controllerMapping))
            //{
            //    profileFound = true;

            //    AssignControllerMappings(controllerMapping.Interactions);

            //    // If no controller mappings found, warn the user.  Does not stop the project from running.
            //    if (Interactions == null || Interactions.Length < 1)
            //    {
            //        SetupDefaultInteractions(ControllerHandedness);

            //        // We still don't have controller mappings, so this may be a custom controller.
            //        if (Interactions == null || Interactions.Length < 1)
            //        {
            //            Debug.LogWarning($"No Controller interaction mappings found for {controllerMapping.Description}!\nThe default interactions were assigned.");
            //        }
            //    }
            //}

            if (!profileFound)
            {
                //Debug.LogWarning($"No controller profile found for type {controllerType.Name}, please ensure all controllers are defined and configured in the {nameof(MixedRealityControllerMappingProfiles)}.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Assign the default interactions based on controller handedness if necessary.
        /// </summary>
        /// <param name="controllerHandedness">The handedness of the controller.</param>
        public abstract void SetupDefaultInteractions(Handedness controllerHandedness);

        /// <summary>
        /// Load the Interaction mappings for this controller from the configured Controller Mapping profile
        /// </summary>
        /// <param name="mappings">Configured mappings from a controller mapping profile</param>
        public void AssignControllerMappings(MixedRealityInteractionMapping[] mappings) => Interactions = mappings;

        /// <inheritdoc />
        public async void TryRenderControllerModel(Type controllerType, byte[] glbData = null) => await TryRenderControllerModelAsync(controllerType, glbData);

        /// <summary>
        /// Attempts to load the controller model render settings from the <see cref="MixedRealityControllerVisualizationProfile"/>
        /// to render the controllers in the scene.
        /// </summary>
        /// <param name="controllerType">The controller type.</param>
        /// <param name="useAlternatePoseAction">Should the visualizer be assigned the alternate pose actions?</param>
        /// <param name="glbData">The raw binary glb data of the controller model, typically loaded from the driver.</param>
        internal async void TryRenderControllerModel(Type controllerType, bool useAlternatePoseAction, byte[] glbData = null) => await TryRenderControllerModelAsync(controllerType, glbData, useAlternatePoseAction);

        /// <summary>
        /// Attempts to load the controller model render settings from the <see cref="MixedRealityControllerVisualizationProfile"/>
        /// to render the controllers in the scene.
        /// </summary>
        /// <param name="controllerType">The controller type.</param>
        /// <param name="glbData">The raw binary glb data of the controller model, typically loaded from the driver.</param>
        /// <param name="useAlternatePoseAction">Should the visualizer be assigned the alternate pose actions?</param>
        /// <returns>True, if controller model is being properly rendered.</returns>
        /// <remarks>
        /// (Given a user can, have no system default and override specific controller types with a system default, OR, enable a system system default but override that default for specific controllers)
        /// Flow is as follows:
        /// 1. Check if either there is a global setting for an system override and if there is a specific customization for that controller type
        /// 2. If either the there is a system data and either the
        ///
        /// </remarks>
#pragma warning disable 1998
        internal async Task TryRenderControllerModelAsync(Type controllerType, byte[] glbData = null, bool useAlternatePoseAction = false)
#pragma warning restore 1998
        {
            //if (controllerType == null)
            //{
            //    Debug.LogError("Unknown type of controller, cannot render");
            //    return;
            //}

            //var visualizationProfile = MixedRealityToolkit.Instance.ActiveProfile.InputSystemProfile.ControllerVisualizationProfile;

            //if (visualizationProfile == null)
            //{
            //    Debug.LogError("Missing ControllerVisualizationProfile!");
            //    return;
            //}

            //if (!visualizationProfile.RenderMotionControllers) { return; }

            //GltfObject gltfObject = null;

            //// If a specific controller template exists, check if it wants to override the global model, or use the system default specifically (in case global default is not used)
            //bool useSystemDefaultModels = visualizationProfile.GetControllerModelOverride(controllerType, ControllerHandedness, out var controllerModel);

            //// If an override is not configured for defaults and has no model, then use the system default check
            //if (!useSystemDefaultModels && controllerModel == null)
            //{
            //    useSystemDefaultModels = visualizationProfile.UseDefaultModels;
            //}

            //// if we have model data from the platform and the controller has been configured to use the default model, attempt to load the controller model from glbData.
            //if (glbData != null && useSystemDefaultModels)
            //{
            //    gltfObject = GltfUtility.GetGltfObjectFromGlb(glbData);
            //    await gltfObject.ConstructAsync();
            //    controllerModel = gltfObject.GameObjectReference;
            //}

            //// If we didn't get an override model, and we didn't load the driver model,
            //// then get the global controller model for each hand.
            //if (controllerModel == null)
            //{
            //    switch (ControllerHandedness)
            //    {
            //        case Handedness.Left when visualizationProfile.GlobalLeftHandModel != null:
            //            controllerModel = visualizationProfile.GlobalLeftHandModel;
            //            break;
            //        case Handedness.Right when visualizationProfile.GlobalRightHandModel != null:
            //            controllerModel = visualizationProfile.GlobalRightHandModel;
            //            break;
            //    }
            //}

            //// If we've got a controller model, then place it in the scene and get/attach the visualizer.
            //if (controllerModel != null)
            //{
            //    //If the model was loaded from a system template
            //    if (useSystemDefaultModels && gltfObject != null)
            //    {
            //        controllerModel.name = $"{controllerType.Name}_Visualization";
            //        controllerModel.transform.SetParent(MixedRealityToolkit.CameraSystem?.CameraRig.PlayspaceTransform);
            //        var visualizationType = visualizationProfile.GetControllerVisualizationTypeOverride(controllerType, ControllerHandedness) ??
            //                                visualizationProfile.ControllerVisualizationType;
            //        controllerModel.AddComponent(visualizationType.Type);
            //        Visualizer = controllerModel.GetComponent<IMixedRealityControllerVisualizer>();
            //    }
            //    //If the model was a prefab
            //    else
            //    {
            //        var controllerObject = UnityEngine.Object.Instantiate(controllerModel, MixedRealityToolkit.CameraSystem?.CameraRig.PlayspaceTransform);
            //        controllerObject.name = $"{controllerType.Name}_Visualization";
            //        Visualizer = controllerObject.GetComponent<IMixedRealityControllerVisualizer>();
            //    }

            //    //If a visualizer exists, set it up and bind it to the controller
            //    if (Visualizer != null)
            //    {
            //        Visualizer.Controller = this;
            //        SetupController(Visualizer);
            //    }
            //    else
            //    {
            //        Debug.LogWarning($"Failed to attach a valid IMixedRealityControllerVisualizer to {controllerType.Name}");
            //    }
            //}

            //if (Visualizer == null)
            //{
            //    Debug.LogError("Failed to render controller model!");
            //}

            //void SetupController(IMixedRealityControllerVisualizer visualizer)
            //{
            //    if (!useAlternatePoseAction &&
            //        visualizationProfile.TryGetControllerPose(controllerType, ControllerHandedness, out MixedRealityInputAction poseAction))
            //    {
            //        visualizer.UseSourcePoseData = false;
            //        visualizer.PoseAction = poseAction;
            //    }
            //    else if (useAlternatePoseAction &&
            //             visualizationProfile.TryGetControllerPoseOverride(controllerType, ControllerHandedness, out MixedRealityInputAction altPoseAction))
            //    {
            //        visualizer.UseSourcePoseData = false;
            //        visualizer.PoseAction = altPoseAction;
            //    }
            //    else if (visualizationProfile.GlobalPointerPose != MixedRealityInputAction.None)
            //    {
            //        visualizer.UseSourcePoseData = false;
            //        visualizer.PoseAction = visualizationProfile.GlobalPointerPose;
            //    }
            //    else
            //    {
            //        Debug.LogError("Failed to get pose actions for controller visual.");
            //    }
            //}
        }
    }
}