using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OUDAL
{
    public class ViewRole
    {
        
        public static string BaseSql= @"select r.id,r.name,r.remark,isnull(count(ru.roleid),0) as num
from roles r left outer join roleusers ru on r.id=ru.roleid
group by r.id,r.name,r.remark
";
        public int id{get;set;}
        public string Name {get;set;}
        public string Remark { get; set; }
        public int Num { get; set; }
    }
}
