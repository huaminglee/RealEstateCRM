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

        //[MyAuthorize("系统管理-部门管理")]
        public ActionResult Config(int projectid)
        {
            List<RoomType> RoomTypes = (from o in db.RoomTypes where o.DepartmentId == projectid select o).ToList();
            ViewBag.RoomTypes = RoomTypes;
            ViewBag.ProjectId = projectid;
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
    }

}
