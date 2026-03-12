using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TcpServer.TaskManager
{
    public class DownloadTaskInfo
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string DownloadUrl { get; set; }
        public string SavePath { get; set; }
        public long TotalBytes { get; set; }
        public long DownloadedBytes { get; set; }
        public double Progress { get; set; }
        public DownloadStatus Status { get; set; }
        public DateTime LastUpdateTime { get; set; }
        public string TaskId { get; set; }
    }

    public enum DownloadStatus
    {
        Pending,      // 等待中
        Downloading,  // 下载中
        Paused,       // 已暂停
        Completed,    // 已完成
        Failed,       // 失败
        Cancelled     // 已取消
    }
}
