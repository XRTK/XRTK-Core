﻿// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using UnityEditor;
using UnityEngine;
using XRTK.Definitions.InputSystem;
using XRTK.Definitions.Utilities;
using XRTK.Inspectors.Extensions;

namespace XRTK.Inspectors.PropertyDrawers
{
    public class MixedRealityInputActionDropdown
    {
        private static readonly string DefaultGuidString = default(Guid).ToString("N");

        private MixedRealityInputActionsProfile[] allInputActionProfiles;

        public void OnGui(GUIContent content, SerializedProperty property, AxisType axisConstraintFilter = AxisType.None)
        {
            var rect = GUILayoutUtility.GetRect(content, "MiniPullDown", null);
            OnGui(rect, property, content, axisConstraintFilter);
        }

        public void OnGui(Rect rect, SerializedProperty property, GUIContent content, AxisType axisConstraintFilter = AxisType.None)
        {
            var label = EditorGUI.BeginProperty(rect, content, property);

            var id = property.FindPropertyRelative("id");
            var profileGuid = property.FindPropertyRelative("profileGuid");
            var description = property.FindPropertyRelative("description");
            var axisConstraint = property.FindPropertyRelative("axisConstraint");

            // Upgrade old action references
            if (string.IsNullOrWhiteSpace(profileGuid.stringValue))
            {
                profileGuid.stringValue = DefaultGuidString;
            }

            var currentAction = new MixedRealityInputAction(Guid.Parse(profileGuid.stringValue), (uint)id.intValue, description.stringValue, (AxisType)axisConstraint.intValue);

            if (allInputActionProfiles == null)
            {
                allInputActionProfiles = ScriptableObjectExtensions.GetAllInstances<MixedRealityInputActionsProfile>();
            }

            var dropdownMenu = new GenericMenu { allowDuplicateNames = true };
            dropdownMenu.AddItem(new GUIContent("None"), false, data => SetInputAction(MixedRealityInputAction.None), null);

            foreach (var inputActionProfile in allInputActionProfiles)
            {
                foreach (var inputAction in inputActionProfile.InputActions)
                {
                    if (axisConstraintFilter != AxisType.None &&
                        axisConstraintFilter != inputAction.AxisConstraint)
                    {
                        if (inputAction == currentAction)
                        {
                            SetInputAction(MixedRealityInputAction.None);
                        }

                        continue;
                    }

                    // Upgrade old action references
                    if (id.intValue != 0 && profileGuid.stringValue == DefaultGuidString)
                    {
                        AssetDatabase.TryGetGUIDAndLocalFileIdentifier(inputActionProfile, out var guid, out long _);
                        var upgradedAction = new MixedRealityInputAction(Guid.Parse(guid), currentAction.Id, currentAction.Description, currentAction.AxisConstraint);
                        SetInputAction(upgradedAction);
                        AddInputActionItem(inputAction, upgradedAction);
                    }
                    else
                    {
                        AddInputActionItem(inputAction, currentAction);
                    }

                    void AddInputActionItem(MixedRealityInputAction actionItem, MixedRealityInputAction selectedItem)
                    {
                        dropdownMenu.AddItem(
                            new GUIContent(inputAction.Description),
                            actionItem.ProfileGuid == selectedItem.ProfileGuid && actionItem.Id == selectedItem.Id,
                            data => SetInputAction(actionItem),
                            null);
                    }
                }
            }

            void SetInputAction(MixedRealityInputAction inputAction)
            {
                id.intValue = (int)inputAction.Id;
                description.stringValue = inputAction.Description;
                axisConstraint.intValue = (int)inputAction.AxisConstraint;
                profileGuid.stringValue = inputAction.ProfileGuid.ToString("N");
                property.serializedObject.ApplyModifiedProperties();
                GUI.changed = currentAction != inputAction;
            }

            var prefix = EditorGUI.PrefixLabel(rect, label);

            if (EditorGUI.DropdownButton(prefix, new GUIContent(description.stringValue), FocusType.Passive))
            {
                dropdownMenu.DropDown(prefix);
            }

            EditorGUI.EndProperty();
        }
    }
}