using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
namespace OUDAL
{
    public class Function
    {
        public int Id { get; set; }
        public int Sort { get; set; }
        [Required]
        [MaxLength(50)]
        [DisplayName("父级名称")]
        public string ParentName { get; set; }
        [Required]
        [MaxLength(50)]
        [DisplayName("名称")]
        public string Name { get; set; }

        public static Dictionary<string, int> Functions;
        public static Dictionary<int, string> FunctionInts;
        static Function()
        {
            Init();     
        }

        static public void Init()
        {
            Functions = new Dictionary<string,int >();
            FunctionInts=new Dictionary<int, string>();
            using (Context db = new Context())
            {
                var query = (from o in db.Functions.AsNoTracking() select o) .ToList();
                foreach(Function f in query)
                {
                    if (f.ParentName != "-") f.Name = string.Format("{0}-{1}", f.ParentName, f.Name);//目前只支持2级权限
                    Functions.Add(f.Name, f.Id);
                    FunctionInts.Add(f.Id,f.Name);
                }
            }   
        }
    }  
    
}
