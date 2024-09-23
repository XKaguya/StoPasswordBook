using PuppeteerSharp;
using System;
using System.Threading.Tasks;
using log4net;

namespace StoPasswordBook.Generic
{
    public class BrowserManager
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(BrowserManager));
        
        public static IBrowser? Browser { get; set; } = null;

        public static async Task<bool> InitBrowser()
        {
            try
            {
                if (Browser == null)
                {
                    Log.Debug("Attempting to create browser...");

                    await new BrowserFetcher().DownloadAsync();
                    Browser = await Puppeteer.LaunchAsync(new LaunchOptions
                    {
                        Headless = true,
                    });

                    Log.Debug("Browser created.");
                    return true;
                }
                else
                {
                    Log.Debug("Browser is already initialized.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error initializing browser: {ex.Message}");
                return false;
            }
        }
    }
}