
using System.IO;
using System.Net.Http;
using TcpServer.TaskManager;

// DownLoadTool.cs
namespace TcpServer
{
    public class DownLoadTool
    {
        private bool _isPaused;
        private bool _isCancelled;
        private TaskCompletionSource<bool> _pauseTcs;
        private readonly object _lockObject = new object();

        // 添加进度通知事件
        public event Action<int, long, long, double> ProgressChanged;
        public event Action<int, DownloadStatus> StatusChanged;

        public async Task DownloadWithProgressAsync(User user,
            long existingBytes = 0, DownloadTaskManager taskManager = null)
        {
            var url=user.DownLoadUrl;
            var userId= user.Id;
            var savePath= user.SavePath;
            _isPaused = false;
            _isCancelled = false;
            _pauseTcs = new TaskCompletionSource<bool>();

            using (HttpClient httpClient = new HttpClient())
            {
                // 设置断点续传的请求头
                if (existingBytes > 0)
                {
                    httpClient.DefaultRequestHeaders.Range = new System.Net.Http.Headers.RangeHeaderValue(existingBytes, null);
                }

                HttpResponseMessage response = await httpClient.GetAsync(url,
                    HttpCompletionOption.ResponseHeadersRead);

                var totalBytes = response.Content.Headers.ContentLength.GetValueOrDefault() + existingBytes;

                StatusChanged?.Invoke(userId, DownloadStatus.Downloading);

                Stream contentStream = null;
                FileStream fileStream = null;

                try
                {
                    contentStream = await response.Content.ReadAsStreamAsync();

                    // 如果是断点续传，以追加方式打开文件
                    FileMode fileMode = existingBytes > 0 ? FileMode.Append : FileMode.Create;
                    fileStream = new FileStream(savePath, fileMode);

                    var buffer = new byte[8192];
                    long bytesRead = existingBytes; // 从已下载字节数开始
                    int bytesReceived;

                    while ((bytesReceived = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        // 检查是否暂停
                        await CheckPauseAsync();

                        // 检查是否取消
                        if (_isCancelled)
                        {
                            StatusChanged?.Invoke(userId, DownloadStatus.Cancelled);
                            return;
                        }

                        await fileStream.WriteAsync(buffer, 0, bytesReceived);
                        bytesRead += bytesReceived;

                        // 计算进度
                        double progress = totalBytes > 0 ? (double)bytesRead / totalBytes * 100 : 0;

                        // 触发进度事件
                        ProgressChanged?.Invoke(userId, bytesRead, totalBytes, progress);

                        // 定期保存进度（例如每1MB保存一次）
                        if (bytesReceived > 0 && taskManager != null && bytesRead % (1024 * 1024) < 8192)
                        {
                            taskManager.SaveDownloadProgress(userId, bytesRead, totalBytes);
                        }
                    }

                    // 下载完成
                    if (bytesRead >= totalBytes)
                    {
                        StatusChanged?.Invoke(userId, DownloadStatus.Completed);
                        taskManager?.CleanupProgressFile(userId);
                    }
                }
                finally
                {
                    fileStream?.Close();
                    contentStream?.Close();
                }
            }
        }

        private async Task CheckPauseAsync()
        {
            while (_isPaused)
            {
                await Task.WhenAny(_pauseTcs.Task, Task.Delay(500));
            }
        }

        public void Pause()
        {
            lock (_lockObject)
            {
                _isPaused = true;
            }
        }

        public void Resume()
        {
            lock (_lockObject)
            {
                if (_isPaused)
                {
                    _isPaused = false;
                    _pauseTcs.TrySetResult(true);
                    _pauseTcs = new TaskCompletionSource<bool>();
                }
            }
        }

        public void Cancel()
        {
            lock (_lockObject)
            {
                _isCancelled = true;
                _isPaused = false;
                _pauseTcs.TrySetResult(true);
            }
        }
    }
}
