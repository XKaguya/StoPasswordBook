using System.Net.Http;
using log4net;

namespace StoPasswordBook.Generic;

public class HttpClientManager
{
    private static readonly ILog Log = LogManager.GetLogger(typeof(HttpClientManager));

    public static HttpClient? HttpClient { get; set; } = null;

    public static async Task<bool> InitBrowser()
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
                    return false;
                }
            }
            else
            {
                Log.Error("HttpClient is already initialized.");
                return true;
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex.Message + ex.StackTrace);
            MainWindow.UpdateText("ERROR Initializing HTTP client. Please check the log.");
            return false;
        }

        return false;
    }
}