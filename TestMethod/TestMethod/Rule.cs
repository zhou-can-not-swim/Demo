using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestMethod
{
    public class ScrewBoltRule
    {
        public Dictionary<string, ScrewBoltRuleItem> Details { get; set; } = [];
    }

    public class ScrewBoltRuleItem
    {
        public bool IsEnabled { get; set; } = false; // 是否启用
        public bool IsLocalCheck { get; set; } = false; // 是否本地校验
        public int ProgNo { get; set; }
        public int? BackwardProgNo { get; set; }
        public int? SockNo { get; set; }
        public decimal? TorqueMin { get; set; }
        public decimal? TorqueMax { get; set; }
        public decimal? AngleMin { get; set; }
        public decimal? AngleMax { get; set; }
        public int StartOrder { get; set; }
        public int ScrewCount { get; set; }
    }
}
