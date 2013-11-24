using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using OUDAL;
namespace RealEstateCRM.Web.BLL
{
    public class Utilities
    {
        public static string FileRoot = System.IO.Path.Combine(HttpRuntime.AppDomainAppPath, "Excel");
        
        public static string Binary2String(byte[] binary)
        {           
            System.Data.Linq.Binary bin = new System.Data.Linq.Binary(binary);

            string s=bin.ToString();
            return s;
            //return s.Substring(1,s.Length-2);//bin.tostring= \"AAAXXX\"
        }
        public static bool BinaryCompare(byte[] binary,string str)
        {
            System.Data.Linq.Binary bin = new System.Data.Linq.Binary(binary);
            return string.Equals(bin.ToString(), str);
        }
        public static List<SelectListItem> Enum2SelectList(Type enumType,string defaultValue,bool AppendBlank)
        {
            string[] list=Enum.GetNames(enumType);
            List<SelectListItem> items = new List<SelectListItem>();
            if(AppendBlank==true){
                SelectListItem item =new SelectListItem();
                item.Text="";item.Value="";
                items.Add(item);
            }
            foreach (string s in list)
            {
                SelectListItem item = new SelectListItem();
                item.Text = s; item.Value = s; item.Selected = s == defaultValue;
                items.Add(item);
            }
            return items;
        }
        public static List<SelectListItem> IdName2SelectList(List<IdName> list , int defaultValue=0, bool appendBlank=false)
        {
            List<SelectListItem> items = new List<SelectListItem>();
            if (appendBlank == true)
            {
                SelectListItem item = new SelectListItem();
                item.Text = ""; item.Value = "";
                items.Add(item);
            }
            foreach (IdName s in list)
            {
                SelectListItem item = new SelectListItem();
                item.Text = s.Name; item.Value = s.Id.ToString(); item.Selected = s.Id == defaultValue;
                items.Add(item);
            }
            return items;
        }
        public static List<SelectListItem> Enum2SelectList(Type enumType, string defaultValue)
        {
            return Enum2SelectList(enumType, defaultValue, true);
        }

        public static void AddLogAndSave( int id, string logClass, string logAction, string info)
        {
            OUDAL.Context db=new OUDAL.Context();
            OUDAL.AccessLog.AddLogAndSave(db, RealEstateCRM.Web.BLL.UserInfo.CurUser.Name, id, logClass, logAction, info);
        }
        
