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
    public class ReportController : BaseController
    {
        private Context db = new Context();

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);

        }

        public ActionResult ClientStateReport()
        {
            DateTime d2 = DateTime.Today;
            int wd1 = (int)d2.DayOfWeek;
            DateTime d1 = wd1 == 0 ? d2.AddDays(-6) : d2.AddDays(1 - wd1);
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
            DateTime d1=Utilities.ParseDate(dateFrom);
            DateTime d2=Utilities.ParseDate(dateTo);
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
                
            var groups= DepartmentBLL.GetGroupsByPid(projectid);
            groups.ForEach(o =>
            {   if(o.Name!="前台"&&o.Name!="公共客户"&&o.Name!="沉睡客户")
                list.Add(new ClientStateProjectReportVO{GroupId=o.Id,GroupName=o.Name });
            });
            groups.ForEach(o =>
            {
                if (o.Name== "公共客户" )
                    list.Add(new ClientStateProjectReportVO { GroupId = o.Id, GroupName = o.Name });
            });
            groups.ForEach(o =>
            {
                if (o.Name == "沉睡客户")
                    list.Add(new ClientStateProjectReportVO { GroupId = o.Id, GroupName = o.Name });
            });
            }

            string preSql = "declare @d1 datetime ,@d2 datetime ;set @d1='{0:yyyy-MM-dd}';set @d2='{1:yyyy-MM-dd}';";
            preSql = string.Format(preSql, d1, d2);
            //来电，来访，，可按这方式取，小卡大卡大定和签约需要另外计算，因为大定和签约算套数，小卡要计算退卡
            
            string sql1 =
                @"{0} select count(*) as num ,groupid as id  from clients c join clientactivities ca on c.id=ca.clientid
