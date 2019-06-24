﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.﻿

using UnityEditor;
using UnityEngine;
using XRTK.Definitions.InputSystem.Simulation;
using XRTK.Inspectors.Utilities;
using XRTK.Services.InputSystem.Simulation;

namespace XRTK.Inspectors.Profiles.InputSystem.Simulation
{
    [CustomEditor(typeof(MixedRealityInputSimulationProfile))]
    public class MixedRealityInputSimulationProfileInspector : BaseMixedRealityProfileInspector
    {
        private SerializedProperty isCameraControlEnabled;

        private SerializedProperty extraMouseSensitivityScale;
        private SerializedProperty defaultMouseSensitivity;
        private SerializedProperty mouseLookButton;
        private SerializedProperty isControllerLookInverted;
        private SerializedProperty currentControlMode;
        private SerializedProperty fastControlKey;
        private SerializedProperty controlSlowSpeed;
        private SerializedProperty controlFastSpeed;
        private SerializedProperty moveHorizontal;
        private SerializedProperty moveVertical;
        private SerializedProperty mouseX;
        private SerializedProperty mouseY;
        private SerializedProperty lookHorizontal;
        private SerializedProperty lookVertical;

        private SerializedProperty handSimulationMode;

        private SerializedProperty simulateEyePosition;

        private SerializedProperty toggleLeftHandKey;
        private SerializedProperty toggleRightHandKey;
        private SerializedProperty handHideTimeout;
        private SerializedProperty leftHandManipulationKey;
        private SerializedProperty rightHandManipulationKey;

        private SerializedProperty defaultHandGesture;
        private SerializedProperty leftMouseHandGesture;
        private SerializedProperty middleMouseHandGesture;
        private SerializedProperty rightMouseHandGesture;
        private SerializedProperty handGestureAnimationSpeed;

        private SerializedProperty defaultHandDistance;
        private SerializedProperty handDepthMultiplier;
        private SerializedProperty handJitterAmount;

        private SerializedProperty yawHandCWKey;
        private SerializedProperty yawHandCCWKey;
        private SerializedProperty pitchHandCWKey;
        private SerializedProperty pitchHandCCWKey;
        private SerializedProperty rollHandCWKey;
        private SerializedProperty rollHandCCWKey;
        private SerializedProperty handRotationSpeed;

        private SerializedProperty holdStartDuration;
        private SerializedProperty manipulationStartThreshold;

        private const string ProfileTitle = "Input Simulation Settings";
        private const string ProfileDescription = "Settings for simulating input devices in the editor.";

        protected override void OnEnable()
        {
            base.OnEnable();

            isCameraControlEnabled = serializedObject.FindProperty("isCameraControlEnabled");

            extraMouseSensitivityScale = serializedObject.FindProperty("extraMouseSensitivityScale");
            defaultMouseSensitivity = serializedObject.FindProperty("defaultMouseSensitivity");
            mouseLookButton = serializedObject.FindProperty("mouseLookButton");
            isControllerLookInverted = serializedObject.FindProperty("isControllerLookInverted");
            currentControlMode = serializedObject.FindProperty("currentControlMode");
            fastControlKey = serializedObject.FindProperty("fastControlKey");
            controlSlowSpeed = serializedObject.FindProperty("controlSlowSpeed");
            controlFastSpeed = serializedObject.FindProperty("controlFastSpeed");
            moveHorizontal = serializedObject.FindProperty("moveHorizontal");
            moveVertical = serializedObject.FindProperty("moveVertical");
            mouseX = serializedObject.FindProperty("mouseX");
            mouseY = serializedObject.FindProperty("mouseY");
            lookHorizontal = serializedObject.FindProperty("lookHorizontal");
            lookVertical = serializedObject.FindProperty("lookVertical");

            handSimulationMode = serializedObject.FindProperty("handSimulationMode");

            simulateEyePosition = serializedObject.FindProperty("simulateEyePosition");

            toggleLeftHandKey = serializedObject.FindProperty("toggleLeftHandKey");
            toggleRightHandKey = serializedObject.FindProperty("toggleRightHandKey");
            handHideTimeout = serializedObject.FindProperty("handHideTimeout");
            leftHandManipulationKey = serializedObject.FindProperty("leftHandManipulationKey");
            rightHandManipulationKey = serializedObject.FindProperty("rightHandManipulationKey");

            defaultHandGesture = serializedObject.FindProperty("defaultHandGesture");
            leftMouseHandGesture = serializedObject.FindProperty("leftMouseHandGesture");
            middleMouseHandGesture = serializedObject.FindProperty("middleMouseHandGesture");
            rightMouseHandGesture = serializedObject.FindProperty("rightMouseHandGesture");
            handGestureAnimationSpeed = serializedObject.FindProperty("handGestureAnimationSpeed");

            holdStartDuration = serializedObject.FindProperty("holdStartDuration");
            manipulationStartThreshold = serializedObject.FindProperty("manipulationStartThreshold");

            defaultHandDistance = serializedObject.FindProperty("defaultHandDistance");
            handDepthMultiplier = serializedObject.FindProperty("handDepthMultiplier");
            handJitterAmount = serializedObject.FindProperty("handJitterAmount");

            yawHandCWKey = serializedObject.FindProperty("yawHandCWKey");
            yawHandCCWKey = serializedObject.FindProperty("yawHandCCWKey");
            pitchHandCWKey = serializedObject.FindProperty("pitchHandCWKey");
            pitchHandCCWKey = serializedObject.FindProperty("pitchHandCCWKey");
            rollHandCWKey = serializedObject.FindProperty("rollHandCWKey");
            rollHandCCWKey = serializedObject.FindProperty("rollHandCCWKey");
            handRotationSpeed = serializedObject.FindProperty("handRotationSpeed");
        }

