﻿// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using XRTK.Definitions;
using XRTK.Interfaces;

namespace XRTK.Services
{
    public abstract class BaseService<T> : BaseService, IMixedRealityService<T> where T : BaseMixedRealityProfile
    {
        /// <inheritdoc />
        public T ProfileType { get; }
    }

    /// <summary>
    /// The base service implements <see cref="IMixedRealityService"/> and provides default properties for all services.
    /// </summary>
    public abstract class BaseService : IMixedRealityService
    {
        #region IMixedRealityService Implementation

        /// <inheritdoc />
        public virtual string Name { get; protected set; }

        /// <inheritdoc />
        public virtual uint Priority { get; protected set; } = 5;

        /// <inheritdoc />
        public virtual void Initialize() { }

        /// <inheritdoc />
        public virtual void Reset() { }

        /// <inheritdoc />
        public virtual void Enable() { }

        /// <inheritdoc />
        public virtual void Update() { }

        /// <inheritdoc />
        public virtual void LateUpdate() { }

        /// <inheritdoc />
        public virtual void FixedUpdate() { }

        /// <inheritdoc />
        public virtual void Disable() { }

        /// <inheritdoc />
        public virtual void Destroy() { }

        /// <inheritdoc />
        public virtual void OnApplicationFocus(bool isFocused) { }

        /// <inheritdoc />
        public void OnApplicationPause(bool isPaused) { }

        #endregion IMixedRealityService Implementation

        #region IDisposable Implementation

        private bool disposed;

        ~BaseService()
        {
            OnDispose(true);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (disposed) { return; }
            disposed = true;
            GC.SuppressFinalize(this);
            OnDispose(false);
        }

        protected virtual void OnDispose(bool finalizing) { }

        #endregion IDisposable Implementation
    }
}
