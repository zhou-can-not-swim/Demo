
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;

namespace TcpServer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly IServiceProvider _serviceProvider;

        public App()
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // 注册服务
            services.AddSingleton<IUserService, UserService>();

            services.AddSingleton<DownLoadPool>();
            services.AddSingleton<MainViewModel>();
            services.AddTransient<DownLoadTool>();
            services.AddTransient<DownLoadMethod>();

            // 注册Views
            services.AddTransient<MainWindow>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }
    }
}
