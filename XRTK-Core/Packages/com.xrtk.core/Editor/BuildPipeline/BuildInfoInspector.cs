// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEditor;

namespace XRTK.Editor.BuildPipeline
{
    [CustomEditor(typeof(BuildInfo), true)]
    public class BuildInfoInspector : UnityEditor.Editor
    {
        private SerializedProperty autoIncrement;
        private SerializedProperty bundleIdentifier;
        private SerializedProperty install;

        protected BuildInfo buildInfo;

        protected virtual void OnEnable()
        {
            autoIncrement = serializedObject.FindProperty(nameof(autoIncrement));
            bundleIdentifier = serializedObject.FindProperty(nameof(bundleIdentifier));
            install = serializedObject.FindProperty(nameof(install));

            buildInfo = (BuildInfo)target;
        }

        /// <inheritdoc />
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(bundleIdentifier);

            if (EditorGUI.EndChangeCheck())
            {
                var buildTargetGroup = UnityEditor.BuildPipeline.GetBuildTargetGroup(buildInfo.BuildTarget);
                PlayerSettings.SetApplicationIdentifier(buildTargetGroup, bundleIdentifier.stringValue);
            }

            EditorGUILayout.PropertyField(autoIncrement);
            EditorGUILayout.PropertyField(install);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
