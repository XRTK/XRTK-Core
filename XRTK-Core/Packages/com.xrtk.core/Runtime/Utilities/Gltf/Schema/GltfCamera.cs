﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using UnityEngine;

namespace XRTK.Utilities.Gltf.Schema
{
    /// <summary>
    /// A camera's projection.  A node can reference a camera to apply a transform
    /// to place the camera in the scene
    /// https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/schema/camera.schema.json
    /// </summary>
    [Serializable]
    public class GltfCamera : GltfChildOfRootProperty, ISerializationCallbackReceiver
    {
        /// <summary>
        /// An orthographic camera containing properties to create an orthographic
        /// projection matrix.
        /// </summary>
        public GltfCameraOrthographic orthographic;

        /// <summary>
        /// A perspective camera containing properties to create a perspective
        /// projection matrix.
        /// </summary>
        public GltfCameraPerspective perspective;

        /// <summary>
        /// Specifies if the camera uses a perspective or orthographic projection.
        /// Based on this, either the camera's `perspective` or `orthographic` property
        /// will be defined.
        /// </summary>
        public GltfCameraType Type { get; set; }

        [SerializeField]
        private string type = string.Empty;

#region ISerializationCallbackReceiver

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            if (Enum.TryParse(type, out GltfCameraType result))
            {
                Type = result;
            }
            else
            {
                Type = default;
            }
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            type = Type.ToString();
        }

#endregion ISerializationCallbackReceiver
    }
}