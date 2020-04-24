﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using UnityEngine;
using XRTK.Interfaces.Providers.SpatialObservers;
using XRTK.Interfaces.SpatialAwarenessSystem;
using XRTK.Services;

namespace XRTK.Providers.SpatialObservers
{
    /// <summary>
    /// Base <see cref="IMixedRealitySpatialSurfaceObserver"/> implementation.
    /// </summary>
    public abstract class BaseMixedRealitySpatialSurfaceObserver : BaseMixedRealitySpatialObserverDataProvider, IMixedRealitySpatialSurfaceObserver
    {
        /// <inheritdoc />
        protected BaseMixedRealitySpatialSurfaceObserver(string name, uint priority, BaseMixedRealitySurfaceObserverProfile profile, IMixedRealitySpatialAwarenessSystem parentService)
            : base(name, priority, profile, parentService)
        {
            if (profile == null)
            {
                profile = MixedRealityToolkit.Instance.ActiveProfile.SpatialAwarenessProfile.GlobalSurfaceObserverProfile;
            }

            if (profile == null)
            {
                throw new ArgumentNullException($"Missing a {profile.GetType().Name} profile for {name}");
            }

            SurfaceFindingMinimumArea = profile.SurfaceFindingMinimumArea;
            DisplayFloorSurfaces = profile.DisplayFloorSurfaces;
            FloorSurfaceMaterial = profile.FloorSurfaceMaterial;
            DisplayCeilingSurfaces = profile.DisplayCeilingSurface;
            CeilingSurfaceMaterial = profile.CeilingSurfaceMaterial;
            DisplayWallSurfaces = profile.DisplayWallSurface;
            WallSurfaceMaterial = profile.WallSurfaceMaterial;
            DisplayPlatformSurfaces = profile.DisplayPlatformSurfaces;
            PlatformSurfaceMaterial = profile.PlatformSurfaceMaterial;
        }

        /// <inheritdoc />
        public float SurfaceFindingMinimumArea { get; }

        /// <inheritdoc />
        public bool DisplayFloorSurfaces { get; set; }

        /// <inheritdoc />
        public Material FloorSurfaceMaterial { get; }

        /// <inheritdoc />
        public bool DisplayCeilingSurfaces { get; set; }

        /// <inheritdoc />
        public Material CeilingSurfaceMaterial { get; }

        /// <inheritdoc />
        public bool DisplayWallSurfaces { get; set; }

        /// <inheritdoc />
        public Material WallSurfaceMaterial { get; }

        /// <inheritdoc />
        public bool DisplayPlatformSurfaces { get; set; }

        /// <inheritdoc />
        public Material PlatformSurfaceMaterial { get; }

        private readonly Dictionary<int, GameObject> planarSurfaces = new Dictionary<int, GameObject>();

        /// <inheritdoc />
        public IReadOnlyDictionary<int, GameObject> PlanarSurfaces => new Dictionary<int, GameObject>(planarSurfaces);
    }
}