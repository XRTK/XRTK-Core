﻿using UnityEditor;
using UnityEngine;
using XRTK.Attributes;

namespace XRTK.Inspectors.PropertyDrawers
{
    /// <summary>
    /// Custom property drawer for <see cref="PrefabAttribute"/> decorated <see cref="GameObject"/> values rendered in the inspector.
    /// </summary>
    [CustomPropertyDrawer(typeof(PrefabAttribute))]
    public class PrefabPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var prefabAttribute = attribute as PrefabAttribute;
            Debug.Assert(prefabAttribute != null);

            if (property.propertyType == SerializedPropertyType.ObjectReference &&
               (property.objectReferenceValue is GameObject || property.objectReferenceValue == null))
            {
                EditorGUI.BeginChangeCheck();
                EditorGUI.PropertyField(position, property);

                if (!EditorGUI.EndChangeCheck()) { return; }
                if (property.objectReferenceValue == null) { return; }

                PrefabAssetType prefabAssetType = PrefabUtility.GetPrefabAssetType(property.objectReferenceValue);

                if (prefabAssetType != PrefabAssetType.Regular && prefabAssetType != PrefabAssetType.Variant)
                {
                    property.objectReferenceValue = null;
                    Debug.LogWarning("Assigned GameObject must be a prefab");
                }

                if (prefabAttribute.Constraint != null)
                {
                    var prefabObject = property.objectReferenceValue as GameObject;
                    Debug.Assert(prefabObject != null);
                    var constraint = prefabObject.GetComponent(prefabAttribute.Constraint);

                    if (constraint == null)
                    {
                        property.objectReferenceValue = null;
                        Debug.LogWarning($"Assigned GameObject must have {prefabAttribute.Constraint.Name} component");
                    }
                }
            }
            else
            {
                EditorGUI.LabelField(position, label.text, "Use PrefabAttribute with GameObject fields only.");
            }
        }
    }
}