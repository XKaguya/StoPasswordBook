﻿using PuppeteerSharp;
using log4net;

namespace StoPasswordBook.Generic
{
    public class PageHandler
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(PageHandler));
        
        private readonly IPage _page;

        public PageHandler(IPage page)
        {
            _page = page;
        }

        public async Task NavigateTo(string url)
        {
            try
            {
                await _page.GoToAsync(url);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error navigating to {url}: {e.Message}");
            }
        }

        public async Task<IElementHandle?> QuerySelector(string selector)
        {
            try
            {
                return await _page.QuerySelectorAsync(selector);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error querying selector {selector}: {e.Message}");
                return null;
            }
        }
    }
}