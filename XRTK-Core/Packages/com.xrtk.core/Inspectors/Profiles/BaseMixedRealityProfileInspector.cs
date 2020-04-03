﻿// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.﻿

using UnityEditor;
using UnityEngine;
using XRTK.Definitions;
using XRTK.Inspectors.Extensions;
using XRTK.Inspectors.Utilities;
using XRTK.Services;
using XRTK.Utilities.Async;

namespace XRTK.Inspectors.Profiles
{
    /// <summary>
    /// Base class for all <see cref="BaseMixedRealityProfile"/> Inspectors to inherit from.
    /// </summary>
    public abstract class BaseMixedRealityProfileInspector : Editor
    {
        private const string IsEditableProfileProperty = "isEditable";

        private static SerializedObject targetProfile;
        private static BaseMixedRealityProfile currentlySelectedProfile;
        private static BaseMixedRealityProfile profileSource;

        protected BaseMixedRealityProfile ThisProfile { get; private set; }

        protected virtual void OnEnable()
        {
            targetProfile = serializedObject;
            currentlySelectedProfile = target as BaseMixedRealityProfile;
            Debug.Assert(currentlySelectedProfile != null);
            ThisProfile = currentlySelectedProfile;
        }

        protected void RenderHeader()
        {
            MixedRealityInspectorUtility.RenderMixedRealityToolkitLogo();

            if (ThisProfile.ParentProfile != null &&
                GUILayout.Button("Back to parent profile"))
            {
                Selection.activeObject = ThisProfile.ParentProfile;
            }

            EditorGUILayout.Space();
        }

        [MenuItem("CONTEXT/BaseMixedRealityProfile/Create Clone from Profile Values", false, 0)]
        private static async void CreateCloneProfile()
        {
            profileSource = currentlySelectedProfile;
            var newProfile = CreateInstance(currentlySelectedProfile.GetType().ToString());
            currentlySelectedProfile = newProfile.CreateAsset() as BaseMixedRealityProfile;
            Debug.Assert(currentlySelectedProfile != null);

            await new WaitUntil(() => profileSource != currentlySelectedProfile);

            Selection.activeObject = null;
            PasteProfileValues();
            Selection.activeObject = currentlySelectedProfile;
            EditorGUIUtility.PingObject(currentlySelectedProfile);

            if (!profileSource.IsEditable)
            {
                // For now we only replace it if it's the master settings profile.
                // Sub-profiles are easy to update in the master settings inspector.
                if (currentlySelectedProfile is MixedRealityToolkitRootProfile rootProfile)
                {
                    MixedRealityToolkit.Instance.ActiveProfile = rootProfile;
                }
            }
        }

        [MenuItem("CONTEXT/BaseMixedRealityProfile/Copy Profile Values", false, 1)]
        private static void CopyProfileValues()
        {
            profileSource = currentlySelectedProfile;
        }

        [MenuItem("CONTEXT/BaseMixedRealityProfile/Paste Profile Values", true)]
        private static bool PasteProfileValuesValidation()
        {
            return currentlySelectedProfile != null &&
                   targetProfile != null &&
                   profileSource != null &&
                   targetProfile.FindProperty(IsEditableProfileProperty).boolValue &&
                   currentlySelectedProfile.GetType() == profileSource.GetType();
        }

        [MenuItem("CONTEXT/BaseMixedRealityProfile/Paste Profile Values", false, 2)]
        private static void PasteProfileValues()
        {
            currentlySelectedProfile.CopySerializedValues(profileSource);
        }

        private static async void PasteProfileValuesDelay(BaseMixedRealityProfile newProfile)
        {
            await new WaitUntil(() => currentlySelectedProfile == newProfile);
            Selection.activeObject = null;
            PasteProfileValues();
            Selection.activeObject = newProfile;
        }
    }
}
