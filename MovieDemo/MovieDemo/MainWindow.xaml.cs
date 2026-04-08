using System;
using System.Threading.Tasks;
using System.Windows;

namespace MovieDemo
{
    public partial class MainWindow : Window
    {
        private MainViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new MainViewModel();
            this.DataContext = _viewModel;
        }

        protected override async void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            await _viewModel.DisposeAsync();
        }
    }
}