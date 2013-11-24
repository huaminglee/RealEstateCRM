using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
namespace OUDAL
{
    public class Department
    {
        public int Id { get; set; }
        [Required]
        public int PId { get; set; }
        [Required]
        [MaxLength(50)]
        [DisplayName("名称")]
        public string Name { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [DisplayName("类别")]
        [MaxLength(50)]
        public string DepartmentType { get; set; }

        public Department()
        {
            DepartmentType = "";
        }
    }
    
}
