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
    public class ClientController : BaseController
    {
        private Context db = new Context();

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);

        }
        [MyAuthorize("客户管理-客户查看")]
        public ActionResult List()
        {
            return View();
        }
        [HttpPost]
        public JsonResult ListQuery(string sidx, string sord, int page, int rows, FormCollection collection)
        {
            if (!UserInfo.CurUser.HasRight("租赁管理-客户查看")) return Json(new Result { obj = "没有权限" });
            List<object> parameters = new List<object>();
            string sql = ClientView.sql;

            Utilities.AddSqlFilterLike(collection, "Name", ref sql, "c.Name", parameters);
            Utilities.AddSqlFilterLike(collection, "PhoneNumber", ref sql, "c.Allphone", parameters);


            if (!string.IsNullOrEmpty(sidx) && !sidx.Contains(';') && !sord.Contains(';'))
            {
                //string pre = "";
                sql += string.Format(" order by {0} {1}", sidx, sord);
            }
            db.Database.Connection.Open();
            var dynamicParams = new DynamicParameters();
            parameters.ForEach(o => { var p = o as SqlParameter; dynamicParams.Add(p.ParameterName, p.Value, p.DbType); });
            var query = db.Database.Connection.Query<ClientView>(sql, param: dynamicParams);
            var list = query.ToList();
            int totalrow = list.Count();
            int pagenum = (totalrow - totalrow % rows) / rows + 1;
            var newquery = (from o in list
                            select o).Take(rows * page).Skip(page * rows - rows).ToList();// 这种写法是在内存中运算
            newquery.ForEach(o =>
            {//这里要获取基金字段

            });
            var jsonData = new
            {
                total = pagenum,
                page = page,
                records = totalrow,
                rows = newquery
            };
            return Json(jsonData);
        }



        public ActionResult View(int id)
        {
            Client c = db.Clients.Find(id);

            return View(c);
        }

        public ActionResult Edit(int id)
        {
            Client c = db.Clients.Find(id);
            int projectid = 0;
            if (Request.RequestContext.RouteData.Values["projectid"] != null) { projectid = int.Parse(Request.RequestContext.RouteData.Values["projectid"].ToString()); }
            if (c == null)
            {
                c = new Client();
                c.ProjectId = projectid;
                c.GroupId = (from o in UserInfo.CurUser.Departments where o.DepartmentType == "小组" select o.Id).FirstOrDefault();
                c.AllPhone = "";
                c.RoomType = "";
            }
            List<string> phoneList = c.AllPhone.Split(',').ToList();
            while (phoneList.Count != 3)
            {
                phoneList.Add("");
            }
            ViewBag.Phones = phoneList;

            List<SelectListItem> typelist = new List<SelectListItem>();
            List<string> hastype = c.RoomType.Split(',').ToList();
            foreach (string s in DepartmentBLL.GetRoomType(projectid))
            {
                typelist.Add(new SelectListItem() { Text = s, Value = s, Selected = hastype.Contains(s) });
            }
            ViewBag.RoomTypes = typelist;
            return View(c);
        }


        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            Client c = db.Clients.Find(id);
            var a = collection["AllPhone"];
            while (!a.Equals("") && a[a.Length - 1] == ',')
                a = a.Remove(a.Length - 1);
            var b = collection["RoomType"];
            collection["AllPhone"] = a;
            if (c == null)
            {
                c = new Client();
                c.CreateTime = DateTime.Now;
                db.Clients.Add(c);
                int check = CheckClientByPhone(collection["AllPhone"]);
                if (check != 0)
                {
                    ModelState.AddModelError("AllPhone", "该客户已经存在，所在项目组为：" + DepartmentBLL.GetNameById(check));
                }
            }

            TryUpdateModel(c, "", new string[] { }, new string[] { "RoomType"}, collection);
            c.RoomType = b;
            if (ModelState.IsValid)
            {
                db.SaveChanges();
                return Redirect("../View/" + c.Id);
            }
            c.AllPhone = c.AllPhone ?? "";
            List<string> phoneList = c.AllPhone.Split(',').ToList();
            while (phoneList.Count != 3)
            {
                phoneList.Add("");
            }
            ViewBag.Phones = phoneList;
            c.RoomType = c.RoomType ?? "";
            List<SelectListItem> typelist = new List<SelectListItem>();
            List<string> hastype = c.RoomType.Split(',').ToList();
            foreach (string s in DepartmentBLL.GetRoomType(c.ProjectId))
            {
                typelist.Add(new SelectListItem() { Text = s, Value = s, Selected = hastype.Contains(s) });
            }
            ViewBag.RoomTypes = typelist;
            return View(c);
        }


        [HttpPost]
        public JsonResult Delete(int id)
        {

            var result = new Result();
            Client s = db.Clients.Find(id); if (!UserInfo.CurUser.HasRight("租赁管理-客户维护")) return Json(new Result { obj = "没有权限" });
            if (1 == 2)
            {
                result.obj = "已有合同记录，不能删除";
            }
            else
            {
                db.Clients.Remove(s);
                db.SaveChanges(); BLL.Utilities.AddLogAndSave(s.Id, Client.LogClass, "删除", "");
                result.success = true;
                result.obj = "已删除";
            }

            return Json(result);
        }
        public int CheckClientByPhone(string allphone)
        {
            int check = 0;
            if (allphone == null || allphone.Equals("")) return check;
            List<string> forcheck = allphone.Split(',').ToList();
            foreach (string s in forcheck)
            {
                if (!s.Equals(""))
                {
                    var query = (from o in db.Clients where o.AllPhone.Contains(s) select o).FirstOrDefault();
                    if (query != null)
                        return query.ProjectId;
                }
            }
            return check;
        }
        [HttpPost]
        public JsonResult PhoneCheck(int id,string phone)
        {
            Result result = new Result();
            long i;
            if(!long.TryParse(phone,out i)||!(phone.Length==8||phone.Length==11))
            {
                result.success = false;
                result.obj = "号码格式错误";
                return Json(result);
            }
            if (id == 0)
            {
                var query = (from o in db.Clients where o.AllPhone.Contains(phone) select o).FirstOrDefault();
                if (query != null)
                {
                    result.success = false;
                    result.obj = "该用户已经存在，所在项目为：" + DepartmentBLL.GetNameById(query.ProjectId);
                    return Json(result);
                }
            }
            result.success = true;
            return Json(result);
        }

    }

}
