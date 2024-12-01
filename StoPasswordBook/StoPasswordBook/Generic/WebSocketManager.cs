using System.Windows.Media;
using log4net;
using WebSocketSharp;
using Newtonsoft.Json;

namespace StoPasswordBook.Generic
{
    public class WebSocketManager
    {
        private static int? _documentNodeId = 1;
        private static readonly ILog Log = LogManager.GetLogger(typeof(WebSocketManager));
        private static string[] LastAccount { get; set; } = { "null", "null" };
        public static WebSocket? WebSocket = null;

        public static void InitWebSocket(string wsUrl)
        {
            if (WebSocket == null)
            {
                WebSocket = new WebSocket(wsUrl);
                WebSocket.OnMessage += OnMessage;
                WebSocket.Connect();
                Log.Info($"Initializing new WebSocket to {wsUrl}");
            }

            if (WebSocket.IsAlive == false)
            {
                WebSocket = new WebSocket(wsUrl);
                WebSocket.OnMessage += OnMessage;
                WebSocket.Connect();
                Log.Info($"Initializing new but not first WebSocket to {wsUrl}");
            }
        }
        
        private static void OnMessage(object? sender, MessageEventArgs ev)
        {
            try
            {
                var response = JsonConvert.DeserializeObject<dynamic>(ev.Data);
                if (response != null)
                {
                    string rst = response.ToString();
                    if (LastAccount[0] != "null" && LastAccount[1] != "null")
                    {
                        if (rst.Contains(LastAccount[0]))
                        {
                            rst = rst.Replace(LastAccount[0], "ACCOUNT_HIDDEN_DUE_TO_PRIVACY");
                            Log.Info($"Response From Username Column: {rst}");
                            
                            MainWindow.UpdateText("Sent account and passwords to Launcher.", Brushes.CornflowerBlue);
                            return;
                        }

                        if (rst.Contains(LastAccount[1]))
                        {
                            rst = rst.Replace(LastAccount[1], "PASSWORD_HIDDEN_DUE_TO_PRIVACY");
                            Log.Info($"Response From Password Column: {rst}");
                            
                            MainWindow.UpdateText("Sent account and passwords to Launcher.", Brushes.CornflowerBlue);
                            return;
                        }
                    }
                    
                    Log.Info($"Response: {rst}");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error processing WebSocket message: {ex.Message}");
            }
        }

        public static bool SetUsernameAndPassword(WebSocket? webSocket, string? userStr, string? pwdStr)
        {
            try
            {
                if (webSocket == null)
                {
                    MainWindow.UpdateText("Websocket is null. Please try again.");
                    return false;
                }

                if (string.IsNullOrEmpty(userStr) || string.IsNullOrEmpty(pwdStr))
                {
                    MainWindow.UpdateText("Account or Password is null or empty. Please check Shadow.xml");
                    return false;
                }
                
                Random random = new Random();
                int random0 = random.Next(1, 10000);
                int random1 = random.Next(1, 10000);
                LastAccount[0] = userStr;
                LastAccount[1] = pwdStr;
            
                var setUsername = new
                {
                    id = random0,
                    method = "Runtime.evaluate",
                    @params = new
                    {
                        expression = $"document.querySelector('input[name=\"username\"]').value = \"{userStr}\";"
                    }
                };
                webSocket.Send(JsonConvert.SerializeObject(setUsername));

                var setPassword = new
                {
                    id = random1,
                    method = "Runtime.evaluate",
                    @params = new
                    {
                        expression = $"document.querySelector('input[name=\"password\"]').value = \"{pwdStr}\";"
                    }
                };
                webSocket.Send(JsonConvert.SerializeObject(setPassword));
                
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message + ex.StackTrace);
                MainWindow.UpdateText($"{ex.Message}", Brushes.Red);
                return false;
            }
        }
    }
}