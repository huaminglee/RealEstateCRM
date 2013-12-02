using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using OUDAL;
using RealEstateCRM.Web.BLL;
using RealEstateCRM.Web.Models;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
namespace RealEstateCRM.Web.Controllers
{
    public class SystemController : BaseController
    {
        //
        // GET: /System/
        private Context db = new Context();
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult DepartmentView()
        {
            if (!UserInfo.CurUser.HasRight("系统管理-部门管理")) return Redirect("~/content/AccessDeny.htm");
            return View();
        }
        //[HttpPost]
        //public JsonResult DepartmentJson()
        //{
        //    Models.Result result = new Models.Result();
        //    var list = from o in db.Departments select new TreeNode { dictId = o.Id, name = o.Name, pId = o.PId, tag = o.DepartmentType,open =o.PId==0};
        //    result.success = true;
        //    result.obj = list;

        //    return Json(result);
        //}
        [HttpPost]
        public JsonResult DepartmentJson(int parentid = 0)
        {
            Models.Result result = new Models.Result();
            result.success = true;
            if (parentid == 0)
            {
                result.obj = from o in DepartmentBLL.Departments select new TreeNode { id = o.Id, name = o.Name, pId = o.PId, tag = o.DepartmentType, open = o.PId == 0 };
            }
            else
            {
                result.obj = from o in DepartmentBLL.GetDepartmentByParent(parentid) select new TreeNode { id = o.Id, name = o.Name, pId = o.PId, tag = o.DepartmentType, open = o.PId == 0 };

            }
            return Json(result);
        }
        /// <summary>
        /// 未完成
        /// </summary>
        /// <param name="rootId"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult PartDepartmentJson(int rootId)
        {
            Models.Result result = new Models.Result();
            if (rootId == 0) rootId = 1;
            List<TreeNode> list = new List<TreeNode>();
            var root = (from o in DepartmentBLL.Departments where o.Id == rootId select o).FirstOrDefault();
            if (root != null)
            {
                list.Add(new TreeNode { id = root.Id, name = root.Name, pId = root.PId, tag = root.DepartmentType, open = true });
            }
            result.success = true;
            result.obj = list;

            return Json(result);
        }
        [HttpPost]
        public JsonResult UpdateDepartment(int id, FormCollection collection)
        {
            if (!UserInfo.CurUser.HasRight("系统管理-部门管理")) return Json(new Result { success = false, obj = "没有权限" });
            Models.Result result = new Models.Result();
            var dept = (from o in db.Departments where o.Id == id select o).FirstOrDefault();
            if (dept == null)
            {
                dept = new Department();
                dept.PId = int.Parse(collection["pid"]);
                db.Departments.Add(dept);
            }
            dept.Name = collection["name"];
            dept.DepartmentType = collection["departmenttype"];
            if (dept.DepartmentType == null) dept.DepartmentType = "";
            db.SaveChanges();
            DepartmentBLL.UpdateDepartments();
            result.success = true;
            result.obj = dept;
            return Json(result);
        }
        [HttpPost]
        public JsonResult UpdateDepartmentParent(int id, int pid)
        {
            if (!UserInfo.CurUser.HasRight("系统管理-部门管理")) return Json(new Result { success = false, obj = "没有权限" });
            Models.Result result = new Models.Result();
            var dept = (from o in db.Departments where o.Id == id select o).FirstOrDefault();
            if (dept == null)
            {
                result.success = false;
                result.obj = "找不到部门";
            }
            else if (DepartmentBLL.IsParnet(id, pid))
            {
                result.success = false;
                result.obj = "不能拖动到子部门下";
            }
            else
            {
                dept.PId = pid;
                db.SaveChanges();
                DepartmentBLL.UpdateDepartments();
                result.success = true;
            }
            return Json(result);
        }
        [HttpPost]
        public JsonResult DeleteDepartment(int id, FormCollection collection)
        {
            if (!UserInfo.CurUser.HasRight("系统管理-部门管理")) return Json(new Result { success = false, obj = "没有权限" });
            Models.Result result = new Models.Result();
            var dept = (from o in db.Departments where o.Id == id select o).FirstOrDefault();
            if (dept == null)
            {
                result.success = false;
                result.obj = "找不到节点";
            }
            else
            {
                if ((from o in db.Departments where o.PId == id select o).Count() > 0)
                {
                    result.success = false;
                    result.obj = "要删除的节点有子节点，请先删除子节点";
                }
                else
                {
                    db.Database.ExecuteSqlCommand(string.Format("delete departmentusers where departmentid={0}", id));
                    db.Departments.Remove(dept);
                    db.SaveChanges();
                    DepartmentBLL.UpdateDepartments();
                    result.success = true;
                }
            }
            return Json(result);
        }
        public ActionResult Test()
        {
            Function.Init();
            DepartmentBLL.UpdateDepartments();
            UserBLL.UpdateUsers();
            return View();
        }
        [HttpPost]
        public JsonResult DepartmentUser(int id)
        {
            if (!UserInfo.CurUser.HasRight("系统管理-部门管理")) return Json(new Result { success = false, obj = "没有权限" });
            Models.Result result = new Models.Result();
            var list = from o in db.DepartmentUsers join u in db.SystemUsers on o.UserId equals u.Id where o.DepartmentId == id select new { userid = o.UserId, username = u.LoginName, workno = u.Name, ismanager = o.IsManager };
            result.success = true;
            result.obj = list;
            return Json(result);
        }
        [HttpPost]

