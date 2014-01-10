using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OUDAL
{
    public class Performance
    {
        public static string LogClass = "项目指标";
        public int Id { set; get; }
        public int ProjectId { get; set; }
        public string RoomType { get; set; }
        [DisplayName("指标开始日期")]
        public DateTime BeginDate { set; get; }
        [DisplayName("来电数")]
        public int CallInNum { get; set; }
        [DisplayName("电转访数")]
        public int CallVisitNum { get; set; }
        [DisplayName("来访数")]
        public int VisitNum { get; set; }
        [DisplayName("办卡数")]
        public int CardNum { get; set; }
        [DisplayName("大定数")]
        public int OrderNum { get; set; }
        [DisplayName("备注")]
        public string Remark { get; set; }
    }
}
