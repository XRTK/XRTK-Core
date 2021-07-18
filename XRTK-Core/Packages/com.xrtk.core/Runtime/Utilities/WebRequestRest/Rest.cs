// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using XRTK.Extensions;
using XRTK.Utilities.Async;

namespace XRTK.Utilities.WebRequestRest
{
    /// <summary>
    /// REST Class for CRUD Transactions.
    /// </summary>
    public static class Rest
    {
        /// <summary>
        /// Use SSL Connections when making rest calls.
        /// </summary>
        public static bool UseSSL { get; set; } = true;

        #region Authentication

        /// <summary>
        /// Gets the Basic auth header.
        /// </summary>
        /// <param name="username">The Username.</param>
        /// <param name="password">The password.</param>
        /// <returns>The Basic authorization header encoded to base 64.</returns>
        public static string GetBasicAuthentication(string username, string password)
        {
            return $"Basic {Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes($"{username}:{password}"))}";
        }

        /// <summary>
        /// Gets the Bearer auth header.
        /// </summary>
        /// <param name="authToken">OAuth Token to be used.</param>
        /// <returns>The Bearer authorization header.</returns>
        public static string GetBearerOAuthToken(string authToken)
        {
            return $"Bearer {authToken}";
        }

        #endregion Authentication

        #region GET

        /// <summary>
        /// Rest GET.
        /// </summary>
        /// <param name="query">Finalized Endpoint Query with parameters.</param>
        /// <param name="headers">Optional header information for the request.</param>
        /// <param name="progress">Optional <see cref="IProgress{T}"/> handler.</param>
        /// <param name="timeout">Optional time in seconds before request expires.</param>
        /// <returns>The response data.</returns>
        public static async Task<Response> GetAsync(string query, Dictionary<string, string> headers = null, IProgress<float> progress = null, int timeout = -1)
        {
            using (var webRequest = UnityWebRequest.Get(query))
            {
                return await ProcessRequestAsync(webRequest, headers, progress, timeout);
            }
        }

        #endregion GET

        #region POST

        /// <summary>
        /// Rest POST.
        /// </summary>
        /// <param name="query">Finalized Endpoint Query with parameters.</param>
        /// <param name="headers">Optional header information for the request.</param>
        /// <param name="progress">Optional <see cref="IProgress{T}"/> handler.</param>
        /// <param name="timeout">Optional time in seconds before request expires.</param>
        /// <returns>The response data.</returns>
        public static async Task<Response> PostAsync(string query, Dictionary<string, string> headers = null, IProgress<float> progress = null, int timeout = -1)
        {
            using (var webRequest = UnityWebRequest.Post(query, null as string))
            {
                return await ProcessRequestAsync(webRequest, headers, progress, timeout);
            }
        }

        /// <summary>
        /// Rest POST.
        /// </summary>
        /// <param name="query">Finalized Endpoint Query with parameters.</param>
        /// <param name="formData">Form Data.</param>
        /// <param name="headers">Optional header information for the request.</param>
        /// <param name="progress">Optional <see cref="IProgress{T}"/> handler.</param>
        /// <param name="timeout">Optional time in seconds before request expires.</param>
        /// <returns>The response data.</returns>
        public static async Task<Response> PostAsync(string query, WWWForm formData, Dictionary<string, string> headers = null, IProgress<float> progress = null, int timeout = -1)
        {
            using (var webRequest = UnityWebRequest.Post(query, formData))
            {
                return await ProcessRequestAsync(webRequest, headers, progress, timeout);
            }
        }

        /// <summary>
        /// Rest POST.
        /// </summary>
        /// <param name="query">Finalized Endpoint Query with parameters.</param>
        /// <param name="jsonData">JSON data for the request.</param>
        /// <param name="headers">Optional header information for the request.</param>
        /// <param name="progress">Optional <see cref="IProgress{T}"/> handler.</param>
        /// <param name="timeout">Optional time in seconds before request expires.</param>
        /// <returns>The response data.</returns>
        public static async Task<Response> PostAsync(string query, string jsonData, Dictionary<string, string> headers = null, IProgress<float> progress = null, int timeout = -1)
        {
            using (var webRequest = UnityWebRequest.Post(query, "POST"))
            {
                var data = new UTF8Encoding().GetBytes(jsonData);
                webRequest.uploadHandler = new UploadHandlerRaw(data);
                webRequest.downloadHandler = new DownloadHandlerBuffer();
                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.SetRequestHeader("Accept", "application/json");
                return await ProcessRequestAsync(webRequest, headers, progress, timeout);
            }
        }

