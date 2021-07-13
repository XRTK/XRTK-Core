// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace XRTK.Interfaces.Authentication
{
    /// <summary>
    /// Represents an authenticated user account
    /// </summary>
    public interface IAuthenticatedAccount
    {
        /// <summary>
        /// The username of the authenticated account
        /// </summary>
        string Username { get; }

        /// <summary>
        /// The access token required for secure access to an online service
        /// </summary>
        string AccessToken { get; }

        /// <summary>
        /// Gets the <see cref="IMixedRealityAuthenticationDataProvider"/> that this account identity is associated with.
        /// </summary>
        IMixedRealityAuthenticationDataProvider Provider { get; }
    }
}
