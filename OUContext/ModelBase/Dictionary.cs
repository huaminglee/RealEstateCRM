using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
namespace OUDAL
{
    public class Dictionary
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(50)]
        [DisplayName("名称")]
        public string Name { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [DisplayName("类别")]
        [MaxLength(50)]
        public string Catalog { get; set; }

        public int KeyId { get; set; }
        //public virtual ICollection<DictionaryItem> Items { get; set; }
    }
    
}