where c.projectid={1} and ca.firsttype={2} and ca.actualtime between @d1 and DATEADD(MINUTE,59, dateadd(HH,23,@d2))
 group by groupid";
            var query1 = db.Database.SqlQuery<IdInt>(string.Format(sql1, preSql, projectid, 1)).ToList();
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
            var query2 = db.Database.SqlQuery<IdInt>(string.Format(sql1, preSql, projectid, 2)).ToList();
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
                @"{0} select count(*) as num ,c.groupid as id from clients c join cards on c.id=cards.clientid   where c.projectid={1} and cards.smalltime between @d1 and @d2 group by c.groupid";
            string sql3 =
               @"{0} select count(*) as num ,c.groupid as id from clients c join cards on c.id=cards.clientid   where c.projectid={1} and cards.canceltime between @d1 and @d2 group by c.groupid";
            var query3 = db.Database.SqlQuery<IdInt>(string.Format(sql2, preSql, projectid)).ToList();
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
            var query4 = db.Database.SqlQuery<IdInt>(string.Format(sql3, preSql, projectid)).ToList();
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
               @"{0} select count(*) as num ,c.groupid as id from clients c join cards  on c.id=cards.clientid  where c.projectid={1} and cards.bigtime between @d1 and @d2 group by c.groupid";
            string sql5 =
               @"{0} select count(*) as num ,c.groupid as id from clients c join cards  on c.id=cards.clientid  where c.projectid={1} and cards.bigtime!=null and cards.canceltime between @d1 and @d2 group by c.groupid";
            var query5 = db.Database.SqlQuery<IdInt>(string.Format(sql4, preSql, projectid)).ToList();
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
            var query6 = db.Database.SqlQuery<IdInt>(string.Format(sql5, preSql, projectid)).ToList();
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
              @"{0} select count(*) as num ,c.groupid as id from clients c  where c.projectid={1} and exists (select 1 from orders o where o.clientid=c.id and o.ordertime between @d1 and @d2) group by c.groupid";
            string sql7 =
            @"{0} select count(*) as num ,c.groupid as id from clients c  where c.projectid={1} and exists (select 1 from orders o where o.clientid=c.id and o.canceltime between @d1 and @d2) group by c.groupid";
            var query7 = db.Database.SqlQuery<IdInt>(string.Format(sql6, preSql, projectid)).ToList();
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
            var query8 = db.Database.SqlQuery<IdInt>(string.Format(sql7, preSql, projectid)).ToList();
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
             @"{0} select count(*) as num ,c.groupid as id from clients c  where c.projectid={1} and exists (select 1 from orders o where o.clientid=c.id and o.signtime between @d1 and @d2) group by c.groupid";
            string sql9 =
            @"{0} select count(*) as num ,c.groupid as id from clients c  where c.projectid={1} and exists (select 1 from orders o where o.clientid=c.id and o.signtime!=null and o.canceltime between @d1 and @d2) group by c.groupid";
            var query9 = db.Database.SqlQuery<IdInt>(string.Format(sql8, preSql, projectid)).ToList();
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
            var query10 = db.Database.SqlQuery<IdInt>(string.Format(sql9, preSql, projectid)).ToList();
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
where c.projectid={1} and ca.firsttype=2 and ca.actualtime between @d1 and DATEADD(MINUTE,59, dateadd(HH,23,@d2))
and ca2.firsttype=1 
 group by groupid";
            var query11 = db.Database.SqlQuery<IdInt>(string.Format(sql11, preSql, projectid)).ToList();
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
            result.obj = new { list = list,d1=d1.ToString("yyyy-MM-dd"),d2=d2.ToString("yyyy-MM-dd") };
            result.success = true;
            return Json(result);
        }
        public ActionResult ClientStateList(int projectid,int groupid, string dateFrom, string dateTo,string type)
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
            return View();
        }
        [HttpPost]
        public ActionResult ClientStateListQuery(int projectid,int groupid, string dateFrom, string dateTo,string type, FormCollection collection)
        {
            bool groupOnly = UserInfo.CurUser.GetClientRight(projectid) < ClientViewScopeEnum.查看项目;
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
            string sql1 =
                @"{0} select '{4}' as action, c.id,c.groupid,c.roomtype,c.name,c.way,c.wayextend ,ca.ActualTime as statedate  from clients c join clientactivities ca on c.id=ca.clientid where c.projectid={1} and ca.actualtime between @d1 and DATEADD(MINUTE,59, dateadd(HH,23,@d2)) {3}";
            //小卡
            string sql2 =
                @"{0} select '办卡' as action, c.id,c.groupid,c.roomtype,c.name,c.way,c.wayextend ,cards.remark,cards.smalltime as statedate from clients c join cards on c.id=cards.clientid   where c.projectid={1} and cards.smalltime between @d1 and @d2 {2}
union select '退卡' as action, c.id,c.groupid,c.roomtype,c.name,c.way ,c.wayextend,cards.remark,cards.canceltime as statedate  from clients c join cards on c.id=cards.clientid   where c.projectid={1} and cards.canceltime between @d1 and @d2 {2}";

            string sql3 =
               @"{0} select '升卡' as action, c.id,c.groupid,c.roomtype,c.name,c.way,c.wayextend,cards.remark ,cards.bigtime as statedate from clients c join cards  on c.id=cards.clientid  where c.projectid={1} and cards.bigtime between @d1 and @d2 {2}
union select '退卡' as action, c.id,c.groupid,c.roomtype,c.name,c.way,c.wayextend ,cards.remark,cards.canceltime as statedate from clients c join cards  on c.id=cards.clientid  where c.projectid={1} and cards.bigtime!=null and cards.canceltime between @d1 and @d2 {2}";

            string sql4 =
              @"{0} select '大定' as action, c.id,c.groupid,c.roomtype,c.name,c.way,c.wayextend,o.remark  ,o.ordertime as statedate from clients c join orders o on c.id=o.clientid  where c.projectid={1} and o.ordertime between @d1 and @d2 {2}
union select '退订' as action, c.id,c.groupid,c.roomtype,c.name,c.way,c.wayextend,o.remark  ,o.canceltime as statedate from clients c join orders o on c.id=o.clientid  where c.projectid={1} and o.canceltime between @d1 and @d2 {2}";

            //签约
            string sql5 =
             @"{0} select '签约' as action, c.id,c.groupid,c.roomtype,c.name,c.way,c.wayextend,o.remark  ,o.signtime as statedate from clients c  join orders o on c.id=o.clientid where c.projectid={1} and o.signtime between @d1 and @d2 {2}
union select '退房' as action, c.id,c.groupid,c.roomtype,c.name,c.way,c.wayextend,o.remark  ,o.canceltime as statedate from clients c  join orders o on c.id=o.clientid where c.projectid={1} and o.signtime!=null and o.canceltime between @d1 and @d2 {2}";
            string sql6 = @"{0} select '{4}' as action, c.id,c.groupid,c.roomtype,c.name,c.way,c.wayextend ,ca.ActualTime  from clients c 
join clientactivities ca on c.id=ca.clientid join clientactivities ca2 on c.id=ca2.clientid  
where c.projectid={1} and ca.firsttype=2 and ca.actualtime between @d1 and DATEADD(MINUTE,59, dateadd(HH,23,@d2))
and ca2.firsttype=1 
{3}";

            string sql = null;
            switch (type)
            {
                case "0":
                    sql = string.Format(sql1, preSql, projectid,  1, groupSql, "来电");
                    break;
                case "1":
                    sql = string.Format(sql1, preSql, projectid,  2, groupSql, "来访");
                    break;
                case "2":
                    sql = string.Format(sql2, preSql, projectid,  groupSql);
                    break;
                case "3":
                    sql = string.Format(sql3, preSql, projectid,  groupSql);
                    break;
                case "4":
                    sql = string.Format(sql4, preSql, projectid,  groupSql);
                    break;
                case "5":
                    sql = string.Format(sql5, preSql, projectid,  groupSql);
                    break;
                case "6":
                    sql = string.Format(sql6, preSql, projectid, 1, groupSql, "电转访");
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
            bool projectOnly = UserInfo.CurUser.GetClientRight(0) < ClientViewScopeEnum.查看所有 && UserInfo.CurUser.HasRight("客户管理-电话中心")==false;
            if (projectOnly)
            {
                if (projectid == null)
                {
                    result.obj = "没有权限";
                    return Json(result);
                }
                if (UserInfo.CurUser.GetClientRight((int) projectid) == ClientViewScopeEnum.无)
                {
                    result.obj = "没有权限";
                    return Json(result);
                }
            }
           
           
            List<object> parameters = new List<object>();
            string sql = @"select  d.name as projectname,c.id ,c.projectid,c.name,c.createtime,c.callperson ,l.accessperson as person
,ca.actualtime as visittime,c.state,c.invalidreason
,cards.smalltime as carddate,cards.canceltime as cardcanceldate,o.ordertime as orderdate,o.canceltime as ordercanceldate from clients c 
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
            result.obj =new {list=list};
            return Json(result);
        }

    }

}