        public override void OnInspectorGUI()
        {
            MixedRealityInspectorUtility.RenderMixedRealityToolkitLogo();

            if (thisProfile.ParentProfile != null &&
                GUILayout.Button("Back to Configuration Profile"))
            {
                Selection.activeObject = thisProfile.ParentProfile;
            }

            thisProfile.CheckProfileLock();

            serializedObject.Update();


            EditorGUILayout.PropertyField(isCameraControlEnabled);
            {
                EditorGUILayout.BeginVertical("Label");

                EditorGUILayout.PropertyField(extraMouseSensitivityScale);
                EditorGUILayout.PropertyField(defaultMouseSensitivity);
                EditorGUILayout.PropertyField(mouseLookButton);
                EditorGUILayout.PropertyField(isControllerLookInverted);
                EditorGUILayout.PropertyField(currentControlMode);
                EditorGUILayout.PropertyField(fastControlKey);
                EditorGUILayout.PropertyField(controlSlowSpeed);
                EditorGUILayout.PropertyField(controlFastSpeed);
                EditorGUILayout.PropertyField(moveHorizontal);
                EditorGUILayout.PropertyField(moveVertical);
                EditorGUILayout.PropertyField(mouseX);
                EditorGUILayout.PropertyField(mouseY);
                EditorGUILayout.PropertyField(lookHorizontal);
                EditorGUILayout.PropertyField(lookVertical);

                EditorGUILayout.EndVertical();

            }

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(simulateEyePosition);

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(handSimulationMode);
            {
                EditorGUILayout.BeginVertical("Label");
                bool isHandSimEnabled = (handSimulationMode.enumValueIndex != (int)HandSimulationMode.Disabled);

                EditorGUILayout.PropertyField(toggleLeftHandKey);
                EditorGUILayout.PropertyField(toggleRightHandKey);
                EditorGUILayout.PropertyField(handHideTimeout);
                EditorGUILayout.PropertyField(leftHandManipulationKey);
                EditorGUILayout.PropertyField(rightHandManipulationKey);
                EditorGUILayout.Space();

                EditorGUILayout.PropertyField(defaultHandGesture);
                EditorGUILayout.PropertyField(leftMouseHandGesture);
                EditorGUILayout.PropertyField(middleMouseHandGesture);
                EditorGUILayout.PropertyField(rightMouseHandGesture);
                EditorGUILayout.PropertyField(handGestureAnimationSpeed);
                EditorGUILayout.Space();

                EditorGUILayout.PropertyField(holdStartDuration);
                EditorGUILayout.PropertyField(manipulationStartThreshold);
                EditorGUILayout.Space();

                EditorGUILayout.PropertyField(defaultHandDistance);
                EditorGUILayout.PropertyField(handDepthMultiplier);
                EditorGUILayout.PropertyField(handJitterAmount);
                EditorGUILayout.Space();

                EditorGUILayout.PropertyField(yawHandCWKey);
                EditorGUILayout.PropertyField(yawHandCCWKey);
                EditorGUILayout.PropertyField(pitchHandCWKey);
                EditorGUILayout.PropertyField(pitchHandCCWKey);
                EditorGUILayout.PropertyField(rollHandCWKey);
                EditorGUILayout.PropertyField(rollHandCCWKey);
                EditorGUILayout.PropertyField(handRotationSpeed);
                EditorGUILayout.Space();

                EditorGUILayout.EndVertical();
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
