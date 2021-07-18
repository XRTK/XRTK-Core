// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine.XR;

#if UNITY_2020_1_OR_NEWER
using UnityEngine;
using System.Collections.Generic;
#endif

namespace XRTK.Utilities
{
    /// <summary>
    /// Utilities to determine connected device capabilities and state.
    /// </summary>
    public static class XRDeviceUtilities
    {
        /// <summary>
        /// Gets whether an XR display device is currently connected to the machine.
        /// </summary>
        public static bool IsDevicePresent
        {
            get
            {
#if UNITY_2020_1_OR_NEWER
                var xrDisplaySubsystems = new List<XRDisplaySubsystem>();
                SubsystemManager.GetInstances(xrDisplaySubsystems);
                for (int i = 0; i < xrDisplaySubsystems.Count; i++)
                {
                    var xrDisplay = xrDisplaySubsystems[i];
                    if (xrDisplay.running)
                    {
                        return true;
                    }
                }

                return false;
#else
                return XRDevice.isPresent;
#endif // UNITY_2020_1_OR_NEWER
            }
        }

#if UNITY_2020_1_OR_NEWER

        /// <summary>
        /// Gets whether the device has an opaque display.
        /// </summary>
        public static bool IsDisplayOpaque
        {
            get
            {
                var xrDisplaySubsystems = new List<XRDisplaySubsystem>();
                SubsystemManager.GetInstances(xrDisplaySubsystems);
                for (int i = 0; i < xrDisplaySubsystems.Count; i++)
                {
                    var xrDisplay = xrDisplaySubsystems[i];
                    if (xrDisplay.running)
                    {
                        return xrDisplay.displayOpaque;
                    }
                }

                // When no device is attached we are assuming the display
                // device is the computer's display, which should be opaque.
                return true;
            }
        }

#endif // UNITY_2020_1_OR_NEWER
    }
}