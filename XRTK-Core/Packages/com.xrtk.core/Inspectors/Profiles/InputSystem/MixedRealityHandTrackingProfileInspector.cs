﻿// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.﻿

using UnityEditor;
using XRTK.Definitions.InputSystem;

namespace XRTK.Inspectors.Profiles.InputSystem
{
    [CustomEditor(typeof(MixedRealityHandTrackingProfile))]
    public class MixedRealityHandTrackingProfileInspector : BaseMixedRealityProfileInspector
    {
        private SerializedProperty handMeshingEnabled;

        private SerializedProperty handPhysicsEnabled;
        private SerializedProperty useTriggers;
        private SerializedProperty boundsMode;

        protected override void OnEnable()
        {
            base.OnEnable();

            handMeshingEnabled = serializedObject.FindProperty(nameof(handMeshingEnabled));

            handPhysicsEnabled = serializedObject.FindProperty(nameof(handPhysicsEnabled));
            useTriggers = serializedObject.FindProperty(nameof(useTriggers));
            boundsMode = serializedObject.FindProperty(nameof(boundsMode));
        }

        public override void OnInspectorGUI()
        {
            RenderHeader();

            EditorGUILayout.LabelField("Global Hand Tracking Options", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("This profile defines global hand tracking options applied to all platforms that support hand tracking. You may override these globals per platform in the platform's data provider profile.", MessageType.Info);
            EditorGUILayout.Space();

            serializedObject.Update();

            EditorGUILayout.PropertyField(handMeshingEnabled);
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(handPhysicsEnabled);
            EditorGUILayout.PropertyField(useTriggers);
            EditorGUILayout.PropertyField(boundsMode);

            serializedObject.ApplyModifiedProperties();
        }
    }
}