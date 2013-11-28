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
        [DisplayName("超期天数")]
        public int OverDays { get; set; }
        [Required]
        [DisplayName("类型名称")]
        public string Name { get; set; }
    }
}
