﻿// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEditor;
using XRTK.Definitions.Controllers.Simulation;
using XRTK.Services;

namespace XRTK.Inspectors.Profiles.InputSystem.Controllers.Simulation
{
    [CustomEditor(typeof(SimulatedControllerDataProviderProfile))]
    public class SimulatedControllerDataProviderProfileInspector : BaseMixedRealityProfileInspector
    {
        private SerializedProperty simulatedUpdateFrequency;
        private SerializedProperty controllerHideTimeout;

        private SerializedProperty defaultDistance;
        private SerializedProperty depthMultiplier;
        private SerializedProperty jitterAmount;

        private SerializedProperty toggleLeftPersistentKey;
        private SerializedProperty leftControllerTrackedKey;
        private SerializedProperty toggleRightPersistentKey;
        private SerializedProperty rightControllerTrackedKey;

        private SerializedProperty rotationSpeed;

        protected override void OnEnable()
        {
            base.OnEnable();

            simulatedUpdateFrequency = serializedObject.FindProperty(nameof(simulatedUpdateFrequency));
            controllerHideTimeout = serializedObject.FindProperty(nameof(controllerHideTimeout));

            defaultDistance = serializedObject.FindProperty(nameof(defaultDistance));
            depthMultiplier = serializedObject.FindProperty(nameof(depthMultiplier));
            jitterAmount = serializedObject.FindProperty(nameof(jitterAmount));

            toggleLeftPersistentKey = serializedObject.FindProperty(nameof(toggleLeftPersistentKey));
            toggleRightPersistentKey = serializedObject.FindProperty(nameof(toggleRightPersistentKey));
            leftControllerTrackedKey = serializedObject.FindProperty(nameof(leftControllerTrackedKey));
            rightControllerTrackedKey = serializedObject.FindProperty(nameof(rightControllerTrackedKey));

            rotationSpeed = serializedObject.FindProperty(nameof(rotationSpeed));
        }

        public override void OnInspectorGUI()
        {
            RenderHeader("This profile contains all of the settings for simulating all kinds of controllers in the editor runtime.");

            EditorGUILayout.Space();

            serializedObject.Update();

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(simulatedUpdateFrequency);
            EditorGUILayout.PropertyField(controllerHideTimeout);
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(defaultDistance);
            EditorGUILayout.PropertyField(depthMultiplier);
            EditorGUILayout.PropertyField(jitterAmount);
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(toggleLeftPersistentKey);
            EditorGUILayout.PropertyField(leftControllerTrackedKey);
            EditorGUILayout.PropertyField(toggleRightPersistentKey);
            EditorGUILayout.PropertyField(rightControllerTrackedKey);
            EditorGUILayout.PropertyField(rotationSpeed);

            serializedObject.ApplyModifiedProperties();
        }
    }
}