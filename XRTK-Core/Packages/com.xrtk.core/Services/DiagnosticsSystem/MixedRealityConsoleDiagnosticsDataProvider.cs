﻿// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using XRTK.Definitions;
using XRTK.Interfaces.DiagnosticsSystem;

namespace XRTK.Services.DiagnosticsSystem
{
    /// <summary>
    /// Console diagnostics data providers mirrors the Unity console and digests logs so the
    /// diagnostics system can work with it.
    /// </summary>
    public class MixedRealityConsoleDiagnosticsDataProvider : BaseMixedRealityDiagnosticsDataProvider
    {
        /// <inheritdoc />
        public MixedRealityConsoleDiagnosticsDataProvider(string name, uint priority, BaseMixedRealityProfile profile, IMixedRealityDiagnosticsSystem parentService)
            : base(name, priority, profile, parentService)
        {
        }

        #region IMixedRealityServce Implementation

        /// <inheritdoc />
        public override void Enable()
        {
            base.Enable();
            Application.logMessageReceived += MixedRealityToolkit.DiagnosticsSystem.RaiseLogReceived;
        }

        /// <inheritdoc />
        public override void Disable()
        {
            base.Disable();

            if (MixedRealityToolkit.DiagnosticsSystem != null)
            {
                Application.logMessageReceived -= MixedRealityToolkit.DiagnosticsSystem.RaiseLogReceived;
            }
        }

        #endregion IMixedRealityServce Implementation
    }
}