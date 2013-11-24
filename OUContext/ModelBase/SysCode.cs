using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
namespace OUDAL
{
    [Table("SysCode")]
    public class SysCode
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(50)]
        [DisplayName("名称")]
        public string Name { get; set; }
        [MaxLength(50)]
        [DisplayName("前缀")]
        public string Prefix { get; set; }
        public int LastIndex { get; set; }
        /// <summary>
        /// 未加锁，未自动db.savechanges ，最好用事务包起来
        /// </summary>
        /// <param name="db"></param>
        /// <param name="name"></param>
        /// <param name="prefix"></param>
        /// <returns></returns>
        public static int GetCurIndex(Context db, string name, string prefix)
        {
            int i=1;
            var query=(from o in db.SysCode where o.Name==name && o.Prefix==prefix select o).FirstOrDefault();
            if (query==null)
            {
                db.SysCode.Add(new SysCode { Name = name, Prefix = prefix, LastIndex = 1 });
            }else
            {
                query.LastIndex++;i=query.LastIndex;
            }
            return i;
        }
        public static string GetCode_ByDate(Context db,string name, DateTime d)
        {
            string prefix = d.ToString("yyyyMMdd");
            int i = GetCurIndex(db, name, prefix);
            return string.Format("{0}{1:0000}", prefix, i);
        }
    }
   
    
}
