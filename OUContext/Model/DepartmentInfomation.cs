using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OUDAL
{
    public class DepartmentInfomation
    {
        public int Id { get; set; }
        public int DepartmentId { get; set; }
        [DisplayName("产品类型")]
        public string RoomType { get; set; }
    }
}
