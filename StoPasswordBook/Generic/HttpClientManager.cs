using System.Net.Http;
using log4net;

namespace StoPasswordBook.Generic;

public class HttpClientManager
{
    private static readonly ILog Log = LogManager.GetLogger(typeof(HttpClientManager));

    public static HttpClient? HttpClient { get; set; } = null;

    public static async Task InitHttpClient()
    {
        try
        {
            if (HttpClient == null)
            {
                try
                {
                    HttpClient = new HttpClient();
                }
                catch (Exception ex)
                {
                    Log.Error(ex.Message + ex.StackTrace);
                    MainWindow.UpdateText("ERROR Initializing HTTP client. Please check the log.");
                    return;
                }
            }
            else
            {
                Log.Error("HttpClient is already initialized.");
                return;
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex.Message + ex.StackTrace);
            MainWindow.UpdateText("ERROR Initializing HTTP client. Please check the log.");
            return;
        }

        return;
    }
}