﻿// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using XRTK.Definitions.Controllers.Simulation.Hands;
using XRTK.Interfaces.InputSystem.Controllers.Hands;

namespace XRTK.Providers.Controllers.Simulation.Hands
{
    /// <summary>
    /// Hand controller type for simulated hand controllers.
    /// </summary>
    public class SimulatedHandControllerDataProvider : SimulatedControllerDataProvider, ISimulatedHandControllerDataProvider
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">Name of the data provider as assigned in the configuration profile.</param>
        /// <param name="priority">Data provider priority controls the order in the service registry.</param>
        /// <param name="profile">Controller data provider profile assigned to the provider instance in the configuration inspector.</param>
        public SimulatedHandControllerDataProvider(string name, uint priority, SimulatedHandControllerDataProviderProfile profile)
            : base(name, priority, profile)
        {
            var poseDefinitions = profile.PoseDefinitions;

            handPoseDefinitions = new List<SimulatedHandControllerPoseData>(poseDefinitions.Count);

            foreach (var poseDefinition in poseDefinitions)
            {
                handPoseDefinitions.Add(poseDefinition);
            }

            HandPoseAnimationSpeed = profile.HandPoseAnimationSpeed;
        }

        private readonly List<SimulatedHandControllerPoseData> handPoseDefinitions;

        /// <inheritdoc />
        public IReadOnlyList<SimulatedHandControllerPoseData> HandPoseDefinitions => handPoseDefinitions;

        /// <inheritdoc />
        public float HandPoseAnimationSpeed { get; }
    }
}