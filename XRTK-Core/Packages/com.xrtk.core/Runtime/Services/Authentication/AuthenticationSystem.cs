// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using UnityEngine;
using XRTK.Definitions.Authentication;
using XRTK.Interfaces.Authentication;

namespace XRTK.Services.Authentication
{
    /// <summary>
    /// Concrete implementation of the <see cref="IMixedRealityAuthenticationSystem"/>
    /// </summary>
    [System.Runtime.InteropServices.Guid("B9AA44EA-23C9-4B4A-8DE6-D598E6F638FB")]
    public class AuthenticationSystem : BaseSystem, IMixedRealityAuthenticationSystem
    {
        /// <inheritdoc />
        public AuthenticationSystem(AuthenticationSystemProfile profile) : base(profile)
        {
            CacheUserTokens = profile.CacheUserTokens;
        }

        #region IMixedRealityAuthenticationSystem Implementation

        /// <inheritdoc />
        public event Action<IMixedRealityAuthenticationDataProvider, IAuthenticatedAccount> OnLoggedIn;

        /// <inheritdoc />
        public event Action<IMixedRealityAuthenticationDataProvider, IAuthenticatedAccount> OnLoggedOut;

        /// <inheritdoc />
        public bool CacheUserTokens { get; set; }

        private readonly HashSet<IAuthenticatedAccount> activeAccounts = new HashSet<IAuthenticatedAccount>();

        /// <inheritdoc />
        public IReadOnlyCollection<IAuthenticatedAccount> ActiveAccounts => activeAccounts;

        private readonly HashSet<IMixedRealityAuthenticationDataProvider> activeDataProviders = new HashSet<IMixedRealityAuthenticationDataProvider>();

        /// <inheritdoc />
        public IReadOnlyCollection<IMixedRealityAuthenticationDataProvider> ActiveAuthenticationProviders => activeDataProviders;

        /// <inheritdoc />
        public bool RegisterAuthenticationDataProvider(IMixedRealityAuthenticationDataProvider provider)
        {
            if (activeDataProviders.Contains(provider))
            {
                return false;
            }

            activeDataProviders.Add(provider);
            LoginEvents(provider, true);
            return true;
        }

        /// <inheritdoc />
        public bool UnregisterAuthenticationDataProvider(IMixedRealityAuthenticationDataProvider provider)
        {
            if (!activeDataProviders.Contains(provider))
            {
                return false;
            }

            LoginEvents(provider, false);
            activeDataProviders.Remove(provider);
            return true;
        }

        /// <inheritdoc />
        public void LogOutAllSessions()
        {
            foreach (var provider in activeDataProviders)
            {
                provider.Logout(false);
            }
        }

        /// <inheritdoc />
        public void ClearAllTokenCaches()
        {
            foreach (var provider in activeDataProviders)
            {
                provider.ClearTokenCache();
            }
        }

        #endregion IMixedRealityAuthenticationSystem Implementation

        private void LoginEvents(IMixedRealityAuthenticationDataProvider provider, bool isRegistered)
        {
            if (activeDataProviders.Contains(provider))
            {
                if (isRegistered)
                {
                    provider.OnLoggedIn += OnProviderLoggedIn;
                    provider.OnLoggedOut += OnProviderLogout;
                }
                else
                {
                    provider.OnLoggedIn -= OnProviderLoggedIn;
                    provider.OnLoggedOut -= OnProviderLogout;
                }
            }

            void OnProviderLoggedIn(IAuthenticatedAccount account)
            {
                if (!activeAccounts.Contains(account))
                {
                    activeAccounts.Add(account);
                    OnLoggedIn?.Invoke(provider, account);
                }
                else
                {
                    Debug.LogError($"{Name}:{nameof(OnLoggedOut)}: Account already logged in!");
                }
            }

            void OnProviderLogout(IAuthenticatedAccount account)
            {
                if (activeAccounts.Contains(account))
                {
                    activeAccounts.Remove(account);
                    OnLoggedOut?.Invoke(provider, account);
                }
                else
                {
                    Debug.LogError($"{Name}:{nameof(OnLoggedOut)}: Account already logged out!");
                }
            }
        }
    }
}
