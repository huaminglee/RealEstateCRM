using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
namespace OUDAL
{
    public class Role
    {
        public int Id { get; set; }        
        [Required]
        [MaxLength(50)]
        [DisplayName("名称")]
        public string Name { get; set; }
        [MaxLength(50)]
        [DisplayName("备注")]
        public string Remark { get; set; }
        public Role()
        {
            Remark = "";
        }
    }
    
}
