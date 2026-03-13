using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogDemo.UILog
{
    // 日志消息
    public class LogMessage
    {
        public DateTime Timestamp { get; set; }
        public LogLevel Level { get; set; }
        public string EventGroup { get; set; }
        public string EventSource { get; set; }
        public string Content { get; set; }
    }

    public enum LogLevel
    {
        Info=1,
        Warn,
        Error,
        Success
    }
}
