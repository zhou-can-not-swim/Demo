using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace MovieDemo
{
    public partial class MainViewModel : ObservableObject, IAsyncDisposable
    {
        private readonly MovieSiteCheckService _checkService = new();
        private CancellationTokenSource _cancellationTokenSource;

        [ObservableProperty]
        private string _siteUrl = "https://v.qq.com";

        [ObservableProperty]
        private string _xPath = "//div[contains(@class, 'movie')]//h3/a | //div[contains(@class, 'film')]//h3/a";

        [ObservableProperty]
        private string _waitSelector = ".movie-item, .movie, .film-item";

        [ObservableProperty]
        private string _statusText = "就绪";

        [ObservableProperty]
        private string _progressText = "准备就绪";

        [ObservableProperty]
        private int _progressValue = 0;

        [ObservableProperty]
        private int _movieCount = 0;

        [ObservableProperty]
        private ObservableCollection<string> _movieList = new();

        [ObservableProperty]
        private Brush _statusColor = Brushes.Black;

        [RelayCommand]
        public async Task StartCheck()
        {
            if (string.IsNullOrWhiteSpace(SiteUrl))
            {
                StatusText = "❌ 请输入网站地址";
                StatusColor = Brushes.Red;
                return;
            }

            StatusText = "正在检测网站...";
            StatusColor = Brushes.Orange;
            ProgressValue = 30;

            try
            {
                // 使用Playwright检测（更准确）
                var isOk = await _checkService.CheckSiteAvailabilityWithPlaywrightAsync(SiteUrl);

                if (isOk)
                {
                    StatusText = $"✅ 网站可访问: {SiteUrl}";
                    StatusColor = Brushes.Green;

                    // 自动获取页面标题
                    var pageTitle = await _checkService.ExecuteJavaScriptAsync<string>("document.title");
                    StatusText += $" - {pageTitle}";
                }
                else
                {
                    StatusText = $"❌ 网站无法访问: {SiteUrl}";
                    StatusColor = Brushes.Red;
                }
            }
            catch (Exception ex)
            {
                StatusText = $"❌ 检测失败: {ex.Message}";
                StatusColor = Brushes.Red;
            }
            finally
            {
                ProgressValue = 100;
                await Task.Delay(500);
                ProgressValue = 0;
            }
        }

        [RelayCommand]
        public async Task StartCrawl()
        {
            if (string.IsNullOrWhiteSpace(SiteUrl))
            {
                StatusText = "❌ 请输入网站地址";
                StatusColor = Brushes.Red;
                return;
            }

            _cancellationTokenSource = new CancellationTokenSource();

            try
            {
                StatusText = "🕷️ 开始爬取电影列表...";
                StatusColor = Brushes.Blue;
                ProgressValue = 10;
                ProgressText = "正在加载页面...";

                // 清空现有列表
                MovieList.Clear();

                // 爬取数据
                var movies = await _checkService.CrawlMovieTitlesAsync(
                    SiteUrl,
                    XPath,
                    WaitSelector);

                // 检查是否取消
                if (_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    StatusText = "⏸️ 爬取已取消";
                    StatusColor = Brushes.Orange;
                    return;
                }

                ProgressValue = 80;
                ProgressText = "正在处理数据...";

                // 添加到列表（模拟渐进式添加）
                for (int i = 0; i < movies.Count; i++)
                {
                    if (_cancellationTokenSource.Token.IsCancellationRequested)
                        break;

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        MovieList.Add(movies[i]);
                        MovieCount = MovieList.Count;
                        ProgressValue = 80 + (i * 20 / movies.Count);
                        ProgressText = $"正在添加: {movies[i]}";
                    });

                    await Task.Delay(10); // 让UI有响应
                }

                ProgressValue = 100;

                if (movies.Count > 0)
                {
                    StatusText = $"✅ 爬取完成！共获取 {movies.Count} 部电影";
                    StatusColor = Brushes.Green;
                    ProgressText = $"完成，共 {movies.Count} 条记录";

                    // 可选：自动保存
                    await AutoSaveAsync(movies);
                }
                else
                {
                    StatusText = "⚠️ 未找到电影列表，请检查XPath或等待选择器设置";
                    StatusColor = Brushes.Orange;
                    ProgressText = "未找到数据";
                }
            }
            catch (OperationCanceledException)
            {
                StatusText = "⏸️ 爬取已取消";
                StatusColor = Brushes.Orange;
            }
            catch (Exception ex)
            {
                StatusText = $"❌ 爬取失败: {ex.Message}";
                StatusColor = Brushes.Red;
                MessageBox.Show($"爬取失败详情:\n{ex.Message}\n\n{ex.StackTrace}",
                    "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                await Task.Delay(1000);
                ProgressValue = 0;
                _cancellationTokenSource?.Dispose();
            }
        }

        [RelayCommand]
        public void StopCrawl()
        {
            _cancellationTokenSource?.Cancel();
            StatusText = "正在停止爬取...";
            StatusColor = Brushes.Orange;
        }

        [RelayCommand]
        public async Task Export()
        {
            if (MovieList.Count == 0)
            {
                MessageBox.Show("没有数据可导出", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var saveDialog = new SaveFileDialog
            {
                Filter = "文本文件|*.txt|CSV文件|*.csv|所有文件|*.*",
                DefaultExt = "txt",
                FileName = $"电影列表_{DateTime.Now:yyyyMMdd_HHmmss}"
            };

            if (saveDialog.ShowDialog() == true)
            {
                try
                {
                    var content = string.Join(Environment.NewLine, MovieList);
                    await File.WriteAllTextAsync(saveDialog.FileName, content, Encoding.UTF8);
                    MessageBox.Show($"导出成功！\n共导出 {MovieList.Count} 条记录\n保存位置：{saveDialog.FileName}",
                        "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"导出失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        [RelayCommand]
        public void ClearList()
        {
            if (MovieList.Count > 0)
            {
                var result = MessageBox.Show($"确定要清空 {MovieList.Count} 条记录吗？",
                    "确认", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    MovieList.Clear();
                    MovieCount = 0;
                    StatusText = "列表已清空";
                    ProgressText = "列表已清空";
                }
            }
        }

        private async Task AutoSaveAsync(List<string> movies)
        {
            try
            {
                string autoSavePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "MovieDemo", $"auto_save_{DateTime.Now:yyyyMMdd}.txt");

                var directory = Path.GetDirectoryName(autoSavePath);
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                var content = $"""
                    爬取时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}
                    网站地址: {SiteUrl}
                    电影数量: {movies.Count}
                    {new string('=', 50)}
                    {string.Join(Environment.NewLine, movies)}
                    """;

                await File.WriteAllTextAsync(autoSavePath, content, Encoding.UTF8);
                Console.WriteLine($"自动保存到: {autoSavePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"自动保存失败: {ex.Message}");
            }
        }

        // 释放资源
        public async ValueTask DisposeAsync()
        {
            await _checkService.DisposeAsync();
        }
    }
}