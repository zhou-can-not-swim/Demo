using HtmlAgilityPack;
using Microsoft.Playwright;
using System.IO;
using System.Net.Http;

namespace MovieDemo
{
    // 电影网站检测服务（核心类）
    public class MovieSiteCheckService : IAsyncDisposable
    {
        private readonly HttpClient _httpClient;
        private IBrowser? _browser;
        private IBrowserContext? _context;
        private IPage? _page;
        private bool _playwrightInitialized = false;

        public MovieSiteCheckService()
        {
            _httpClient = new HttpClient();
            // 模拟浏览器请求，防止被屏蔽
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 Chrome/120.0.0.0 Safari/537.36");
        }

        /// <summary>
        /// 初始化Playwright（在使用动态爬取前调用）
        /// </summary>
        private async Task InitializePlaywrightAsync()
        {
            if (_playwrightInitialized)
            {
                return;
            }
            try
            {
                var playwright = await Playwright.CreateAsync();
                _browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
                {
                    Headless = false, // 设置为false可以看到浏览器窗口
                    SlowMo = 50
                });

                _context = await _browser.NewContextAsync(new BrowserNewContextOptions
                {
                    ViewportSize = new ViewportSize { Width = 1920, Height = 1080 },
                    UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 Chrome/120.0.0.0 Safari/537.36"
                });

                _page = await _context.NewPageAsync();
                _playwrightInitialized = true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Playwright初始化失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 检测网站是否可访问（使用HttpClient快速检测）
        /// </summary>
        public async Task<bool> CheckSiteAvailabilityAsync(string url)
        {
            try
            {
                var response = await _httpClient.GetAsync(url);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// 检测网站是否可访问
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public async Task<bool> CheckSiteAvailabilityWithPlaywrightAsync(string url)
        {
            try
            {
                await InitializePlaywrightAsync();
                var response = await _page!.GotoAsync(url, new PageGotoOptions
                {
                    WaitUntil = WaitUntilState.NetworkIdle,
                    Timeout = 30000
                });
                return response != null && response.Status >= 200 && response.Status < 400;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 爬取电影名称列表（使用Playwright获取动态内容）
        /// </summary>
        /// <param name="url">目标网址</param>
        /// <param name="xpath">XPath表达式，例如："//div[@class='movie-item']/h3/a"</param>
        /// <param name="waitForSelector">等待的选择器，例如：".movie-item"</param>
        /// <returns>电影名称列表</returns>
        public async Task<List<string>> CrawlMovieTitlesAsync(string url, string xpath = "", string waitForSelector = "")
        {
            try
            {
                await InitializePlaywrightAsync();

                // 导航到页面
                Console.WriteLine($"正在访问: {url}");
                await _page!.GotoAsync(url, new PageGotoOptions
                {
                    WaitUntil = WaitUntilState.NetworkIdle, // 等待网络空闲
                    Timeout = 30000
                });

                // 如果有等待选择器，等待动态内容加载
                if (!string.IsNullOrEmpty(waitForSelector))
                {
                    try
                    {
                        await _page.WaitForSelectorAsync(waitForSelector, new PageWaitForSelectorOptions
                        {
                            Timeout = 10000
                        });
                        Console.WriteLine($"成功等待到选择器: {waitForSelector}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"等待选择器超时: {ex.Message}");
                    }
                }

                // 额外等待一段时间，确保动态内容完全加载
                await Task.Delay(2000);

                // 获取页面HTML并保存
                var html = await _page.ContentAsync();
                string savePath = @"D:\test.html";
                await File.WriteAllTextAsync(savePath, html, System.Text.Encoding.UTF8);
                Console.WriteLine($"页面已保存到: {savePath}");

                var titles = new List<string>();

                // 方式1：使用XPath（如果提供）
                if (!string.IsNullOrEmpty(xpath))
                {
                    try
                    {
                        var elements = await _page.QuerySelectorAllAsync(xpath);
                        foreach (var element in elements)
                        {
                            var text = await element.TextContentAsync();
                            if (!string.IsNullOrWhiteSpace(text))
                                titles.Add(text.Trim());
                        }

                        if (titles.Any())
                        {
                            Console.WriteLine($"使用XPath找到 {titles.Count} 个电影名称");
                            return titles;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"XPath查询失败: {ex.Message}");
                    }
                }

                // 方式2：使用JavaScript评估（更灵活）
                var jsTitles = await _page.EvaluateAsync<List<string>>(@"
                    () => {
                        // 尝试多种常见的选择器
                        const selectors = [
                            '.movie-item h3 a',
                            '.movie-title',
                            '.film-title',
                            '.title',
                            '[class*=""movie""] h3 a',
                            '[class*=""film""] a',
                            'h3 a'
                        ];
                        
                        for (const selector of selectors) {
                            const items = document.querySelectorAll(selector);
                            if (items.length > 0) {
                                return Array.from(items).map(item => item.textContent.trim());
                            }
                        }
                        
                        // 如果都没找到，返回所有a标签的文本
                        const allLinks = document.querySelectorAll('a');
                        return Array.from(allLinks)
                            .map(a => a.textContent.trim())
                            .filter(text => text.length > 0 && text.length < 100);
                    }
                ");

                titles.AddRange(jsTitles);
                Console.WriteLine($"使用JavaScript找到 {jsTitles.Count} 个标题");

                // 保存到HtmlAgilityPack的HtmlDocument（可选，用于兼容旧代码）
                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                return titles;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"爬取失败: {ex.Message}");
                return new List<string>();
            }
        }

        /// <summary>
        /// 爬取电影名称列表（使用默认配置，兼容旧调用）
        /// </summary>
        public async Task<List<string>> CrawlMovieTitlesAsync(string url)
        {
            // 使用常见的XPath和选择器
            return await CrawlMovieTitlesAsync(url,
                xpath: "//div[contains(@class, 'movie')]//h3/a | //div[contains(@class, 'film')]//h3/a",
                waitForSelector: ".movie-item, .movie, .film-item");
        }

        /// <summary>
        /// 截图功能（可用于调试）
        /// </summary>
        public async Task TakeScreenshotAsync(string url, string savePath)
        {
            try
            {
                await InitializePlaywrightAsync();
                await _page!.GotoAsync(url, new PageGotoOptions
                {
                    WaitUntil = WaitUntilState.NetworkIdle
                });
                await _page.ScreenshotAsync(new PageScreenshotOptions
                {
                    Path = savePath,
                    FullPage = true
                });
                Console.WriteLine($"截图已保存到: {savePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"截图失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 执行自定义JavaScript并返回结果
        /// </summary>
        public async Task<T> ExecuteJavaScriptAsync<T>(string javascript)
        {
            await InitializePlaywrightAsync();
            return await _page!.EvaluateAsync<T>(javascript);
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            if (_page != null)
                await _page.CloseAsync();

            if (_context != null)
                await _context.CloseAsync();

            if (_browser != null)
                await _browser.CloseAsync();

            _httpClient?.Dispose();
        }
    }
}