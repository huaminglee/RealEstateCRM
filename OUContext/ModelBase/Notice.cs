using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
namespace OUDAL
{
    public class Notice
    {
        public static string LogClass = "通知";
        public int Id { get; set; }
        [Required]
        [MaxLength(50)]
        [DisplayName("标题")]
        public string Title { get; set; } 
        [DisplayName("内容")]
        public string Content { get; set; }
        
        [DisplayName("创建人")]
        public int PersonId { get; set; }
        [DisplayName("创建时间")]
        public DateTime CreateTime { get; set; }
        [DisplayName("置顶")]
        public bool KeepTop { get; set; }
    }
}