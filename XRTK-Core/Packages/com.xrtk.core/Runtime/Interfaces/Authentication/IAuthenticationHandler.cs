// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace XRTK.Interfaces.Authentication
{
    public interface IAuthenticationHandler : UnityEngine.EventSystems.IEventSystemHandler
    {
        /// <summary>
        /// Display code flow message.
        /// </summary>
        /// <param name="message"></param>
        void DisplayCodeFlowMessage(string message);
    }
}
