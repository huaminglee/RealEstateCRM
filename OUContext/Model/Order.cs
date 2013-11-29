using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OUDAL
{
    public class Order
    {
        public static string LogClass = "大定签约";
        public int Id { set; get; }
        public int ClientId { get; set; }
        public int GroupId { get; set; }
        [DisplayName("房间")]
        public string Room { get; set; }
        [DisplayName("大定时间")]
        public DateTime OrderTime { set; get; }
        [DisplayName("签约时间")]
        public DateTime? SignTime { get; set; }
        [DisplayName("取消时间")]
        public DateTime? CancelTime { get; set; }
        [DisplayName("备注")]
        public string Remark { get; set; }
    }
}
