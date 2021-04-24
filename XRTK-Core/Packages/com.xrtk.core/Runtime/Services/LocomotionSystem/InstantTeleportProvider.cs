﻿// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using XRTK.EventDatum.Teleport;

namespace XRTK.Services.LocomotionSystem
{
    /// <summary>
    /// This <see cref="Interfaces.LocomotionSystem.IMixedRealityTeleportProvider"/> implementation will
    /// instantly teleport to the target location.
    /// </summary>
    [System.Runtime.InteropServices.Guid("34f7d57f-c3d2-448b-83cf-0ccccca4306c")]
    public class InstantTeleportProvider : BaseTeleportProvider
    {
        /// <inheritdoc />
        public override void OnTeleportStarted(TeleportEventData eventData)
        {
            var teleportTransform = LocomotionTarget.parent;

            var targetRotation = Vector3.zero;
            var targetPosition = eventData.Pointer.Result.EndPoint;
            targetRotation.y = eventData.Pointer.PointerOrientation;

            if (eventData.HotSpot != null)
            {
                targetPosition = eventData.HotSpot.Position;
                if (eventData.HotSpot.OverrideTargetOrientation)
                {
                    targetRotation.y = eventData.HotSpot.TargetOrientation;
                }
            }

            var height = targetPosition.y;
            targetPosition -= LocomotionTarget.position - teleportTransform.position;
            targetPosition.y = height;
            teleportTransform.position = targetPosition;
            teleportTransform.RotateAround(LocomotionTarget.position, Vector3.up, targetRotation.y - LocomotionTarget.eulerAngles.y);

            LocomotionSystem.RaiseTeleportComplete(eventData.Pointer, eventData.HotSpot);
        }
    }
}