        public JsonResult DepartmentAddUser(int id, int userid)
        {
            if (!UserInfo.CurUser.HasRight("系统管理-部门管理")) return Json(new Result { success = false, obj = "没有权限" });
            Models.Result result = new Models.Result();
            DepartmentUser du = (from o in db.DepartmentUsers where o.DepartmentId == id && o.UserId == userid select o).FirstOrDefault();
            if (du == null)
            {
                du = new DepartmentUser { UserId = userid, DepartmentId = id };
                db.DepartmentUsers.Add(du);
                db.SaveChanges();
                DepartmentBLL.UpdateDepartments();
            }
            result.success = true;
            return Json(result);
        }
        [HttpPost]
        public JsonResult DepartmentRemoveUser(int id, FormCollection collection)
        {
            if (!UserInfo.CurUser.HasRight("系统管理-部门管理")) return Json(new Result { success = false, obj = "没有权限" });
            Models.Result result = new Models.Result();
            string users = collection["users[]"];
            string[] userlist = users.Split(',');
            foreach (string s in userlist)
            {
                int i; int.TryParse(s, out i);
                if (i > 0)
                {
                    DepartmentUser du = (from o in db.DepartmentUsers where o.DepartmentId == id && o.UserId == i select o).FirstOrDefault();
                    if (du != null)
                    {
                        db.DepartmentUsers.Remove(du);
                    }
                }
            }
            db.SaveChanges();
            DepartmentBLL.UpdateDepartments();
            result.success = true;
            return Json(result);
        }
        [HttpPost]
        public JsonResult DepartmentSetManager(int id, FormCollection collection)
        {
            if (!UserInfo.CurUser.HasRight("系统管理-部门管理")) return Json(new Result { success = false, obj = "没有权限" });
            Models.Result result = new Models.Result();
            string managers = collection["managers[]"];
            string[] managerlist = managers.Split(',');
            foreach (string s in managerlist)
            {
                int i; int.TryParse(s, out i);
                if (i > 0)
                {
                    DepartmentUser du = (from o in db.DepartmentUsers where o.DepartmentId == id && o.UserId == i select o).FirstOrDefault();
                    if (du != null)
                    {
                        du.IsManager = 1;
                    }
                }
            }
            db.SaveChanges(); DepartmentBLL.UpdateDepartments();
            result.success = true;
            return Json(result);
        }
        [HttpPost]
        public JsonResult DepartmentRemoveManager(int id, FormCollection collection)
        {
            if (!UserInfo.CurUser.HasRight("系统管理-部门管理")) return Json(new Result { success = false, obj = "没有权限" });
            Models.Result result = new Models.Result();
            string managers = collection["managers[]"];
            string[] managerlist = managers.Split(',');
            foreach (string s in managerlist)
            {
                int i; int.TryParse(s, out i);
                if (i > 0)
                {
                    DepartmentUser du = (from o in db.DepartmentUsers where o.DepartmentId == id && o.UserId == i select o).FirstOrDefault();
                    if (du != null)
                    {
                        du.IsManager = 0;
                    }
                }
            }
            db.SaveChanges(); DepartmentBLL.UpdateDepartments();
            result.success = true;
            return Json(result);
        }
        public ActionResult DepartmentUserEdit(int id)
        {
            if (!UserInfo.CurUser.HasRight("系统管理-部门管理")) return Redirect("~/content/AccessDeny.htm");
            Department d = (from o in db.Departments where o.Id == id select o).FirstOrDefault();
            return View(d);
        }


