using LogDemo.UILog;
using MediatR;
using Microsoft.Extensions.Configuration;
using System.Windows;

namespace LogDemo.Handler
{
    public class UILogNotificationHandler : INotificationHandler<UILogNotification>
    {
        private readonly UILogsViewModel _vm;
        private readonly IMediator _mediator;
        private readonly IConfiguration _configuration;

        public UILogNotificationHandler(IMediator mediator, IConfiguration configuration, UILogsViewModel vm)
        {
            _vm = vm;
            _mediator = mediator;
            _configuration = configuration;
        }

        // 明确async Task，避免同步阻塞
        public async Task Handle(UILogNotification notification, CancellationToken cancellationToken)
        {
            // 取消令牌检查，避免任务被取消
            if (cancellationToken.IsCancellationRequested)
                return;

            var msg = notification.LogMessage;
            if (msg != null && msg.Level >= LogLevel.Info)
            {
                // 确保在UI线程执行（原代码已有，补充await）
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    _vm.OnNext(msg);
                });
            }
        }
    }
}
