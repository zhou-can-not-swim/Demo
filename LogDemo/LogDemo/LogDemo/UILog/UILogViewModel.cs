using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows;

namespace LogDemo.UILog
{
    public partial class UILogsViewModel : ObservableObject, IDisposable
    {
        // 内部可变集合
        private readonly ObservableCollection<LogMessage> _source = new ObservableCollection<LogMessage>();

        // 外部绑定用只读集合
        public ReadOnlyObservableCollection<LogMessage> Logs { get; }

        public UILogsViewModel()
        {
            Logs = new ReadOnlyObservableCollection<LogMessage>(_source);

            // 初始化测试数据
            _source.Add(new LogMessage
            {
                Timestamp = DateTime.Now,
                Level = LogLevel.Info,
                EventGroup = "TestGroup",
                EventSource = "System",
                Content = "Hello World"
            });

            _source.Add(new LogMessage
            {
                Timestamp = DateTime.Now.AddSeconds(1),
                Level = LogLevel.Error,
                EventGroup = "ErrorGroup",
                EventSource = "System",
                Content = "Something went wrong"
            });
        }

        // 添加新日志的方法
        public void OnNext(LogMessage msg)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                // 保证日志数量不超过1000
                while (_source.Count > 1000)
                    _source.RemoveAt(0);

                _source.Add(msg);
            });
        }

        public void Dispose()
        {
            // 如果有资源释放需求
        }
    }
}