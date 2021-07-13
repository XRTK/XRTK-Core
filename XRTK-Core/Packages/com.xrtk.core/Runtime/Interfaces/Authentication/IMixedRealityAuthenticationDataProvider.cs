// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using XRTK.Definitions.Authentication;

namespace XRTK.Interfaces.Authentication
{
    /// <summary>
    /// Interface contract for specific identity provider implementations for use in the <see cref="IMixedRealityAuthenticationSystem"/>.
    /// </summary>
    public interface IMixedRealityAuthenticationDataProvider : IMixedRealityDataProvider
    {
        /// <summary>
        /// Event called when a user has successfully logged in.
        /// </summary>
        event Action<IAuthenticatedAccount> OnLoggedIn;

        /// <summary>
        /// Event called when a user has logged out.
        /// </summary>
        event Action<IAuthenticatedAccount> OnLoggedOut;

        /// <summary>
        /// The <see cref="IAuthenticatedAccount"/>. Null if no user is logged in.
        /// </summary>
        IAuthenticatedAccount AuthenticatedAccount { get; }

        /// <summary>
        /// Is there currently a valid user logged in with a valid token?
        /// </summary>
        bool IsUserLoggedIn { get; }

        /// <summary>
        /// Start Login task.
        /// </summary>
        /// <remarks>
        /// This may prompt the user to authenticate with the <see cref="AuthenticationDataProviderProfile.LoginPrefab"/>
        /// </remarks>
        /// <returns>Completed <see cref="Task"/>.</returns>
        Task<IAuthenticatedAccount> LoginAsync();

        /// <summary>
        /// Log the user out.
        /// </summary>
        /// <param name="reAuthenticate"></param>
        void Logout(bool reAuthenticate = true);

        /// <summary>
        /// Removes all account tokens from the cache.
        /// </summary>
        void ClearTokenCache();
    }
}
