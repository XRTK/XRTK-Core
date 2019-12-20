﻿// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using XRTK.Definitions.Utilities;

namespace XRTK.Definitions.Diagnostics
{
    /// <summary>
    /// Configuration profile settings for setting up a diagnostics data provider.
    /// </summary>
    [CreateAssetMenu(menuName = "Mixed Reality Toolkit/Diagnostics System/Diagnostics Data Provider Profile", fileName = "MixedRealityDiagnosticsDataProviderProfile", order = (int)CreateProfileMenuItemIndices.Diagnostics)]
    public class MixedRealityDiagnosticsDataProviderProfile : BaseMixedRealityProfile
    {
        #region General Settings

        [SerializeField]
        [Tooltip("The prefab instantiated to visualize diagnostics data.")]
        private GameObject visualizationPrefab;

        /// <summary>
        /// The prefab instantiated to visualize diagnostics data.
        /// </summary>
        public GameObject VisualizationPrefab
        {
            get => visualizationPrefab;
            private set => visualizationPrefab = value;
        }

        #endregion

        #region Profiler Diagnostics Settings

        [SerializeField]
        [Tooltip("Display profiler")]
        private bool showProfiler = true;

        /// <summary>
        /// Show or hide the profiler UI.
        /// </summary>
        public bool ShowProfiler => showProfiler;

        [SerializeField]
        [Tooltip("The amount of time, in seconds, to collect frames for frame rate calculation.")]
        [Range(0, 5)]
        private float frameSampleRate = 0.1f;

        /// <summary>
        /// The amount of time, in seconds, to collect frames for frame rate calculation.
        /// </summary>
        public float FrameSampleRate => frameSampleRate;

        [Range(0, 3)]
        [SerializeField]
        [Tooltip("How many decimal places to display on numeric strings.")]
        private int displayedDecimalDigits = 1;

        /// <summary>
        /// How many decimal places to display on numeric strings.
        /// </summary>
        public int DisplayedDecimalDigits => displayedDecimalDigits;

        [SerializeField]
        [Tooltip("The color to display on frames which meet or exceed the target frame rate.")]
        private Color targetFrameRateColor = new Color(127 / 256.0f, 186 / 256.0f, 0 / 256.0f, 1.0f);

        /// <summary>
        /// The color to display on frames which meet or exceed the target frame rate.
        /// </summary>
        public Color TargetFrameRateColor => targetFrameRateColor;

        [SerializeField]
        [Tooltip("The color to display on frames which fall below the target frame rate.")]
        private Color missedFrameRateColor = new Color(242 / 256.0f, 80 / 256.0f, 34 / 256.0f, 1.0f);

        /// <summary>
        /// The color to display on frames which fall below the target frame rate.
        /// </summary>
        public Color MissedFrameRateColor => missedFrameRateColor;

        [SerializeField]
        [Tooltip("The color to display for current memory usage values.")]
        private Color memoryUsedColor = new Color(0 / 256.0f, 164 / 256.0f, 239 / 256.0f, 1.0f);

        /// <summary>
        /// The color to display for current memory usage values.
        /// </summary>
        public Color MemoryUsedColor => memoryUsedColor;

        [SerializeField]
        [Tooltip("The color to display for peak (aka max) memory usage values.")]
        private Color memoryPeakColor = new Color(255 / 256.0f, 185 / 256.0f, 0 / 256.0f, 1.0f);

        /// <summary>
        /// The color to display for peak (aka max) memory usage values.
        /// </summary>
        public Color MemoryPeakColor => memoryPeakColor;

        [SerializeField]
        [Tooltip("The color to display for the platforms memory usage limit.")]
        private Color memoryLimitColor = new Color(150 / 256.0f, 150 / 256.0f, 150 / 256.0f, 1.0f);

        /// <summary>
        /// The color to display for the platforms memory usage limit.
        /// </summary>
        public Color MemoryLimitColor => memoryLimitColor;

        #endregion

        #region Console Diagnostics Settings

        [SerializeField]
        [Tooltip("Display profiler")]
        private bool showConsole = true;

        /// <summary>
        /// Show or hide the console UI.
        /// </summary>
        public bool ShowConsole => showConsole;

        #endregion
    }
}