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
                    Log.Info($"Response: {response}");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error processing WebSocket message: {ex.Message}");
            }
        }

        public static bool SetUsernameAndPassword(WebSocket webSocket, string userStr, string pwdStr)
        {
            try
            {
                Random random = new Random();
                int random0 = random.Next(1, 10000);
                int random1 = random.Next(1, 10000);
            
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
                MainWindow.UpdateText("Sent account and passwords to Launcher.", Brushes.CornflowerBlue);
                
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