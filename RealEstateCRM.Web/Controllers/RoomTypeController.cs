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
    public class RoomTypeController : BaseController
    {
        private Context db = new Context();

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);

        }

        [MyAuthorize("系统管理-部门管理")]
        public ActionResult Config()
        {
            List<Department> projects = (from o in db.Departments where o.DepartmentType=="项目" select o).ToList();
            ViewBag.Projects = projects;
            return View();
        }
        [HttpPost]
        public JsonResult RoomTypeQuery()
        {
            if (!UserInfo.CurUser.HasRight("系统管理-部门管理")) return Json(new Result {success=false, obj = "没有权限" });
            Result result = new Result();
            result.success = true;
            var list = (from o in db.RoomTypes select o).ToList();
            result.obj = list;
            return Json(result);
        }

        public ActionResult RoomTypeEdit(int id, int? deptid)
        {
            RoomType s = db.RoomTypes.Find(id);
            if (s == null)
            {
                s = new RoomType { DepartmentId = (int)deptid };
            }
            ViewBag.Project = db.Departments.Find(s.DepartmentId);
            return View(s);
        }

        [Authorize]
        [HttpPost]
        public ActionResult RoomTypeEdit(int id, int? deptid, FormCollection collection)
        {
            RoomType s = db.RoomTypes.Find(id);
            if (s == null)
            {
                s = new RoomType { DepartmentId = (int)deptid };
                db.RoomTypes.Add(s);
            }
            TryUpdateModel(s, "", new string[] { }, new string[] { "" }, collection);
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
    }

}