        /*---------------- Role ----------------------------*/
        public ActionResult RoleIndex()
        {
            return View();
        }
        [HttpPost]
        public JsonResult RoleQuery(string sidx, string sord, int page, int rows, FormCollection collection)
        {
            if (!UserInfo.CurUser.HasRight("系统管理-部门管理")) return Json(new Result { success = false, obj = "没有权限" });
            string s = ViewRole.BaseSql;
            string order = "";
            if (sidx != "")
            {
                order = sidx + " " + sord;
                s += " order by " + order;
            }
            var query = db.Database.SqlQuery<ViewRole>(s).ToArray();
            string name = collection["name"] + "";
            if (name.Length > 0)
            {
                query = (from o in query where o.Name.Contains(name) select o).ToArray();
            }
            int totalrow = query.Count();
            int pagenum = (totalrow - totalrow % rows) / rows + 1;
            var newquery = (from o in query select o).Take(rows * page).Skip(page * rows - rows).ToList();// 这种写法是在内存中运算
            //int records = newquery.Count();
            var jsonData = new
            {
                total = pagenum,
                page = page,
                records = totalrow,
                rows = newquery
            };
            return Json(jsonData);
        }

        public ActionResult RoleView(int id)
        {
            if (!UserInfo.CurUser.HasRight("系统管理-角色管理")) return Redirect("~/content/AccessDeny.htm");
            Role r = (from o in db.Roles where o.Id == id select o).FirstOrDefault();
            if (r == null)
            {
                r = new Role();
            }
            else
            {
                ViewBag.Funtions = (from o in db.RoleFunctions where o.RoleId == id select o).ToList();
                ViewBag.Users = (from o in db.RoleUsers where o.RoleId == id select o).ToList();
            }
            return View(r);
        }
        public ActionResult RoleEdit(int id)
        {
            if (!UserInfo.CurUser.HasRight("系统管理-角色管理")) return Redirect("~/content/AccessDeny.htm");
            Role r = (from o in db.Roles where o.Id == id select o).FirstOrDefault();
            if (r == null) r = new Role();
            return View(r);
        }
        [HttpPost]
        public ActionResult RoleEdit(int id, string name, string remark, FormCollection collection)
        {
            System.Threading.Thread.Sleep(10000);
            if (!UserInfo.CurUser.HasRight("系统管理-角色管理")) return Redirect("~/content/AccessDeny.htm");
            Role r = (from o in db.Roles where o.Id == id select o).FirstOrDefault();
            if (r == null)
            {
                r = new Role(); db.Roles.Add(r);
            }
            r.Name = name;
            r.Remark = remark;
            db.SaveChanges();
            if (id == 0)
            {
                return Redirect("../RoleView/" + r.Id);
            }
            else
            {
                return Redirect("~/content/close.htm");
            }

        }
        public ActionResult RoleFunctionEdit(int id)
        {
            if (!UserInfo.CurUser.HasRight("系统管理-角色管理")) return Redirect("~/content/AccessDeny.htm");
            var list = (from o in db.RoleFunctions where o.RoleId == id select o).ToList();
            return View(list);
        }
        [HttpPost]
        public ActionResult RoleFunctionEdit(int id, FormCollection collection)
        {
            if (!UserInfo.CurUser.HasRight("系统管理-角色管理")) return Redirect("~/content/AccessDeny.htm");
            Role r = (from o in db.Roles where o.Id == id select o).FirstOrDefault();
            if (r == null)
            {
                r = new Role(); db.Roles.Add(r);
            }
            (from o in db.RoleFunctions where o.RoleId == id select o).ToList().ForEach(o => db.RoleFunctions.Remove(o));
            string[] rights = collection["rights"].Split(',');
            foreach (string s in rights)
            {
                int i; int.TryParse(s, out i);
                if (i > 0)
                {
                    db.RoleFunctions.Add(new RoleFunction { RoleId = id, FunctionId = i });
                }
            }
            db.SaveChanges();

            return Redirect("~/content/close.htm");

        }
        [HttpPost]
        public JsonResult DeleteRole(int id, FormCollection collection)
        {
            if (!UserInfo.CurUser.HasRight("系统管理-部门管理")) return Json(new Result { success = false, obj = "没有权限" });
            Models.Result result = new Models.Result();
            var usercount = (from o in db.RoleUsers where o.RoleId == id select o).Count();
            if (usercount > 0)
            {
                result.success = false;
                result.obj = "已有用户使用此角色，不能删除";
            }
            else
            {
                var r = (from o in db.Roles where o.Id == id select o).FirstOrDefault();
                if (r == null)
                {
                    result.success = false;
                    result.obj = "找不到节点";
                }
                else
                {
                    db.Roles.Remove(r);
                    db.SaveChanges();
                    result.success = true;
                }
            }

            return Json(result);
        }
        [HttpPost]
        public JsonResult GetParentFunctionList(int id)
        {
            Function f = (from o in db.Functions where o.Id == id select o).FirstOrDefault();
            var functionList = (from o in db.Functions where o.Name == f.ParentName orderby o.Sort select o.Id).FirstOrDefault();
            return Json(JsonConvert.SerializeObject(functionList));
        }
        /*----------------------User------------------------------*/
        public ActionResult UserIndex()
        {
            if (!UserInfo.CurUser.HasRight("系统管理-用户管理")) return Redirect("~/content/AccessDeny.htm");
            return View();
        }
        [HttpPost]
        public JsonResult UserQuery(string sidx, string sord, int page, int rows, FormCollection collection)
        {
            if (!UserInfo.CurUser.HasRight("系统管理-用户管理")) return Json(new Result { success = false, obj = "没有权限" });
            List<object> parameters = new List<object>();
            string s = ViewUser.BaseSql;

            string role = collection["role"] + "";
            string department = collection["department"] + "";
            BLL.Utilities.AddSqlFilterLike(collection, "Name", ref s, "u.name", parameters);
            if (role != "")
            {
                s += " and u.Id in (select userid from roleusers ru join roles r on ru.roleid=r.dictId where r.name like @Role)";
                parameters.Add(new SqlParameter("Role", string.Format("%{0}%", role)));
            }
            if (department != "")
            {
                s += " and u.Id in (select userid from departmentusers du join departments d on du.departmentid=d.Id  where d.name like @Department)";
                parameters.Add(new SqlParameter("Department", string.Format("%{0}%", department)));
            }
            string nodept = collection["NoDept"] + "";
            if (nodept == "true")
            {
                s += " and not exists (select 1 from departmentusers du where du.userid=u.Id)";
            }
            bool state1; bool state2;
            bool.TryParse(collection["State1"], out state1); bool.TryParse(collection["State2"], out state2);
            if (state1 ^ state2)
            {
                if (state1) s += " and u.state=0";
                if (state2) s += " and u.state=1";
            }
            string order = "";
            if (sidx != "")
            {
                order = sidx + " " + sord;
                s += " order by " + order;
            }
            var query = db.Database.SqlQuery<ViewUser>(s, parameters.ToArray()).ToArray();

            int totalrow = query.Count();
            int pagenum = (totalrow - totalrow % rows) / rows + 1;
            var newquery = (from o in query select o).Take(rows * page).Skip(page * rows - rows).ToList();// 这种写法是在内存中运算
            foreach (ViewUser u in newquery)
            {
                var q1 = (from o in db.DepartmentUsers from p in db.Departments where o.UserId == u.id && o.DepartmentId == p.Id select p.Name);
                foreach (string departname in q1)
                {
                    if (u.Departments == null)
                    {
                        u.Departments = departname;
                    }
                    else
                    {
                        u.Departments += "," + departname;
                    }
                }
                var q2 = (from o in db.RoleUsers from p in db.Roles where o.UserId == u.id && o.RoleId == p.Id select p.Name);
                foreach (string rolename in q2)
                {
                    if (u.Roles == null)
                    {
                        u.Roles = rolename;
                    }
                    else
                    {
                        u.Roles += "," + rolename;
                    }
                }
            }
            //int records = newquery.Count();
            var jsonData = new
            {
                total = pagenum,
                page = page,
                records = totalrow,
                rows = newquery
            };
            return Json(jsonData);
        }

