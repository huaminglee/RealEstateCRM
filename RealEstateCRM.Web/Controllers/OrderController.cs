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
            Client client = db.Clients.Find(c.ClientId);
            if (ModelState.IsValid)
            {
                if (c.SignTime != null)
                {
                    if ((client.State != ClientStateEnum.签约客户))
                    {
                        client.State = ClientStateEnum.签约客户;
                        client.StateDate = DateTime.Today;
                        Utilities.AddLog(db, client.Id, Client.LogClass, "转签约客户", "");
                    }
                }
                else
                {
                    if ((client.State != ClientStateEnum.签约客户 && client.State != ClientStateEnum.大定客户))
                    {
                        client.State = ClientStateEnum.大定客户;
                        client.StateDate = DateTime.Today;
                        Utilities.AddLog(db, client.Id, Client.LogClass, "转大定客户", "");
                    }
                }
                
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
            Client client = db.Clients.Find(c.ClientId);
            if (ModelState.IsValid)
            {
                if ((client.State != ClientStateEnum.签约客户))
                {
                    client.State = ClientStateEnum.签约客户;
                    client.StateDate = DateTime.Today;
                    Utilities.AddLog(db, client.Id, Client.LogClass, "转签约客户", "");
                }
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
            int clientid = c.ClientId;
            if (collection["CancelTime"] == null || collection["CancelTime"].Equals(""))
                ModelState.AddModelError("CancelTime", "取消时间是必须填写的");
            TryUpdateModel(c, "", new string[] { "CancelTime", "Remark" }, new string[] { "" }, collection);
            if (ModelState.IsValid)
            {
                db.SaveChanges();
                string check = Client.StateUpdate(clientid, null);
                if (!check.Equals(""))
                    Utilities.AddLog(db, clientid, Client.LogClass, "退单", check);
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
            int clientid = c.ClientId;
            db.Orders.Remove(c);
            string check = Client.StateUpdate(clientid, null);
            if (!check.Equals(""))
                Utilities.AddLog(db, clientid, Client.LogClass, "删单", check);
            db.SaveChanges();
            result.success = true;
            result.obj = "已删除";
            return Json(result);
        }
    }

}
