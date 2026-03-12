using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace TcpServer.TaskManager
{
    public class DownloadTaskManager
    {
        private readonly string _configPath;
        private readonly string _downloadInfoPath;
        private ConcurrentDictionary<int, DownloadTaskInfo> _downloadTasks;
        private readonly object _lockObj = new object();

        public DownloadTaskManager()
        {
            string appDataPath=Environment.CurrentDirectory;
            if (!Directory.Exists(appDataPath))
            {
                Directory.CreateDirectory(appDataPath);
            }

            _configPath = Path.Combine(appDataPath, "download_tasks.json");
            _downloadInfoPath = Path.Combine(appDataPath, "downloads");

            if (!Directory.Exists(_downloadInfoPath))
            {
                Directory.CreateDirectory(_downloadInfoPath);
            }

            LoadTasks();
        }

        // 加载所有保存的任务
        private void LoadTasks()
        {
            try
            {
                if (File.Exists(_configPath))
                {
                    string json = File.ReadAllText(_configPath);
                    _downloadTasks = JsonSerializer.Deserialize<ConcurrentDictionary<int, DownloadTaskInfo>>(json)//json字符串->对象
                        ?? new ConcurrentDictionary<int, DownloadTaskInfo>();
                }
                else
                {
                    _downloadTasks = new ConcurrentDictionary<int, DownloadTaskInfo>();
                }
            }
            catch
            {
                _downloadTasks = new ConcurrentDictionary<int, DownloadTaskInfo>();
            }
        }

        // 保存任务到配置文件
        public void SaveTasks()
        {
            try
            {
                lock (_lockObj)
                {
                    string json = JsonSerializer.Serialize(_downloadTasks, new JsonSerializerOptions
                    {
                        WriteIndented = true
                    });
                    File.WriteAllText(_configPath, json);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"保存任务失败: {ex.Message}");
            }
        }

        // 添加或更新任务
        public void AddOrUpdateTask(DownloadTaskInfo taskInfo)
        {
            _downloadTasks.AddOrUpdate(taskInfo.UserId, taskInfo, (key, oldValue) => taskInfo);
            SaveTasks();
        }

        // 获取任务
        public DownloadTaskInfo GetTask(int userId)
        {
            _downloadTasks.TryGetValue(userId, out var task);
            return task;
        }

        // 获取所有任务
        public List<DownloadTaskInfo> GetAllTasks()
        {
            return _downloadTasks.Values.ToList();
        }

        // 移除任务
        public bool RemoveTask(int userId)
        {
            var result = _downloadTasks.TryRemove(userId, out _);
            if (result)
            {
                SaveTasks();
            }
            return result;
        }

        // 保存下载进度信息到单独的文件
        public void SaveDownloadProgress(int userId, long downloadedBytes, long totalBytes)
        {
            string progressFile = Path.Combine(_downloadInfoPath, $"progress_{userId}.dat");
            try
            {
                var progress = new
                {
                    UserId = userId,
                    DownloadedBytes = downloadedBytes,
                    TotalBytes = totalBytes,
                    LastUpdate = DateTime.Now
                };

                string json = JsonSerializer.Serialize(progress);
                File.WriteAllText(progressFile, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"保存进度失败: {ex.Message}");
            }
        }

        // 加载下载进度
        public (long downloadedBytes, long totalBytes) LoadDownloadProgress(int userId)
        {
            string progressFile = Path.Combine(_downloadInfoPath, $"progress_{userId}.dat");
            try
            {
                if (File.Exists(progressFile))
                {
                    string json = File.ReadAllText(progressFile);
                    var progress = JsonSerializer.Deserialize<dynamic>(json);
                    return (progress.GetProperty("DownloadedBytes").GetInt64(),
                            progress.GetProperty("TotalBytes").GetInt64());
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"加载进度失败: {ex.Message}");
            }
            return (0, 0);
        }

        // 清理下载进度文件
        public void CleanupProgressFile(int userId)
        {
            string progressFile = Path.Combine(_downloadInfoPath, $"progress_{userId}.dat");
            try
            {
                if (File.Exists(progressFile))
                {
                    File.Delete(progressFile);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"清理进度文件失败: {ex.Message}");
            }
        }
    }
}