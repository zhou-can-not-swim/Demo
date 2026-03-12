using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TcpServer.TaskManager;

// DownLoadMethod.cs
namespace TcpServer
{
    public class DownLoadMethod
    {
        //DI
        private IUserService _userService;

        private Thread thread;
        public string TaskId { get; private set; }
        public bool IsBegin { get; private set; }

        private DownLoadTool _tool;
        public User CurrentUser { get; private set; }

        private DownloadTaskManager _taskManager;
        private long _downloadedBytes;
        private long _totalBytes;
        public DownloadStatus Status { get; private set; }

        //定义日志，返回给上一级
        public event Action<string, double> methodLogMessage;
        



        public DownLoadMethod()
        {
            _userService = new UserService();
            _tool = new DownLoadTool();
            _taskManager = new DownloadTaskManager();

            // 订阅进度事件
            _tool.ProgressChanged += OnProgressChanged;
            _tool.StatusChanged += OnStatusChanged;
        }

        #region 一个thread
        public void StartDownload(User user)
        {
            if (user == null) return;

            CurrentUser = user;
            TaskId = Guid.NewGuid().ToString();

            // 检查是否有未完成的下载任务
            var savedTask = _taskManager.GetTask(user.Id);
            if (savedTask != null && savedTask.Status == DownloadStatus.Paused)//如果当前任务处于暂停转台
            {
                _downloadedBytes = savedTask.DownloadedBytes;
                _totalBytes = savedTask.TotalBytes;
                Status = DownloadStatus.Paused;
            }
            thread = new Thread(async () =>
            {
                try
                {
                    thread.IsBackground = true;
                    IsBegin = true;
                    Status = DownloadStatus.Downloading;

                    // 更新json任务状态
                    UpdateTaskInfo();

                    await _tool.DownloadWithProgressAsync(
                        user,
                        _downloadedBytes,
                        _taskManager);
                }
                catch (Exception ex)
                {
                    Status = DownloadStatus.Failed;
                    UpdateTaskInfo();
                }
                finally
                {
                    IsBegin = false;
                }
            });

            thread.Start();
        }
        #endregion

        #region 跟具体的下载过程有关
        //从下载的过程中Invock,到这里更新json信息
        private void OnProgressChanged(int userId, long downloadedBytes, long totalBytes, double progress)
        {
            _downloadedBytes = downloadedBytes;
            _totalBytes = totalBytes;
            var user = _userService.GetUserByIdAsync(userId);
            methodLogMessage.Invoke(user.Result?.Name??userId.ToString(), progress);
            // 更新json任务信息
            UpdateTaskInfo();
        }

        private void OnStatusChanged(int userId, DownloadStatus status)
        {
            Status = status;
            UpdateTaskInfo();

            // 如果完成或取消，清理进度文件
            if (status == DownloadStatus.Completed || status == DownloadStatus.Cancelled)
            {
                _taskManager.RemoveTask(userId);
            }
        }

        #endregion

        //将当前从下载过程中的数据添加到json文件中去
        private void UpdateTaskInfo()
        {
            if (CurrentUser == null) return;

            var taskInfo = new DownloadTaskInfo
            {
                UserId = CurrentUser.Id,
                UserName = CurrentUser.Name,
                DownloadUrl = CurrentUser.DownLoadUrl,
                SavePath = CurrentUser.SavePath,
                TotalBytes = _totalBytes,
                DownloadedBytes = _downloadedBytes,
                Progress = _totalBytes > 0 ? (double)_downloadedBytes / _totalBytes * 100 : 0,
                Status = Status,
                LastUpdateTime = DateTime.Now,
                TaskId = TaskId
            };

            _taskManager.AddOrUpdateTask(taskInfo);
        }

        //将当前pause状态放到json文件去
        public void PauseDownload()
        {
            if (CurrentUser != null)
            {
                Status = DownloadStatus.Paused;
                UpdateTaskInfo();
                _tool.Pause();
            }
        }

        //直接恢复下载
        public void ResumeDownload()
        {
            if (CurrentUser != null)
            {
                // 重新开始下载（会使用已保存的进度）
                StartDownload(CurrentUser);
            }
        }

        //将task状态改为cancel并从json文件中删除
        public void CancelDownload()
        {
            if (CurrentUser != null)
            {
                _tool.Cancel();
                Status = DownloadStatus.Cancelled;
                UpdateTaskInfo();
                _taskManager.RemoveTask(CurrentUser.Id);
                CurrentUser = null;
            }
        }
    }
}
