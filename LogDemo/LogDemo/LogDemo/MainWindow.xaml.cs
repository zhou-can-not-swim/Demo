using LogDemo.Handler;
using LogDemo.UILog;
using MediatR;
using System.Windows;

namespace LogDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        // 从DI容器注入VM，而非手动new
        private readonly UILogsViewModel _logVm;
        private readonly IMediator _mediator;

        // 构造函数注入DI容器中的单例VM
        public MainWindow(IMediator mediator, UILogsViewModel logVm)
        {
            InitializeComponent();
            _mediator = mediator;
            _logVm = logVm;
            logsView.ViewModel = _logVm; // 绑定DI容器中的单例VM
        }

        // 将同步循环改为异步，避免UI阻塞
        private async void moniData(object sender, RoutedEventArgs e)
        {
            int i = 0;
            while (i < 100)
            {
                // 异步Publish，给UI线程喘息时间
                await _mediator.Publish(new UILogNotification(new LogMessage
                {
                    Level = LogLevel.Error,
                    Timestamp = DateTime.Now,
                    EventGroup = "steam++",
                    EventSource = "模拟数据", // 补充缺失的EventSource（原代码未赋值）
                    Content = $"下载进度{i + 1}"
                }));

                if (i % 3 == 0)
                {
                    await _mediator.Publish(new UILogNotification(new LogMessage
                    {
                        Level = LogLevel.Info,
                        Timestamp = DateTime.Now,
                        EventGroup = "utools",
                        EventSource = "模拟数据",
                        Content = $"下载进度{i + 2}"
                    }));

                    if (i % 5 == 0)
                    {
                        await _mediator.Publish(new UILogNotification(new LogMessage
                        {
                            Level = LogLevel.Success,
                            Timestamp = DateTime.Now,
                            EventGroup = "有道",
                            EventSource = "模拟数据",
                            Content = $"下载进度{i + 3}"
                        }));
                    }
                }

                i++;
                // 延迟10ms，避免数据刷屏过快（可选）
                await Task.Delay(10);
            }
        }
    }
    //public partial class MainWindow : Window
    //{
    //    private readonly UILogsViewModel _logVm = new UILogsViewModel();
    //    private readonly IMediator _mediator;
    //    public MainWindow(IMediator mediator)
    //    {
    //        InitializeComponent();
    //        _mediator = mediator;
    //        logsView.ViewModel = _logVm; // logsView 是 XAML 中的 Name="logsView" 的控件
    //    }

    //    private void moniData(object sender, RoutedEventArgs e)
    //    {
    //        int i = 0;
    //        while (i<100)
    //        {
    //            this._mediator.Publish(new UILogNotification(new LogMessage
    //            {
    //                Level = LogLevel.Error,
    //                Timestamp = DateTime.Now,
    //                EventGroup = "steam++",
    //               Content=$"下载进度{i+1}"
    //            }));

    //            if (i % 3==0) {
    //                this._mediator.Publish(new UILogNotification(new LogMessage
    //                {
    //                    Level = LogLevel.Info,
    //                    Timestamp = DateTime.Now,
    //                    EventGroup = "utools",
    //                    Content = $"下载进度{i+2}"

    //                }));

    //                if (i % 5 == 0)
    //                {
    //                    this._mediator.Publish(new UILogNotification(new LogMessage
    //                    {
    //                        Level = LogLevel.Success,
    //                        Timestamp = DateTime.Now,
    //                        EventGroup = "有道",
    //                        Content = $"下载进度{i+3}"

    //                    }));
    //                }
    //            }

    //            i++;

    //        }
    //    }
    //}
}