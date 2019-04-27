﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

#if UNITY_EDITOR || !UNITY_WSA
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using XRTK.Definitions.Utilities;

namespace XRTK.Extensions
{
    /// <summary>
    /// <see cref="Process"/> Extension class.
    /// </summary>
    public static class ProcessExtensions
    {
        /// <summary>
        /// Runs an external process.
        /// </summary>
        /// <param name="process"></param>
        /// <param name="args">The passed arguments.</param>
        /// <param name="output">The output of the process.</param>
        /// <param name="application">The Application to run through the command line. Default application is "cmd.exe"</param>
        /// <returns>Output string.</returns>
        /// <remarks>This process will block the main thread of the editor if command takes too long to run. Use <see cref="RunAsync(Process,string,string,bool,CancellationToken)"/> for a background process.</remarks>
        public static bool Run(this Process process, string args, out string output, string application = @"cmd.exe")
        {
            if (string.IsNullOrEmpty(args))
            {
                output = "You cannot pass null or empty parameter.";
                UnityEngine.Debug.LogError(output);
                return false;
            }

            process.StartInfo = new ProcessStartInfo
            {
                WindowStyle = ProcessWindowStyle.Normal,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                FileName = application,
                Arguments = args
            };

            try
            {
                if (!process.Start())
                {
                    output = "Failed to start process!";
                    UnityEngine.Debug.LogError(output);
                    return false;
                }

                string error = process.StandardError.ReadToEnd();

                if (!string.IsNullOrEmpty(error))
                {
                    output = error;
                    return false;
                }

                output = process.StandardOutput.ReadToEnd();

                process.WaitForExit();
                process.Close();
                process.Dispose();
            }
            catch (Exception e)
            {
                output = e.Message;
                UnityEngine.Debug.LogException(e);
            }

            return true;
        }

        /// <summary>
        /// Starts a process asynchronously.
        /// </summary>
        /// <param name="process">This Process.</param>
        /// <param name="application">The process executable to run.</param>
        /// <param name="args">The Process arguments.</param>
        /// <param name="showDebug">Should output debug code to Editor Console?</param>
        /// <param name="cancellationToken"></param>
        /// <returns><see cref="ProcessResult"/></returns>
        public static async Task<ProcessResult> RunAsync(this Process process, string args, string application = @"cmd.exe", bool showDebug = false, CancellationToken cancellationToken = default)
        {
            return await RunAsync(process, new ProcessStartInfo
            {
                FileName = application,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                Arguments = args
            }, showDebug, cancellationToken);
        }

        /// <summary>
        /// Starts a process asynchronously.<para/>
        /// </summary>
        /// <remarks>The provided Process Start Info must not use shell execution, and should redirect the standard output and errors.</remarks>
        /// <param name="process">This Process.</param>
        /// <param name="startInfo">The Process start info.</param>
        /// <param name="showDebug">Should output debug code to Editor Console?</param>
        /// <param name="cancellationToken"></param>
        /// <returns><see cref="ProcessResult"/></returns>
        public static async Task<ProcessResult> RunAsync(this Process process, ProcessStartInfo startInfo, bool showDebug = false, CancellationToken cancellationToken = default)
        {
            Debug.Assert(!startInfo.UseShellExecute, "Process Start Info must not use shell execution.");
            Debug.Assert(startInfo.RedirectStandardOutput, "Process Start Info must redirect standard output.");
            Debug.Assert(startInfo.RedirectStandardError, "Process Start Info must redirect standard errors.");

            process.StartInfo = startInfo;
            process.EnableRaisingEvents = true;

            var processResult = new TaskCompletionSource<ProcessResult>();
            var errorCodeResult = new TaskCompletionSource<string[]>();
            var errorList = new List<string>();
            var outputCodeResult = new TaskCompletionSource<string[]>();
            var outputList = new List<string>();

            process.Exited += OnProcessExited;
            process.ErrorDataReceived += OnErrorDataReceived;
            process.OutputDataReceived += OnOutputDataReceived;

            async void OnProcessExited(object sender, EventArgs args)
            {
                processResult.TrySetResult(new ProcessResult(process.ExitCode, await errorCodeResult.Task, await outputCodeResult.Task));
                process.Close();
                process.Dispose();
            }

            void OnErrorDataReceived(object sender, DataReceivedEventArgs args)
            {
                if (args.Data != null)
                {
                    errorList.Add(args.Data);

                    if (!showDebug)
                    {
                        return;
                    }

                    UnityEngine.Debug.LogError(args.Data);
                }
                else
                {
                    errorCodeResult.TrySetResult(errorList.ToArray());
                }
            }

            void OnOutputDataReceived(object sender, DataReceivedEventArgs args)
            {
                if (args.Data != null)
                {
                    outputList.Add(args.Data);

                    if (!showDebug)
                    {
                        return;
                    }

                    UnityEngine.Debug.Log(args.Data);
                }
                else
                {
                    outputCodeResult.TrySetResult(outputList.ToArray());
                }
            }

            if (!process.Start())
            {
                if (showDebug)
                {
                    UnityEngine.Debug.LogError("Failed to start process!");
                }

                processResult.TrySetResult(new ProcessResult(process.ExitCode, new[] { "Failed to start process!" }, null));
            }
            else
            {
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                CancellationWatcher(process);
            }

            async void CancellationWatcher(Process runningProcess)
            {
                // ReSharper disable once MethodSupportsCancellation
                // We utilize the cancellation token in the loop
                await Task.Run(() =>
                {
                    try
                    {
                        while (!runningProcess.HasExited)
                        {
                            if (cancellationToken.IsCancellationRequested)
                            {
                                runningProcess.Kill();
                            }
                        }
                    }
                    catch
                    {
                        // ignored
                    }
                });
            }

            return await processResult.Task;
        }
    }
}
#endif