        /// <summary>
        /// Rest POST.
        /// </summary>
        /// <param name="query">Finalized Endpoint Query with parameters.</param>
        /// <param name="bodyData">The raw data to post.</param>
        /// <param name="headers">Optional header information for the request.</param>
        /// <param name="progress">Optional <see cref="IProgress{T}"/> handler.</param>
        /// <param name="timeout">Optional time in seconds before request expires.</param>
        /// <returns>The response data.</returns>
        public static async Task<Response> PostAsync(string query, byte[] bodyData, Dictionary<string, string> headers = null, IProgress<float> progress = null, int timeout = -1)
        {
            using (var webRequest = UnityWebRequest.Post(query, "POST"))
            {
                webRequest.uploadHandler = new UploadHandlerRaw(bodyData);
                webRequest.downloadHandler = new DownloadHandlerBuffer();
                webRequest.SetRequestHeader("Content-Type", "application/octet-stream");
                return await ProcessRequestAsync(webRequest, headers, progress, timeout);
            }
        }

        #endregion POST

        #region PUT

        /// <summary>
        /// Rest PUT.
        /// </summary>
        /// <param name="query">Finalized Endpoint Query with parameters.</param>
        /// <param name="jsonData">Data to be submitted.</param>
        /// <param name="headers">Optional header information for the request.</param>
        /// <param name="progress">Optional <see cref="IProgress{T}"/> handler.</param>
        /// <param name="timeout">Optional time in seconds before request expires.</param>
        /// <returns>The response data.</returns>
        public static async Task<Response> PutAsync(string query, string jsonData, Dictionary<string, string> headers = null, IProgress<float> progress = null, int timeout = -1)
        {
            using (var webRequest = UnityWebRequest.Put(query, jsonData))
            {
                webRequest.SetRequestHeader("Content-Type", "application/json");
                return await ProcessRequestAsync(webRequest, headers, progress, timeout);
            }
        }

        /// <summary>
        /// Rest PUT.
        /// </summary>
        /// <param name="query">Finalized Endpoint Query with parameters.</param>
        /// <param name="bodyData">Data to be submitted.</param>
        /// <param name="headers">Optional header information for the request.</param>
        /// <param name="progress">Optional <see cref="IProgress{T}"/> handler.</param>
        /// <param name="timeout">Optional time in seconds before request expires.</param>
        /// <returns>The response data.</returns>
        public static async Task<Response> PutAsync(string query, byte[] bodyData, Dictionary<string, string> headers = null, IProgress<float> progress = null, int timeout = -1)
        {
            using (var webRequest = UnityWebRequest.Put(query, bodyData))
            {
                webRequest.SetRequestHeader("Content-Type", "application/octet-stream");
                return await ProcessRequestAsync(webRequest, headers, progress, timeout);
            }
        }

        #endregion PUT

        #region DELETE

        /// <summary>
        /// Rest DELETE.
        /// </summary>
        /// <param name="query">Finalized Endpoint Query with parameters.</param>
        /// <param name="headers">Optional header information for the request.</param>
        /// <param name="progress">Optional <see cref="IProgress{T}"/> handler.</param>
        /// <param name="timeout">Optional time in seconds before request expires.</param>
        /// <returns>The response data.</returns>
        public static async Task<Response> DeleteAsync(string query, Dictionary<string, string> headers = null, IProgress<float> progress = null, int timeout = -1)
        {
            using (var webRequest = UnityWebRequest.Delete(query))
            {
                return await ProcessRequestAsync(webRequest, headers, progress, timeout);
            }
        }

        #endregion DELETE

        #region Get Multimedia Content

        private static string DownloadCacheDirectory => $"{Application.temporaryCachePath}\\download_cache";

        /// <summary>
        /// Download a <see cref="Texture2D"/> from the provided <see cref="url"/>.
        /// </summary>
        /// <param name="url">The url to download the <see cref="Texture2D"/> from.</param>
        /// <param name="headers">Optional header information for the request.</param>
        /// <param name="progress">Optional <see cref="IProgress{T}"/> handler.</param>
        /// <param name="timeout">Optional time in seconds before request expires.</param>
        /// <returns>A new <see cref="Texture2D"/> instance.</returns>
        public static async Task<Texture2D> DownloadTextureAsync(string url, Dictionary<string, string> headers = null, IProgress<float> progress = null, int timeout = -1)
        {
            bool isCached = TryGetDownloadCacheItem(url, out var cachePath);

            if (isCached)
            {
                url = cachePath;
            }

            using (var webRequest = UnityWebRequestTexture.GetTexture(url))
            {
                var response = await ProcessRequestAsync(webRequest, headers, progress, timeout);

                if (!response.Successful)
                {
                    Debug.LogError($"Failed to download texture from \"{url}\"!");

                    return null;
                }

                var downloadHandler = (DownloadHandlerTexture)webRequest.downloadHandler;

                if (!isCached)
                {
                    using (var fileStream = File.OpenWrite(cachePath))
                    {
                        await fileStream.WriteAsync(downloadHandler.data, 0, downloadHandler.data.Length, CancellationToken.None);
                    }
                }

                return downloadHandler.texture;
            }
        }

