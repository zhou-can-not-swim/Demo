using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TcpServer
{
    public class User
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? DownLoadUrl { get; set; }
        public string? SavePath { get; set; }
        public bool IsActive { get; set; }

        //按钮状态
        public bool IsDownLoading { get; set; } = true;
        public bool IsPausing { get; set; } = false;
        public readonly bool IsDeleting = true;
    }
}
