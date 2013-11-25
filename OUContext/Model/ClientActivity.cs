using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OUDAL
{
    public class ClientActivity
    {
        public static string LogClass = "联系记录";
        public int Id { get; set; }
        public int ClientId { get; set; }
        [DisplayName("类型")]
        public string Type { get; set; }
        [DisplayName("邀约时间")]
        public DateTime? PlanTime { get; set; }
        [DisplayName("实际时间")]
        public DateTime? ActualTime { get; set; }
        [DisplayName("情况说明")]
        public string Detail { get; set; }
        [DisplayName("完成情况")]
        public bool? IsDone { get; set; }
    }
}
