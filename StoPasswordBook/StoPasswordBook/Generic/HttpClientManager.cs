using System.Net.Http;
using log4net;

namespace StoPasswordBook.Generic
{
    public class HttpClientManager
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(HttpClientManager));

        public static HttpClient? HttpClient { get; set; } = null;

        private static readonly SemaphoreSlim HttpClientSemaphore = new SemaphoreSlim(1, 1);

        public static async Task InitHttpClient()
        {
            try
            {
                await HttpClientSemaphore.WaitAsync();

                if (HttpClient == null)
                {
                    try
                    {
                        HttpClient = new HttpClient();
                        Log.Info("HttpClient successfully initialized.");
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"Error initializing HttpClient: {ex.Message}\n{ex.StackTrace}");
                        MainWindow.UpdateText("ERROR Initializing HTTP client. Please check the log.");
                        return;
                    }
                }
                else
                {
                    Log.Info("HttpClient is already initialized.");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error initializing HttpClient: {ex.Message}\n{ex.StackTrace}");
                MainWindow.UpdateText("ERROR Initializing HTTP client. Please check the log.");
            }
            finally
            {
                HttpClientSemaphore.Release();
            }
        }
    }
}