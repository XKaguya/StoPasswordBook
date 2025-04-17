using log4net;
using Newtonsoft.Json.Linq;
using StoPasswordBook.Native;
using WebSocketSharp;

namespace StoPasswordBook.Generic;

public class NewWebSocketManager
{
    private static readonly ILog Log = LogManager.GetLogger(typeof(NewWebSocketManager));
    public static WebSocket? WebSocket;
    public static Credential Credential = new Credential();
    
    public static void InitWebSocket(string wsUrl)
    {
        try
        {
            if (WebSocket == null || !WebSocket.IsAlive)
            {
                WebSocket = new WebSocket(wsUrl);
                WebSocket.OnMessage += OnMessage;

                WebSocket.Connect();
                Log.Info($"Initializing WebSocket to {wsUrl}");
            }
        }
        catch (Exception ex)
        {
            Log.Error($"Error initializing WebSocket: {ex.Message}", ex);
        }
    }
    
    private static void OnMessage(object? sender, MessageEventArgs ev)
    {
        try
        {
            JObject response = JObject.Parse(ev.Data);
            ProcessResponse(response);
        }
        catch (Exception ex)
        {
            Log.Error($"Error processing message: {ex.Message}", ex);
        }
    }
    
    private static void ProcessResponse(JObject response)
    {
        if (response["result"] != null && response["result"]["result"] != null)
        {
            var result = response["result"]["result"];
            
            if (result["value"] != null)
            {
                string value = result["value"].ToString();

                if (value == Credential.Password)
                {
                    result["value"] = "";
                    Log.Info($"Received response: {result}");

                    GlobalVariables.SetColumnNum++;
                }
                else if (value == Credential.Username)
                {
                    result["value"] = "";
                    Log.Info($"Received response: {result}");
                    
                    GlobalVariables.SetColumnNum++;
                }
                else
                {
                    Log.Info($"Received response: {result}");
                }
            }
            
            if (result["objectId"] != null)
            {
                GlobalVariables.ObjectId = result["objectId"].ToString();
                Log.Info($"Received response: {result}");
            }
        }
    }
    
    private static async Task SendWebSocketRequestAsync(JObject request)
    {
        try
        {
            if (WebSocket != null && WebSocket.IsAlive)
            {
                WebSocket.Send(request.ToString());
                await Task.Delay(50);
                Log.Info("Request sent: " + request["method"]?.ToString());
            }
            else
            {
                Log.Error("WebSocket is not connected.");
            }
        }
        catch (Exception ex)
        {
            Log.Error($"Error sending request: {ex.Message}", ex);
        }
    }
    
    private static JObject CreateEvaluateRequest(string expression, int id = 0)
    {
        if (id == 0)
        {
            id = Random.Shared.Next(1, 100);
        }
        
        return new JObject
        {
            { "id", id },
            { "method", "Runtime.evaluate" },
            { "params", new JObject
                {
                    { "expression", expression }
                }
            }
        };
    }
    
    private static JObject CreateGetResponseByObjectIdRequest(string objectId, int id = 0)
    {
        if (id == 0)
        {
            id = Random.Shared.Next(1, 100);
        }
        
        return new JObject
        {
            { "id", id },
            { "method", "Runtime.getProperties" },
            { "params", new JObject
                {
                    { "objectId", objectId },
                }
            }
        };
    }
    
    public static async Task GetCurrentCredentials()
    {
        try
        {
            GlobalVariables.SetColumnNum = 0;
            
            string expression = @"document.querySelector('input[name=""username""]').value;";
            var request = CreateEvaluateRequest(expression);
            await SendWebSocketRequestAsync(request);
            
            expression = @"document.querySelector('input[name=""password""]').value;";
            request = CreateEvaluateRequest(expression);
            await SendWebSocketRequestAsync(request);
        }
        catch (Exception e)
        {
            Log.Error($"Error sending evaluate request: {e.Message}", e);
        }
    }
    
    public static async Task SetCredentials(string? userStr, string? pwdStr)
    {
        if (string.IsNullOrEmpty(userStr) || string.IsNullOrEmpty(pwdStr))
        {
            MainWindow.UpdateText("Account or Password is null or empty. Please check Shadow.xml");
            return;
        }
        
        Credential.Username = userStr;
        Credential.Password = pwdStr;

        try
        {
            string expression = $@"
            document.querySelector('input[name=""username""]').value = ""{userStr}"";
            document.querySelector('input[name=""password""]').value = ""{pwdStr}"";";
            var request = CreateEvaluateRequest(expression);
            await SendWebSocketRequestAsync(request);
        }
        catch (Exception e)
        {
            Log.Error($"Error sending evaluate request: {e.Message}", e);
        }
    }
}