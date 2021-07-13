// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using XRTK.Attributes;
using XRTK.Definitions.Utilities;
using XRTK.Interfaces.Authentication;

namespace XRTK.Definitions.Authentication
{
    /// <summary>
    /// The configuration profile for <see cref="IMixedRealityAuthenticationDataProvider"/>.
    /// </summary>
    [CreateAssetMenu(menuName = "Mixed Reality Toolkit/Authentication System/Generic Authentication Data Provider", fileName = "AuthenticationDataProviderProfile", order = (int)CreateProfileMenuItemIndices.AuthenticationSystem)]
    public class AuthenticationDataProviderProfile : BaseMixedRealityProfile
    {
        [SerializeField]
        [Tooltip("The client ID is the unique application (client) ID assigned to your app by registering your application with the identity provider.")]
        private string clientId = "";

        /// <summary>
        /// The client ID is the unique application (client) ID assigned to your app by registering your application with the identity provider.
        /// </summary>
        public string ClientId => clientId;

        [SerializeField]
        [Tooltip("The identity provider url to request OAuth tokens from.")]
        private string identityProviderUrl = "";

        /// <summary>
        /// The identity provider url to request OAuth tokens from.
        /// </summary>
        public string IdentityProviderUrl => identityProviderUrl;

        [SerializeField]
        [Tooltip("Scopes or Permissions to request access to.")]
        private string[] scopes = new string[0];

        /// <summary>
        /// The Scopes or Permissions to request access to.
        /// </summary>
        public string[] Scopes => scopes;

        [SerializeField]
        [Tooltip("The redirect url is the endpoint the identity provider will send the security tokens back to.")]
        private string redirectUrl = "";

        /// <summary>
        /// The redirect url is the endpoint the identity provider will send the security tokens back to.
        /// </summary>
        public string RedirectUrl => redirectUrl;

        [SerializeField]
        [Prefab(typeof(IAuthenticationHandler))]
        [Tooltip("The login prefab to display when the user is logging in.")]
        private GameObject loginPrefab = null;

        /// <summary>
        /// The login prefab to display when the user is logging in.
        /// </summary>
        public GameObject LoginPrefab => loginPrefab;
    }
}
