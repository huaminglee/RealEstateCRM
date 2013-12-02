using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RealEstateCRM.Web.Models;
using OUDAL;
namespace RealEstateCRM.Web.BLL
{
    public class UserInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Role> Roles;
        public List<Department> Departments;
        public int[] Rights;

        public bool HasRight(string right)
        {
            if (Id == 1) return true;
            if (Function.Functions.ContainsKey(right))
            {
                return Rights.Contains(Function.Functions[right]);
            }
            return false;
        }

        public bool HasRight(string right, int storeid)
        {
            bool hasright = false;
            if (Function.Functions.ContainsKey(right))
            {
                hasright = Rights.Contains(Function.Functions[right]);
            }
            if (hasright == false) return false;
            if (storeid == 0) return hasright; //如果业务对象还没有store属性
            foreach (var department in Departments)
            {
                if (department.Id == storeid) return true;
            }
            return false;
        }

        public static UserInfo CurUser
        {
            get
            {
                if (HttpContext.Current.Session["User"] == null)
                {
                    object lockobj = new object();
                    lock (lockobj)
                    {
                        UserInfo user = new UserInfo();
                        using (Context db = new Context())
                        {
                            SystemUser systemuser = null;
                            if (HttpContext.Current.Application["Login"] as string == "password")
                            {
                                int code;
                                if (int.TryParse(HttpContext.Current.User.Identity.Name, out code) == false)
                                    throw new Exception("未登录");
                                systemuser = (from o in db.SystemUsers where o.Id == code select o).FirstOrDefault();
                            }
                            else
                            {
                                int i = HttpContext.Current.User.Identity.Name.LastIndexOf('\\');
                                if (i > 0) i = i + 1;
                                string ad = HttpContext.Current.User.Identity.Name.Substring(i);
                                systemuser = (from o in db.SystemUsers where o.Password == ad select o).FirstOrDefault();
                            }

                            if (systemuser == null) throw new Exception("找不到用户资料");
                            user.Id = systemuser.Id;
                            user.Name = systemuser.Name;
                            user.Roles =
                                (from o in db.RoleUsers
                                    join p in db.Roles.AsNoTracking() on o.RoleId equals p.Id
                                    where o.UserId == user.Id
                                    select p).ToList();
                            user.Departments = (from o in db.DepartmentUsers
                                join p in db.Departments.AsNoTracking() on o.DepartmentId equals p.Id
                                where o.UserId == user.Id
                                select p).ToList();
                            user.Rights = (from o in db.RoleUsers
                                join p in db.Roles on o.RoleId equals p.Id
                                join q in db.RoleFunctions on p.Id equals q.RoleId
                                where o.UserId == user.Id
                                select q.FunctionId).ToArray();
                            if (user.Departments.Count > 0)
                            {
                                user.IsProject = true;
                            }
                            foreach (var department in user.Departments)
                            {
                                if (department.DepartmentType != "门店")
                                {
                                    user.IsProject = false;
                                }
                            }
                            HttpContext.Current.Session["User"] = user;
                        }
                    }
                }
                return HttpContext.Current.Session["User"] as UserInfo;
            }
        }

        /// <summary>
        /// 如果用户只在门店中，则isStore=true
        /// </summary>
        public bool IsProject { get; set; }

        public int GetGroup(int projectId)
        {
            int i = 0;
            (from o in Departments where o.PId == projectId select o).ToList().ForEach(
                o =>
                {
                    i = o.Id;
                });
            return i;
        }
        //ToDo:做静态变量
        public List<Department> GetProjects()
        {
            //List<Department> projects=
            
            List<Department> list = new List<Department>();
            foreach (var d in Departments)
            {
                if (d.Id == 2)//管理部门
                {
                    return DepartmentBLL.GetDepartments("项目");
                }
                if (d.DepartmentType == "项目")
                {
                    addProject(list, d);
                }
                else
                {
                    if (d.PId != 0)
                    {
                        Department p = DepartmentBLL.GetById(d.PId);
                        if (p.DepartmentType == "项目")
                        {
                            addProject(list, p);
                        }
                    }
                }

            }
            return list;
        }

        void addProject(List<Department> list, Department d)
        {
            bool found = false;
            foreach (var i in list)
            {
                if (i.Id == d.Id)
                {
                    found = true;
                }
            }
            if (!found) list.Add(d);
        }
        public ClientViewScopeEnum GetClientRight(int projectid)
        {


            if (HasRight("客户管理-查看所有客户") == true)
            {
                return ClientViewScopeEnum.查看所有;
            }
            else if (HasRight("客户管理-查看项目客户") == true)
            {
                return ClientViewScopeEnum.查看项目;
            }
            else if (HasRight("客户管理-") == true)
            {
                return ClientViewScopeEnum.查看本组;
            }
            else
            {
                return ClientViewScopeEnum.无;
            }

        }
    }

}