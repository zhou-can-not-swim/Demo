using LogDemo.UILog;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogDemo.Handler
{
    public class UILogNotification : INotification
    {
        public LogMessage LogMessage { get; set; }

        public UILogNotification()
        {
        }

        public UILogNotification(LogMessage msg)
        {
            LogMessage = msg;
        }
    }
}
