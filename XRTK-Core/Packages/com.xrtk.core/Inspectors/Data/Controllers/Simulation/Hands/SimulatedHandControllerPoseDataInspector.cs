﻿// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEditor;
using XRTK.Definitions.Controllers.Simulation.Hands;
using XRTK.Inspectors.Profiles;

namespace XRTK.Inspectors.Data.Controllers.Simulation.Hands
{
    [CustomEditor(typeof(SimulatedHandControllerPoseData))]
    public class SimulatedHandControllerPoseDataInspector : BaseMixedRealityProfileInspector
    {
        private SerializedProperty id;
        private SerializedProperty description;
        private SerializedProperty isDefault;
        private SerializedProperty keyCode;
        private SerializedProperty data;

        protected override void OnEnable()
        {
            base.OnEnable();

            id = serializedObject.FindProperty(nameof(id));
            description = serializedObject.FindProperty(nameof(description));
            isDefault = serializedObject.FindProperty(nameof(isDefault));
            keyCode = serializedObject.FindProperty(nameof(keyCode));
            data = serializedObject.FindProperty(nameof(data));
        }

        public override void OnInspectorGUI()
        {
            RenderHeader("This profile helps to define all of the available poses for simulated hands.");

            serializedObject.Update();

            EditorGUILayout.PropertyField(id);
            EditorGUILayout.PropertyField(description);
            EditorGUILayout.PropertyField(isDefault);
            EditorGUILayout.PropertyField(keyCode);
            EditorGUILayout.PropertyField(data);

            serializedObject.ApplyModifiedProperties();
        }
    }
}