        /// <summary>
        /// Download a <see cref="AudioClip"/> from the provided <see cref="url"/>.
        /// </summary>
        /// <param name="url">The url to download the <see cref="AudioClip"/> from.</param>
        /// <param name="audioType"><see cref="AudioType"/> to download.</param>
        /// <param name="headers">Optional header information for the request.</param>
        /// <param name="progress">Optional <see cref="IProgress{T}"/> handler.</param>
        /// <param name="timeout">Optional time in seconds before request expires.</param>
        /// <returns>A new <see cref="AudioClip"/> instance.</returns>
        public static async Task<AudioClip> DownloadAudioClipAsync(string url, AudioType audioType, Dictionary<string, string> headers = null, IProgress<float> progress = null, int timeout = -1)
        {
            bool isCached = TryGetDownloadCacheItem(url, out var cachePath);

            if (isCached)
            {
                url = cachePath;
            }

            using (var webRequest = UnityWebRequestMultimedia.GetAudioClip(url, audioType))
            {
                var response = await ProcessRequestAsync(webRequest, headers, progress, timeout);

                if (!response.Successful)
                {
                    Debug.LogError($"Failed to download audio clip from \"{url}\"!");

                    return null;
                }

                var downloadHandler = (DownloadHandlerAudioClip)webRequest.downloadHandler;

                if (!isCached)
                {
                    using (var fileStream = File.OpenWrite(cachePath))
                    {
                        await fileStream.WriteAsync(downloadHandler.data, 0, downloadHandler.data.Length, CancellationToken.None);
                    }
                }

                return downloadHandler.audioClip;
            }
        }

        /// <summary>
        /// Download a <see cref="AssetBundle"/> from the provided <see cref="url"/>.
        /// </summary>
        /// <param name="url">The url to download the <see cref="AssetBundle"/> from.</param>
        /// <param name="headers">Optional header information for the request.</param>
        /// <param name="progress">Optional <see cref="IProgress{T}"/> handler.</param>
        /// <param name="timeout">Optional time in seconds before request expires.</param>
        /// <returns>A new <see cref="AssetBundle"/> instance.</returns>
        public static async Task<AssetBundle> DownloadAssetBundleAsync(string url, Dictionary<string, string> headers = null, IProgress<float> progress = null, int timeout = -1)
        {
            if (TryGetDownloadCacheItem(url, out var cachePath))
            {
                return await AssetBundle.LoadFromFileAsync(cachePath);
            }

            using (var webRequest = UnityWebRequestAssetBundle.GetAssetBundle(url))
            {
                var response = await ProcessRequestAsync(webRequest, headers, progress, timeout);

                if (!response.Successful)
                {
                    Debug.LogError($"Failed to download asset bundle from \"{url}\"!");

                    return null;
                }

                var downloadHandler = (DownloadHandlerAssetBundle)webRequest.downloadHandler;

                using (var fileStream = File.OpenWrite(cachePath))
                {
                    await fileStream.WriteAsync(downloadHandler.data, 0, downloadHandler.data.Length, CancellationToken.None);
                }

                return downloadHandler.assetBundle;
            }
        }

        /// <summary>
        /// Download a file from the provided <see cref="url"/>.
        /// </summary>
        /// <param name="url">The url to download the file from.</param>
        /// <param name="fileName">Optional file name to download (including extension).</param>
        /// <param name="headers">Optional header information for the request.</param>
        /// <param name="progress">Optional <see cref="IProgress{T}"/> handler.</param>
        /// <param name="timeout">Optional time in seconds before request expires.</param>
        /// <returns>The path to the downloaded file.</returns>
        public static async Task<string> DownloadFileAsync(string url, string fileName = null, Dictionary<string, string> headers = null, IProgress<float> progress = null, int timeout = -1)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                // We will try go guess the name based on the url endpoint.
                var index = url.LastIndexOf('/');
                fileName = url.Substring(index, url.Length - index);
            }

