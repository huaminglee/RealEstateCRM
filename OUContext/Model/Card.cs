using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OUDAL
{
    public class Card
    {
        public static string LogClass = "客户办卡";
        public int Id { set; get; }
        public int ClientId { get; set; }
        public int GroupId { get; set; }
        [DisplayName("小卡时间")]
        public DateTime SmallTime { set; get; }
        [DisplayName("大卡时间")]
        public DateTime? BigTime { get; set; }
        [DisplayName("退卡时间")]
        public DateTime? CancelTime { get; set; }
        [DisplayName("备注")]
        public string Remark { get; set; }
    }
}
