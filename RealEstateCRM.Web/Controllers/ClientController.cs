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
using System.Transactions;
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

        [MyAuthorize("客户管理-客户查询")]
        public ActionResult List()
        {
            return View();
        }
        [HttpPost]
        public JsonResult ListQuery(string sidx, string sord, int page, int rows, FormCollection collection)
        {
            List<object> parameters = new List<object>();
            string sql = ClientView.sql;

            Utilities.AddSqlFilterLike(collection, "Name", ref sql, "c.Name", parameters);
            Utilities.AddSqlFilterLike(collection, "PhoneNumber", ref sql, "c.Allphone", parameters);
            Utilities.AddSqlFilterInInts(collection, "GroupId", ref sql, "c.GroupId");
            int projectid = 0;//按项目过滤客户
            if (Request.RequestContext.RouteData.Values["projectid"] != null) { projectid = int.Parse(Request.RequestContext.RouteData.Values["projectid"].ToString()); }
            if (projectid != 0)
            {
                sql += string.Format(" and c.ProjectId={0}", projectid);
            }
            if (UserInfo.CurUser.GetClientRight(projectid) < ClientViewScopeEnum.查看项目)
            {
                sql += string.Format(" and c.groupid={0}", UserInfo.CurUser.GetGroup(projectid));
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
                //o.ProjectName = DepartmentBLL.GetNameById(o.ProjectId);
                //o.GroupName = DepartmentBLL.GetNameById(o.GroupId);
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

        public ActionResult ToCreate(int projectid, string type)
        {
            ViewBag.Type = type;
            if (projectid == 0)
            {
                if (!UserInfo.CurUser.HasRight("客户管理-电话中心"))
                {
                    return Redirect("~/content/AccessDeny.htm");
                }

                ViewBag.Projects = DepartmentBLL.GetDepartmentByType("项目");
            }
            ViewBag.Show = false;
            ViewBag.Button = false;
            ViewBag.Message = false;
            return View(new Client { });
        }

        [HttpPost]
        public ActionResult ToCreate(int projectid, string type, FormCollection collection)
        {
            ViewBag.Show = false;
            ViewBag.Button = false;
            ViewBag.Type = type;
            string name = collection["Name"];
            string phoneNumber = collection["AllPhone"];
            string projectId = collection["ProjectId"];
            if (type == "4")
            {
                if (string.IsNullOrEmpty(projectId))
                {
                    ModelState.AddModelError("ProjectId", "请选择项目");
                }
                else
                {
                    projectid = int.Parse(projectId);
                }
                ViewBag.Projects = DepartmentBLL.GetDepartmentByType("项目");
            }

            if (phoneNumber == null || phoneNumber.Equals(""))
            {
                ModelState.AddModelError("AllPhone", "电话不能为空");
            }
            JsonResult check = PhoneCheck(phoneNumber);
            Result result = (Result)check.Data;
            if (!result.success)
            {
                if (result.obj.Equals("号码格式错误"))
                    ModelState.AddModelError("AllPhone", result.obj.ToString());
            }
            if (!ModelState.IsValid)
            {
            }
            else
            {
                ViewBag.Show = true;
                SqlParameter pName = new SqlParameter { ParameterName = "Name", Value = name };
                SqlParameter pPhone = new SqlParameter
                {
                    ParameterName = "Phone",
                    Value = string.Format("%{0}%", phoneNumber)
                };
                //ToDo:这里有注入漏洞
                List<ClientView> clients1 = db.Database.SqlQuery<ClientView>(
                        string.Format(
                            "select * from Clients where projectid={0} and (Phone1 like @Phone or Phone2 like @Phone ) ", projectid
                            ), pPhone).ToList();
                if (clients1.Count > 0)
                {
                    ViewBag.Button = false;

                }
                else
                {
                    ViewBag.Button = true;
                }
                ViewBag.Clients1 = clients1;
                if (name != null && name.Length > 1)
                {
                    List<ClientView> clients2 = db.Database.SqlQuery<ClientView>(
                        string.Format(
                            "select * from Clients  where projectid={0} and Name like @Name  ", projectid), pName).ToList();
                    ViewBag.Clients2 = clients2;
                }
            }
            return View(new Client() { Name = name, AllPhone = phoneNumber, ProjectId = projectid });
        }

        public ActionResult Create(int projectid, int type, string name, string phone)
        {
            ClientCreate cc = new ClientCreate();
            cc.AppointmentType = "来访邀约";
            ViewBag.Type = type;
            cc.Name = name;
            cc.Phone1 = phone;
            cc.ProjectId = projectid;
            
            ViewBag.QuDao = DictionaryBLL.GetList("渠道类型", true);
            ViewBag.HasAppointment = false;
            switch (type)
            {
                case 1:
                    ViewBag.Msg = "来电客户登记";
                    cc.AppointmentType = "";break;
                case 2:
                    ViewBag.Msg = "直访客户登记";
                    cc.AppointmentType = "";
                    if (!UserInfo.CurUser.HasRight("客户管理-前台"))//非前台不能做来访登记
                    {
                        return Redirect("~/content/AccessDeny.htm");
                    }
                    break;
                case 3:
                    ViewBag.HasAppointment = true;
                    ViewBag.QuDao = new List<SelectListItem>
                                    {
                                        new SelectListItem {Text = "中介", Value = "中介"}
                                    };
                    ViewBag.Msg = "中介邀约客户报备";
                    break;
                case 4:
                    ViewBag.HasAppointment = true;
                    ViewBag.QuDao = new List<SelectListItem>
                                    {
                                        new SelectListItem {Text = "电话中心", Value = "电话中心"}
                                    };
                    ViewBag.Msg = "电话中心客户报备";
                    //ToDo:如果邀约客户未到访，然后再次报备如何处理？
                    break;
            }
            cc.ContactActualTime = DateTime.Now;

            return View(cc);
        }
        [HttpPost]
        public ActionResult Create(int projectid, int type, FormCollection collection)
        {
            ViewBag.QuDao = DictionaryBLL.GetList("渠道类型", true);
            ViewBag.HasAppointment = false;

            ViewBag.Type = type;
            bool HasAppointment = (collection["HasAppointment"] != null && collection["HasAppointment"].Equals("Add")) ? true : false;
            ViewBag.HasAppointment = HasAppointment;
            ClientCreate cc = new ClientCreate();
            cc.ProjectId = projectid;
            switch (type)
            {
                case 1:
                    cc.GroupId = UserInfo.CurUser.GetGroup(projectid);
                    ViewBag.Msg = "来电客户登记"; break;
                case 2:
                    ViewBag.Msg = "直访客户登记";//直访客户要选择小组
                    if (!UserInfo.CurUser.HasRight("客户管理-前台"))//非前台不能做来访登记
                    {
                        return Redirect("~/content/AccessDeny.htm");
                    }
                    break;
                case 3:
                    cc.GroupId = UserInfo.CurUser.GetGroup(projectid);
                    ViewBag.HasAppointment = true;
                    ViewBag.QuDao = new List<SelectListItem>
                                    {
                                        new SelectListItem {Text = "中介", Value = "中介"}
                                    };
                    ViewBag.Msg = "中介邀约客户报备";
                    break;
                case 4:
                    cc.GroupId = Project.GetGroupByName(projectid, "前台");
                    ViewBag.HasAppointment = true;
                    ViewBag.QuDao = new List<SelectListItem>
                                    {
                                        new SelectListItem {Text = "电话中心", Value = "电话中心"}
                                    };
                    ViewBag.Msg = "电话中心客户报备";
                    //ToDo:如果邀约客户未到访，然后再次报备如何处理？
                    break;
            }
            ClientStateEnum state = ClientStateEnum.邀约客户;
            TryUpdateModel(cc, collection);
            switch (type)
            {
                case 1:
                    state = ClientStateEnum.来电客户;
                    if (cc.AppointmentPlanTime != null || cc.AppointmentType != null)
                    {
                        if (cc.AppointmentPlanTime == null)
                        {
                            ModelState.AddModelError("AppointmentPlanTime", "请输入邀约时间");
                        }
                        if (cc.AppointmentType == null)
                        {
                            ModelState.AddModelError("AppointmentType", "请输入邀约类型");
                        }
                    }
                    break;
                case 2:
                    state = ClientStateEnum.来访客户;
                    if (cc.AppointmentPlanTime != null || cc.AppointmentType != null)
                    {
                        if (cc.AppointmentPlanTime == null)
                        {
                            ModelState.AddModelError("AppointmentPlanTime", "请输入邀约时间");
                        }
                        if (cc.AppointmentType == null)
                        {
                            ModelState.AddModelError("AppointmentType", "请输入邀约类型");
                        }
                    }
                    break;
                case 3:
                case 4:
                    state = ClientStateEnum.邀约客户;
                    if (cc.AppointmentPlanTime == null)
                    {
                        ModelState.AddModelError("AppointmentPlanTime", "请输入邀约时间");
                    }
                    if (cc.AppointmentType == null)
                    {
                        ModelState.AddModelError("AppointmentType", "请输入邀约类型");
                    }
                    break;
            }
            Client checkClient = new Client
                                 {
                                     Id = cc.Id,
                                     ProjectId = cc.ProjectId,
                                     Phone1 = cc.Phone1,
                                     Phone2 = cc.Phone2
                                 };
            JsonResult numcheck = PhoneCheck(checkClient.Phone1);
            Result result = (Result)numcheck.Data;
            if (!result.success)
            {
                if (result.obj.Equals("号码格式错误"))
                    ModelState.AddModelError("Phone1", result.obj.ToString());
            }
            if (!string.IsNullOrEmpty(checkClient.Phone2))
            {
                numcheck = PhoneCheck(checkClient.Phone2);
                result = (Result)numcheck.Data;
                if (!result.success)
                {
                    if (result.obj.Equals("号码格式错误"))
                        ModelState.AddModelError("Phone2", result.obj.ToString());
                }
            }
            int check = CheckClientByPhone(checkClient);
            if (check != 0)
            {
                ModelState.AddModelError("", "同电话号码客户已经存在，所在组为：" + DepartmentBLL.GetNameById(check));
            }
            if (ModelState.IsValid)
            {
                Client c = new Client();
                UpdateModel(c, collection);
                c.ProjectId = cc.ProjectId;
                c.GroupId = cc.GroupId;
                c.CreateTime = DateTime.Now;
                c.StateDate = DateTime.Today;
                c.State = state;
                db.Clients.Add(c);
                db.SaveChanges();
                ClientActivity ca = new ClientActivity();
                ca.ClientId = c.Id;
                switch (type)
                {
                    case 1:
                        db.ClientActivities.Add(ca);
                        ca.ActualTime = DateTime.Today;
                        ca.Type = "来电";
                        break;
                    case 2:
                        db.ClientActivities.Add(ca);
                        ca.ActualTime = DateTime.Today;
                        ca.Type = "来访";
                        break;
                    case 3:
                    case 4:
                        break;
                }
                if (cc.AppointmentType != null)
                {
                    ClientActivity appoint = new ClientActivity();
                    appoint.ClientId = c.Id;
                    appoint.PlanTime = cc.AppointmentPlanTime;
                    appoint.Type = cc.AppointmentType;
                    appoint.Detail = cc.AppointmentDetail;

                    db.ClientActivities.Add(appoint);
                }
                if (!string.IsNullOrEmpty(c.Phone2))
                    c.AllPhone = string.Format("{0},{1}", c.Phone1, c.Phone2);
                else
                    c.AllPhone = c.Phone1;
                Utilities.AddLog(db, c.Id, Client.LogClass, "客户登记",string.Format("{0} 姓名:{1} 电话:{2}", ca.Type??cc.AppointmentType,c.Name,c.AllPhone));

                db.SaveChanges();

                return Redirect("./View/" + c.Id);
                //ToDo：这个地方有BUG，提交不成功后出错 
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
            //ContactList = (from o in db.ClientActivities where o.ClientId == id && !o.PlanTime.HasValue select o).ToList();
            AppointmentList = (from o in db.ClientActivities where o.ClientId == id select o).ToList();
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
            
            return View(c);
        }


        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            Client c = db.Clients.Find(id);

            string oldname = c.Name;
            string oldphone1 = c.Phone1;
            string oldphone2 = c.Phone2;
            string oldwayextend = c.WayExtend;
            TryUpdateModel(c, "", new string[] { }, new string[] { }, collection);
            int check = CheckClientByPhone(c);
            if (check != 0)
            {
                ModelState.AddModelError("", "同电话号码客户已经存在，所在组为：" + DepartmentBLL.GetNameById(check));
            }
            if (ModelState.IsValid)
            {
                db.SaveChanges();
                if (!string.IsNullOrEmpty(c.Phone2))
                    c.AllPhone = string.Format("{0},{1}", c.Phone1, c.Phone2);
                else
                    c.AllPhone = c.Phone1;
                StringBuilder sb = new StringBuilder();
                if (c.Name != oldname) sb.Append(string.Format(" 姓名:{0} 改为{1}", oldname, c.Name));
                if (c.Phone1 != oldphone1) sb.Append(string.Format(" 电话1:{0} 改为{1}", oldphone1, c.Phone1));
                if (c.Phone2 != oldphone2) sb.Append(string.Format(" 电话2:{0} 改为{1}", oldphone2, c.Phone2));
                if (c.WayExtend != oldwayextend) sb.Append(string.Format(" 渠道说明:{0} 改为{1}", oldwayextend, c.WayExtend));
                Utilities.AddLog(db, c.Id, Client.LogClass, "客户修改", sb.ToString());
                db.SaveChanges();
                //if (id == 0)
                //    return Redirect("../View/" + c.Id);
                return Redirect("../View/" + c.Id);
            }
            else
            {
              
                return View(c);
            }
        }

        public ActionResult AddContact(int id,int? clientid)//id为client的id
        {
            ClientActivity c = db.ClientActivities.Find(id);
            if (c != null)
            {
                if (c.PlanTime != null)
                {
                    return View("ShowError", "", "邀约记录不能通过这个入口修改");
                }
            }
            else
            {
                c = new ClientActivity();
                 c.ActualTime = DateTime.Now;
                c.ClientId =(int)clientid;
            }
            Client client = db.Clients.Find(c.ClientId);
            ViewBag.ProjectId = client.ProjectId;
            if (DepartmentBLL.GetById(client.GroupId).Name == "前台")
            {
                ViewBag.ChangeGroup = true;

            }
            return View(c);
        }

        [HttpPost]
        public ActionResult AddContact(int id,int? clientid,FormCollection collection)
        {
            ClientActivity c = db.ClientActivities.Find(id);
            if (c != null)
            {
                if (c.PlanTime != null)
                {
                    return View("ShowError", "", "邀约记录不能通过这个入口修改");
                }
            }
            else
            {
                c = new ClientActivity();
                c.ClientId = (int)clientid;
                db.ClientActivities.Add(c);
            }
            TryUpdateModel(c, collection);
            if (c.Id == 0&&c.Type=="来访")
            {
                if (!UserInfo.CurUser.HasRight("客户管理-客户来访记录"))
                {
                    return View("ShowError", "", "无权新增来访联系记录");
                }
            }
            if (!c.ActualTime.HasValue)
            {
                ModelState.AddModelError("ActualTime", "联系时间不能为空");
            }
            else if (((DateTime) c.ActualTime).Date != DateTime.Today)
            {
                ModelState.AddModelError("ActualTime", "联系日期只能为当天");
            }
            Client client = db.Clients.Find(c.ClientId);
            ViewBag.ProjectId = client.ProjectId;
            if (DepartmentBLL.GetById(client.GroupId).Name == "前台")
            {
                ViewBag.ChangeGroup = true;
                int group = 0;
                if (int.TryParse(collection["GroupId"], out group))
                {
                    client.GroupId = group;
                }
                else
                {
                    ModelState.AddModelError("", "请选择小组");
                    ViewBag.GroupPrompt = "请选择小组";
                }
            }
            if (ModelState.IsValid)
            {
                if (c.Type == "来访" && (client.State == ClientStateEnum.邀约客户 || client.State == ClientStateEnum.来电客户))
                {
                    client.State = ClientStateEnum.来访客户;
                    client.StateDate = DateTime.Today;
                    Utilities.AddLog(db, client.Id, Client.LogClass, "转来访客户", "");
                }

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
            //ToDo:来访记录删除后要处理客户状态
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
            if (c != null&&c.ActualTime!=null)
            {
                db.ClientActivities.Remove(c);
                db.SaveChanges();
                result.success = true;
                return Json(result);
            }
            result.success = false;
            result.obj = "已有到访记录，不能删除";
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
            c.ActualTime = DateTime.Today;
            Client client = db.Clients.Find(c.ClientId);
            ViewBag.ProjectId = client.ProjectId;
            if (DepartmentBLL.GetById(client.GroupId).Name == "前台")
            {
                ViewBag.ChangeGroup = true;

            }
            return View(c);
        }

        [HttpPost]
        public ActionResult FinishAppointment(int id, FormCollection collection)//id为Appointment的id
        {
            ClientActivity c = db.ClientActivities.Find(id);
            Client client = db.Clients.Find(c.ClientId);
            ViewBag.ProjectId = client.ProjectId;
            if (DepartmentBLL.GetById(client.GroupId).Name == "前台")
            {
                ViewBag.ChangeGroup = true;
                int group = 0;
                if (int.TryParse(collection["GroupId"], out group))
                {
                    client.GroupId = group;
                }
                else
                {
                    ModelState.AddModelError("", "请选择小组");
                    ViewBag.GroupPrompt = "请选择小组";
                }
            }
            TryUpdateModel(c, collection);
            if (c.ActualTime == null)
            {
                ModelState.AddModelError("ActualTime", "请输入到访时间");
            }
            DateTime d = (DateTime)c.ActualTime;
            if (d.Date != ((DateTime)c.PlanTime).Date)
            {
                ModelState.AddModelError("ActualTime", "到访日期不能与邀约日期不同");
            }
            if (ModelState.IsValid)
            {
                if ((client.State == ClientStateEnum.邀约客户 || client.State == ClientStateEnum.来电客户))
                {
                    client.State = ClientStateEnum.来访客户;
                    client.StateDate = DateTime.Today;
                    Utilities.AddLog(db, client.Id, Client.LogClass, "转来访客户", "");
                }
                if (c.Type == "来访邀约")
                {
                    c.IsDone = true;
                }
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="c"></param>
        /// <returns>同电话的组</returns>
        public int CheckClientByPhone(Client c)//检查是否重复
        {
            int check = 0;
            
            var query = (from o in db.Clients where o.ProjectId==c.ProjectId && o.Id!=c.Id &&( o.Phone1==c.Phone1 || (o.Phone2!=null &&o.Phone2==c.Phone1)) select o).FirstOrDefault();
            if (query != null)
            {
                return query.GroupId;
            }
            if (!string.IsNullOrEmpty(c.Phone2))
            {
                var query2 = (from o in db.Clients
                    where
                        o.ProjectId == c.ProjectId && o.Id != c.Id &&
                        (o.Phone1 == c.Phone2 || (o.Phone2 != null && o.Phone2 == c.Phone2))
                    select o).FirstOrDefault();
                if (query2 != null)
                {
                   return query2.GroupId;
                }
            }
            return 0;
        }
        [HttpPost]
        public JsonResult PhoneCheck(string phone)//检查格式是否正确
        {
            Result result = new Result();
            if (phone.Length < 8 || (phone[0].Equals('1') && phone.Length != 11))
            {
                result.success = false;
                result.obj = "号码格式错误";
                return Json(result);
            }
            else
            {
                foreach (char c in phone)
                {
                    if(!Char.IsDigit(c)&&!c.Equals('*')&&!c.Equals('-'))
                    {
                        result.success = false;
                        result.obj = "号码格式错误";
                        return Json(result);
                    }
                }
            }
            //if (id == 0)
            //{
            //    var query = (from o in db.Clients where o.AllPhone.Contains(phone) select o).FirstOrDefault();
            //    if (query != null)
            //    {
            //        result.success = false;
            //        result.obj = "该号码已经存在用户，所在项目为：" + DepartmentBLL.GetNameById(query.ProjectId);
            //        return Json(result);
            //    }
            //}
            result.success = true;
            return Json(result);
        }

        public ActionResult TransferBatch(int projectid)
        {
            List<SelectListItem> groups = new List<SelectListItem>();
            //groups.Add(new SelectListItem());
            DepartmentBLL.GetGroupsByPid(projectid).ForEach(o =>
            {
                groups.Add(new SelectListItem { Text = o.Name, Value = o.Id.ToString() });
            });
            ViewBag.Groups = groups;
            ViewBag.Default = (from o in groups where o.Text.Equals("公共客户") select o.Value).FirstOrDefault();
            return View();
        }

        [HttpPost]
        public JsonResult TransferBatch(string selectedIds, int newGroupId)
        {
            Result result = new Result();
            int count = 0;
            string[] strs = selectedIds.Split(',');
            using (
                TransactionScope tran = new TransactionScope(TransactionScopeOption.RequiresNew, new TimeSpan(0, 5, 0)))
            {
                foreach (string s in strs)
                {
                    int clientId;
                    if (int.TryParse(s, out clientId))
                    {
                        Client client = db.Clients.Find(clientId);
                        if (client.GroupId != newGroupId)
                        {
                            ClientTransfer ct = new ClientTransfer
                                                {
                                                    ClientId = clientId,
                                                    InGroup = newGroupId,
                                                    OutGroup = client.GroupId,
                                                    Person = UserInfo.CurUser.Id,
                                                    TransferDate = DateTime.Today
                                                };
                            db.ClientTransfers.Add(ct);
                            client.GroupId = newGroupId;
                            db.SaveChanges();
                            string check=Client.StateUpdate(client.Id,DateTime.Today);
                            Utilities.AddLog(db,clientId, Client.LogClass, "客户转移",
                                string.Format("从{0}转移到{1} {2}", DepartmentBLL.GetNameById(ct.OutGroup),
                                    DepartmentBLL.GetNameById(newGroupId),check));
                            //client.StateDate = DateTime.Today;
                            count++;
                        }
                    }
                }
                db.SaveChanges();
                tran.Complete();
                result.success = true;
                result.obj = string.Format("已转移了{0}个客户", count);
            }

            return Json(result);
        }

        public ActionResult TransferSingle(int projectid, int id)//此处id为clientid
        {
            List<SelectListItem> groups = new List<SelectListItem>();
            groups.Add(new SelectListItem());
            DepartmentBLL.GetGroupsByPid(projectid).ForEach(o =>
            {
                groups.Add(new SelectListItem { Text = o.Name, Value = o.Id.ToString() });
            });
            ViewBag.Groups = groups;
            ViewBag.ClientId = id;
            return View();
        }

        [HttpPost]
        public JsonResult TransferSingleProcess(int newGroupId, int clientId)
        {
            Result result = new Result();
            Client client = db.Clients.Find(clientId);
            if (client.GroupId != newGroupId)
            {
                ClientTransfer ct = new ClientTransfer
                {
                    ClientId = clientId,
                    InGroup = newGroupId,
                    OutGroup = client.GroupId,
                    Person = UserInfo.CurUser.Id,
                    TransferDate = DateTime.Today
                };
                db.ClientTransfers.Add(ct);
                client.GroupId = newGroupId;
                db.SaveChanges();
                string check = Client.StateUpdate(client.Id, DateTime.Today);
                Utilities.AddLog(db, clientId, Client.LogClass, "客户转移",
                    string.Format("从{0}转移到{1} {2}", DepartmentBLL.GetNameById(ct.OutGroup),
                        DepartmentBLL.GetNameById(newGroupId), check));

            }
            else
            {
                result.success = false;
                result.obj = string.Format("此客户已在{0}中，不需要转移", DepartmentBLL.GetNameById(client.GroupId));
                return Json(result);
            }

            db.SaveChanges();
            result.success = true;
            result.obj = string.Format("转移客户成功");


            return Json(result);
        }

        public ActionResult ClientTransferIn()
        {
            ViewBag.GroupId = (from o in UserInfo.CurUser.Departments where o.DepartmentType == "小组" select o.Id).FirstOrDefault();
            ViewBag.Date1 = DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd");
            ViewBag.Date2 = DateTime.Today.ToString("yyyy-MM-dd");
            return View();
        }

        [HttpPost]
        public JsonResult ClientTransferInQuery(int projectid, FormCollection collection)
        {
            //if (!UserInfo.CurUser.HasRight("系统管理-部门管理")) return Json(new Result {success=false, obj = "没有权限" });
            Result result = new Result();
            List<object> parameters = new List<object>();
            //int GroupId = int.Parse(collection["groupId"]);
            //string GroupName = DepartmentBLL.GetNameById(GroupId);


            string sql = ClientTransformLog.sql;
            if (UserInfo.CurUser.GetClientRight(projectid) > ClientViewScopeEnum.查看本组)
            {
            }
            else
            {
                sql += " and cf.ingroup =" + UserInfo.CurUser.GetGroup(projectid).ToString();
            }
            Utilities.AddSqlFilterDateGreaterThen(collection, "from", ref sql, "cf.TransferDate", parameters);
            Utilities.AddSqlFilterDateLessThen(collection, "to", ref sql, "cf.TransferDate", parameters);

            db.Database.Connection.Open();
            var dynamicParams = new DynamicParameters();
            parameters.ForEach(o => { var p = o as SqlParameter; dynamicParams.Add(p.ParameterName, p.Value, p.DbType); });
            var query = db.Database.Connection.Query<ClientTransformLog>(sql, param: dynamicParams);
            db.Database.Connection.Close();
            result.success = true;
            result.obj = query;
            return Json(result);
        }

        public ActionResult ClientTransferOut()
        {
            //ViewBag.GroupId = (from o in UserInfo.CurUser.Departments where o.DepartmentType == "小组" select o.Id).FirstOrDefault();
            ViewBag.Date1 = DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd");
            ViewBag.Date2 = DateTime.Today.ToString("yyyy-MM-dd");
            return View();
        }

        [HttpPost]
        public JsonResult ClientTransferOutQuery(int projectid, FormCollection collection)
        {
            //if (!UserInfo.CurUser.HasRight("系统管理-部门管理")) return Json(new Result {success=false, obj = "没有权限" });
            Result result = new Result();
            List<object> parameters = new List<object>();
            //int GroupId = int.Parse(collection["groupId"]);
            //string GroupName = DepartmentBLL.GetNameById(GroupId);
            string sql = ClientTransformLog.sql;
            if (UserInfo.CurUser.GetClientRight(projectid) > ClientViewScopeEnum.查看本组)
            {
            }
            else
            {
                sql += " and cf.outgroup =" + UserInfo.CurUser.GetGroup(projectid).ToString();
            }
            Utilities.AddSqlFilterDateGreaterThen(collection, "from", ref sql, "cf.TransferDate", parameters);
            Utilities.AddSqlFilterDateLessThen(collection, "to", ref sql, "cf.TransferDate", parameters);

            db.Database.Connection.Open();
            var dynamicParams = new DynamicParameters();
            parameters.ForEach(o => { var p = o as SqlParameter; dynamicParams.Add(p.ParameterName, p.Value, p.DbType); });
            var query = db.Database.Connection.Query<ClientTransformLog>(sql, param: dynamicParams);
            db.Database.Connection.Close();
            result.success = true;
            result.obj = query;
            return Json(result);
        }

        public ActionResult InviteList(string type)
        {
            ViewBag.Date1 = DateTime.Today.ToString("yyyy-MM-dd");
            ViewBag.Date2 = DateTime.Today.ToString("yyyy-MM-dd");
            ViewBag.Type = type;
            return View();
        }

        public ActionResult InviteListQuery(int projectid, string dateFrom, string dateTo, FormCollection collection,string type)
        {
            Result result = new Result();
            DateTime date1 = new DateTime();
            DateTime date2 = new DateTime();
            if (!DateTime.TryParse(dateFrom, out date1))
            {
                result.obj = "请输入有效日期";
                return Json(result);
            }
            if (!DateTime.TryParse(dateTo, out date2))
            {
                result.obj = "请输入有效日期";
                return Json(result);
            }
            int groupid = 0;
            if (UserInfo.CurUser.GetClientRight(projectid) < ClientViewScopeEnum.查看项目)
            {
                groupid = UserInfo.CurUser.GetGroup(projectid);
            }
            var list = ClientActivityListView.GetReport(projectid, groupid, date1, date2, collection["client"]);
            if(!string.IsNullOrEmpty(type))
            {
                list = (from o in list where o.Type.Equals(type) select o).ToList();
            }
            
            if(!string.IsNullOrEmpty(collection["Phone"]))
            {
                list = (from o in list where o.AllPhone.Contains(collection["Phone"]) select o).ToList();
            }
            result.obj = list;
            result.success = true;
            return Json(result);
        }
        public ActionResult InviteReport(int projectid)
        {
            ViewBag.Date1 = DateTime.Today.ToString("yyyy-MM-dd");
            ViewBag.Date2 = DateTime.Today.ToString("yyyy-MM-dd");
            return View();
        }

        public ActionResult InviteReportQuery(int projectid, string dateFrom, string dateTo, FormCollection collection)
        {
            Result result = new Result();
            DateTime date1 = new DateTime();
            DateTime date2 = new DateTime();
            if (!DateTime.TryParse(dateFrom, out date1))
            {
                result.obj = "请输入有效日期";
                return Json(result);
            }
            if (!DateTime.TryParse(dateTo, out date2))
            {
                result.obj = "请输入有效日期";
                return Json(result);
            }
            int groupid = 0;
            if (UserInfo.CurUser.GetClientRight(projectid) <= ClientViewScopeEnum.查看本组)
            {
                groupid = UserInfo.CurUser.GetGroup(projectid);
            }

            List<ClientActivityReportView> inviteList = new List<ClientActivityReportView>();
            List<string> yaoyueTypes = DictionaryBLL.GetByName("邀约类型", false);
            yaoyueTypes.ForEach(o =>
            {
                inviteList.Add(new ClientActivityReportView { Type = o });
            });
            List<ClientActivityReportView> caList = ClientActivityReportView.GetReport(projectid, groupid,
                DateTime.Today, DateTime.Today);
            foreach (var i in caList)
            {
                foreach (var j in inviteList)
                {
                    if (i.Type == j.Type)
                    {
                        j.Num += i.Num;
                        j.VisitNum += i.VisitNum;
                        j.DoneNum += i.DoneNum;
                    }
                }
            }

            result.obj = new { Total = inviteList, list = caList };
            result.success = true;
            return Json(result);
        }

        public ActionResult AlertList(string type)
        {
            ViewBag.Type = type;
            return View();
        }

        public ActionResult AlertListQuery(int projectid, string dateFrom, string dateTo, FormCollection collection, string type)
        {
            Result result = new Result();
            int groupid = 0;
            if (UserInfo.CurUser.GetClientRight(projectid) < ClientViewScopeEnum.查看项目)
            {
                groupid = UserInfo.CurUser.GetGroup(projectid);
            }
            Project project = Project.Get(projectid);
            List<OutTimeClient> list = new List<OutTimeClient>(); 
            if (string.IsNullOrEmpty(type))
            {
                list=(project.GetOutTimeAlert(projectid, groupid, ClientStateEnum.来电客户));
                list.AddRange(project.GetOutTimeAlert(projectid, groupid, ClientStateEnum.来访客户));
                list.AddRange(project.GetOutTimeAlert(projectid, groupid, ClientStateEnum.办卡客户));
            }
            else
            {
                switch (type)
                {
                    case "电转访超期预警":
                        list = project.GetOutTimeAlert(projectid, groupid, ClientStateEnum.来电客户);
                        break;
                    case "访转卡超期预警":
                        list = project.GetOutTimeAlert(projectid, groupid, ClientStateEnum.来访客户);
                        break;
                    case "卡转定超期预警":
                        list = project.GetOutTimeAlert(projectid, groupid, ClientStateEnum.办卡客户);
                        break;
                }
            }
            result.obj = list;
            result.success = true;
            return Json(result);
        }

    }

}