        public ActionResult UserView(int id)
        {
            if (!UserInfo.CurUser.HasRight("系统管理-用户管理")) return Redirect("~/content/AccessDeny.htm");
            if (!UserInfo.CurUser.HasRight("系统管理-用户管理")) return Redirect("~/content/AccessDeny.htm");
            SystemUser r = (from o in db.SystemUsers where o.Id == id select o).FirstOrDefault();
            ViewBag.Role1 = ""; ViewBag.Role2 = ""; ViewBag.Role3 = "";
            if (r == null)
            {
                r = new SystemUser();
            }
            else
            {
                var roles = (from o in db.RoleUsers from p in db.Roles where o.UserId == id && p.Id == o.RoleId select p.Name).ToList();
                if (roles.Count > 0) ViewBag.Role1 = roles[0];
                if (roles.Count > 1) ViewBag.Role2 = roles[1];
                if (roles.Count > 2) ViewBag.Role3 = roles[2];
            }
            return View(r);
        }
        public ActionResult UserEdit(int id)
        {
            if (!UserInfo.CurUser.HasRight("系统管理-用户管理")) return Redirect("~/content/AccessDeny.htm");
            SystemUser r = (from o in db.SystemUsers where o.Id == id select o).FirstOrDefault();
            ViewBag.Role1 = 0; ViewBag.Role2 = 0; ViewBag.Role3 = 0;
            if (r == null)
            {
                r = new SystemUser();
            }
            else
            {
                var roles = (from o in db.RoleUsers where o.UserId == id select o.RoleId).ToList();
                if (roles.Count > 0) ViewBag.Role1 = roles[0];
                if (roles.Count > 1) ViewBag.Role2 = roles[1];
                if (roles.Count > 2) ViewBag.Role3 = roles[2];
            }
            r.Password = null;
            return View(r);
        }
        [HttpPost]
        public ActionResult UserEdit(int id,string loginname, string name, string password, int state, int role1, int role2, int role3, string email,  FormCollection collection)
        {
            if (!UserInfo.CurUser.HasRight("系统管理-用户管理")) return Redirect("~/content/AccessDeny.htm");
            SystemUser r = db.SystemUsers.Find(id);
            if (r == null) r = new SystemUser();
            
            if (loginname.Length == 0)
            {
                ModelState.AddModelError("LoginName", "用户姓名不能为空");
            }
            if (name.Length == 0)
            {
                ModelState.AddModelError("Name", "用户姓名不能为空");
            }
            if (role1 == 0 && role2 == 0 && role3 == 0)
            {
                ModelState.AddModelError("Role", "请选择至少一个角色");
            }if (id == 0 && string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("Password", "请输入密码");
            }else  if (name != r.LoginName && string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("Password", "修改用户名时，需要重置密码");
            }
            string checkuser =
                (from o in db.SystemUsers where o.Id != id && o.LoginName == loginname select name).FirstOrDefault();
            if (checkuser != null)
            {
                ModelState.AddModelError("Name", "该用户已存在");
            }
            
            if (ModelState.IsValid == false)
            {
                ViewBag.Role1 = role1;
                ViewBag.Role2 = role2;
                ViewBag.Role3 = role3;
                r.Password = null;
                return View(r);
            }
            else
            {
                
                SystemUser vo = new SystemUser();
                vo.LoginName = loginname;
                vo.Password = password;
                    vo.State = state;
                    vo.Name = name;
                    vo.Email = email;
                r.Save(db, vo);
                
                var query = (from o in db.RoleUsers where o.UserId == r.Id select o);
                foreach (RoleUser ru in query) db.RoleUsers.Remove(ru);
                if (role1 != 0) db.RoleUsers.Add(new RoleUser { UserId = r.Id, RoleId = role1 });
                if (role2 != 0 && role2 != role1) db.RoleUsers.Add(new RoleUser { UserId = r.Id, RoleId = role2 });
                if (role3 != 0 && role3 != role2 && role3 != role1) db.RoleUsers.Add(new RoleUser { UserId = r.Id, RoleId = role3 });
                db.SaveChanges();
                UserBLL.UpdateUsers();
                return Redirect("../UserView/" + r.Id);
                
            }

        }
        [HttpPost]
        public JsonResult DeleteUser(int id, FormCollection collection)
        {
            if (!UserInfo.CurUser.HasRight("系统管理-部门管理")) return Json(new Result { success = false, obj = "没有权限" });
            Models.Result result = new Models.Result();
            result.success = true;
            //var usercount = (from o in db.Plans where o.UserId == dictId select o).Count();
            //if (usercount > 0)
            //{
            //    result.success = false;
            //    result.obj = "已有计划，不能删除";
            //}
            //var auditcount = (from o in db.PlanAudits where o.Auditor == dictId select o).Count();
            //if (auditcount > 0)
            //{
            //    result.success = false;
            //    result.obj = "曾经做过计划考评，不能删除";
            //}

            if (result.success == true)
            {
                var r = (from o in db.SystemUsers where o.Id == id select o).FirstOrDefault();
                if (r == null)
                {
                    result.success = false;
                    result.obj = "找不到用户";
                }
                else
                {
                    db.SystemUsers.Remove(r);
                    db.SaveChanges();
                }
            }

            return Json(result);
        }
        //public ActionResult SynUser()
        //{
        //    return View("");
        //}
        //[HttpPost]
        //public ActionResult SynUser(FormCollection collection)
        //{
        //    UserSyn syn = new UserSyn();
        //    string s = syn.Syn();
        //    return View(s as object);
        //}
        /// <summary>
        /// 选择用户控件使用
        /// </summary>
        /// <returns></returns>
        public ActionResult SelectUser()
        {
            return View();
        }
        /// <summary>
        /// 选择用户控件使用
        /// </summary>
        /// <param name="sidx"></param>
        /// <param name="sord"></param>
        /// <param name="page"></param>
        /// <param name="rows"></param>
        /// <param name="collection"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult SelectUserQuery(string sidx, string sord, int page, int rows, FormCollection collection)
        {

            List<object> parameters = new List<object>();
            var query = (from o in db.SystemUsers
                         join p in db.DepartmentUsers on o.Id equals p.UserId
                         join q in db.Departments on p.DepartmentId equals q.Id
                         select new { id = o.Id, Name = o.LoginName, Department = q.Name, DeptId = q.Id });

            string name = collection["name"] + ""; ;
            string depart = collection["department"] + "";
            string departmentid = collection["departmentid"] + "";//url传来参数
            string mallidstring = collection["mallid"] + "";
            int mallid = 0;
            int.TryParse(mallidstring, out mallid);
            if (name != "")
            {
                query = (from o in query where o.Name.Contains(name) select o);
            }
            if (depart != "")
            {
                query = (from o in query where o.Department.Contains(depart) select o);
            }
            int departmentId = 0;
            if (int.TryParse(departmentid, out departmentId))
            {
                query = (from o in query where o.DeptId == departmentId select o);
            }
            string order = "";
            if (sidx != "")
            {
                order = sidx + " " + sord;
                query = (from o in query orderby order select o);
            }
            var list = query.ToList();
            if (mallid > 0)
            {
                list = (from o in list where DepartmentBLL.IsParnet(mallid, o.DeptId) select o).ToList();
            }

            int totalrow = list.Count();
            int pagenum = (totalrow - totalrow % rows) / rows + 1;
            var newquery = (from o in list select new UserName { id = o.id, Name = o.Name, Department = o.Department }).Take(rows * page).Skip(page * rows - rows).ToList();// 这种写法是在内存中运算

            var jsonData = new
            {
                total = pagenum,
                page = page,
                records = totalrow,
                rows = newquery
            };
            return Json(jsonData);
        }
        public ActionResult Dictionary()
        {
            if (!UserInfo.CurUser.HasRight("系统管理-数据字典")) return Redirect("~/content/AccessDeny.htm");
            var list = (from o in db.Dictionaries orderby o.Catalog select o).ToList();
            System.Collections.Generic.Dictionary<string, List<Dictionary>> dictList = new Dictionary<string, List<Dictionary>>();
            //OUDAL.Dictionary curDict = null;
            foreach (var dict in list)
            {
                if (!dictList.ContainsKey(dict.Catalog))
                {
                    dictList.Add(dict.Catalog, new List<Dictionary>());
                }
                dictList[dict.Catalog].Add(dict);
            }
            return View(dictList);
        }
        [HttpPost]
        public JsonResult GetDictionaryItems(int dictId)
        {
            if (!UserInfo.CurUser.HasRight("系统管理-数据字典")) return Json(new Result { obj = "没有权限" });
            Result result = new Result();
            var query = (from o in db.DictionaryItems where o.DictId == dictId orderby o.IndexNum select o).ToList();
            result.success = true;
            result.obj = query;
            return Json(result);
        }
        [HttpPost]
        public JsonResult AddDictionaryItem(int dictId, string text)
        {
            if (!UserInfo.CurUser.HasRight("系统管理-数据字典")) return Json(new Result { obj = "没有权限" });
            Result result = new Result();

            var query = (from o in db.DictionaryItems where o.DictId == dictId orderby o.IndexNum select o).ToList();
            int next = 0;
            if (query.Count > 0) next = query[query.Count - 1].IndexNum + 1;
            DictionaryItem item = new DictionaryItem { DictId = dictId, Name = text, IndexNum = next };
            db.DictionaryItems.Add(item);
            db.SaveChanges();
            query.Add(item);
            result.success = true;
            result.obj = query;
            return Json(result);
        }
        [HttpPost]
        public JsonResult EditDictionaryItem(int itemId, string text)
        {
            if (!UserInfo.CurUser.HasRight("系统管理-数据字典")) return Json(new Result { obj = "没有权限" });
            Result result = new Result();
            DictionaryItem item = db.DictionaryItems.Find(itemId);
            if (item == null)
            {
                result.obj = "找不到条目";
            }
            else
            {
                item.Name = text;
                db.SaveChanges();
                result.success = true;
            } return Json(result);
        }
        [HttpPost]
        public JsonResult DeleteDictionaryItem(int itemId)
        {
            if (!UserInfo.CurUser.HasRight("系统管理-数据字典")) return Json(new Result { obj = "没有权限" });
            Result result = new Result();
            DictionaryItem item = db.DictionaryItems.Find(itemId);
            if (item == null)
            {
                result.obj = "找不到条目";
            }
            else
            {
                int dictId = item.DictId;
                db.DictionaryItems.Remove(item);
                db.SaveChanges();
                var query = (from o in db.DictionaryItems where o.DictId == dictId orderby o.IndexNum select o).ToList();
                int next = 0;
                if (query.Count > 0) next = query[query.Count - 1].IndexNum + 1;


                result.success = true;
                result.obj = query;
            } return Json(result);
        }
        public JsonResult ChangeDictionaryItemIndex(int itemId, int increase)
        {
            if (!UserInfo.CurUser.HasRight("系统管理-数据字典")) return Json(new Result { obj = "没有权限" });
            Result result = new Result();
            DictionaryItem item = db.DictionaryItems.Find(itemId);
            if (item == null)
            {
                result.obj = "找不到条目";
            }
            else
            {
                DictionaryItem another = null;
                if (increase > 0)
                {
                    another =
                        (from o in db.DictionaryItems
                         where o.DictId == item.DictId && o.IndexNum > item.IndexNum
                         orderby o.IndexNum
                         select o).FirstOrDefault();
                }
                else
                {
                    another =
                        (from o in db.DictionaryItems
                         where o.DictId == item.DictId && o.IndexNum < item.IndexNum
                         orderby o.IndexNum descending
                         select o).FirstOrDefault();
                }
                if (another == null)
                {
                    result.obj = "找不到可以交换位置的条目";
                }
                else
                {
                    int temp = item.IndexNum;
                    item.IndexNum = another.IndexNum;
                    another.IndexNum = temp;
                    db.SaveChanges();
                    result.success = true;
                }
            }
            if (result.success == true)
            {
                var query = (from o in db.DictionaryItems where o.DictId == item.DictId orderby o.IndexNum select o).ToList();
                result.obj = query;
            }
            return Json(result);
        }
        [HttpPost]
        public JsonResult UpDictionaryItem(int itemid)
        {
            if (!UserInfo.CurUser.HasRight("系统管理-数据字典")) return Json(new Result { obj = "没有权限" });
            return ChangeDictionaryItemIndex(itemid, -1);
        }
        [HttpPost]
        public JsonResult DownDictionaryItem(int itemid)
        {
            if (!UserInfo.CurUser.HasRight("系统管理-数据字典")) return Json(new Result { obj = "没有权限" });
            return ChangeDictionaryItemIndex(itemid, 1);
        }
    }
}
