using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TcpServer
{
    public partial class User: ObservableObject
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? DownLoadUrl { get; set; }
        public string? SavePath { get; set; }
        public bool IsActive { get; set; }

        //按钮状态
        [ObservableProperty]
        public bool isOpenDownLoading  = true;
        [ObservableProperty]
        public bool isOpenPausing= false;
    }
}
