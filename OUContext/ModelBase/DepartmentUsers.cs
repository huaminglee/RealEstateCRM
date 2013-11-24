using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
namespace OUDAL
{
    public class DepartmentUser
    {
        public int UserId { get; set; }
        public int DepartmentId { get; set; }
        [Required]
        public int IsManager { get; set; }
    }
    
}