            ValidateCacheDirectory();
            var filePath = $"{DownloadCacheDirectory}\\{fileName}";

            if (File.Exists(filePath))
            {
                return filePath;
            }

            using (var webRequest = UnityWebRequest.Get(url))
            using (var fileDownloadHandler = new DownloadHandlerFile(filePath)
            {
                removeFileOnAbort = true
            })
            {
                webRequest.downloadHandler = fileDownloadHandler;
                var response = await ProcessRequestAsync(webRequest, headers, progress, timeout);
                fileDownloadHandler.Dispose();

                if (!response.Successful)
                {
                    Debug.LogError($"Failed to download file from \"{url}\"!");

                    return null;
                }

                return filePath;
            }
        }

        private static void ValidateCacheDirectory()
        {
            if (!Directory.Exists(DownloadCacheDirectory))
            {
                Directory.CreateDirectory(DownloadCacheDirectory);
            }
        }

        /// <summary>
        /// Try to get a file out of the download cache by uri reference.
        /// </summary>
        /// <param name="uri">The uri key of the item.</param>
        /// <param name="filePath">The file path to the cached item.</param>
        /// <returns>True, if the item was in cache, otherwise false.</returns>
        public static bool TryGetDownloadCacheItem(string uri, out string filePath)
        {
            ValidateCacheDirectory();
            filePath = $"{DownloadCacheDirectory}\\{uri.GenerateGuid()}";
            return File.Exists(filePath);
        }

        /// <summary>
        /// Deletes all the files in the download cache.
        /// </summary>
        public static void DeleteDownloadCache()
        {
            if (Directory.Exists(DownloadCacheDirectory))
            {
                Directory.Delete(DownloadCacheDirectory, true);
            }
        }

        #endregion Get Multimedia Content

        private static async Task<Response> ProcessRequestAsync(UnityWebRequest webRequest, Dictionary<string, string> headers, IProgress<float> progress, int timeout)
        {
            if (timeout > 0)
            {
                webRequest.timeout = timeout;
            }

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    webRequest.SetRequestHeader(header.Key, header.Value);
                }
            }

            var isUpload = webRequest.method == UnityWebRequest.kHttpVerbPOST ||
                           webRequest.method == UnityWebRequest.kHttpVerbPUT;

            // HACK: Workaround for extra quotes around boundary.
            if (isUpload)
            {
                var contentType = webRequest.GetRequestHeader("Content-Type");

                if (contentType != null)
                {
                    contentType = contentType.Replace("\"", "");
                    webRequest.SetRequestHeader("Content-Type", contentType);
                }
            }

            var backgroundThread = new Thread(async () =>
            {
                await Awaiters.UnityMainThread;

                while (!webRequest.isDone)
                {
                    progress?.Report(isUpload ? webRequest.uploadProgress : webRequest.downloadProgress * 100f);

                    await Awaiters.UnityMainThread;
                }
            })
            {
                IsBackground = true
            };

            backgroundThread.Start();
            await webRequest.SendWebRequest();
            backgroundThread.Join();
            progress?.Report(100f);

#if UNITY_2020_1_OR_NEWER
            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
#else
            if (webRequest.isNetworkError || webRequest.isHttpError)
#endif
            {
                if (webRequest.responseCode == 401)
                {
                    return new Response(false, "Invalid Credentials", null, webRequest.responseCode);
                }

                if (webRequest.GetResponseHeaders() == null)
                {
                    return new Response(false, "Invalid Headers", null, webRequest.responseCode);
                }

                var responseHeaders = webRequest.GetResponseHeaders().Aggregate(string.Empty, (current, header) => $"\n{header.Key}: {header.Value}");
                Debug.LogError($"REST Error {webRequest.responseCode}:{webRequest.downloadHandler?.text}{responseHeaders}");
                return new Response(false, $"{responseHeaders}\n{webRequest.downloadHandler?.text}", null, webRequest.responseCode);
            }

            switch (webRequest.downloadHandler)
            {
                case DownloadHandlerFile _:
                case DownloadHandlerBuffer _:
                case DownloadHandlerScript _:
                case DownloadHandlerTexture _:
                case DownloadHandlerAudioClip _:
                case DownloadHandlerAssetBundle _:
                    return new Response(true, null, null, webRequest.responseCode);
                default:
                    return new Response(true, webRequest.downloadHandler?.text, webRequest.downloadHandler?.data, webRequest.responseCode);
            }
        }
    }
}
