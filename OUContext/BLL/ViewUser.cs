using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OUDAL
{
    public class ViewUser
    {
        
        public static string BaseSql= @"select id,loginname,name,state,password,email from systemusers u where 1=1";
        public int id{get;set;}
        public string Name {get;set;}
        public string Email { get; set; }
        public int State { get; set; }
        public string LoginName { get; set; }
        public string Roles { get; set; }
        public string Departments { get; set; }
    }
}
