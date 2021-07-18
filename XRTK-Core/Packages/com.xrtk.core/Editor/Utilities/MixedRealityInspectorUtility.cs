// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace XRTK.Editor.Utilities
{
    /// <summary>
    /// This class has handy inspector utilities and functions.
    /// </summary>
    public static class MixedRealityInspectorUtility
    {
        public const float DottedLineScreenSpace = 4.65f;

        /// <summary>
        /// Found at https://answers.unity.com/questions/960413/editor-window-how-to-center-a-window.html
        /// </summary>
        public static Rect GetEditorMainWindowPos()
        {
            var containerWinType = AppDomain.CurrentDomain.GetAllDerivedTypes(typeof(ScriptableObject)).FirstOrDefault(type => type.Name == "ContainerWindow");

            if (containerWinType == null)
            {
                throw new MissingMemberException("Can't find internal type ContainerWindow. Maybe something has changed inside Unity");
            }

            var showModeField = containerWinType.GetField("m_ShowMode", BindingFlags.NonPublic | BindingFlags.Instance);
            var positionProperty = containerWinType.GetProperty("position", BindingFlags.Public | BindingFlags.Instance);

            if (showModeField == null || positionProperty == null)
            {
                throw new MissingFieldException("Can't find internal fields 'm_ShowMode' or 'position'. Maybe something has changed inside Unity");
            }

            var windows = Resources.FindObjectsOfTypeAll(containerWinType);

            foreach (var win in windows)
            {
                var showMode = (int)showModeField.GetValue(win);
                if (showMode == 4) // main window
                {
                    var pos = (Rect)positionProperty.GetValue(win, null);
                    return pos;
                }
            }

            throw new NotSupportedException("Can't find internal main window. Maybe something has changed inside Unity");
        }

        private static IEnumerable<Type> GetAllDerivedTypes(this AppDomain appDomain, Type aType)
        {
            var result = new List<Type>();
            var assemblies = appDomain.GetAssemblies();

            for (var i = 0; i < assemblies.Length; i++)
            {
                result.AddRange(assemblies[i].GetTypes().Where(type => type.IsSubclassOf(aType)));
            }

            return result;
        }

        /// <summary>
        /// Centers an editor window on the main display.
        /// </summary>
        /// <param name="window"></param>
        public static void CenterOnMainWin(this EditorWindow window)
        {
            var main = GetEditorMainWindowPos();
            var pos = window.position;
            float w = (main.width - pos.width) * 0.5f;
            float h = (main.height - pos.height) * 0.5f;
            pos.x = main.x + w;
            pos.y = main.y + h;
            window.position = pos;
        }

        #region Styles

        private static GUIStyle boldCenteredHeaderStyle = null;

        public static GUIStyle BoldCenteredHeaderStyle
        {
            get
            {
                if (boldCenteredHeaderStyle == null)
                {
                    var editorStyle = EditorGUIUtility.isProSkin ? EditorStyles.whiteLargeLabel : EditorStyles.largeLabel;

                    if (editorStyle != null)
                    {
                        boldCenteredHeaderStyle = new GUIStyle(editorStyle)
                        {
                            alignment = TextAnchor.MiddleCenter,
                            fontSize = 18,
                            padding = new RectOffset(0, 0, -8, -8)
                        };
                    }
                }

                return boldCenteredHeaderStyle;
            }
        }

        #endregion

        #region Logos

        public static Texture2D DarkThemeLogo
        {
            get
            {
                if (darkThemeLogo == null)
                {
                    darkThemeLogo = (Texture2D)AssetDatabase.LoadAssetAtPath($"{PathFinderUtility.XRTK_Core_RelativeFolderPath}/Runtime/StandardAssets/Textures/XRTK_Logo.png", typeof(Texture2D));
                }

                return darkThemeLogo;
            }
        }

        private static Texture2D darkThemeLogo = null;

        public static Texture2D LightThemeLogo
        {
            get
            {
                if (lightThemeLogo == null)
                {
                    lightThemeLogo = (Texture2D)AssetDatabase.LoadAssetAtPath($"{PathFinderUtility.XRTK_Core_RelativeFolderPath}/Runtime/StandardAssets/Textures/XRTK_Logo.png", typeof(Texture2D));
                }

                return lightThemeLogo;
            }
        }

        private static Texture2D lightThemeLogo = null;

        private static GUIStyle centeredGuiStyle;

        private static GUIStyle CenteredGuiStyle => centeredGuiStyle ?? (centeredGuiStyle = new GUIStyle { alignment = TextAnchor.MiddleCenter });

        /// <summary>
        /// Render the Mixed Reality Toolkit Logo.
        /// </summary>
        public static void RenderMixedRealityToolkitLogo()
        {
            RenderInspectorHeader(EditorGUIUtility.isProSkin ? DarkThemeLogo : LightThemeLogo);
        }

        /// <summary>
        /// Render a header for the profile inspector.
        /// </summary>
        /// <param name="image"></param>
        public static void RenderInspectorHeader(Texture2D image)
        {
            GUILayout.Label(image, CenteredGuiStyle, GUILayout.MaxHeight(128f));
            GUILayout.Space(12f);
        }

        #endregion Logos

        #region Gizmos

        /// <summary>
        /// Renders a capsule wire gizmo.
        /// </summary>
        /// <param name="position">Center position of the capsule.</param>
        /// <param name="rotation">Center rotation of the capsule.</param>
        /// <param name="radius">Capsule radius.</param>
        /// <param name="height">Capsule height.</param>
        /// <param name="color">Optional color override, will use <see cref="Gizmos.color"/> by default.</param>
        public static void DrawWireCapsule(Vector3 position, Quaternion rotation, float radius, float height, Color color = default)
        {
            if (color != default)
            {
                Handles.color = color;
            }
            else
            {
                Handles.color = Gizmos.color;
            }

            Matrix4x4 angleMatrix = Matrix4x4.TRS(position, rotation, Handles.matrix.lossyScale);

            using (new Handles.DrawingScope(angleMatrix))
            {
                var pointOffset = (height - (radius * 2)) / 2;

                // Draw sideways
                Handles.DrawWireArc(Vector3.up * pointOffset, Vector3.left, Vector3.back, -180, radius);
                Handles.DrawLine(new Vector3(0, pointOffset, -radius), new Vector3(0, -pointOffset, -radius));
                Handles.DrawLine(new Vector3(0, pointOffset, radius), new Vector3(0, -pointOffset, radius));
                Handles.DrawWireArc(Vector3.down * pointOffset, Vector3.left, Vector3.back, 180, radius);

                // Draw frontways
                Handles.DrawWireArc(Vector3.up * pointOffset, Vector3.back, Vector3.left, 180, radius);
                Handles.DrawLine(new Vector3(-radius, pointOffset, 0), new Vector3(-radius, -pointOffset, 0));
                Handles.DrawLine(new Vector3(radius, pointOffset, 0), new Vector3(radius, -pointOffset, 0));
                Handles.DrawWireArc(Vector3.down * pointOffset, Vector3.back, Vector3.left, -180, radius);

                // Draw center
                Handles.DrawWireDisc(Vector3.up * pointOffset, Vector3.up, radius);
                Handles.DrawWireDisc(Vector3.down * pointOffset, Vector3.up, radius);

            }
        }

        #endregion

        #region Colors

        public static readonly Color DisabledColor = new Color(0.6f, 0.6f, 0.6f);
        public static readonly Color WarningColor = new Color(1f, 0.85f, 0.6f);
        public static readonly Color ErrorColor = new Color(1f, 0.55f, 0.5f);
        public static readonly Color SuccessColor = new Color(0.8f, 1f, 0.75f);
        public static readonly Color SectionColor = new Color(0.85f, 0.9f, 1f);
        public static readonly Color DarkColor = new Color(0.1f, 0.1f, 0.1f);
        public static readonly Color HandleColorSquare = new Color(0.0f, 0.9f, 1f);
        public static readonly Color HandleColorCircle = new Color(1f, 0.5f, 1f);
        public static readonly Color HandleColorSphere = new Color(1f, 0.5f, 1f);
        public static readonly Color HandleColorAxis = new Color(0.0f, 1f, 0.2f);
        public static readonly Color HandleColorRotation = new Color(0.0f, 1f, 0.2f);
        public static readonly Color HandleColorTangent = new Color(0.1f, 0.8f, 0.5f, 0.7f);
        public static readonly Color LineVelocityColor = new Color(0.9f, 1f, 0f, 0.8f);

        #endregion Colors

        #region Handles

        /// <summary>
        /// Draw an axis move handle.
        /// </summary>
        /// <param name="target"><see cref="UnityEngine.Object"/> that is undergoing the transformation. Also used for recording undo.</param>
        /// <param name="origin">The initial position of the axis.</param>
        /// <param name="direction">The direction the axis is facing.</param>
        /// <param name="distance">Distance from the axis.</param>
        /// <param name="handleSize">Optional handle size.</param>
        /// <param name="autoSize">Optional, auto sizes the handles based on position and handle size.</param>
        /// <param name="recordUndo">Optional, records undo state.</param>
        /// <returns>The new <see cref="float"/> value.</returns>
        public static float AxisMoveHandle(Object target, Vector3 origin, Vector3 direction, float distance, float handleSize = 0.2f, bool autoSize = true, bool recordUndo = true)
        {
            Vector3 position = origin + (direction.normalized * distance);

            Handles.color = HandleColorAxis;

            if (autoSize)
            {
                handleSize = Mathf.Lerp(handleSize, HandleUtility.GetHandleSize(position) * handleSize, 0.75f);
            }

            Handles.DrawDottedLine(origin, position, DottedLineScreenSpace);
            Handles.ArrowHandleCap(0, position, Quaternion.LookRotation(direction), handleSize * 2, EventType.Repaint);
            Vector3 newPosition = Handles.FreeMoveHandle(position, Quaternion.identity, handleSize, Vector3.zero, Handles.CircleHandleCap);

            if (recordUndo)
            {
                float newDistance = Vector3.Distance(origin, newPosition);

                if (!distance.Equals(newDistance))
                {
                    Undo.RegisterCompleteObjectUndo(target, target.name);
                    distance = newDistance;
                }
            }

            return distance;
        }

        /// <summary>
        /// Draw a Circle Move Handle.
        /// </summary>
        /// <param name="target"><see cref="Object"/> that is undergoing the transformation. Also used for recording undo.</param>
        /// <param name="position">The position to draw the handle.</param>
        /// <param name="xScale">Scale the new value on the x axis by this amount.</param>
        /// <param name="yScale">Scale the new value on the x axis by this amount.</param>
        /// <param name="zScale">Scale the new value on the x axis by this amount.</param>
        /// <param name="handleSize">Optional handle size.</param>
        /// <param name="autoSize">Optional, auto sizes the handles based on position and handle size.</param>
        /// <param name="recordUndo">Optional, records undo state.</param>
        /// <returns>The new <see cref="Vector3"/> value.</returns>
        public static Vector3 CircleMoveHandle(Object target, Vector3 position, float xScale = 1f, float yScale = 1f, float zScale = 1f, float handleSize = 0.2f, bool autoSize = true, bool recordUndo = true)
        {
            Handles.color = HandleColorCircle;

            if (autoSize)
            {
                handleSize = Mathf.Lerp(handleSize, HandleUtility.GetHandleSize(position) * handleSize, 0.75f);
            }

            Vector3 newPosition = Handles.FreeMoveHandle(position, Quaternion.identity, handleSize, Vector3.zero, Handles.CircleHandleCap);

            if (recordUndo && position != newPosition)
            {
                Undo.RegisterCompleteObjectUndo(target, target.name);

                position.x = Mathf.Lerp(position.x, newPosition.x, Mathf.Clamp01(xScale));
                position.y = Mathf.Lerp(position.z, newPosition.y, Mathf.Clamp01(yScale));
                position.z = Mathf.Lerp(position.y, newPosition.z, Mathf.Clamp01(zScale));
            }

            return position;
        }

        /// <summary>
        /// Draw a square move handle.
        /// </summary>
        /// <param name="target"><see cref="Object"/> that is undergoing the transformation. Also used for recording undo.</param>
        /// <param name="position">The position to draw the handle.</param>
        /// <param name="xScale">Scale the new value on the x axis by this amount.</param>
        /// <param name="yScale">Scale the new value on the x axis by this amount.</param>
        /// <param name="zScale">Scale the new value on the x axis by this amount.</param>
        /// <param name="handleSize">Optional handle size.</param>
        /// <param name="autoSize">Optional, auto sizes the handles based on position and handle size.</param>
        /// <param name="recordUndo">Optional, records undo state.</param>
        /// <returns>The new <see cref="Vector3"/> value.</returns>
        public static Vector3 SquareMoveHandle(Object target, Vector3 position, float xScale = 1f, float yScale = 1f, float zScale = 1f, float handleSize = 0.2f, bool autoSize = true, bool recordUndo = true)
        {
            Handles.color = HandleColorSquare;

            if (autoSize)
            {
                handleSize = Mathf.Lerp(handleSize, HandleUtility.GetHandleSize(position) * handleSize, 0.75f);
            }

            // Multiply square handle to match other types
            Vector3 newPosition = Handles.FreeMoveHandle(position, Quaternion.identity, handleSize * 0.8f, Vector3.zero, Handles.RectangleHandleCap);

            if (recordUndo && position != newPosition)
            {
                Undo.RegisterCompleteObjectUndo(target, target.name);

                position.x = Mathf.Lerp(position.x, newPosition.x, Mathf.Clamp01(xScale));
                position.y = Mathf.Lerp(position.z, newPosition.y, Mathf.Clamp01(yScale));
                position.z = Mathf.Lerp(position.y, newPosition.z, Mathf.Clamp01(zScale));
            }

            return position;
        }

        /// <summary>
        /// Draw a sphere move handle.
        /// </summary>
        /// <param name="target"><see cref="Object"/> that is undergoing the transformation. Also used for recording undo.</param>
        /// <param name="position">The position to draw the handle.</param>
        /// <param name="xScale">Scale the new value on the x axis by this amount.</param>
        /// <param name="yScale">Scale the new value on the x axis by this amount.</param>
        /// <param name="zScale">Scale the new value on the x axis by this amount.</param>
        /// <param name="handleSize">Optional handle size.</param>
        /// <param name="autoSize">Optional, auto sizes the handles based on position and handle size.</param>
        /// <param name="recordUndo">Optional, records undo state.</param>
        /// <returns>The new <see cref="Vector3"/> value.</returns>
        public static Vector3 SphereMoveHandle(Object target, Vector3 position, float xScale = 1f, float yScale = 1f, float zScale = 1f, float handleSize = 0.2f, bool autoSize = true, bool recordUndo = true)
        {
            Handles.color = HandleColorSphere;

            if (autoSize)
            {
                handleSize = Mathf.Lerp(handleSize, HandleUtility.GetHandleSize(position) * handleSize, 0.75f);
            }

            // Multiply sphere handle size to match other types
            Vector3 newPosition = Handles.FreeMoveHandle(position, Quaternion.identity, handleSize * 2, Vector3.zero, Handles.SphereHandleCap);

            if (recordUndo && position != newPosition)
            {
                Undo.RegisterCompleteObjectUndo(target, target.name);

                position.x = Mathf.Lerp(position.x, newPosition.x, Mathf.Clamp01(xScale));
                position.y = Mathf.Lerp(position.z, newPosition.y, Mathf.Clamp01(yScale));
                position.z = Mathf.Lerp(position.y, newPosition.z, Mathf.Clamp01(zScale));
            }

            return position;
        }

        /// <summary>
        /// Draw a vector handle.
        /// </summary>
        /// <param name="target"><see cref="Object"/> that is undergoing the transformation. Also used for recording undo.</param>
        /// <param name="origin"></param>
        /// <param name="vector"></param>
        /// <param name="normalize">Optional, Normalize the new vector value.</param>
        /// <param name="clamp">Optional, Clamp new vector's value based on the distance to the origin.</param>
        /// <param name="handleLength">Optional, handle length.</param>
        /// <param name="handleSize">Optional, handle size.</param>
        /// <param name="autoSize">Optional, auto sizes the handles based on position and handle size.</param>
        /// <param name="recordUndo">Optional, records undo state.</param>
        /// <returns>The new <see cref="Vector3"/> value.</returns>
        public static Vector3 VectorHandle(Object target, Vector3 origin, Vector3 vector, bool normalize = true, bool clamp = true, float handleLength = 1f, float handleSize = 0.1f, bool recordUndo = true, bool autoSize = true)
        {
            Handles.color = HandleColorTangent;

            if (autoSize)
            {
                handleSize = Mathf.Lerp(handleSize, HandleUtility.GetHandleSize(origin) * handleSize, 0.75f);
            }

            Vector3 handlePosition = origin + (vector * handleLength);
            float distanceToOrigin = Vector3.Distance(origin, handlePosition) / handleLength;

            if (normalize)
            {
                vector.Normalize();
            }
            else
            {
                // If the handle isn't normalized, brighten based on distance to origin
                Handles.color = Color.Lerp(Color.gray, HandleColorTangent, distanceToOrigin * 0.85f);

                if (clamp)
                {
                    // To indicate that we're at the clamped limit, make the handle 'pop' slightly larger
                    if (distanceToOrigin >= 0.98f)
                    {
                        Handles.color = Color.Lerp(HandleColorTangent, Color.white, 0.5f);
                        handleSize *= 1.5f;
                    }
                }
            }

            // Draw a line from origin to origin + direction
            Handles.DrawLine(origin, handlePosition);

            Quaternion rotation = Quaternion.identity;
            if (vector != Vector3.zero)
            {
                rotation = Quaternion.LookRotation(vector);
            }

            Vector3 newPosition = Handles.FreeMoveHandle(handlePosition, rotation, handleSize, Vector3.zero, Handles.DotHandleCap);

            if (recordUndo && handlePosition != newPosition)
            {
                Undo.RegisterCompleteObjectUndo(target, target.name);
                vector = (newPosition - origin).normalized;

                // If we normalize, we're done
                // Otherwise, multiply the vector by the distance between origin and target
                if (!normalize)
                {
                    distanceToOrigin = Vector3.Distance(origin, newPosition) / handleLength;

                    if (clamp)
                    {
                        distanceToOrigin = Mathf.Clamp01(distanceToOrigin);
                    }

                    vector *= distanceToOrigin;
                }
            }

            return vector;
        }

        /// <summary>
        /// Draw a rotation handle.
        /// </summary>
        /// <param name="target"><see cref="Object"/> that is undergoing the transformation. Also used for recording undo.</param>
        /// <param name="position">The position to draw the handle.</param>
        /// <param name="rotation">The rotation to draw the handle.</param>
        /// <param name="handleSize">Optional, handle size.</param>
        /// <param name="autoSize">Optional, auto sizes the handles based on position and handle size.</param>
        /// <param name="recordUndo">Optional, records undo state.</param>
        /// <returns>The new <see cref="Quaternion"/> value.</returns>
        public static Quaternion RotationHandle(Object target, Vector3 position, Quaternion rotation, float handleSize = 0.2f, bool autoSize = true, bool recordUndo = true)
        {
            Handles.color = HandleColorRotation;

            if (autoSize)
            {
                handleSize = Mathf.Lerp(handleSize, HandleUtility.GetHandleSize(position) * handleSize, 0.75f);
            }

            // Make rotation handles larger so they can overlay movement handles
            Quaternion newRotation = Handles.FreeRotateHandle(rotation, position, handleSize * 2);

            if (recordUndo)
            {
                Handles.color = Handles.zAxisColor;
                Handles.ArrowHandleCap(0, position, Quaternion.LookRotation(newRotation * Vector3.forward), handleSize * 2, EventType.Repaint);
                Handles.color = Handles.xAxisColor;
                Handles.ArrowHandleCap(0, position, Quaternion.LookRotation(newRotation * Vector3.right), handleSize * 2, EventType.Repaint);
                Handles.color = Handles.yAxisColor;
                Handles.ArrowHandleCap(0, position, Quaternion.LookRotation(newRotation * Vector3.up), handleSize * 2, EventType.Repaint);

                if (rotation != newRotation)
                {
                    Undo.RegisterCompleteObjectUndo(target, target.name);
                    rotation = newRotation;
                }
            }

            return rotation;
        }

        #endregion Handles
    }
}
