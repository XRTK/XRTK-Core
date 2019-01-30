﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using XRTK.Services;

namespace XRTK.Utilities
{
    /// <summary>
    /// Helper class for setting up canvases for use in the XRTK.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Canvas))]
    public class CanvasUtility : MonoBehaviour
    {
        [SerializeField]
        [HideInInspector]
        private Canvas canvas;

        /// <summary>
        /// The canvas this helper script is targeting.
        /// </summary>
        public Canvas Canvas
        {
            get => canvas;
            set => canvas = value;
        }

        private void Awake()
        {
            if (Canvas == null)
            {
                Canvas = GetComponent<Canvas>();
            }
        }

        private void Start()
        {
            Debug.Assert(Canvas != null);

            if (MixedRealityToolkit.Instance.ActiveProfile.IsInputSystemEnabled &&
                Canvas.isRootCanvas && Canvas.renderMode == RenderMode.WorldSpace)
            {
                Canvas.worldCamera = MixedRealityToolkit.InputSystem?.FocusProvider?.UIRaycastCamera;
            }
        }
    }
}
