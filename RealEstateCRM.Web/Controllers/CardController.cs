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
    public class CardController : BaseController
    {
        private Context db = new Context();

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);

        }

        public ActionResult Create(int id)//此id为clientid
        {
            Card c = new Card() {ClientId=id };
            c.GroupId = (from o in UserInfo.CurUser.Departments where o.DepartmentType == "小组" select o.Id).FirstOrDefault();
            return View(c);
        }

        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            Card c = new Card();
            db.Cards.Add(c);
            
            TryUpdateModel(c, "", new string[] { }, new string[] { "" }, collection);
            var query = (from o in db.Cards where o.ClientId == c.ClientId select o).FirstOrDefault();
            if (query != null)
            {
                ModelState.AddModelError("", "该客户已有卡");
            }
            Client client = db.Clients.Find(c.ClientId);
            if (ModelState.IsValid)
            {
                if ((client.State != ClientStateEnum.办卡客户))
                {
                    client.State = ClientStateEnum.办卡客户;
                    client.StateDate = DateTime.Today;
                    Utilities.AddLog(db, client.Id, Client.LogClass, "转办卡客户", "");
                }
                db.SaveChanges();
                return Redirect("~/Content/close.htm");
            }
            return View(c);
        }

        public ActionResult LevelUp(int id)//此id为CardId
        {
            Card c = db.Cards.Find(id);
            return View(c);
        }

        [HttpPost]
        public ActionResult LevelUp(int id,FormCollection collection)
        {
            Card c = db.Cards.Find(id);
            if (collection["BigTime"] == null || collection["BigTime"].Equals(""))
                ModelState.AddModelError("BigTime","大卡时间是必须填写的");
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
            Card c = db.Cards.Find(id);
            return View(c);
        }

        [HttpPost]
        public ActionResult Cancel(int id, FormCollection collection)
        {
            Card c = db.Cards.Find(id);
            if (collection["CancelTime"] == null || collection["CancelTime"].Equals(""))
                ModelState.AddModelError("CancelTime", "取消时间是必须填写的");
            TryUpdateModel(c, "", new string[] {"CancelTime","Remark" }, new string[] { "" }, collection);
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
            Card c = db.Cards.Find(id);
            db.Cards.Remove(c);
            db.SaveChanges();
            result.success = true;
            result.obj = "已删除";
            return Json(result);
        }
        //ToDo:要检查办卡对应客户状态
    }

}
