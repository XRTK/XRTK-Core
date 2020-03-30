﻿// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEditor;
using XRTK.Definitions.Controllers.Simulation.Hands;

namespace XRTK.Inspectors.Profiles.InputSystem.Controllers.Simulation
{
    [CustomEditor(typeof(SimulatedHandControllerDataProviderProfile))]
    public class SimulatedHandControllerDataProviderProfileInspector : SimulatedControllerDataProviderProfileInspector
    {
        private SerializedProperty poseDefinitions;
        private SerializedProperty handPoseAnimationSpeed;

        protected override void OnEnable()
        {
            base.OnEnable();

            poseDefinitions = serializedObject.FindProperty(nameof(poseDefinitions));
            handPoseAnimationSpeed = serializedObject.FindProperty(nameof(handPoseAnimationSpeed));
        }

        protected override void OnInspectorAdditionalGUI()
        {
            EditorGUILayout.PropertyField(poseDefinitions, true);
            EditorGUILayout.PropertyField(handPoseAnimationSpeed);
            EditorGUILayout.Space();
        }
    }
}