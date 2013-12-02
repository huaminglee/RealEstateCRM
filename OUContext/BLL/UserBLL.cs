using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
namespace OUDAL
{
    
    public class UserBLL
    {
        static Context db = new Context();
        static public List<SystemUser> Users;
        static UserBLL()
        {
            UpdateUsers();   
        }
        public static void UpdateUsers()
        {
            Users = (from o in db.SystemUsers.AsNoTracking() select o).ToList();
        }
        public static SystemUser GetById(int id)
        {
            return Users.Find(o => o.Id == id);
        }
        public static string GetNameById(int? id)
        {
            if (id == null) return "";
            SystemUser u = GetById((int)id);
            if (u == null) return "";
            return u.Name;
        }
        public static List<SelectListItem> GetUsers(int? defaultValue)
        {
            List<SelectListItem> list = new List<SelectListItem>();
            using(Context db=new Context())
            {
                var query = (from o in db.SystemUsers orderby o.LoginName select new { o.Id, Name = o.Name });
                    
                foreach (dynamic o in query)
                {
                    list.Add(new SelectListItem{ Text=o.Name, Value=o.Id.ToString() ,Selected=defaultValue==o.Id});
                }
                return list;
            }
        }

        public static List<IdName> GetSales()
        {
            List<IdName> list = new List<IdName>();
            using (Context db = new Context())
            {
                var query =
                    db.Database.SqlQuery<IdName>(
                        @"select u.id,u.name from systemusers u join roleusers ru on ru.userid=u.id
join rolefunctions rf on rf.roleid=ru.roleid join functions f on f.id=rf.functionid where f.ParentName='客户管理' and f.name='查看级别:本人客户'")
                        .ToList();
               
               return query;
            }
        }

        
    }
}