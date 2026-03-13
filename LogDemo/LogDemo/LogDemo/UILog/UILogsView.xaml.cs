

using System.Windows;
using System.Windows.Controls;

namespace LogDemo.UILog
{
    /// <summary>
    /// Interaction logic for UILogsView.xaml
    /// </summary>
    public partial class UILogsView:UserControl
    {
        public UILogsViewModel ViewModel
        {
            get => (UILogsViewModel)DataContext;
            set => DataContext = value;
        }

        public UILogsView() => InitializeComponent();
        // 自动滚动到最新项

        // 自动滚动到最新项
        private void ContentPresenter_Loaded(object sender, RoutedEventArgs e)
        {
            var presenter = sender as ContentPresenter;
            logScrollViewer.ScrollToBottom();
        }


    }
}