using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
namespace OUDAL
{
    public class DictionaryItem
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(50)]
        [DisplayName("名称")]
        public string Name { get; set; }
        public int DictId { get; set; }
        public int IndexNum { get; set; }
        //[ForeignKey("DictId")]
        //[InverseProperty("Items")]
        //[NonSerialized]
        //public virtual Dictionary Dict { get; set; }
    }
    
}