        public static void AddLog(Context db, int id, string logClass, string logAction, string info)
        {
            OUDAL.AccessLog.AddLog(db, RealEstateCRM.Web.BLL.UserInfo.CurUser.Name, id, logClass, logAction, info);
        }
        public static List<OUDAL.AccessLog> GetLogByClass(int key, string logClass)
        {
            return OUDAL.AccessLog.GetByClass(key, logClass);
        }
        public static bool CheckRight(params string[] rolenames)
        {
            //RealEstateCRM.Web.Models.User u = RealEstateCRM.Web.Models.User.GetAccount();
            //foreach (OUDAL.AdRole r in u.Roles)
            //{
            //    foreach (string s in rolenames)
            //    {
            //        if (r.RoleName == s) return true;
            //    }
            //}
            return false;
        }
        public static ActionResult NoRight()
        {
            return new RedirectResult("~/Content/AccessDeny.htm");
        }
        /// <summary>
        /// return codename+code
        /// </summary>
        /// <param name="db"></param>
        /// <param name="codeName"></param>
        /// <returns></returns>
        public static int GetSysCode(OUDAL.Context db, string codeName)
        {
            
            System.Data.SqlClient.SqlParameter p2=new System.Data.SqlClient.SqlParameter();
            p2.ParameterName="@value";
            p2.Direction=System.Data.ParameterDirection.Output;
            p2.Size = 100;
            p2.SqlDbType = System.Data.SqlDbType.Int;
            db.Database.ExecuteSqlCommand("exec sp_get_syscode @codename,@value output", new System.Data.SqlClient.SqlParameter("@codename", codeName), p2);
            return (int)p2.Value ;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="name"></param>
        /// <param name="sql"></param>
        /// <param name="column"> like i.name</param>
        static public void AddSqlFilterLike(FormCollection collection, string name, ref string sql, string column, List<object> parameters)
        {
            if (collection[name] != null && collection[name] != "")
            {
                parameters.Add(new SqlParameter(name, string.Format("%{0}%", collection[name])));
                sql += string.Format(" and {0} like @{1}", column, name);
            }

        }
        static public void AddSqlFilterLikePinYin(FormCollection collection, string name, ref string sql, string column, List<object> parameters)
        {
            if (collection[name] != null && collection[name] != "")
            {
                parameters.Add(new SqlParameter(name, string.Format("%{0}%", collection[name])));
                sql += string.Format(" and ({0} like @{1} or pinyin like @{1})", column, name);
            }

        }
        static public void AddSqlFilterEqual(FormCollection collection, string name, ref string sql, string column, List<object> parameters)
        {
            if (collection[name] != null && collection[name] != "")
            {
                parameters.Add(new SqlParameter(name, collection[name]));
                sql += string.Format(" and {0}=@{1}", column, name);
            }
        }
        static public void AddSqlFilterDateGreaterThen(FormCollection collection, string name, ref string sql, string column, List<object> parameters)
        {
            if (collection[name] != null && collection[name] != "")
            {
                parameters.Add(new SqlParameter(name, collection[name]));
                sql += string.Format(" and {0} >= @{1}", column, name);
            }
        }
        static public void AddSqlFilterDateLessThen(FormCollection collection, string name, ref string sql, string column, List<object> parameters)
        {
            if (collection[name] != null && collection[name] != "")
            {
                parameters.Add(new SqlParameter(name, collection[name]));
                sql += string.Format(" and {0} <= @{1}", column, name);
            }
        }
        static public void AddSqlFilterTimeLessThen(FormCollection collection, string name, ref string sql, string column, List<object> parameters)
        {
            if (collection[name] != null && collection[name] != "")
            {
                DateTime d;
                DateTime.TryParse(collection[name], out d);

                parameters.Add(new SqlParameter(name, d.AddDays(1).ToShortDateString()));
                sql += string.Format(" and {0} < @{1}", column, name);
            }
        }
        /// <summary>
        /// 传进来的int参数用,分割
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="name"></param>
        /// <param name="sql"></param>
        /// <param name="column"></param>
        static public void AddSqlFilterInInts(FormCollection collection, string name, ref string sql, string column)
        {
            if (collection[name] != null && collection[name] != "")
            {
                string[] list = collection[name].Split(',');
                if(list.Count()==0){}
                else if(list.Count()==1)
                {
                    int i = 0;
                    if(int.TryParse(collection[name],out i))
                    {
                        sql += string.Format(" and {0}={1}", column, collection[name]);
                    }
                }
                else
                {
                    StringBuilder sb=new StringBuilder(" "+column+" in(");
                    int j = 0;
                    foreach (var s in list)
                    {
                        int i = 0;
                        if(int.TryParse(s,out i))
                        {
                            if (j > 0) sb.Append(",");
                            sb.Append(s);
                        }
                        j++;
                    }
                    sb.Append(")");
                    sql += sb.ToString();
                }
            }
        }
        static public int GetCurMallId()
        {
            string url = HttpContext.Current.Request.Url.AbsolutePath.ToLower();
            int i = url.IndexOf("/city/");
            
            if (i >= 0)
            {
                int j = url.IndexOf('/', i + 6);
                if (j < 0) { j = i + 6 + 1; }

                string s = url.Substring(i + 6, j - i - 6);
                int id;
                int.TryParse(s, out id);
                return id;
                
            }
            return 0;
        }
        static public string GetCurUnitName(string url)
        {
            int i = url.ToLower().IndexOf("/city/");
            if(i>=0)
            {
                int j = url.IndexOf('/', i + 6);
                if (j < 0) { j = i+6+1; }

                string s = url.Substring(i + 6, j - i - 6);
                switch (s)
                {
                    case "2":
                        return "总部";
                    default:
                        return "";
                }
            }
            return "系统管理";
        }
        static public string EnumToString(Type enumType,int v)
        {
            if(Enum.IsDefined(enumType,v))
            {
                return Enum.GetName(enumType, v);
            }
            return v.ToString();
        }
        static public string EnumToString(Type enumType, string v)
        {
            int val = int.Parse(v);
            if (Enum.IsDefined(enumType, val))
            {
                return Enum.GetName(enumType, val);
            }
            return v;
        }
       
    }
}