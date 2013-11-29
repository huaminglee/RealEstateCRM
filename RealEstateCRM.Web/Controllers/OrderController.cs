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
    public class OrderController : BaseController
    {
        private Context db = new Context();

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);

        }

        public ActionResult Create(int id,int type)//此id为clientid
        {
            Order c = new Order() { ClientId = id };
            c.GroupId = (from o in UserInfo.CurUser.Departments where o.DepartmentType == "小组" select o.Id).FirstOrDefault();
            ViewBag.Type = type;
            return View(c);
        }

        [HttpPost]
        public ActionResult Create(int type,FormCollection collection)
        {
            Order c = new Order();
            db.Orders.Add(c);     
            if(collection["OrderTime"]==null)
            {
                if (collection["SignTime"] == null || collection["SignTime"].Equals(""))
                    ModelState.AddModelError("SignTime", "签约时间是必须填写的");
                else
                {
                    collection["OrderTime"] = collection["SignTime"];
                }
            }
            TryUpdateModel(c, "", new string[] { }, new string[] { "" }, collection);
            if (ModelState.IsValid)
            {
                db.SaveChanges();
                return Redirect("~/Content/close.htm");
            }
            ViewBag.Type = type;
            return View(c);
        }

        public ActionResult LevelUp(int id)//此id为OrderId
        {
            Order c = db.Orders.Find(id);
            return View(c);
        }

        [HttpPost]
        public ActionResult LevelUp(int id,FormCollection collection)
        {
            Order c = db.Orders.Find(id);
            if (collection["SignTime"] == null || collection["SignTime"].Equals(""))
                ModelState.AddModelError("SignTime", "签约时间是必须填写的");
            TryUpdateModel(c, "", new string[] { }, new string[] { "" }, collection);
            if (ModelState.IsValid)
            {
                db.SaveChanges();
                return Redirect("~/Content/close.htm");
            }
            return View(c);
        }

        public ActionResult Cancel(int id)//此id为CardId
        {
            Order c = db.Orders.Find(id);
            return View(c);
        }

        [HttpPost]
        public ActionResult Cancel(int id, FormCollection collection)
        {
            Order c = db.Orders.Find(id);
            if (collection["CancelTime"] == null || collection["CancelTime"].Equals(""))
                ModelState.AddModelError("CancelTime", "取消时间是必须填写的");
            TryUpdateModel(c, "", new string[] { "CancelTime", "Remark" }, new string[] { "" }, collection);
            if (ModelState.IsValid)
            {
                db.SaveChanges();
                return Redirect("~/Content/close.htm");
            }
            return View(c);
        }

        [Authorize]
        [HttpPost]
        public JsonResult Delete(int id)
        {

            Models.Result result = new Models.Result();
            Order c = db.Orders.Find(id);
            db.Orders.Remove(c);
            db.SaveChanges();
            result.success = true;
            result.obj = "已删除";
            return Json(result);
        }
    }

}
