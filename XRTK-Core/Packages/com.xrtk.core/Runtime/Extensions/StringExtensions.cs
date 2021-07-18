// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace XRTK.Extensions
{
    /// <summary>
    /// <see cref="string"/> Extensions.
    /// </summary>
    public static class StringExtensions
    {
        public const string WhiteSpace = " ";

        /// <summary>
        /// Encodes the string to base 64 ASCII.
        /// </summary>
        /// <param name="toEncode">String to encode.</param>
        /// <returns>Encoded string.</returns>
        public static string EncodeTo64(this string toEncode)
            => Convert.ToBase64String(Encoding.ASCII.GetBytes(toEncode));

        /// <summary>
        /// Decodes string from base 64 ASCII.
        /// </summary>
        /// <param name="encodedData">String to decode.</param>
        /// <returns>Decoded string.</returns>
        public static string DecodeFrom64(this string encodedData)
            => Encoding.ASCII.GetString(Convert.FromBase64String(encodedData));

        /// <summary>
        /// Capitalize the first character and add a space before
        /// each capitalized letter (except the first character).
        /// </summary>
        /// <param name="value"></param>
        public static string ToProperCase(this string value)
        {
            // If there are 0 or 1 characters, just return the string.
            if (value == null) { return string.Empty; }
            if (value.Length < 4) { return value.ToUpper(); }
            // If there's already spaces in the string, return.
            if (value.Contains(WhiteSpace)) { return value; }

            // Start with the first character.
            var result = new StringBuilder(value.Substring(0, 1).ToUpper());

            // Add the remaining characters.
            for (int i = 1; i < value.Length; i++)
            {
                var wasLastCharUpper = char.IsUpper(value[i - 1]);
                var nextIsLower = i + 1 < value.Length && char.IsLower(value[i + 1]);
                var isUpper = char.IsLetter(value[i]) && char.IsUpper(value[i]);

                if (isUpper && !wasLastCharUpper && nextIsLower)
                {
                    result.Append(WhiteSpace);
                }

                result.Append(value[i]);

                if (isUpper && wasLastCharUpper && !nextIsLower)
                {
                    result.Append(WhiteSpace);
                }
            }

            return result.ToString();
        }

        /// <summary>
        /// Replaces all back slashes in the string with forward slashes.
        /// </summary>
        public static string ForwardSlashes(this string value)
            => value.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        /// <summary>
        /// Replaces all forward slashes in the string with back slashes.
        /// </summary>
        public static string BackSlashes(this string value)
            => value.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

        /// <summary>
        /// Returns the URI path, excluding the filename
        /// </summary>
        public static string PathFromURI(this string value)
            => value.Substring(0, value.LastIndexOf("/", StringComparison.Ordinal) + 1);

        /// <summary>
        /// Returns the filename from a URI path
        /// </summary>
        public static string FilenameFromURI(this string value)
            => value.Substring(value.LastIndexOf("/", StringComparison.Ordinal) + 1, value.Length - value.LastIndexOf("/", StringComparison.Ordinal) - 1);

        /// <summary>
        /// Creates a relative path from one file or folder to another.
        /// </summary>
        /// <param name="fromPath">Contains the directory that defines the start of the relative path.</param>
        /// <param name="toPath">Contains the path that defines the endpoint of the relative path.</param>
        /// <param name="relativePath"></param>
        /// <returns>The relative path from the start directory to the end path or <c>toPath</c> if the paths are not related.</returns>
        public static bool TryMakeRelativePath(this string fromPath, string toPath, out string relativePath)
        {
            relativePath = string.Empty;

            try
            {
                if (string.IsNullOrEmpty(fromPath)) { throw new ArgumentNullException(nameof(fromPath)); }
                if (string.IsNullOrEmpty(toPath)) { throw new ArgumentNullException(nameof(toPath)); }

                var toUri = new Uri(toPath);
                var fromUri = new Uri(fromPath);

                if (fromUri.Scheme != toUri.Scheme)
                {
                    // path can't be made relative.
                    relativePath = Uri.UnescapeDataString(toUri.ToString());
                    return false;
                }

                var relativeUri = fromUri.MakeRelativeUri(toUri);
                relativePath = Uri.UnescapeDataString(relativeUri.ToString());

                if (toUri.Scheme.Equals("file", StringComparison.InvariantCultureIgnoreCase))
                {
                    relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
                }

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            return false;
        }

        /// <summary>
        /// Generates a <see cref="Guid"/> based on the string.
        /// </summary>
        /// <param name="string">The string to generate the <see cref="Guid"/>.</param>
        /// <returns>A new <see cref="Guid"/> that represents the string.</returns>
        public static Guid GenerateGuid(this string @string)
        {
            using (MD5 md5 = MD5.Create())
            {
                return new Guid(md5.ComputeHash(Encoding.Default.GetBytes(@string)));
            }
        }
    }
}
