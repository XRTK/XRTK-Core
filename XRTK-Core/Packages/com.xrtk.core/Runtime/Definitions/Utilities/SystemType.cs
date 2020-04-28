﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using UnityEngine;

namespace XRTK.Definitions.Utilities
{
    /// <summary>
    /// Reference to a class <see cref="System.Type"/> with support for Unity serialization.
    /// </summary>
    [Serializable]
    public sealed class SystemType : ISerializationCallbackReceiver
    {
        [SerializeField]
        private string reference = string.Empty;

        private Type type;

        public static string GetReference(Type type)
        {
            if (type == null || string.IsNullOrEmpty(type.AssemblyQualifiedName))
            {
                return string.Empty;
            }

            var qualifiedNameComponents = type.AssemblyQualifiedName.Split(',');
            Debug.Assert(qualifiedNameComponents.Length >= 2);
            return $"{qualifiedNameComponents[0]}, {qualifiedNameComponents[1].Trim()}";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SystemType"/> class.
        /// </summary>
        /// <param name="assemblyQualifiedClassName">Assembly qualified class name.</param>
        public SystemType(string assemblyQualifiedClassName)
        {
            if (!string.IsNullOrEmpty(assemblyQualifiedClassName))
            {
                Type = Type.GetType(assemblyQualifiedClassName);

                if (Type != null && Type.IsAbstract)
                {
                    Type = null;
                }
            }
            else
            {
                Type = null;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SystemType"/> class.
        /// </summary>
        /// <param name="type">Class type.</param>
        public SystemType(Type type)
        {
            Type = type;
        }

        #region ISerializationCallbackReceiver Members

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            type = !string.IsNullOrEmpty(reference) ? Type.GetType(reference) : null;
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize() { }

        #endregion ISerializationCallbackReceiver Members

        /// <summary>
        /// Gets or sets type of class reference.
        /// </summary>
        public Type Type
        {
            get => type;
            set
            {
                if (value != null)
                {
                    bool isValid = value.IsValueType && !value.IsEnum && !value.IsAbstract || value.IsClass;

                    if (!isValid)
                    {
                        Debug.LogError($"'{value.FullName}' is not a valid class or struct type.");
                    }
                }

                type = value;
                reference = GetReference(value);
            }
        }

        public static implicit operator string(SystemType type)
        {
            return type.reference;
        }

        public static implicit operator Type(SystemType type)
        {
            return type?.Type;
        }

        public static implicit operator SystemType(Type type)
        {
            return new SystemType(type);
        }

        public override string ToString()
        {
            return Type?.FullName ?? (string.IsNullOrWhiteSpace(reference) ? "{None}" : reference);
        }
    }
}