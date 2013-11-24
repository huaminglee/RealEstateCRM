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
                var query = (from o in db.SystemUsers orderby o.Name select new { o.Id, o.Name });
                    
                foreach (dynamic o in query)
                {
                    list.Add(new SelectListItem{ Text=o.Name, Value=o.Id.ToString() ,Selected=defaultValue==o.Id});
                }
                return list;
            }
        }


        
    }
}