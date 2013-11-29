using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OUDAL
{
    public class RoomType
    {
        public int Id { get; set; }
        public int DepartmentId { get; set; }
        [DisplayName("电转访时限")]
        public int TelToVisitDays { get; set; }
        [DisplayName("访转卡时限")]
        public int VisitToCardDays { get; set; }
        [DisplayName("卡转大定时限")]
        public int CardToOrderDays { get; set; }
        [DisplayName("电转访提醒期")]
        public int TelToVisitAheads { get; set; }
        [DisplayName("访转卡提醒期")]
        public int VisitToCardAheads { get; set; }
        [DisplayName("卡转大定提醒期")]
        public int CardToOrderAheads { get; set; }
        [Required]
        [DisplayName("类型名称")]
        public string Name { get; set; }
    }
}
