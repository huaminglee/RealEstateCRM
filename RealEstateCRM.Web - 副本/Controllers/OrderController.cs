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

        public ActionResult Create(int id, int type)//此id为clientid
        {
            Order c = new Order() { ClientId = id };
            c.GroupId = (from o in UserInfo.CurUser.Departments where o.DepartmentType == "小组" select o.Id).FirstOrDefault();
            ViewBag.Type = type;
            return View(c);
        }

        [HttpPost]
        public ActionResult Create(int type, string ismobile,FormCollection collection)
        {
            Order c = new Order();
            db.Orders.Add(c);
            if (collection["OrderTime"] == null)
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
                    List<ClientActivity> invitelist = (from o in db.ClientActivities where o.PlanTime.HasValue && (o.Type.Equals("签约邀约") || o.Type.Equals("大定邀约")) && o.ClientId == c.ClientId select o).ToList().Where(o => DateTime.Compare(o.PlanTime.Value.Date, DateTime.Today) == 0).ToList();
                    foreach (ClientActivity invite in invitelist)
                    {
                        if (invite != null)
                        {
                            if (!invite.ActualTime.HasValue)
                                invite.ActualTime = DateTime.Today;
                            invite.IsDone = true;
                        }
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

                    ClientActivity invite = (from o in db.ClientActivities where o.PlanTime.HasValue && o.Type.Equals("大定邀约") && o.ClientId == c.ClientId select o).ToList().Where(o => DateTime.Compare(o.PlanTime.Value.Date, DateTime.Today) == 0).FirstOrDefault();
                    if (invite != null)
                    {
                        if (!invite.ActualTime.HasValue)
                            invite.ActualTime = DateTime.Today;
                        invite.IsDone = true;
                    }
                }

                db.SaveChanges();
                if (!string.IsNullOrEmpty(ismobile))
                {
                    return Redirect("../../Client/View/" + c.ClientId.ToString());
                }
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
        public ActionResult LevelUp(int id, string ismobile, FormCollection collection)
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
                ClientActivity invite = (from o in db.ClientActivities where o.PlanTime.HasValue && o.Type.Equals("签约邀约") && o.ClientId == c.ClientId select o).ToList().Where(o => DateTime.Compare(o.PlanTime.Value.Date, DateTime.Today) == 0).FirstOrDefault();
                if (invite != null)
                {
                    if (!invite.ActualTime.HasValue)
                        invite.ActualTime = DateTime.Today;
                    invite.IsDone = true;
                }
                db.SaveChanges();
                if (!string.IsNullOrEmpty(ismobile))
                {
                    return Redirect("../../Client/View/" + c.ClientId.ToString());
                }
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
        public ActionResult Cancel(int id, string ismobile, FormCollection collection)
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
                if (!string.IsNullOrEmpty(ismobile))
                {
                    return Redirect("../../Client/View/" + c.ClientId.ToString());
                }
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
            if (c.OrderTime < DateTime.Today.AddDays(-1))
            {
                result.obj = "不能删除1天以前的记录";
                return Json(result);
            }
            db.Orders.Remove(c);
            string check = Client.StateUpdate(clientid, null);
            if (!check.Equals(""))
                Utilities.AddLog(db, clientid, Client.LogClass, "删单", check);
            List<ClientActivity> invitelist = new List<ClientActivity>();
            invitelist.Add((from o in db.ClientActivities where o.ClientId==c.ClientId&&o.Type.Equals("大定邀约") select o).ToList().Where(o=>o.PlanTime.GetValueOrDefault().Date==c.OrderTime.Date).FirstOrDefault());
            if (c.SignTime.HasValue)
            {
                invitelist.Add((from o in db.ClientActivities where o.ClientId == c.ClientId && o.Type.Equals("签约邀约") select o).ToList().Where(o => o.PlanTime.GetValueOrDefault().Date == c.SignTime.GetValueOrDefault().Date).FirstOrDefault());
            }
            foreach (ClientActivity invite in invitelist)
            {
                if (invite != null)
                {
                    invite.IsDone = false;
                }
            }
            db.SaveChanges();
            result.success = true;
            result.obj = "已删除";
            return Json(result);
        }
    }

}
