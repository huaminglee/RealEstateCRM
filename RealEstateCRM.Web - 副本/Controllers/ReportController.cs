using System;
using System.Collections.Generic;
using System.Dynamic;
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
    public class ReportController : BaseController
    {
        private Context db = new Context();

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);

        }

        public ActionResult ClientStateReport(int projectid,string datefrom,string dateto)
        {
            if (datefrom == null && dateto == null)
            {
                DateTime d2 = DateTime.Today;
                int wd1 = (int) d2.DayOfWeek;
                DateTime d1 = wd1 == 0 ? d2.AddDays(-6) : d2.AddDays(1 - wd1);
                ViewBag.Date1 = d1.ToString("yyyy-MM-dd");
                ViewBag.Date2 = d2.ToString("yyyy-MM-dd");
            }
            else
            {
                ViewBag.Date1 = datefrom;
                ViewBag.Date2 = dateto;
            }
            
            List<RoomType> roomTypes = (from o in db.RoomTypes where o.DepartmentId == projectid select o).ToList();
            ViewBag.RoomTypes = roomTypes;
            return View();
        }

        public ActionResult ClientStateReportQuery(int projectid, string dateFrom, string dateTo, FormCollection collection)
        {
            bool groupOnly = UserInfo.CurUser.GetClientRight(projectid) < ClientViewScopeEnum.查看项目;
            int groupid = 0;
            if (groupOnly)
            {
                groupid = UserInfo.CurUser.GetGroup(projectid);
            }
            Result result = new Result();
            DateTime d1 = Utilities.ParseDate(dateFrom);
            DateTime d2 = Utilities.ParseDate(dateTo);
            List<ClientStateProjectReportVO> list = new List<ClientStateProjectReportVO>();
            if (groupOnly)
            {
                list.Add(new ClientStateProjectReportVO
                         {
                             GroupId = groupid,
                             GroupName = DepartmentBLL.GetNameById(groupid)
                         });
            }
            else
            {

                var groups = DepartmentBLL.GetGroupsByPid(projectid);
                groups.ForEach(o =>
                {
                    if (o.Name != "前台" && o.Name != "公共客户" && o.Name != "沉睡客户")
                        list.Add(new ClientStateProjectReportVO { GroupId = o.Id, GroupName = o.Name });
                });
                groups.ForEach(o =>
                {
                    if (o.Name == "公共客户")
                        list.Add(new ClientStateProjectReportVO { GroupId = o.Id, GroupName = o.Name });
                });
                groups.ForEach(o =>
                {
                    if (o.Name == "沉睡客户")
                        list.Add(new ClientStateProjectReportVO { GroupId = o.Id, GroupName = o.Name });
                });
            }
            string roomTypeSql = "";
            if (!string.IsNullOrEmpty(collection["roomType"]))
            {
                roomTypeSql = " and c.roomtype=@roomType ";
            }
            
            string preSql = "declare @d1 datetime ,@d2 datetime ;set @d1='{0:yyyy-MM-dd}';set @d2='{1:yyyy-MM-dd}';declare @roomType nvarchar(50);set @roomType='{2}';";
            preSql = string.Format(preSql, d1, d2,collection["roomType"]);
            //来电，来访，，可按这方式取，小卡大卡大定和签约需要另外计算，因为大定和签约算套数，小卡要计算退卡

            string sql1 =
                @"{0} select count(*) as num ,groupid as id  from clients c join clientactivities ca on c.id=ca.clientid
where c.projectid={1} and c.state!={4} and ca.firsttype={2} and ca.actualtime between @d1 and DATEADD(MINUTE,59, dateadd(HH,23,@d2)) {3}
 group by groupid";
            var query1 = db.Database.SqlQuery<IdInt>(string.Format(sql1, preSql, projectid, 1,roomTypeSql,(int)ClientStateEnum.无效客户)).ToList();
            foreach (var i in query1)
            {
                foreach (var j in list)
                {
                    if (i.Id == j.GroupId)
                    {
                        j.CallInNum = i.Num;
                    }
                }
            }
            var query2 = db.Database.SqlQuery<IdInt>(string.Format(sql1, preSql, projectid, 2, roomTypeSql, (int)ClientStateEnum.无效客户)).ToList();
            foreach (var i in query2)
            {
                foreach (var j in list)
                {
                    if (i.Id == j.GroupId)
                    {
                        j.VisitNum = i.Num;
                    }
                }
            }
            //ToDo: 如果小组变化了，那办卡等数据算谁的？
            //小卡
            string sql2 =
                @"{0} select count(*) as num ,c.groupid as id from clients c join cards on c.id=cards.clientid   where c.projectid={1} and cards.smalltime between @d1 and @d2 {2} group by c.groupid";
            string sql3 =
               @"{0} select count(*) as num ,c.groupid as id from clients c join cards on c.id=cards.clientid   where c.projectid={1} and cards.canceltime between @d1 and @d2 {2} group by c.groupid";
            var query3 = db.Database.SqlQuery<IdInt>(string.Format(sql2, preSql, projectid,roomTypeSql)).ToList();
            foreach (var i in query3)
            {
                foreach (var j in list)
                {
                    if (i.Id == j.GroupId)
                    {
                        j.Card1Num = i.Num;
                    }
                }
            }
            var query4 = db.Database.SqlQuery<IdInt>(string.Format(sql3, preSql, projectid,roomTypeSql)).ToList();
            foreach (var i in query4)
            {
                foreach (var j in list)
                {
                    if (i.Id == j.GroupId)
                    {
                        j.Card1Num -= i.Num;
                    }
                }
            }
            //大卡
            string sql4 =
               @"{0} select count(*) as num ,c.groupid as id from clients c join cards  on c.id=cards.clientid  where c.projectid={1} and cards.bigtime between @d1 and @d2 {2} group by c.groupid";
            string sql5 =
               @"{0} select count(*) as num ,c.groupid as id from clients c join cards  on c.id=cards.clientid  where c.projectid={1} and cards.bigtime!=null and cards.canceltime between @d1 and @d2 {2} group by c.groupid";
            var query5 = db.Database.SqlQuery<IdInt>(string.Format(sql4, preSql, projectid,roomTypeSql)).ToList();
            foreach (var i in query5)
            {
                foreach (var j in list)
                {
                    if (i.Id == j.GroupId)
                    {
                        j.Card2Num = i.Num;
                    }
                }
            }
            var query6 = db.Database.SqlQuery<IdInt>(string.Format(sql5, preSql, projectid,roomTypeSql)).ToList();
            foreach (var i in query6)
            {
                foreach (var j in list)
                {
                    if (i.Id == j.GroupId)
                    {
                        j.Card2Num -= i.Num;
                    }
                }
            }
            //大定
            string sql6 =
              @"{0} select count(*) as num ,c.groupid as id from clients c  where c.projectid={1} and exists (select 1 from orders o where o.clientid=c.id and o.ordertime between @d1 and @d2) {2} group by c.groupid";
            string sql7 =
            @"{0} select count(*) as num ,c.groupid as id from clients c  where c.projectid={1} and exists (select 1 from orders o where o.clientid=c.id and o.canceltime between @d1 and @d2) {2} group by c.groupid";
            var query7 = db.Database.SqlQuery<IdInt>(string.Format(sql6, preSql, projectid,roomTypeSql)).ToList();
            foreach (var i in query7)
            {
                foreach (var j in list)
                {
                    if (i.Id == j.GroupId)
                    {
                        j.OrderNum = i.Num;
                    }
                }
            }
            var query8 = db.Database.SqlQuery<IdInt>(string.Format(sql7, preSql, projectid,roomTypeSql)).ToList();
            foreach (var i in query8)
            {
                foreach (var j in list)
                {
                    if (i.Id == j.GroupId)
                    {
                        j.OrderNum = -i.Num;
                    }
                }
            }
            //签约
            string sql8 =
             @"{0} select count(*) as num ,c.groupid as id from clients c  join orders o on o.clientid=c.id where c.projectid={1}  and o.signtime between @d1 and @d2 {2} group by c.groupid";
            string sql9 =
            @"{0} select count(*) as num ,c.groupid as id from clients c join orders o on o.clientid=c.id  where c.projectid={1} and o.signtime!=null and o.canceltime between @d1 and @d2 {2} group by c.groupid";
            var query9 = db.Database.SqlQuery<IdInt>(string.Format(sql8, preSql, projectid,roomTypeSql)).ToList();
            foreach (var i in query9)
            {
                foreach (var j in list)
                {
                    if (i.Id == j.GroupId)
                    {
                        j.ContractNum = i.Num;
                    }
                }
            }
            var query10 = db.Database.SqlQuery<IdInt>(string.Format(sql9, preSql, projectid,roomTypeSql)).ToList();
            foreach (var i in query10)
            {
                foreach (var j in list)
                {
                    if (i.Id == j.GroupId)
                    {
                        j.ContractNum -= i.Num;
                    }
                }
            }
            string sql11 =
                @"{0} select count(*) as num ,groupid as id  from clients c join clientactivities ca on c.id=ca.clientid
join clientactivities ca2 on c.id=ca2.clientid 
where c.projectid={1} and c.state!={3} and ca.firsttype=2 and ca.actualtime between @d1 and DATEADD(MINUTE,59, dateadd(HH,23,@d2))
and ca2.firsttype=1 {2}
 group by groupid";
            var query11 = db.Database.SqlQuery<IdInt>(string.Format(sql11, preSql, projectid,roomTypeSql,(int)ClientStateEnum.无效客户)).ToList();
            foreach (var i in query11)
            {
                foreach (var j in list)
                {
                    if (i.Id == j.GroupId)
                    {
                        j.CallVisitNum = i.Num;
                    }
                }
            }
            result.obj = new { list = list, d1 = d1.ToString("yyyy-MM-dd"), d2 = d2.ToString("yyyy-MM-dd"),RoomType=collection["roomtype"] };
            result.success = true;
            return Json(result);
        }
        public ActionResult ClientStateList(int projectid, int groupid, string dateFrom, string dateTo, string type,string roomtype)
        {
            //DateTime d2 = DateTime.Today;
            //int wd1 = (int)d2.DayOfWeek;
            //DateTime d1 = wd1 == 0 ? d2.AddDays(-6) : d2.AddDays(1 - wd1);
            switch (type)
            {
                case "0":
                    ViewBag.State = "来电客户";
                    break;
                case "1":
                    ViewBag.State = "来访客户";
                    break;
                case "2":
                    ViewBag.State = "小卡客户";
                    break;
                case "3":
                    ViewBag.State = "大卡客户";
                    break;
                case "4":
                    ViewBag.State = "大定客户";
                    break;
                case "5":
                    ViewBag.State = "签约客户";
                    break;
            }
            ViewBag.GroupId = groupid;
            ViewBag.D1 = dateFrom;
            ViewBag.D2 = dateTo;
            ViewBag.Type = type;
            ViewBag.RoomType = roomtype;
            return View();
        }
        [HttpPost]
        public ActionResult ClientStateListQuery(int projectid, int groupid, string dateFrom, string dateTo, string type, FormCollection collection)
        {
            bool groupOnly = UserInfo.CurUser.GetClientRight(projectid) < ClientViewScopeEnum.查看项目;
            if (groupOnly)
            {
                groupid = UserInfo.CurUser.GetGroup(projectid);
            }
            Result result = new Result();
            DateTime d1 = Utilities.ParseDate(dateFrom);
            DateTime d2 = Utilities.ParseDate(dateTo);
            string roomTypeSql = "";
            if (!string.IsNullOrEmpty(collection["roomType"]))
            {
                roomTypeSql = " and c.roomtype=@roomType ";
            }

            string preSql = "declare @d1 datetime ,@d2 datetime ;set @d1='{0:yyyy-MM-dd}';set @d2='{1:yyyy-MM-dd}';declare @roomType nvarchar(50);set @roomType='{2}';";
            
            preSql = string.Format(preSql, d1, d2,collection["roomtype"]);
            //来电，来访，，可按这方式取，小卡大卡大定和签约需要另外计算，因为大定和签约算套数，小卡要计算退卡
            string groupSql = "";
            if (groupid != 0)
            {
                groupSql = " and c.groupid=" + groupid.ToString();
            }
            string sql1 =
                @"{0} select '{4}' as action, c.id,c.groupid,c.roomtype,c.name,c.way,c.wayextend ,ca.ActualTime as statedate  from clients c join clientactivities ca on c.id=ca.clientid where c.projectid={1} and c.state!={7} and ca.firsttype={5} and ca.actualtime between @d1 and DATEADD(MINUTE,59, dateadd(HH,23,@d2)) {3} {6}";
            //小卡
            string sql2 =
                @"{0} select '办卡' as action, c.id,c.groupid,c.roomtype,c.name,c.way,c.wayextend ,cards.remark,cards.smalltime as statedate from clients c join cards on c.id=cards.clientid   where c.projectid={1} and cards.smalltime between @d1 and @d2 {2} {3}
union select '退卡' as action, c.id,c.groupid,c.roomtype,c.name,c.way ,c.wayextend,cards.remark,cards.canceltime as statedate  from clients c join cards on c.id=cards.clientid   where c.projectid={1} and cards.canceltime between @d1 and @d2 {2} {3}";

            string sql3 =
               @"{0} select '升卡' as action, c.id,c.groupid,c.roomtype,c.name,c.way,c.wayextend,cards.remark ,cards.bigtime as statedate from clients c join cards  on c.id=cards.clientid  where c.projectid={1} and cards.bigtime between @d1 and @d2 {2} {3}
union select '退卡' as action, c.id,c.groupid,c.roomtype,c.name,c.way,c.wayextend ,cards.remark,cards.canceltime as statedate from clients c join cards  on c.id=cards.clientid  where c.projectid={1} and cards.bigtime!=null and cards.canceltime between @d1 and @d2 {2} {3}";

            string sql4 =
              @"{0} select '大定' as action, c.id,c.groupid,c.roomtype,c.name,c.way,c.wayextend,o.remark  ,o.ordertime as statedate from clients c join orders o on c.id=o.clientid  where c.projectid={1} and o.ordertime between @d1 and @d2 {2} {3}
union all select '退订' as action, c.id,c.groupid,c.roomtype,c.name,c.way,c.wayextend,o.remark  ,o.canceltime as statedate from clients c join orders o on c.id=o.clientid  where c.projectid={1} and o.canceltime between @d1 and @d2 {2} {3}";

            //签约
            string sql5 =
             @"{0} select '签约' as action, c.id,c.groupid,c.roomtype,c.name,c.way,c.wayextend,o.remark  ,o.signtime as statedate from clients c  join orders o on c.id=o.clientid where c.projectid={1} and o.signtime between @d1 and @d2 {2} {3}
union all select '退房' as action, c.id,c.groupid,c.roomtype,c.name,c.way,c.wayextend,o.remark  ,o.canceltime as statedate from clients c  join orders o on c.id=o.clientid where c.projectid={1} and o.signtime!=null and o.canceltime between @d1 and @d2 {2} {3}";
            string sql6 = @"{0} select '{4}' as action, c.id,c.groupid,c.roomtype,c.name,c.way,c.wayextend ,ca.ActualTime as statedate  from clients c 
join clientactivities ca on c.id=ca.clientid join clientactivities ca2 on c.id=ca2.clientid  
where c.projectid={1} and  c.state!={6} and ca.firsttype=2 and ca.actualtime between @d1 and DATEADD(MINUTE,59, dateadd(HH,23,@d2))
and ca2.firsttype=1 
{3}  {5}";

            string sql = null;
            switch (type)
            {
                case "0":
                    sql = string.Format(sql1, preSql, projectid, 1, groupSql, "来电", 1,roomTypeSql,(int)ClientStateEnum.无效客户);
                    break;
                case "1":
                    sql = string.Format(sql1, preSql, projectid, 2, groupSql, "来访", 2, roomTypeSql, (int)ClientStateEnum.无效客户);
                    break;
                case "2":
                    sql = string.Format(sql2, preSql, projectid, groupSql, roomTypeSql);
                    break;
                case "3":
                    sql = string.Format(sql3, preSql, projectid, groupSql, roomTypeSql);
                    break;
                case "4":
                    sql = string.Format(sql4, preSql, projectid, groupSql, roomTypeSql);
                    break;
                case "5":
                    sql = string.Format(sql5, preSql, projectid, groupSql, roomTypeSql);
                    break;
                case "6":
                    sql = string.Format(sql6, preSql, projectid, 1, groupSql, "电转访", roomTypeSql,(int)ClientStateEnum.无效客户);
                    break;
            }
            var list = db.Database.SqlQuery<ClientListVO>(sql).ToList();
            list.ForEach(o =>
            {
                o.GroupName = DepartmentBLL.GetNameById(o.GroupId);
            });
            result.obj = new { list = list };
            result.success = true;
            return Json(result);
        }

        public ActionResult ClientWayReport1(int projectid,string datefrom ,string dateto)
        {
            if (datefrom == null && dateto == null)
            {
                DateTime d2 = DateTime.Today;
                int wd1 = (int)d2.DayOfWeek;
                DateTime d1 = wd1 == 0 ? d2.AddDays(-6) : d2.AddDays(1 - wd1);
                ViewBag.Date1 = d1.ToString("yyyy-MM-dd");
                ViewBag.Date2 = d2.ToString("yyyy-MM-dd");
            }
            else
            {
                ViewBag.Date1 = datefrom;
                ViewBag.Date2 = dateto;
            }
            List<RoomType> roomTypes = (from o in db.RoomTypes where o.DepartmentId == projectid select o).ToList();
            ViewBag.RoomTypes = roomTypes;
            return View();
        }
        public ActionResult ClientWayReport2(int projectid,string datefrom, string dateto)
        {
            if (datefrom == null && dateto == null)
            {
                DateTime d2 = DateTime.Today;
                int wd1 = (int)d2.DayOfWeek;
                DateTime d1 = wd1 == 0 ? d2.AddDays(-6) : d2.AddDays(1 - wd1);
                ViewBag.Date1 = d1.ToString("yyyy-MM-dd");
                ViewBag.Date2 = d2.ToString("yyyy-MM-dd");
            }
            else
            {
                ViewBag.Date1 = datefrom;
                ViewBag.Date2 = dateto;
            }
            List<RoomType> roomTypes = (from o in db.RoomTypes where o.DepartmentId == projectid select o).ToList();
            ViewBag.RoomTypes = roomTypes;
            return View();
        }
        public ActionResult ClientWayReportQuery(int projectid, string dateFrom, string dateTo, FormCollection collection)
        {
            bool groupOnly = UserInfo.CurUser.GetClientRight(projectid) < ClientViewScopeEnum.查看项目;


            if (groupOnly)
            {
                return Redirect("~/content/accessdeny.htm");
            }
            Result result = new Result();
            DateTime d1 = Utilities.ParseDate(dateFrom);
            DateTime d2 = Utilities.ParseDate(dateTo);
            List<WayReportVO> list = new List<WayReportVO>();
            string preSql = "declare @d1 datetime ,@d2 datetime ;set @d1='{0:yyyy-MM-dd}';set @d2='{1:yyyy-MM-dd}';declare @way nvarchar(50);set @way='{2}';declare @roomType nvarchar(50);set @roomType='{3}';";
            preSql = string.Format(preSql, d1, d2, collection["way"], collection["roomType"]);
         
            string roomTypeSql = "";
            if (!string.IsNullOrEmpty(collection["roomType"]))
            {
                roomTypeSql = " and c.roomtype=@roomType ";
            }
            
            //来电，来访，，可按这方式取，小卡大卡大定和签约需要另外计算，因为大定和签约算套数，小卡要计算退卡

            string sql1 =
                @"{0} select count(*) as id ,c.way as Name from clients c join clientactivities ca on c.id=ca.clientid
where c.projectid={1} and c.state!={3} and ca.firsttype={2} and ca.actualtime >=@d1 and ca.actualtime<DATEADD(d,1, @d2) {4}
 group by c.way";
            var query1 = db.Database.SqlQuery<IdName>(string.Format(sql1, preSql, projectid, 1,(int)ClientStateEnum.无效客户,roomTypeSql)).ToList();
            foreach (var i in query1)
            {
                WayReportVO vo = WayReportVO.GetVo(list, i.Name);
                vo.CallInNum = i.Id;
            }
            var query2 = db.Database.SqlQuery<IdName>(string.Format(sql1, preSql, projectid, 2,(int)ClientStateEnum.无效客户,roomTypeSql)).ToList();
            foreach (var i in query2)
            {
                WayReportVO vo = WayReportVO.GetVo(list, i.Name);
                vo.VisitNum = i.Id;
            }
            //小卡
            string sql2 =
               @"{0} select count(*) as id ,c.way as Name from clients c join cards on c.id=cards.clientid   where c.projectid={1} {2} and cards.smalltime between @d1 and @d2 group by c.way";
            string sql3 =
               @"{0} select count(*) as id ,c.way as Name from clients c join cards on c.id=cards.clientid   where c.projectid={1} {2} and cards.canceltime between @d1 and @d2 group by c.way";
            var query3 = db.Database.SqlQuery<IdName>(string.Format(sql2, preSql, projectid,roomTypeSql)).ToList();

            var query4 = db.Database.SqlQuery<IdName>(string.Format(sql3, preSql, projectid,roomTypeSql)).ToList();
            foreach (var i in query3)
            {
                WayReportVO vo = WayReportVO.GetVo(list, i.Name);
                vo.Card1Num = i.Id;
            }
            foreach (var i in query4)
            {
                WayReportVO vo = WayReportVO.GetVo(list, i.Name);
                vo.Card1Num -= i.Id;
            }
            //大卡
            string sql4 =
               @"{0} select count(*) as id ,c.way as Name from clients c join cards  on c.id=cards.clientid  where c.projectid={1} {2} and cards.bigtime between @d1 and @d2 group by c.way";
            string sql5 =
               @"{0} select count(*) as id ,c.way as Name from clients c join cards  on c.id=cards.clientid  where c.projectid={1} {2} and cards.bigtime!=null and cards.canceltime between @d1 and @d2 group by c.way";
            var query5 = db.Database.SqlQuery<IdName>(string.Format(sql4, preSql, projectid,roomTypeSql)).ToList();
            var query6 = db.Database.SqlQuery<IdName>(string.Format(sql5, preSql, projectid,roomTypeSql)).ToList();
            foreach (var i in query5)
            {
                WayReportVO vo = WayReportVO.GetVo(list, i.Name);
                vo.Card2Num = i.Id;
            }
            foreach (var i in query6)
            {
                WayReportVO vo = WayReportVO.GetVo(list, i.Name);
                vo.Card2Num -= i.Id;
            }

            //大定
            string sql6 =
              @"{0} select count(*) as id ,c.way as Name  from clients c  where c.projectid={1} {2} and exists (select 1 from orders o where o.clientid=c.id and o.ordertime between @d1 and @d2) group by c.way";
            string sql7 =
            @"{0} select count(*) as id ,c.way as Name  from clients c  where c.projectid={1} {2} and exists (select 1 from orders o where o.clientid=c.id and o.canceltime between @d1 and @d2) group by c.way";
            var query7 = db.Database.SqlQuery<IdName>(string.Format(sql6, preSql, projectid,roomTypeSql)).ToList();
            var query8 = db.Database.SqlQuery<IdName>(string.Format(sql7, preSql, projectid,roomTypeSql)).ToList();
            foreach (var i in query7)
            {
                WayReportVO vo = WayReportVO.GetVo(list, i.Name);
                vo.OrderNum = i.Id;
            }
            foreach (var i in query8)
            {
                WayReportVO vo = WayReportVO.GetVo(list, i.Name);
                vo.OrderNum -= i.Id;
            }

            //签约
            string sql8 =
             @"{0} select count(*)  as id ,c.way as Name from clients c  where c.projectid={1} {2} and exists (select 1 from orders o where o.clientid=c.id and o.signtime between @d1 and @d2) group by c.way";
            string sql9 =
            @"{0} select count(*)  as id ,c.way as Name from clients c  where c.projectid={1} {2} and exists (select 1 from orders o where o.clientid=c.id and o.signtime!=null and o.canceltime between @d1 and @d2) group by c.way";
            var query9 = db.Database.SqlQuery<IdName>(string.Format(sql8, preSql, projectid,roomTypeSql)).ToList();
            var query10 = db.Database.SqlQuery<IdName>(string.Format(sql9, preSql, projectid,roomTypeSql)).ToList();
            foreach (var i in query9)
            {
                WayReportVO vo = WayReportVO.GetVo(list, i.Name);
                vo.ContractNum = i.Id;
            }
            foreach (var i in query10)
            {
                WayReportVO vo = WayReportVO.GetVo(list, i.Name);
                vo.ContractNum -= i.Id;
            }

            string sql11 =
                @"{0} select count(*) as id ,c.way as Name from clients c join clientactivities ca on c.id=ca.clientid
join clientactivities ca2 on c.id=ca2.clientid 
where c.projectid={1} {2} and ca.firsttype=2 and ca.actualtime between @d1 and DATEADD(MINUTE,59, dateadd(HH,23,@d2))
and ca2.firsttype=1 
 group by c.way";
            var query11 = db.Database.SqlQuery<IdName>(string.Format(sql11, preSql, projectid,roomTypeSql)).ToList();
            foreach (var i in query11)
            {
                WayReportVO vo = WayReportVO.GetVo(list, i.Name);
                vo.CallVisitNum = i.Id;
            }


            result.obj = new { list = list, d1 = d1.ToString("yyyy-MM-dd"), d2 = d2.ToString("yyyy-MM-dd"), RoomType = collection["roomType"] };
            result.success = true;
            return Json(result);
        }
        public ActionResult ClientWayList(int projectid, string way,string roomType, string dateFrom, string dateTo, string type)
        {
            //DateTime d2 = DateTime.Today;
            //int wd1 = (int)d2.DayOfWeek;
            //DateTime d1 = wd1 == 0 ? d2.AddDays(-6) : d2.AddDays(1 - wd1);
            switch (type)
            {
                case "0":
                    ViewBag.State = "来电客户";
                    break;
                case "1":
                    ViewBag.State = "来访客户";
                    break;
                case "2":
                    ViewBag.State = "小卡客户";
                    break;
                case "3":
                    ViewBag.State = "大卡客户";
                    break;
                case "4":
                    ViewBag.State = "大定客户";
                    break;
                case "5":
                    ViewBag.State = "签约客户";
                    break;
            }
            ViewBag.Way = way;
            ViewBag.D1 = dateFrom;
            ViewBag.D2 = dateTo;
            ViewBag.Type = type;
            ViewBag.RoomType = roomType;
            return View();
        }


        [HttpPost]
        public ActionResult ClientWayListQuery(int projectid, string way,string roomtype, string dateFrom, string dateTo, string type, FormCollection collection)
        {
            bool groupOnly = UserInfo.CurUser.GetClientRight(projectid) < ClientViewScopeEnum.查看项目;
            if (groupOnly)
            {
                return Redirect("~/Content/accessdeny.htm");
            }
            Result result = new Result();
            DateTime d1 = Utilities.ParseDate(dateFrom);
            DateTime d2 = Utilities.ParseDate(dateTo);
            string preSql = "declare @d1 datetime ,@d2 datetime ;set @d1='{0:yyyy-MM-dd}';set @d2='{1:yyyy-MM-dd}';declare @way nvarchar(50);set @way='{2}';declare @roomType nvarchar(50);set @roomType='{3}';";
            preSql = string.Format(preSql, d1, d2,way, roomtype);
           
            string roomTypeSql = "";
            if (!string.IsNullOrEmpty(roomtype))
            {
                roomTypeSql = " and c.roomtype=@roomType ";
            }
            
            //来电，来访，，可按这方式取，小卡大卡大定和签约需要另外计算，因为大定和签约算套数，小卡要计算退卡
            string waySql = "";
            if (way == "来电/直访")
            {
                waySql = " and c.way not in('电话中心','中介','销售员拓客')";
            }
            else
            {
                waySql = " and c.way=@way";
            }

            string sql1 =
                @"{0} select '{4}' as action, c.id,c.groupid,c.roomtype,c.name,c.way,c.wayextend ,ca.ActualTime as statedate  from clients c join clientactivities ca on c.id=ca.clientid where c.projectid={1} and c.state!={6} {7} and ca.firsttype={5} and ca.actualtime between @d1 and DATEADD(MINUTE,59, dateadd(HH,23,@d2)) {3}";
            //小卡
            string sql2 =
                @"{0} select '办卡' as action, c.id,c.groupid,c.roomtype,c.name,c.way,c.wayextend ,cards.remark,cards.smalltime as statedate from clients c join cards on c.id=cards.clientid   where c.projectid={1} {3} and cards.smalltime between @d1 and @d2 {2}
union select '退卡' as action, c.id,c.groupid,c.roomtype,c.name,c.way ,c.wayextend,cards.remark,cards.canceltime as statedate  from clients c join cards on c.id=cards.clientid   where c.projectid={1} {3} and cards.canceltime between @d1 and @d2 {2}";

            string sql3 =
               @"{0} select '升卡' as action, c.id,c.groupid,c.roomtype,c.name,c.way,c.wayextend,cards.remark ,cards.bigtime as statedate from clients c join cards  on c.id=cards.clientid  where c.projectid={1} {3} and cards.bigtime between @d1 and @d2 {2}
union select '退卡' as action, c.id,c.groupid,c.roomtype,c.name,c.way,c.wayextend ,cards.remark,cards.canceltime as statedate from clients c join cards  on c.id=cards.clientid  where c.projectid={1} {3} and cards.bigtime!=null and cards.canceltime between @d1 and @d2 {2}";

            string sql4 =
              @"{0} select '大定' as action, c.id,c.groupid,c.roomtype,c.name,c.way,c.wayextend,o.remark  ,o.ordertime as statedate from clients c join orders o on c.id=o.clientid  where c.projectid={1} {3} and o.ordertime between @d1 and @d2 {2}
union select '退订' as action, c.id,c.groupid,c.roomtype,c.name,c.way,c.wayextend,o.remark  ,o.canceltime as statedate from clients c join orders o on c.id=o.clientid  where c.projectid={1} {3} and o.canceltime between @d1 and @d2 {2}";

            //签约
            string sql5 =
             @"{0} select '签约' as action, c.id,c.groupid,c.roomtype,c.name,c.way,c.wayextend,o.remark  ,o.signtime as statedate from clients c  join orders o on c.id=o.clientid where c.projectid={1} {3} and o.signtime between @d1 and @d2 {2}
union select '退房' as action, c.id,c.groupid,c.roomtype,c.name,c.way,c.wayextend,o.remark  ,o.canceltime as statedate from clients c  join orders o on c.id=o.clientid where c.projectid={1} {3} and o.signtime!=null and o.canceltime between @d1 and @d2 {2}";
            string sql6 = @"{0} select '{4}' as action, c.id,c.groupid,c.roomtype,c.name,c.way,c.wayextend ,ca.ActualTime as startdate from clients c 
join clientactivities ca on c.id=ca.clientid join clientactivities ca2 on c.id=ca2.clientid  
where c.projectid={1} {5} and ca.firsttype=2 and ca.actualtime between @d1 and DATEADD(MINUTE,59, dateadd(HH,23,@d2))
and ca2.firsttype=1 
{3}";

            string sql = null;
            switch (type)
            {
                case "0":
                    sql = string.Format(sql1, preSql, projectid, 1, waySql, "来电", 1, (int)ClientStateEnum.无效客户,roomTypeSql);
                    break;
                case "1":
                    sql = string.Format(sql1, preSql, projectid, 2, waySql, "来访", 2, (int)ClientStateEnum.无效客户,roomTypeSql);
                    break;
                case "2":
                    sql = string.Format(sql2, preSql, projectid, waySql,roomTypeSql);
                    break;
                case "3":
                    sql = string.Format(sql3, preSql, projectid, waySql,roomTypeSql);
                    break;
                case "4":
                    sql = string.Format(sql4, preSql, projectid, waySql,roomTypeSql);
                    break;
                case "5":
                    sql = string.Format(sql5, preSql, projectid, waySql,roomTypeSql);
                    break;
                case "6":
                    sql = string.Format(sql6, preSql, projectid, 1, waySql, "电转访",roomTypeSql);
                    break;
            }
            var list = db.Database.SqlQuery<ClientListVO>(sql).ToList();
            list.ForEach(o =>
            {
                o.GroupName = DepartmentBLL.GetNameById(o.GroupId);
            });
            result.obj = new { list = list };
            result.success = true;
            return Json(result);
        }
        public ActionResult CallCenterList(int? projectid)
        {
            ViewBag.Params = Request.Params;

            return View();
        }
        [HttpPost]
        public ActionResult CallCenterListQuery(int? projectid, FormCollection collection)
        {
            Result result = new Result();
            bool projectOnly = UserInfo.CurUser.GetClientRight(0) < ClientViewScopeEnum.查看所有 && UserInfo.CurUser.HasRight("客户管理-电话中心") == false;
            if (projectOnly)
            {
                if (projectid == null)
                {
                    result.obj = "没有权限";
                    return Json(result);
                }
                if (UserInfo.CurUser.GetClientRight((int)projectid) == ClientViewScopeEnum.无)
                {
                    result.obj = "没有权限";
                    return Json(result);
                }
            }


            List<object> parameters = new List<object>();
            string sql = @"select  d.name as projectname,c.id ,c.projectid,c.name,c.allphone,c.createtime,c.callperson ,l.accessperson as person
,ca.actualtime as visittime,c.state,c.invalidreason
,cards.smalltime ,cards.bigtime,cards.canceltime as cardcanceldate,o.ordertime ,o.signtime,o.canceltime as ordercanceldate from clients c 
join departments d on c.projectid=d.id
left outer join accesslogs l on c.id=l.keyid and l.accessclass='客户' and l.accessaction='客户登记'
left outer join clientactivities ca on c.id=ca.clientid and ca.firsttype=2
left outer join cards on cards.clientid=c.id
left outer join orders o on o.clientid=c.id
where c.way='电话中心'";
            Utilities.AddSqlFilterDateGreaterThen(collection, "dateFrom", ref sql, "c.createtime", parameters);
            Utilities.AddSqlFilterTimeLessThen(collection, "dateTo", ref sql, "c.createtime", parameters);
            Utilities.AddSqlFilterDateGreaterThen(collection, "dateFromVisit", ref sql, "ca.actualtime", parameters);
            Utilities.AddSqlFilterTimeLessThen(collection, "dateToVisit", ref sql, "ca.actualtime", parameters);
            Utilities.AddSqlFilterDateGreaterThen(collection, "dateFromCard", ref sql, "cards.smalltime", parameters);
            Utilities.AddSqlFilterTimeLessThen(collection, "dateToCard", ref sql, "cards.smalltime", parameters);
            Utilities.AddSqlFilterDateGreaterThen(collection, "dateFromOrder", ref sql, "o.ordertime", parameters);
            Utilities.AddSqlFilterTimeLessThen(collection, "dateToOrder", ref sql, "o.ordertime", parameters);
            Utilities.AddSqlFilterInInts(collection, "projectid", ref sql, "c.projectid");
            db.Database.Connection.Open();
            var dynamicParams = new DynamicParameters();
            parameters.ForEach(o => { var p = o as SqlParameter; dynamicParams.Add(p.ParameterName, p.Value, p.DbType); });
            var query = db.Database.Connection.Query<CallCentenListVO>(sql, param: dynamicParams);
            var list = query.ToList();
            foreach (var item in list)
            {
                if (item.State == ClientStateEnum.无效客户)
                {
                    if (string.IsNullOrEmpty(item.InvalidReason))
                    {
                        item.InvalidReason = "未填写原因";
                    }
                }
            }
            result.success = true;
            result.obj = new { list = list };
            return Json(result);
        }
        public ActionResult CallInReport()
        {
            DateTime d2 = DateTime.Today;
            int wd1 = (int)d2.DayOfWeek;
            DateTime d1 = wd1 == 0 ? d2.AddDays(-6) : d2.AddDays(1 - wd1);
            return View();
        }

        class GroupInfo
        {
            public int Id;
            public string Name;
            public List<IdName> Users;
        }
        public ActionResult CallInReportQuery(int projectid, string dateFrom, string dateTo, FormCollection collection)
        {
            bool groupOnly = UserInfo.CurUser.GetClientRight(projectid) < ClientViewScopeEnum.查看项目;
            int groupid = 0;
            if (groupOnly)
            {
                groupid = UserInfo.CurUser.GetGroup(projectid);
            }
            Result result = new Result();
            DateTime d1 = Utilities.ParseDate(dateFrom);
            DateTime d2 = Utilities.ParseDate(dateTo);


            string preSql = "declare @d1 datetime ,@d2 datetime ;set @d1='{0:yyyy-MM-dd}';set @d2='{1:yyyy-MM-dd}';declare @roomType nvarchar(50);set @roomType='{2}';";
            preSql = string.Format(preSql, d1, d2, collection["roomType"]);
           
            //来电，来访，，可按这方式取，小卡大卡大定和签约需要另外计算，因为大定和签约算套数，小卡要计算退卡

            string sql1 =
                @"{0} select count(*) as num ,ca.person as id  from clients c join clientactivities ca on c.id=ca.clientid
where c.projectid={1} and ca.firsttype={2} and ca.actualtime between @d1 and DATEADD(MINUTE,59, dateadd(HH,23,@d2))
 group by ca.person";
            var query1 = db.Database.SqlQuery<IdInt>(string.Format(sql1, preSql, projectid, 1)).ToList();
            
           
            string sql11 =
                @"{0} select count(*) as num ,ca.person as id  from clients c join clientactivities ca on c.id=ca.clientid
join clientactivities ca2 on c.id=ca2.clientid 
where c.projectid={1} and c.state!={2} and ca.firsttype=2 and ca.actualtime between @d1 and DATEADD(MINUTE,59, dateadd(HH,23,@d2))
and ca2.firsttype=1 
 group by ca.person";
            var query11 = db.Database.SqlQuery<IdInt>(string.Format(sql11, preSql, projectid, (int)ClientStateEnum.无效客户)).ToList();
            
            List<Department> projectgroups = DepartmentBLL.GetDepartmentByParent(projectid);
            List<IdName> users = new List<IdName>();
            var _groups = new List<GroupInfo>();

          
            foreach (var _group in projectgroups)
            {
                if (groupid==0||( groupid != 0 && _group.Id == groupid))
                {
                    if (_group.Name != "公共客户" && _group.Name != "沉睡客户")
                    {
                        
                        dynamic __group = new GroupInfo();
                        __group.Id = _group.Id;
                        __group.Name = _group.Name;
                        _groups.Add(__group);
                    }
                   
                }
            }
            foreach (var _group in _groups)
            {
                _group.Users = new List<IdName>();
                var groupusers =
                    (from o in DepartmentBLL.DepartmentUsers where o.DepartmentId == _group.Id select o).ToList();
                foreach (var u in groupusers)
                {
                    _group.Users.Add(new IdName {Id = u.UserId, Name = UserBLL.GetNameById(u.UserId)});
                }
            }
            
            result.obj = new { Groups=_groups,Users=users, list1 = query1,list2=query11, d1 = d1.ToString("yyyy-MM-dd"), d2 = d2.ToString("yyyy-MM-dd") };
            result.success = true;
            return Json(result);
        }
        public ActionResult CallInList(int projectid, int personid, string dateFrom, string dateTo, string type)
        {
            //DateTime d2 = DateTime.Today;
            //int wd1 = (int)d2.DayOfWeek;
            //DateTime d1 = wd1 == 0 ? d2.AddDays(-6) : d2.AddDays(1 - wd1);
            switch (type)
            {
                case "0":
                    ViewBag.State = "来电客户";
                    break;
                case "1":
                    ViewBag.State = "来访客户";
                    break;
                case "2":
                    ViewBag.State = "小卡客户";
                    break;
                case "3":
                    ViewBag.State = "大卡客户";
                    break;
                case "4":
                    ViewBag.State = "大定客户";
                    break;
                case "5":
                    ViewBag.State = "签约客户";
                    break;
            }
            ViewBag.PersonId = personid;
            ViewBag.D1 = dateFrom;
            ViewBag.D2 = dateTo;
            ViewBag.Type = type;
            return View();
        }
        [HttpPost]
        public ActionResult CallInListQuery(int projectid, int personid, string dateFrom, string dateTo, string type, FormCollection collection)
        {
            bool groupOnly = UserInfo.CurUser.GetClientRight(projectid) < ClientViewScopeEnum.查看项目;
            int groupid = 0;
            if (groupOnly)
            {
                groupid = UserInfo.CurUser.GetGroup(projectid);
            }
            Result result = new Result();
            DateTime d1 = Utilities.ParseDate(dateFrom);
            DateTime d2 = Utilities.ParseDate(dateTo);
            string preSql = "declare @d1 datetime ,@d2 datetime ;set @d1='{0:yyyy-MM-dd}';set @d2='{1:yyyy-MM-dd}';";
            preSql = string.Format(preSql, d1, d2);
            //来电，来访，，可按这方式取，小卡大卡大定和签约需要另外计算，因为大定和签约算套数，小卡要计算退卡
            string groupSql = "";
            if (groupid != 0)
            {
                groupSql = " and c.groupid=" + groupid.ToString();
            }
            string personSql = "";
            if (personid != 0)
            {
                personSql = " and ca.person=" + personid.ToString();
            }
            string sql1 =
                @"{0} select '{4}' as action,isnull(u.name,'') as person, c.id,c.groupid,c.roomtype,c.name,c.way,c.wayextend ,ca.ActualTime as statedate  from clients c 
join clientactivities ca on c.id=ca.clientid 
left outer join systemusers u on u.id=ca.person
where c.projectid={1} and ca.firsttype={5} and ca.actualtime between @d1 and DATEADD(MINUTE,59, dateadd(HH,23,@d2)) {3} {6}";
            string sql6 = @"{0} select '{4}' as action,isnull(u.name,'') as person, c.id,c.groupid,c.roomtype,c.name,c.way,c.wayextend ,ca.ActualTime as startdate  from clients c 
join clientactivities ca on c.id=ca.clientid join clientactivities ca2 on c.id=ca2.clientid 
left outer join systemusers u on u.id=ca.person 
where c.projectid={1} and ca.firsttype=2 and ca.actualtime between @d1 and DATEADD(MINUTE,59, dateadd(HH,23,@d2))
and ca2.firsttype=1 
{3} {5}";

            string sql = null;
            switch (type)
            {
                case "0":
                    sql = string.Format(sql1, preSql, projectid, 1, groupSql, "来电", 1,personSql);
                    break;
               
                case "6":
                    sql = string.Format(sql6, preSql, projectid, 1, groupSql, "电转访",personSql);
                    break;
            }
            var list = db.Database.SqlQuery<ClientListVO>(sql).ToList();
            list.ForEach(o =>
            {
                o.GroupName = DepartmentBLL.GetNameById(o.GroupId);
            });
            result.obj = new { list = list };
            result.success = true;
            return Json(result);
        }
        //------------------- 公司报表 ----------------------------------------------------
        public ActionResult ClientCompanyReport(string datefrom, string dateto)
        {
            if (datefrom == null && dateto == null)
            {
                DateTime d2 = DateTime.Today;
                int wd1 = (int)d2.DayOfWeek;
                DateTime d1 = wd1 == 0 ? d2.AddDays(-6) : d2.AddDays(1 - wd1);
                ViewBag.Date1 = d1.ToString("yyyy-MM-dd");
                ViewBag.Date2 = d2.ToString("yyyy-MM-dd");
            }
            else
            {
                ViewBag.Date1 = datefrom;
                ViewBag.Date2 = dateto;
            }
            
            return View();
        }

        public ActionResult ClientCompanyReportQuery( string dateFrom, string dateTo, FormCollection collection)
        {
            
            Result result = new Result();
            DateTime d1 = Utilities.ParseDate(dateFrom);
            DateTime d2 = Utilities.ParseDate(dateTo);
            List<ClientStateCompanyReportVO> list = new List<ClientStateCompanyReportVO>();
            var projects = DepartmentBLL.GetDepartments("项目");
            foreach (var p in projects)
            {
                list.Add(new ClientStateCompanyReportVO {ProjectId = p.Id, ProjectName = p.Name});
            }
            string roomTypeSql = "";
            if (!string.IsNullOrEmpty(collection["roomType"]))
            {
                roomTypeSql = " and c.roomtype=@roomType ";
            }

            string preSql = "declare @d1 datetime ,@d2 datetime ;set @d1='{0:yyyy-MM-dd}';set @d2='{1:yyyy-MM-dd}';declare @roomType nvarchar(50);set @roomType='{2}';";
            preSql = string.Format(preSql, d1, d2, collection["roomType"]);
            //来电，来访，，可按这方式取，小卡大卡大定和签约需要另外计算，因为大定和签约算套数，小卡要计算退卡

            string sql1 =
                @"{0} select count(*) as num ,projectid as id  from clients c join clientactivities ca on c.id=ca.clientid
where c.state!={1} and ca.firsttype={2} and ca.actualtime between @d1 and DATEADD(MINUTE,59, dateadd(HH,23,@d2)) {3}
 group by projectid";
            var query1 = db.Database.SqlQuery<IdInt>(string.Format(sql1, preSql, (int)ClientStateEnum.无效客户, 1, roomTypeSql)).ToList();
            foreach (var i in query1)
            {
                foreach (var j in list)
                {
                    if (i.Id == j.ProjectId)
                    {
                        j.CallInNum = i.Num;
                    }
                }
            }
            var query2 = db.Database.SqlQuery<IdInt>(string.Format(sql1, preSql, (int)ClientStateEnum.无效客户, 2, roomTypeSql)).ToList();
            foreach (var i in query2)
            {
                foreach (var j in list)
                {
                    if (i.Id == j.ProjectId)
                    {
                        j.VisitNum = i.Num;
                    }
                }
            }
            //ToDo: 如果小组变化了，那办卡等数据算谁的？
            //小卡
            string sql2 =
                @"{0} select count(*) as num ,c.projectid as id from clients c join cards on c.id=cards.clientid   where {1}  cards.smalltime between @d1 and @d2 {2} group by c.projectid";
            string sql3 =
               @"{0} select count(*) as num ,c.projectid as id from clients c join cards on c.id=cards.clientid   where {1}  cards.canceltime between @d1 and @d2 {2} group by c.projectid";
            var query3 = db.Database.SqlQuery<IdInt>(string.Format(sql2, preSql, "", roomTypeSql)).ToList();
            foreach (var i in query3)
            {
                foreach (var j in list)
                {
                    if (i.Id == j.ProjectId)
                    {
                        j.Card1Num = i.Num;
                    }
                }
            }
            var query4 = db.Database.SqlQuery<IdInt>(string.Format(sql3, preSql, "", roomTypeSql)).ToList();
            foreach (var i in query4)
            {
                foreach (var j in list)
                {
                    if (i.Id == j.ProjectId)
                    {
                        j.Card1Num -= i.Num;
                    }
                }
            }
            //大卡
            string sql4 =
               @"{0} select count(*) as num ,c.projectid as id from clients c join cards  on c.id=cards.clientid  where {1}  cards.bigtime between @d1 and @d2 {2} group by c.projectid";
            string sql5 =
               @"{0} select count(*) as num ,c.projectid as id from clients c join cards  on c.id=cards.clientid  where {1}  cards.bigtime!=null and cards.canceltime between @d1 and @d2 {2} group by c.projectid";
            var query5 = db.Database.SqlQuery<IdInt>(string.Format(sql4, preSql, "", roomTypeSql)).ToList();
            foreach (var i in query5)
            {
                foreach (var j in list)
                {
                    if (i.Id == j.ProjectId)
                    {
                        j.Card2Num = i.Num;
                    }
                }
            }
            var query6 = db.Database.SqlQuery<IdInt>(string.Format(sql5, preSql, "", roomTypeSql)).ToList();
            foreach (var i in query6)
            {
                foreach (var j in list)
                {
                    if (i.Id == j.ProjectId)
                    {
                        j.Card2Num -= i.Num;
                    }
                }
            }
            //大定
            string sql6 =
              @"{0} select count(*) as num ,c.projectid as id from clients c join orders o on o.clientid=c.id where {1}   and o.ordertime between @d1 and @d2 {2} group by c.projectid";
            string sql7 =
            @"{0} select count(*) as num ,c.projectid as id from clients c join orders o on o.clientid=c.id where {1}  and o.canceltime between @d1 and @d2 {2} group by c.projectid";
            var query7 = db.Database.SqlQuery<IdInt>(string.Format(sql6, preSql, "1=1", roomTypeSql)).ToList();
            foreach (var i in query7)
            {
                foreach (var j in list)
                {
                    if (i.Id == j.ProjectId)
                    {
                        j.OrderNum = i.Num;
                    }
                }
            }
            var query8 = db.Database.SqlQuery<IdInt>(string.Format(sql7, preSql, "1=1", roomTypeSql)).ToList();
            foreach (var i in query8)
            {
                foreach (var j in list)
                {
                    if (i.Id == j.ProjectId)
                    {
                        j.OrderNum = -i.Num;
                    }
                }
            }
            //签约
            string sql8 =
             @"{0} select count(*) as num ,c.projectid as id from clients c join orders o on o.clientid=c.id where {1}   and o.signtime between @d1 and @d2 {2} group by c.projectid";
            string sql9 =
            @"{0} select count(*) as num ,c.projectid as id from clients c join orders o on o.clientid=c.id where {1}   and o.signtime!=null and o.canceltime between @d1 and @d2 {2} group by c.projectid";
            var query9 = db.Database.SqlQuery<IdInt>(string.Format(sql8, preSql, "1=1", roomTypeSql)).ToList();
            foreach (var i in query9)
            {
                foreach (var j in list)
                {
                    if (i.Id == j.ProjectId)
                    {
                        j.ContractNum = i.Num;
                    }
                }
            }
            var query10 = db.Database.SqlQuery<IdInt>(string.Format(sql9, preSql, "1=1", roomTypeSql)).ToList();
            foreach (var i in query10)
            {
                foreach (var j in list)
                {
                    if (i.Id == j.ProjectId)
                    {
                        j.ContractNum -= i.Num;
                    }
                }
            }
            string sql11 =
                @"{0} select count(*) as num ,projectid as id  from clients c join clientactivities ca on c.id=ca.clientid
join clientactivities ca2 on c.id=ca2.clientid 
where  c.state!={1} and  ca.firsttype=2 and ca.actualtime between @d1 and DATEADD(MINUTE,59, dateadd(HH,23,@d2))
and ca2.firsttype=1 {2}
 group by projectid";
            var query11 = db.Database.SqlQuery<IdInt>(string.Format(sql11, preSql, (int)ClientStateEnum.无效客户, roomTypeSql)).ToList();
            foreach (var i in query11)
            {
                foreach (var j in list)
                {
                    if (i.Id == j.ProjectId)
                    {
                        j.CallVisitNum = i.Num;
                    }
                }
            }
            result.obj = new { list = list, d1 = d1.ToString("yyyy-MM-dd"), d2 = d2.ToString("yyyy-MM-dd"), RoomType = collection["roomtype"] };
            result.success = true;
            return Json(result);
        }
        public ActionResult ClientCompanyWayReport1(string datefrom, string dateto)
        {
            if (datefrom == null && dateto == null)
            {
                DateTime d2 = DateTime.Today;
                int wd1 = (int)d2.DayOfWeek;
                DateTime d1 = wd1 == 0 ? d2.AddDays(-6) : d2.AddDays(1 - wd1);
                ViewBag.Date1 = d1.ToString("yyyy-MM-dd");
                ViewBag.Date2 = d2.ToString("yyyy-MM-dd");
            }
            else
            {
                ViewBag.Date1 = datefrom;
                ViewBag.Date2 = dateto;
            }
            return View();
        }
        public ActionResult ClientCompanyWayReport2(string datefrom, string dateto)
        {
            if (datefrom == null && dateto == null)
            {
                DateTime d2 = DateTime.Today;
                int wd1 = (int)d2.DayOfWeek;
                DateTime d1 = wd1 == 0 ? d2.AddDays(-6) : d2.AddDays(1 - wd1);
                ViewBag.Date1 = d1.ToString("yyyy-MM-dd");
                ViewBag.Date2 = d2.ToString("yyyy-MM-dd");
            }
            else
            {
                ViewBag.Date1 = datefrom;
                ViewBag.Date2 = dateto;
            }
            return View();
        }
        public ActionResult ClientCompanyWayReportQuery(string dateFrom, string dateTo, FormCollection collection)
        {
           //ToDo:权限判断 ，上一张报表也需要添加
            Result result = new Result();
            DateTime d1 = Utilities.ParseDate(dateFrom);
            DateTime d2 = Utilities.ParseDate(dateTo);
            var projects = (from o in  DepartmentBLL.GetDepartments("项目") select new IdName{Id=o.Id,Name=o.Name}).ToList();
            
            List<WayReportVO> list = new List<WayReportVO>();



            string preSql = "declare @d1 datetime ,@d2 datetime ;set @d1='{0:yyyy-MM-dd}';set @d2='{1:yyyy-MM-dd}';";
            preSql = string.Format(preSql, d1, d2);
            //来电，来访，，可按这方式取，小卡大卡大定和签约需要另外计算，因为大定和签约算套数，小卡要计算退卡

            string sql1 =
                @"{0} select c.projectid as id,count(*) as num ,c.way as Name from clients c join clientactivities ca on c.id=ca.clientid
where c.state!={1} and ca.firsttype={2} and ca.actualtime >=@d1 and ca.actualtime<DATEADD(d,1, @d2)
 group by c.projectid, c.way";
            var query1 = db.Database.SqlQuery<IdNameNum>(string.Format(sql1, preSql, (int)ClientStateEnum.无效客户, 1)).ToList();
            foreach (var i in query1)
            {
                WayReportVO vo = WayReportVO.GetVo(list,i.Id, i.Name);
                vo.CallInNum =i.Num;
            }
            var query2 = db.Database.SqlQuery<IdNameNum>(string.Format(sql1, preSql, (int)ClientStateEnum.无效客户, 2)).ToList();
            foreach (var i in query2)
            {
                WayReportVO vo = WayReportVO.GetVo(list,i.Id, i.Name);
                vo.VisitNum =i.Num;
            }
            //小卡
            string sql2 =
               @"{0} select c.projectid as id,count(*) as num ,c.way as Name from clients c join cards on c.id=cards.clientid   where {1} and cards.smalltime between @d1 and @d2 group by c.projectid,c.way";
            string sql3 =
               @"{0} select c.projectid as id,count(*) as num ,c.way as Name from clients c join cards on c.id=cards.clientid   where {1} and cards.canceltime between @d1 and @d2 group by c.projectid,c.way";
            var query3 = db.Database.SqlQuery<IdNameNum>(string.Format(sql2, preSql, "1=1")).ToList();

            var query4 = db.Database.SqlQuery<IdNameNum>(string.Format(sql3, preSql, "1=1")).ToList();
            foreach (var i in query3)
            {
                WayReportVO vo = WayReportVO.GetVo(list,i.Id, i.Name);
                vo.Card1Num =i.Num;
            }
            foreach (var i in query4)
            {
                WayReportVO vo = WayReportVO.GetVo(list,i.Id, i.Name);
                vo.Card1Num -=i.Num;
            }
            //大卡
            string sql4 =
               @"{0} select c.projectid as id,count(*) as num ,c.way as Name from clients c join cards  on c.id=cards.clientid  where {1} and cards.bigtime between @d1 and @d2 group by c.projectid,c.way";
            string sql5 =
               @"{0} select c.projectid as id,count(*) as num ,c.way as Name from clients c join cards  on c.id=cards.clientid  where {1} and cards.bigtime!=null and cards.canceltime between @d1 and @d2 group by c.projectid,c.way";
            var query5 = db.Database.SqlQuery<IdNameNum>(string.Format(sql4, preSql, "1=1")).ToList();
            var query6 = db.Database.SqlQuery<IdNameNum>(string.Format(sql5, preSql, "1=1")).ToList();
            foreach (var i in query5)
            {
                WayReportVO vo = WayReportVO.GetVo(list,i.Id, i.Name);
                vo.Card2Num =i.Num;
            }
            foreach (var i in query6)
            {
                WayReportVO vo = WayReportVO.GetVo(list,i.Id, i.Name);
                vo.Card2Num -=i.Num;
            }

            //大定
            string sql6 =
              @"{0} select c.projectid as id,count(*) as num ,c.way as Name  from clients c  join orders o on o.clientid=c.id where {1} and  o.ordertime between @d1 and @d2 group by c.projectid,c.way";
            string sql7 =
            @"{0} select c.projectid as id,count(*) as num ,c.way as Name  from clients c join orders o on o.clientid=c.id where {1} and  o.canceltime between @d1 and @d2 group by c.projectid,c.way";
            var query7 = db.Database.SqlQuery<IdNameNum>(string.Format(sql6, preSql, "1=1")).ToList();
            var query8 = db.Database.SqlQuery<IdNameNum>(string.Format(sql7, preSql, "1=1")).ToList();
            foreach (var i in query7)
            {
                WayReportVO vo = WayReportVO.GetVo(list,i.Id, i.Name);
                vo.OrderNum =i.Num;
            }
            foreach (var i in query8)
            {
                WayReportVO vo = WayReportVO.GetVo(list,i.Id, i.Name);
                vo.OrderNum -=i.Num;
            }

            //签约
            string sql8 =
             @"{0} select count(*)  as id ,c.way as Name from clients c join orders o on o.clientid=c.id where {1}  and o.signtime between @d1 and @d2 group by c.projectid,c.way";
            string sql9 =
            @"{0} select count(*)  as id ,c.way as Name from clients c join orders o on o.clientid=c.id where {1}  and o.signtime!=null and o.canceltime between @d1 and @d2 group by c.projectid,c.way";
            var query9 = db.Database.SqlQuery<IdNameNum>(string.Format(sql8, preSql, "1=1")).ToList();
            var query10 = db.Database.SqlQuery<IdNameNum>(string.Format(sql9, preSql, "1=1")).ToList();
            foreach (var i in query9)
            {
                WayReportVO vo = WayReportVO.GetVo(list,i.Id, i.Name);
                vo.ContractNum =i.Num;
            }
            foreach (var i in query10)
            {
                WayReportVO vo = WayReportVO.GetVo(list,i.Id, i.Name);
                vo.ContractNum -=i.Num;
            }

            string sql11 =
                @"{0} select c.projectid as id,count(*) as num ,c.way as Name from clients c join clientactivities ca on c.id=ca.clientid
join clientactivities ca2 on c.id=ca2.clientid 
where c.state!={1} and ca.firsttype=2 and ca.actualtime between @d1 and DATEADD(MINUTE,59, dateadd(HH,23,@d2))
and ca2.firsttype=1 
 group by c.projectid,c.way";
            var query11 = db.Database.SqlQuery<IdNameNum>(string.Format(sql11, preSql, (int)ClientStateEnum.无效客户)).ToList();
            foreach (var i in query11)
            {
                WayReportVO vo = WayReportVO.GetVo(list,i.Id, i.Name);
                vo.CallVisitNum =i.Num;
            }


            result.obj = new {projects=projects, list = list, d1 = d1.ToString("yyyy-MM-dd"), d2 = d2.ToString("yyyy-MM-dd") };
            result.success = true;
            return Json(result);
        }

        public ActionResult PerformanceProjectReport()
        {
            return View();
        }
        public ActionResult PerformanceTeamReport(string dateFrom, string dateTo, int projectid, string small)
        {
            ViewBag.Small = small;
            ViewBag.Date1 = dateFrom;
            ViewBag.Date2 = dateTo;
            return View();
        }
        [HttpPost]
        public ActionResult PerformanceProjectQuery(string dateFrom, string dateTo, int projectid,FormCollection collection)
        {
            Result result = null;

            result = PerformanceQuery(dateFrom, dateTo, 1, projectid);
            
            return Json(result);
        }
        [HttpPost]
        public ActionResult PerformanceCompanyQuery(string dateFrom, string dateTo, FormCollection collection)
        {
            Result result = null;
            
            result= PerformanceQuery(dateFrom, dateTo,2,0);
            return Json(result);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dateFrom"></param>
        /// <param name="dateTo"></param>
        /// <param name="grouptype">1=group ,2=project</param>
        /// <param name="collection"></param>
        /// <returns></returns>
        public Result PerformanceQuery(string dateFrom, string dateTo,int grouptype,int projectid)
        {
       

        //ToDo:权限判断 ，上一张报表也需要添加
            Result result = new Result();
            DateTime d1 = Utilities.ParseDate(dateFrom);
            DateTime d2 = Utilities.ParseDate(dateTo);
            
            
            List<PorformanceReportVO> list = new List<PorformanceReportVO>();
            string groupField = "";
            if (grouptype == 2)
            {
                var projects = DepartmentBLL.GetDepartments("项目");
                foreach (var p in projects)
                {
                    list.Add(new PorformanceReportVO {Id = p.Id, Name = p.Name});
                }
                groupField = "  c.projectid ";
            }
            else
            {
                groupField = "c.groupid";
                var groups = DepartmentBLL.GetGroupsByPid(projectid);
                groups.ForEach(o =>
                {
                    if (o.Name != "前台" && o.Name != "公共客户" && o.Name != "沉睡客户")
                        list.Add(new PorformanceReportVO { Id = o.Id, Name = o.Name });
                });
            }
            
          

            string preSql = "declare @d1 datetime ,@d2 datetime ;set @d1='{0:yyyy-MM-dd}';set @d2='{1:yyyy-MM-dd}';declare @roomType nvarchar(50);set @roomType='{2}';";
            preSql = string.Format(preSql, d1, d2,"");
            //来电，来访，，可按这方式取，小卡大卡大定和签约需要另外计算，因为大定和签约算套数，小卡要计算退卡

            string sql1 =
                @"{0} select count(*) as num ,{1} as id  from clients c join clientactivities ca on c.id=ca.clientid
where c.state!={2} and ca.firsttype={3} and ca.actualtime between @d1 and DATEADD(MINUTE,59, dateadd(HH,23,@d2)) 
 group by {1}";
            var query1 = db.Database.SqlQuery<IdInt>(string.Format(sql1, preSql,groupField, (int)ClientStateEnum.无效客户, 1)).ToList();
            foreach (var i in query1)
            {
                foreach (var j in list)
                {
                    if (i.Id == j.Id)
                    {
                        j.CallInNum = i.Num;
                    }
                }
            }
            var query2 = db.Database.SqlQuery<IdInt>(string.Format(sql1, preSql,groupField, (int)ClientStateEnum.无效客户, 2)).ToList();
            foreach (var i in query2)
            {
                foreach (var j in list)
                {
                    if (i.Id == j.Id)
                    {
                        j.VisitNum = i.Num;
                    }
                }
            }
            //ToDo: 如果小组变化了，那办卡等数据算谁的？
            //小卡
            string sql2 =
                @"{0} select count(*) as num ,{1} as id from clients c join cards on c.id=cards.clientid   where   cards.smalltime between @d1 and @d2 {2} group by {1}";
            string sql3 =
               @"{0} select count(*) as num ,{1} as id from clients c join cards on c.id=cards.clientid   where   cards.canceltime between @d1 and @d2 {2} group by {1}";
            var query3 = db.Database.SqlQuery<IdInt>(string.Format(sql2, preSql,groupField, "")).ToList();
            foreach (var i in query3)
            {
                foreach (var j in list)
                {
                    if (i.Id == j.Id)
                    {
                        j.Card1Num = i.Num;
                    }
                }
            }
            var query4 = db.Database.SqlQuery<IdInt>(string.Format(sql3, preSql, groupField, "")).ToList();
            foreach (var i in query4)
            {
                foreach (var j in list)
                {
                    if (i.Id == j.Id)
                    {
                        j.Card1Num -= i.Num;
                    }
                }
            }
            
            //大定
            string sql6 =
              @"{0} select count(*) as num ,{1} as id from clients c join orders o on o.clientid=c.id where  o.ordertime between @d1 and @d2 {2} group by {1}";
            string sql7 =
            @"{0} select count(*) as num ,{1} as id from clients c join orders o on o.clientid=c.id where  o.canceltime between @d1 and @d2 {2} group by {1}";
            var query7 = db.Database.SqlQuery<IdInt>(string.Format(sql6, preSql,groupField,"")).ToList();
            foreach (var i in query7)
            {
                foreach (var j in list)
                {
                    if (i.Id == j.Id)
                    {
                        j.OrderNum = i.Num;
                    }
                }
            }
            var query8 = db.Database.SqlQuery<IdInt>(string.Format(sql7, preSql, groupField, "")).ToList();
            foreach (var i in query8)
            {
                foreach (var j in list)
                {
                    if (i.Id == j.Id)
                    {
                        j.OrderNum = -i.Num;
                    }
                }
            }
            //电转访
            string sql11 =
                @"{0} select count(*) as num ,{1} as id  from clients c join clientactivities ca on c.id=ca.clientid
join clientactivities ca2 on c.id=ca2.clientid 
where  c.state!={2} and  ca.firsttype=2 and ca.actualtime between @d1 and DATEADD(MINUTE,59, dateadd(HH,23,@d2))
and ca2.firsttype=1 
 group by {1}";
            var query11 = db.Database.SqlQuery<IdInt>(string.Format(sql11, preSql,groupField, (int)ClientStateEnum.无效客户)).ToList();
            foreach (var i in query11)
            {
                foreach (var j in list)
                {
                    if (i.Id == j.Id)
                    {
                        j.CallVisitNum = i.Num;
                    }
                }
            }
            List<Performance> targetList = new List<Performance>();
            if (grouptype == 1)
            {
                Performance performance = GetPerformance(projectid, d1);
                if (performance != null) targetList.Add(performance);
            }
            else
            {
                foreach (var item in list)
                {
                    Performance performance = GetPerformance(item.Id, d1);
                    if (performance != null) targetList.Add(performance);
                }
            }
            result.obj = new {targetList=targetList, actList = list, d1 = d1.ToString("yyyy-MM-dd"), d2 = d2.ToString("yyyy-MM-dd") };
            result.success = true;
             return result;
         }

        Performance GetPerformance(int projectId, DateTime d1)
        {
            var query =
                (from o in db.Performance
                    where o.ProjectId == projectId && o.BeginDate <= d1
                    orderby o.BeginDate descending
                    select o).FirstOrDefault();
            return query;
        }
    }

}
