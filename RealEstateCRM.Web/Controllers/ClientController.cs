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
            int projectid = 0;//按项目过滤客户
            if (Request.RequestContext.RouteData.Values["projectid"] != null) { projectid = int.Parse(Request.RequestContext.RouteData.Values["projectid"].ToString()); }
            if (projectid != 0)
            {
                sql += string.Format(" and c.ProjectId={0}", projectid);
            }
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
            {
                o.ProjectName = DepartmentBLL.GetNameById(o.ProjectId);
                o.GroupName = DepartmentBLL.GetNameById(o.GroupId);
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

        public ActionResult ToCreate()
        {
            ViewBag.Show = false;
            ViewBag.Button = false;
            ViewBag.Message = false;
            return View();
        }

        [HttpPost]
        public ActionResult ToCreate(FormCollection collection)
        {
            ViewBag.Show = false;
            ViewBag.Button = false;
            ViewBag.Message = false;
            string Name = collection["Name"];
            string PhoneNumber = collection["AllPhone"];
            if (Name == null || Name.Equals(""))
            {
                ModelState.AddModelError("Name", "名称不能为空");
            }
            if (PhoneNumber == null || PhoneNumber.Equals(""))
            {
                ModelState.AddModelError("AllPhone", "电话不能为空");
            }
            JsonResult check = PhoneCheck(0, PhoneNumber);
            Result result = (Result)check.Data;
            if (!result.success)
            {
                if (result.obj.Equals("号码格式错误"))
                    ModelState.AddModelError("AllPhone", result.obj.ToString());
            }
            if (!ModelState.IsValid)
            {
                return View(new Client() { Name = Name, AllPhone = PhoneNumber });
            }
            ViewBag.Show = true;
            ClientView c = db.Database.SqlQuery<ClientView>(string.Format("select * from Clients where (AllPhone like'{0},%' or AllPhone like '%,{0}' or AllPhone ='{0}' ) and Name='{1}'", PhoneNumber, Name)).FirstOrDefault();
            List<ClientView> Clients = new List<ClientView>();
            if (c != null)
            {
                Clients.Add(c);
            }
            else
            {
                Clients = db.Database.SqlQuery<ClientView>(string.Format(@"select * from Clients where AllPhone like'%{0}%' or Name='{1}'", PhoneNumber, Name)).ToList();
                ViewBag.Button = true;
                ViewBag.Message = true;
                if (db.Database.SqlQuery<ClientView>(string.Format("select * from Clients where (AllPhone like'{0},%' or AllPhone like '%,{0}' or AllPhone ='{0}' )", PhoneNumber)).Count() > 0)
                {
                    ViewBag.Button = false;
                }
            }
            Clients.ForEach(
                o =>
                {
                    o.ProjectName = DepartmentBLL.GetNameById(o.ProjectId);
                    var PhoneList = o.AllPhone.Split(',').ToList();
                    o.AllPhone = "";
                    foreach (string s in PhoneList)
                    {
                        o.AllPhone += (s.Substring(0, s.Length - 3) + "***,");
                    }
                    o.AllPhone = o.AllPhone.Equals("") ? "" : o.AllPhone.Remove(o.AllPhone.Length - 1);
                });
            ViewBag.Clients = Clients;
            return View(new Client() { Name = Name, AllPhone = PhoneNumber });
        }

        public ActionResult Create(int projectid, int type, string Name, string AllPhone)
        {
            ClientCreate c = new ClientCreate();
            ViewBag.Type = type;
            c.Name = Name;
            c.AllPhone = AllPhone;
            c.ProjectId = projectid;
            c.GroupId = (from o in UserInfo.CurUser.Departments where o.DepartmentType == "小组" select o.Id).FirstOrDefault();
            switch (type)
            {
                case 1:
                    c.ContactType = "来电"; break;
                case 2:
                    c.ContactType = "来访"; break;
                case 3:
                    ViewBag.HasAppointment = true;
                    return View(c);
            }
            c.ContactActualTime = DateTime.Now;
            ViewBag.HasAppointment = false;
            return View(c);
        }

        public ActionResult CreateClient(int type,FormCollection collection)
        {
            ClientCreate cc = new ClientCreate();
            ViewBag.Type = type;
            if (type != 3)
            {
                if (collection["ContactActualTime"] == null || collection["ContactActualTime"].Equals(""))
                {
                    ModelState.AddModelError("ContactActualTime", "联系时间不能为空");
                }
            }
            bool HasAppointment = (collection["HasAppointment"] != null && collection["HasAppointment"].Equals("Add")) ? true : false;
            if (HasAppointment)
            {
                if (collection["AppointmentPlanTime"] == null || collection["AppointmentPlanTime"].Equals(""))
                {
                    ModelState.AddModelError("AppointmentPlanTime", "邀约时间不能为空");
                }
                if (collection["AppointmentType"] == null || collection["AppointmentType"].Equals(""))
                {
                    ModelState.AddModelError("AppointmentType", "邀约类型不能为空");
                }
            }
            ViewBag.HasAppointment = HasAppointment;
            if (collection["AllPhone1"] != null && !collection["AllPhone1"].Equals(""))
                collection["AllPhone"] = collection["AllPhone"].ToString() + "," + collection["AllPhone1"].ToString();
            TryUpdateModel(cc, collection);
            int check = CheckClientByPhone(cc.AllPhone);
            if (check != 0)
            {
                ModelState.AddModelError("AllPhone", "该客户已经存在，所在项目组为：" + DepartmentBLL.GetNameById(check));
            }
            if (ModelState.IsValid)
            {
                Client c = new Client();
                UpdateModel(c, collection);
                c.CreateTime = DateTime.Now;
                db.Clients.Add(c);
                db.SaveChanges();
                ClientActivity ca = new ClientActivity();
                ca.ClientId = c.Id;
                ca.ActualTime = cc.ContactActualTime;
                ca.Type = cc.ContactType;
                ca.Detail = cc.ContactDetail;
                db.ClientActivities.Add(ca);
                if (HasAppointment)
                {
                    ClientActivity ap = new ClientActivity();
                    ap.ClientId = c.Id;
                    ap.PlanTime = cc.AppointmentPlanTime;
                    ap.Detail = cc.AppointmentDetail;
                    ap.Type = cc.AppointmentType;
                    db.ClientActivities.Add(ap);
                }
                db.SaveChanges();
                return Redirect("./View/" + c.Id);
            }
            else
            {
                return View("Create", cc);
            }
        }

        public ActionResult View(int id)
        {
            Client c = db.Clients.Find(id);
            List<ClientActivity> ContactList = new List<ClientActivity>();
            List<ClientActivity> AppointmentList = new List<ClientActivity>();
            List<Card> CardList = new List<Card>();
            List<Order> OrderList = new List<Order>();            
            ContactList = (from o in db.ClientActivities where o.ClientId == id && !o.PlanTime.HasValue select o).ToList();
            AppointmentList = (from o in db.ClientActivities where o.ClientId == id && o.PlanTime.HasValue select o).ToList();
            CardList = (from o in db.Cards where o.ClientId == id select o).ToList();
            OrderList = (from o in db.Orders where o.ClientId == id select o).ToList();
            ViewBag.Contacts = ContactList;
            ViewBag.Appointments = AppointmentList;
            ViewBag.Cards = CardList;
            ViewBag.Orders = OrderList;
            return View(c);
        }

        public ActionResult Edit(/*int projectid,*/int id)//仅作为编辑使用
        {
            Client c = db.Clients.Find(id);
            //int projectid = 0;
            //if (Request.RequestContext.RouteData.Values["projectid"] != null) { projectid = int.Parse(Request.RequestContext.RouteData.Values["projectid"].ToString()); }
            //if (c == null)
            //{
            //    c = new Client();
            //    c.ProjectId = projectid;
            //    c.GroupId = (from o in UserInfo.CurUser.Departments where o.DepartmentType == "小组" select o.Id).FirstOrDefault();
            //    c.AllPhone = "";
            //    c.RoomType = "";
            //}
            List<string> phoneList = c.AllPhone.Split(',').ToList();
            while (phoneList.Count != 2)
            {
                phoneList.Add("");
            }
            ViewBag.Phones = phoneList;

            //List<SelectListItem> typelist = new List<SelectListItem>();
            //List<string> hastype = c.RoomType.Split(',').ToList();
            //foreach (string s in DepartmentBLL.GetRoomType(c.ProjectId))
            //{
            //    typelist.Add(new SelectListItem() { Text = s, Value = s, Selected = hastype.Contains(s) });
            //}
            //ViewBag.RoomTypes = typelist;
            return View(c);
        }


        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            Client c = db.Clients.Find(id);
            var a = collection["AllPhone"];
            while (!a.Equals("") && a[a.Length - 1] == ',')
                a = a.Remove(a.Length - 1);
            collection["AllPhone"] = a;
            //if (c == null)
            //{
            //    c = new Client();
            //    c.CreateTime = DateTime.Now;
            //    db.Clients.Add(c);
            //    int check = CheckClientByPhone(collection["AllPhone"]);
            //    if (check != 0)
            //    {
            //        ModelState.AddModelError("AllPhone", "该客户已经存在，所在项目组为：" + DepartmentBLL.GetNameById(check));
            //    }
            //}

            TryUpdateModel(c, "", new string[] { }, new string[] { }, collection);
            if (ModelState.IsValid)
            {
                db.SaveChanges();
                //if (id == 0)
                //    return Redirect("../View/" + c.Id);
                return Redirect("~/Content/close.htm");
            }
            else
            {
                c.AllPhone = c.AllPhone ?? "";
                List<string> phoneList = c.AllPhone.Split(',').ToList();
                while (phoneList.Count != 2)
                {
                    phoneList.Add("");
                }
                ViewBag.Phones = phoneList;
                //c.RoomType = c.RoomType ?? "";
                //List<SelectListItem> typelist = new List<SelectListItem>();
                //List<string> hastype = c.RoomType.Split(',').ToList();
                //foreach (string s in DepartmentBLL.GetRoomType(c.ProjectId))
                //{
                //    typelist.Add(new SelectListItem() { Text = s, Value = s, Selected = hastype.Contains(s) });
                //}
                //ViewBag.RoomTypes = typelist;
                return View(c);
            }
        }

        public ActionResult AddContact(int id)//id为client的id
        {
            ClientActivity c = new ClientActivity();
            c.ActualTime = DateTime.Now;
            c.ClientId = id;
            return View(c);
        }

        [HttpPost]
        public ActionResult AddContact(FormCollection collection)
        {
            ClientActivity c = new ClientActivity();
            db.ClientActivities.Add(c);

            TryUpdateModel(c, collection);
            if (!c.ActualTime.HasValue)
            {
                ModelState.AddModelError("ActualTime", "联系时间不能为空");
            }
            if (ModelState.IsValid)
            {
                db.SaveChanges();
                return Redirect("~/Content/close.htm");
            }
            else
            {
                return View(c);
            }

        }

        [HttpPost]
        public JsonResult RemoveContact(int id)//id为contactid
        {
            Result result = new Result();
            ClientActivity c = db.ClientActivities.Find(id);
            if (c != null)
            {
                db.ClientActivities.Remove(c);
                db.SaveChanges();
                result.success = true;
                return Json(result);
            }
            result.success = false;
            result.obj = "删除失败";
            return Json(result);
        }

        public ActionResult AddAppointment(int id)//id为client的id
        {
            ClientActivity c = new ClientActivity();
            c.ClientId = id;
            return View(c);
        }

        [HttpPost]
        public ActionResult AddAppointment(FormCollection collection)
        {
            ClientActivity c = new ClientActivity();
            db.ClientActivities.Add(c);

            TryUpdateModel(c, collection);
            if (!c.PlanTime.HasValue)
            {
                ModelState.AddModelError("PlanTime", "邀约时间不能为空");
            }
            if (ModelState.IsValid)
            {
                db.SaveChanges();
                return Redirect("~/Content/close.htm");
            }
            else
            {
                return View(c);
            }

        }

        [HttpPost]
        public JsonResult RemoveAppointment(int id)//id为appointmentid
        {
            Result result = new Result();
            ClientActivity c = db.ClientActivities.Find(id);
            if (c != null)
            {
                db.ClientActivities.Remove(c);
                db.SaveChanges();
                result.success = true;
                return Json(result);
            }
            result.success = false;
            result.obj = "删除失败";
            return Json(result);
        }

        public ActionResult EditAppointment(int id)//id为Appointment的id
        {
            ClientActivity c = db.ClientActivities.Find(id);
            return View(c);
        }

        [HttpPost]
        public ActionResult EditAppointment(int id, FormCollection collection)//id为Appointment的id
        {
            ClientActivity c = db.ClientActivities.Find(id);
            TryUpdateModel(c, collection);
            if (ModelState.IsValid)
            {
                db.SaveChanges();
                return Redirect("~/Content/close.htm");
            }
            else
            {
                return View(c);
            }

        }

        public ActionResult FinishAppointment(int id)//id为Appointment的id
        {
            ClientActivity c = db.ClientActivities.Find(id);
            return View(c);
        }

        [HttpPost]
        public ActionResult FinishAppointment(int id, FormCollection collection)//id为Appointment的id
        {
            ClientActivity c = db.ClientActivities.Find(id);
            TryUpdateModel(c, collection);
            if (ModelState.IsValid)
            {
                db.SaveChanges();
                return Redirect("~/Content/close.htm");
            }
            else
            {
                return View(c);
            }

        }


        [HttpPost]
        public JsonResult Delete(int id)
        {

            var result = new Result();
            Client s = db.Clients.Find(id);
            if (!UserInfo.CurUser.HasRight("租赁管理-客户维护")) return Json(new Result { obj = "没有权限" });
            ClientActivity c = (from o in db.ClientActivities where o.ClientId == id select o).FirstOrDefault();
            if (c != null)
            {
                result.success = false;
                result.obj = "已有联系记录，不能删除";
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
        public JsonResult PhoneCheck(int id, string phone)
        {
            Result result = new Result();
            long i;
            if (!long.TryParse(phone, out i) || phone.Length < 8 || (phone[0].Equals('1') && phone.Length != 11))
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
                    result.obj = "该号码已经存在用户，所在项目为：" + DepartmentBLL.GetNameById(query.ProjectId);
                    return Json(result);
                }
            }
            result.success = true;
            return Json(result);
        }

    }

}
