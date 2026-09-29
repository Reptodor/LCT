using System;
using System.IO;
using System.Threading.Tasks;
using MCPForUnity.Editor.Services;
using UnityEditor;
using UnityEngine;

namespace LCT.Editor
{
    [InitializeOnLoad]
    internal static class McpProjectBootstrap
    {
        private const string SessionKey = "LCT.McpProjectBootstrap.Started";
        private static readonly string StatusPath = Path.Combine("Library", "McpBootstrap.status.txt");

        static McpProjectBootstrap()
        {
            EditorPrefs.SetBool("MCPForUnity.UseHttpTransport", true);
            EditorPrefs.SetBool("MCPForUnity.AutoStartOnLoad", true);
            EditorPrefs.SetBool("MCPForUnity.SetupCompleted", true);
            EditorPrefs.SetBool("MCPForUnity.SetupDismissed", true);
            EditorPrefs.SetString("MCPForUnity.HttpTransportScope", "local");
            EditorPrefs.SetString("MCPForUnity.HttpUrl", "http://127.0.0.1:8080");

            EditorApplication.delayCall += StartOnce;
        }

        private static void StartOnce()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += StartOnce;
                return;
            }

            if (SessionState.GetBool(SessionKey, false))
            {
                _ = EnsureBridgeAsync();
                return;
            }

            SessionState.SetBool(SessionKey, true);
            EditorApplication.delayCall += FinashkaAndroidGameView.Apply;
            _ = StartAsync();
        }

        private static async Task EnsureBridgeAsync()
        {
            try
            {
                if (MCPServiceLocator.Bridge.IsRunning)
                {
                    return;
                }

                WriteStatus("reconnect-starting");

                if (!MCPServiceLocator.Server.IsLocalHttpServerReachable())
                {
                    bool started = MCPServiceLocator.Server.StartLocalHttpServer(quiet: true);
                    WriteStatus(started ? "http-server-started" : "http-server-start-failed");
                }
                else
                {
                    WriteStatus("http-server-already-up");
                }

                bool connected = await MCPServiceLocator.Bridge.StartAsync();
                WriteStatus(connected ? "bridge-connected" : "bridge-connect-failed");
            }
            catch (Exception ex)
            {
                WriteStatus("reconnect-error: " + ex.Message);
                Debug.LogError("[LCT MCP] Reconnect failed: " + ex);
            }
        }

        private static async Task StartAsync()
        {
            try
            {
                WriteStatus("starting");

                try
                {
                    var summary = MCPServiceLocator.Client.ConfigureAllDetectedClients();
                    WriteStatus("configured: " + summary.GetSummaryMessage());
                }
                catch (Exception ex)
                {
                    WriteStatus("configure-failed: " + ex.Message);
                }

                if (!MCPServiceLocator.Server.IsLocalHttpServerReachable())
                {
                    bool started = MCPServiceLocator.Server.StartLocalHttpServer(quiet: true);
                    WriteStatus(started ? "http-server-started" : "http-server-start-failed");
                }
                else
                {
                    WriteStatus("http-server-already-up");
                }

                if (!MCPServiceLocator.Bridge.IsRunning)
                {
                    bool connected = await MCPServiceLocator.Bridge.StartAsync();
                    WriteStatus(connected ? "bridge-connected" : "bridge-connect-failed");
                }
                else
                {
                    WriteStatus("bridge-already-running");
                }

                WriteStatus("ready");
            }
            catch (Exception ex)
            {
                WriteStatus("error: " + ex.Message);
                Debug.LogError("[LCT MCP] Bootstrap failed: " + ex);
            }
        }

        private static void WriteStatus(string line)
        {
            try
            {
                File.AppendAllText(StatusPath, DateTime.Now.ToString("HH:mm:ss") + " " + line + Environment.NewLine);
            }
            catch
            {
                // Status file is best-effort only.
            }

            Debug.Log("[LCT MCP] " + line);
        }
    }
}
