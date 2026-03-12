using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Printing;
using System.Reactive;
using System.Text.Json.Serialization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using TcpServer.TaskManager;

namespace TcpServer
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly IUserService _userService;

        //线程安全的字典
        private ConcurrentDictionary<int, DownLoadMethod> _downloadTasks = new();//初始化完成后，自动创建字典key和val，val类型是线程安全的DownLoadMethod
        private DownloadTaskManager _taskManager;

        [ObservableProperty]
        private string statusMessage = "就绪";

        [ObservableProperty]
        private string searchText = string.Empty;

        [ObservableProperty]
        private bool isLoading;


        public ObservableCollection<User> Users { get; set; } = new();

        public MainViewModel(IUserService userService)
        {

            _userService = userService;
            _taskManager = new DownloadTaskManager();
            
            LoadUsersCommand.ExecuteAsync(null);//初始化的时候就调用LoadUser

            if (Application.Current != null)
            {
                // 使用 Dispatcher 确保在UI线程上访问
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (Application.Current.MainWindow != null)
                    {
                        Application.Current.MainWindow.Closing += OnMainWindowClosing;
                    }
                    else
                    {
                        // 如果 MainWindow 还不存在，等它加载完成后再订阅
                        Application.Current.Activated += OnApplicationActivated;
                    }
                });
            }
        }
        private void OnApplicationActivated(object? sender, EventArgs e)
        {
            if (Application.Current != null && Application.Current.MainWindow != null)
            {
                Application.Current.Activated -= OnApplicationActivated;
                Application.Current.MainWindow.Closing += OnMainWindowClosing;
            }
        }
        private void OnMainWindowClosing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            foreach (var task in _downloadTasks.Values)
            {
                if (task.Status == DownloadStatus.Downloading)
                {
                    //task.PauseDownload();
                }
            }
        }
        

        #region 初始化加载列表，加载进程
        [RelayCommand]
        private async Task LoadUsers()
        {

            var users = await _userService.GetUsersAsync();

            Application.Current.Dispatcher.Invoke(() =>
            {
                Users.Clear();
                foreach (var user in users)
                {
                    Users.Add(user);
                }
            });

            // 恢复未完成的任务
            RestorePendingTasks();

            StatusMessage = $"已加载 {users.Count} 个用户";
        }

        // 恢复未完成的任务
        private void RestorePendingTasks()
        {
            var pendingTasks = _taskManager.GetAllTasks()
                .Where(t => t.Status == DownloadStatus.Paused || t.Status == DownloadStatus.Downloading);//status=2,3

            if (pendingTasks!=null)
            {
                foreach (var taskInfo in pendingTasks)
                {
                    //找到对应的一行列表数据
                    var user = Users.FirstOrDefault(u => u.Id == taskInfo.UserId);
                    if (user != null)
                    {
                        var downloadMethod = GetOrCreateDownloadTask(user);
                        // 更新下载进度显示
                        StatusMessage = $"发现未完成的下载任务: {user.Name} ({taskInfo.Progress:F1}%)";
                    }
                }
            }
            
        }
        private DownLoadMethod GetOrCreateDownloadTask(User user)
        {
            //添加DownLoadMethod到对应的任务中去
            return _downloadTasks.GetOrAdd(user.Id, _ => new DownLoadMethod());
        }
        #endregion
        [RelayCommand]
        private void DownLoad(User? user)
        {
            user.IsOpenDownLoading = false;
            user.IsOpenPausing = true;
            if (user == null) return;

            //拿/创建任务到全局_downloadTasks
            var task = GetOrCreateDownloadTask(user);
            task.StartDownload(user);
            task.methodLogMessage += UpdateLogMessage;
            StatusMessage = $"开始下载: {user.Name}";
        }

        [RelayCommand]
        private void Pause(User? user)
        {
            user.IsOpenDownLoading = true;
            user.IsOpenPausing = false;
            if (user == null) return;
            
            if (_downloadTasks.TryGetValue(user.Id, out var task))
            {
                task.PauseDownload();
                StatusMessage = $"已暂停: {user.Name}";
            }
        }
        
        [RelayCommand]
        private void Resume(User? user)
        {
            if (user == null) return;
            
            var taskInfo = _taskManager.GetTask(user.Id);
            if (taskInfo != null && (taskInfo.Status == DownloadStatus.Paused || taskInfo.Status == DownloadStatus.Downloading))
            {
                var task = GetOrCreateDownloadTask(user);
                task.ResumeDownload();
                StatusMessage = $"已恢复: {user.Name}";
            }
        }
        
        [RelayCommand]
        private void CancelDownload(User? user)
        {
            if (user == null) return;
            
            if (_downloadTasks.TryRemove(user.Id, out var task))
            {
                task.CancelDownload();
                StatusMessage = $"已取消下载: {user.Name}";
            }
        }

        [RelayCommand]
        private async Task Delete(User? user)
        {
            if (user == null) return;
            
            // 先取消下载
            if (_downloadTasks.TryRemove(user.Id, out var task))
            {
                task.CancelDownload();
            }
            
            // 清理任务信息
            _taskManager.RemoveTask(user.Id);
            _taskManager.CleanupProgressFile(user.Id);

            var result = MessageBox.Show(
                $"确定要删除用户 {user.Name} 吗？",
                "确认删除",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                var deleteResult = await _userService.DeleteUserAsync(user.Id);
                if (deleteResult)
                {
                    await LoadUsers();
                    StatusMessage = $"用户 {user.Name} 已删除";
                }
            }
        }

        public void UpdateLogMessage(string name, double p)
        {
            StatusMessage = $"{name}的下载进度为{p}";
        }

    }
}