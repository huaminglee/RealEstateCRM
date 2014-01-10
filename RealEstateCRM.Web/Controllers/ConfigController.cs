using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RealEstateCRM.Web.Models;
using RealEstateCRM.Web.BLL;
using OUDAL;
using Dapper;
namespace RealEstateCRM.Web.Controllers
{
    public class ConfigController : BaseController
    {
        private Context db = new Context();

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);

        }

        //[MyAuthorize("系统管理-部门管理")]
        public ActionResult Config(int projectid)
        {
            List<RoomType> RoomTypes = (from o in db.RoomTypes where o.DepartmentId == projectid select o).ToList();
            ViewBag.RoomTypes = RoomTypes;
            ViewBag.ProjectId = projectid;
            ViewBag.ProjectCode = Project.Get(projectid).Code;
            return View();
        }
        [HttpPost]
        public JsonResult RoomTypeQuery(int projectid)
        {
            //if (!UserInfo.CurUser.HasRight("系统管理-部门管理")) return Json(new Result {success=false, obj = "没有权限" });
            Result result = new Result();
            result.success = true;
            var list = (from o in db.RoomTypes where o.DepartmentId == projectid select o).ToList();
            result.obj = list;
            return Json(result);
        }

        public ActionResult RoomTypeEdit(int id, int projectid)
        {
            RoomType s = db.RoomTypes.Find(id);
            if (s == null)
            {
                s = new RoomType { DepartmentId = (int)projectid };
            }
            ViewBag.Reload = false;
            ViewBag.Project = db.Departments.Find(s.DepartmentId);
            return View(s);
        }

        [Authorize]
        [HttpPost]
        public ActionResult RoomTypeEdit(int id, int projectid, FormCollection collection)
        {
            RoomType s = db.RoomTypes.Find(id);
            if (s == null)
            {
                s = new RoomType { DepartmentId = (int)projectid };
                ViewBag.Reload = true;
                db.RoomTypes.Add(s);
            }
            else if(!collection["Name"].Equals(s.Name)){
                ViewBag.Reload = true;
            }
            TryUpdateModel(s, "", new string[] { }, new string[] { "" }, collection);
            var query = (from o in db.RoomTypes where o.DepartmentId == s.DepartmentId && o.Name == s.Name && o.Id != s.Id select o).FirstOrDefault();
            if (query != null)
                ModelState.AddModelError("", "该产品类型已经存在");
            if (ModelState.IsValid)
            {
                db.SaveChanges();
                ViewBag.Success = true;
            }
            ViewBag.Project = db.Departments.Find(s.DepartmentId);
            return View(s);
        }

        [Authorize]
        [HttpPost]
        public JsonResult RoomTypeDelete(int id)
        {

            Models.Result result = new Models.Result();
            RoomType s = db.RoomTypes.Find(id);
            db.RoomTypes.Remove(s);
            db.SaveChanges();
            result.success = true;
            result.obj = "已删除";
            return Json(result);
        }

        public ActionResult ProjectCodeEdit(int projectid)
        {
            Project project = db.Projects.Find(projectid);
            if(project==null)
            {
                project = new Project() { DepartmentId = projectid };
            }
            ViewBag.Project = db.Departments.Find(projectid);
            return View(project);
        }

        [Authorize]
        [HttpPost]
        public ActionResult ProjectCodeEdit(int projectid,FormCollection collection)
        {
            Project p = db.Projects.Find(projectid);
            if (p == null)
            {
                p = new Project { DepartmentId = (int)projectid };
                db.Projects.Add(p);
            }
            TryUpdateModel(p, "", new string[] { }, new string[] { "" }, collection);
            
            if (ModelState.IsValid)
            {
                db.SaveChanges();
                ViewBag.Success = true;
            }
            ViewBag.Project = db.Departments.Find(p.DepartmentId);
            return View(p);
        }

        public ActionResult PerformanceList()
        {
            ViewBag.Projects = DepartmentBLL.GetDepartmentByType("项目");
            return View();
        }
        [HttpPost]
        public JsonResult PerformanceQuery()
        {
            //if (!UserInfo.CurUser.HasRight("系统管理-部门管理")) return Json(new Result {success=false, obj = "没有权限" });
            Result result = new Result();
            result.success = true;
            var list = (from o in db.Performance orderby o.ProjectId ,o.BeginDate descending select o).ToList();
            result.obj = list;
            return Json(result);
        }
        public ActionResult PerformanceEdit(int id, int? projectid)
        {
            Performance s = db.Performance.Find(id);
            if (s == null)
            {
                s = new Performance { ProjectId = (int)projectid };
            }
            ViewBag.Reload = false;
            ViewBag.Project = db.Departments.Find(s.ProjectId);
            return View(s);
        }

        [Authorize]
        [HttpPost]
        public ActionResult PerformanceEdit(int id, int? projectid, FormCollection collection)
        {
            Performance s = db.Performance.Find(id);
            if (s == null)
            {
                s = new Performance { ProjectId = (int)projectid };
                db.Performance.Add(s);
            }
           
            TryUpdateModel(s, "", new string[] { }, new string[] { "" }, collection);
            
            if (ModelState.IsValid)
            {
                db.SaveChanges();
                ViewBag.Success = true;
            }
            ViewBag.Project = db.Departments.Find(s.ProjectId);
            return View(s);
        }
        [HttpPost]
        public JsonResult PerformanceDelete(int id)
        {

            Models.Result result = new Models.Result();
            Performance s = db.Performance.Find(id);
            db.Performance.Remove(s);
            db.SaveChanges();
            result.success = true;
            result.obj = "已删除";
            return Json(result);
        }
    }

}
