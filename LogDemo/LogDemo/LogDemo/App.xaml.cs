using LogDemo.UILog;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using System.Data;
using System.Windows;

namespace LogDemo
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; }

        private void OnStartup(object sender, StartupEventArgs e)
        {
            var serviceCollection = new ServiceCollection();

            ConfigureServices(serviceCollection);

            ServiceProvider = serviceCollection.BuildServiceProvider();

            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<UILogsViewModel>();
            services.AddSingleton<UILogsView>();


            var configuration = new ConfigurationBuilder().Build();

            services.AddSingleton<IConfiguration>(configuration);

            // 先注册日志
            services.AddLogging();
            services.AddMediatR(cfg => {
                cfg.RegisterServicesFromAssembly(typeof(App).Assembly);
            });
            services.AddSingleton<MainWindow>();
        }
    }

}
