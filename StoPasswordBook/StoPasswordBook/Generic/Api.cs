using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Reflection;
using System.Windows.Media;
using System.Xml;
using log4net;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace StoPasswordBook.Generic
{
    public class Api
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Api));
        private static string ConfigFilePath { get; } = "Config.xml";
        
        private static readonly SemaphoreSlim Semaphore = new SemaphoreSlim(1, 1);
        private static readonly SemaphoreSlim SemaphoreLocate = new SemaphoreSlim(1, 1);
        private static readonly SemaphoreSlim SemaphoreInitApi = new SemaphoreSlim(1, 1);
        
        [AttributeUsage(AttributeTargets.Property)]
        public class IgnoreSettingAttribute : Attribute
        {
        }
        
        public static void KillExistingInstances(string processName)
        {
            var processes = Process.GetProcessesByName(processName);

            foreach (var process in processes)
            {
                try
                {
                    process.Kill();
                    process.WaitForExit();
                }
                catch (Exception ex)
                {
                    Log.Error($"Failed to kill process {process.Id}: {ex.Message}");
                }
            }
        }
        
        public static bool ParseConfig()
        {
            try
            {
                if (!File.Exists(ConfigFilePath))
                {
                    Log.Error("Config file not exist.");
                    File.WriteAllText(ConfigFilePath, string.Empty);

                    SaveToXml(ConfigFilePath);  
                }

                LoadFromXml(ConfigFilePath);
                MainWindow.UpdateText("Config loaded.");
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message + ex.StackTrace);
                return false;
            }
        }
        
        private static void LoadFromXml(string filePath)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(filePath);
            var root = doc.DocumentElement;

            foreach (var prop in typeof(GlobalVariables).GetProperties())
            {
                if (prop.GetCustomAttribute<IgnoreSettingAttribute>() != null)
                {
                    continue;
                }

                var element = root.SelectSingleNode(prop.Name);
                if (element == null) continue;

                var value = element.InnerText;
                try
                {
                    if (prop.PropertyType == typeof(int))
                    {
                        if (int.TryParse(value, out int intValue))
                        {
                            prop.SetValue(null, intValue);
                        }
                        else
                        {
                            Log.Error($"Error converting value '{value}' to int for property '{prop.Name}'");
                        }
                    }
                    else if (prop.PropertyType == typeof(string))
                    {
                        prop.SetValue(null, value);
                    }
                    else if (prop.PropertyType == typeof(bool))
                    {
                        if (bool.TryParse(value, out bool boolValue))
                        {
                            prop.SetValue(null, boolValue);
                        }
                        else
                        {
                            Log.Error($"Error converting value '{value}' to bool for property '{prop.Name}'");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"Error setting property '{prop.Name}'", ex);
                }
            }
        }
        
        private static void PostLoadConfig()
        {
        }
        
        public static void SaveSettings()
        {
            SaveToXml(ConfigFilePath);
            MainWindow.UpdateText("Config saved.");
        }

        private static void SaveToXml(string filePath)
        {
            XmlDocument doc = new XmlDocument();
            XmlElement root = doc.CreateElement("Settings");
            doc.AppendChild(root);

            foreach (var prop in typeof(GlobalVariables).GetProperties())
            {
                var value = prop.GetValue(null);
                var descriptionAttr = (DescriptionAttribute)prop.GetCustomAttributes(typeof(DescriptionAttribute), false).FirstOrDefault();
                var description = descriptionAttr?.Description ?? string.Empty;

                if (prop.GetCustomAttribute<IgnoreSettingAttribute>() != null)
                {
                    continue;
                }

                string valueString = null;

                if (prop.PropertyType == typeof(ushort[]))
                {
                    valueString = value != null ? string.Join(",", (ushort[])value) : string.Empty;
                }
                else if (prop.PropertyType == typeof(string))
                {
                    valueString = value?.ToString();
                }
                else if (prop.PropertyType == typeof(int))
                {
                    valueString = value?.ToString();
                }
                else if (prop.PropertyType == typeof(bool))
                {
                    valueString = value?.ToString().ToLower();
                }
                else
                {
                    valueString = value?.ToString();
                }

                if (valueString != null)
                {
                    AppendElement(doc, root, prop.Name, valueString, description);
                }
            }

            doc.Save(filePath);
        }

        private static void AppendElement(XmlDocument doc, XmlNode root, string name, string value, string description)
        {
            var element = doc.CreateElement(name);
            element.InnerText = value;
            
            foreach (var line in description.Split('\n'))
            {
                var comment = doc.CreateComment(line);
                root.AppendChild(comment);
            }

            root.AppendChild(element);
        }
        
       private static async Task<bool> Locate()
        {
            try
            {
                await SemaphoreLocate.WaitAsync();

                if (HttpClientManager.HttpClient == null)
                {
                    Log.Error("HttpClient is null.");
                    return false;
                }

                GlobalVariables.DebugUrl = $"http://127.0.0.1:{GlobalVariables.DebugPort}/json/list";

                HttpResponseMessage? response = null;

                try
                {
                    response = await HttpClientManager.HttpClient.GetAsync(GlobalVariables.DebugUrl);
                }
                catch (HttpRequestException ex)
                {
                    return false;
                }

                Log.Debug($"Attempting accessing {GlobalVariables.DebugUrl}");
                MainWindow.UpdateText("Attempting accessing STO Launcher");

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    List<DebugInfo>? debugInfoList = JsonConvert.DeserializeObject<List<DebugInfo>>(await response.Content.ReadAsStringAsync());
                    if (debugInfoList == null || debugInfoList.Count == 0)
                    {
                        return false;
                    }

                    GlobalVariables.DebugUrl = debugInfoList[0].WebSocketDebuggerUrl;
                    Log.Debug(GlobalVariables.DebugUrl);
                }

                GlobalVariables.WebSocketUrl = GlobalVariables.DebugUrl;
                Log.Debug($"WebSocket URL: {GlobalVariables.WebSocketUrl}");
                MainWindow.UpdateText($"WebSocket URL: {GlobalVariables.WebSocketUrl}");

                WebSocketManager.InitWebSocket(GlobalVariables.WebSocketUrl);

                MainWindow.UpdateText("Done! Please choose an Account for login.", Brushes.Green);
                Log.Info("Api initialized.");

                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message + ex.StackTrace);
                return false;
            }
            finally
            {
                SemaphoreLocate.Release();
            }
        }

        public static async Task InitApi()
        {
            try
            {
                await SemaphoreInitApi.WaitAsync();
                Log.Debug("Called InitApi");

                if (GlobalVariables.LauncherPath == "null" || !File.Exists(GlobalVariables.LauncherPath))
                {
                    OpenFileDialog openFileDialog = new OpenFileDialog
                    {
                        Filter = "Star Trek Online.exe|Star Trek Online.exe",
                        Title = "Select the game launcher",
                        FileName = "Star Trek Online.exe"
                    };

                    bool? result = openFileDialog.ShowDialog();

                    if (result == true)
                    {
                        GlobalVariables.LauncherPath = openFileDialog.FileName;
                    }
                }

                SaveSettings();

                int availablePort = GetAvailablePort();
                GlobalVariables.DebugPort = availablePort;

                if (!File.Exists(GlobalVariables.LauncherPath))
                {
                    MainWindow.UpdateText("Launcher not found. Please reset the game launcher settings.");
                    return;
                }

                var processStartInfo = new ProcessStartInfo
                {
                    FileName = GlobalVariables.LauncherPath,
                    Arguments = $"--remote-debugging-port={GlobalVariables.DebugPort}",
                };

                Process? process = null;

                try
                {
                    process = Process.Start(processStartInfo);
                }
                catch (Exception ex)
                {
                    Log.Error(ex.Message + ex.StackTrace);
                }

                if (process == null)
                {
                    MainWindow.UpdateText("Failed to start the launcher. Please try again or check the log.");
                    return;
                }
            }
            finally
            {
                SemaphoreInitApi.Release();
            }

            await HttpClientManager.InitHttpClient();
            await RetryLocate();
        }
        
        private static int GetAvailablePort()
        {
            using TcpListener listener = new TcpListener(IPAddress.Loopback, 0);

            listener.Start();
            IPEndPoint endpoint = (IPEndPoint)listener.LocalEndpoint;
            listener.Stop();
            
            return endpoint.Port;
        }

        private static async Task RetryLocate()
        {
            bool success = false;

            while (!success)
            {
                success = await Locate();

                if (!success)
                {
                    Log.Error("Locate failed. Retrying...");
                    await Task.Yield();
                }
            }
        }
    }
}
