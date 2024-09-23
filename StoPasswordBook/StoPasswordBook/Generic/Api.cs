using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Media;
using System.Xml;
using log4net;
using Microsoft.Win32;

namespace StoPasswordBook.Generic
{
    public class Api
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Api));
        private static string ConfigFilePath { get; } = "Config.xml";
        
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
        
        private static async Task Locate()
        {
            try
            {
                if (BrowserManager.Browser == null)
                {
                    Log.Error("Browser is null.");
                    return;
                }

                var page = await BrowserManager.Browser.NewPageAsync();
                var pageHandler = new PageHandler(page);
                
                GlobalVariables.DebugUrl = $"http://127.0.0.1:{GlobalVariables.DebugPort}";
                
                await pageHandler.NavigateTo(GlobalVariables.DebugUrl);
                Log.Debug($"Attempting accessing {GlobalVariables.DebugUrl}");
                MainWindow.UpdateText($"Attempting accessing {GlobalVariables.DebugUrl}");

                var linkElement = await pageHandler.QuerySelector("a[title='Cryptic Launcher | Star Trek']");
                if (linkElement != null)
                {
                    var href = await linkElement.GetPropertyAsync("href");
                    var hrefJson = await href.JsonValueAsync();
                    GlobalVariables.DebugUrl = hrefJson.ToString();
                    await page.GoToAsync(GlobalVariables.DebugUrl);
                    Log.Debug(GlobalVariables.DebugUrl);
                    MainWindow.UpdateText($"Attempting accessing {GlobalVariables.DebugUrl}");
                }
                
                string pattern = @"ws=127\.0\.0\.1:\d+/devtools/page/([A-F0-9]{32})";
                Match match = Regex.Match(GlobalVariables.DebugUrl, pattern);
                string pageId = "null";
                if (match.Success)
                {
                    pageId = match.Groups[1].Value;
                }
                else
                {
                    MainWindow.UpdateText("Launcher timed out.");
                    KillExistingInstances("Star Trek Online.exe");
                }

                if (pageId == "null")
                {
                    Log.Error("PageId is null.");
                    MainWindow.UpdateText("Please try again.");
                    return;
                }
                
                string webSocketUrl = $"ws://localhost:{GlobalVariables.DebugPort}/devtools/page/{pageId}";
                GlobalVariables.WebSocketUrl = webSocketUrl;
                Log.Debug($"WebSocket URL: {GlobalVariables.WebSocketUrl}");
                MainWindow.UpdateText($"WebSocket URL: {GlobalVariables.WebSocketUrl}");
                
                WebSocketManager.InitWebSocket(GlobalVariables.WebSocketUrl);
                
                MainWindow.UpdateText("Done.", Brushes.Green);
                Log.Info("Api initialized.");

                await page.CloseAsync();
                await BrowserManager.Browser.CloseAsync();
                BrowserManager.Browser = null;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message + ex.StackTrace);
            }
        }

        public static async Task InitApi()
        {
            Log.Debug("Called InitApi");
            
            if (GlobalVariables.LauncherPath == "null")
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
                    Log.Debug(GlobalVariables.LauncherPath);
                }
            }
            
            SaveSettings();
            Log.Debug(GlobalVariables.LauncherPath);

            Random random = new Random();
            int randomNum = random.Next(500, 65535);
            GlobalVariables.DebugPort = randomNum;
            Log.Debug(GlobalVariables.DebugPort);
            
            if (!File.Exists(GlobalVariables.LauncherPath))
            {
                return;
            }
            
            var processStartInfo = new ProcessStartInfo
            {
                FileName = GlobalVariables.LauncherPath,
                Arguments = $"--remote-debugging-port={GlobalVariables.DebugPort}",
            };

            Process.Start(processStartInfo);
            Log.Debug($"Trying to start launcher with {processStartInfo.Arguments}");
            
            await Task.Delay(TimeSpan.FromSeconds(GlobalVariables.WaitInterval));
            await BrowserManager.InitBrowser();
            await Locate();
        }
    }
}