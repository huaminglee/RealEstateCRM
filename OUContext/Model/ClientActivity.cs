using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace OUDAL
{
    public class ClientActivity
    {
        public static string LogClass = "联系记录";
        public int Id { get; set; }
        public int ClientId { get; set; }
        [DisplayName("类型")]
        [Required]
        public string Type { get; set; }
        [DisplayName("邀约时间")]
        public DateTime? PlanTime { get; set; }
        [DisplayName("实际时间")]
        public DateTime? ActualTime { get; set; }
        [DisplayName("情况说明")]
        public string Detail { get; set; }
        [DisplayName("完成情况")]
        public bool? IsDone { get; set; }
        //1=初次来电 2=初次来访
        public int FirstType { get; set; }
        public int Person { get; set; }
    }
    public class ClientActivityListView
    {
        
       
        public int Id { get; set; }
        public int ClientId { get; set; }
        public string Type { get; set; }
        public DateTime? PlanTime { get; set; }
        public DateTime? ActualTime { get; set; }
        public string Detail { get; set; }
        public bool? IsDone { get; set; }
        public string Client { get; set; }
        public string GroupName { get; set; }
        public string AllPhone { get; set; }
        public string Done { get; set; }

        public string Way { get; set; }
        public string WayExtend { get; set; }
        public static List<ClientActivityListView> GetReport(int projectid, int groupid, DateTime dateFrom,
            DateTime dateTo,string clientName,string isdone)
        {
            List<object> parameters = new List<object>();
            string sql = @"declare @name nvarchar(50);{5} select c.id as clientid,c.name as client ,c.allphone,c.way,c.wayextend,d.name as groupname,ca.*
from clients c join clientactivities ca on c.id=ca.clientid 
join departments d on d.id=c.groupid
where c.projectid={0} {1} and ca.plantime between '{2:yyyy-MM-dd}' and '{3:yyyy-MM-dd} 23:59' {4} {6}";
            string group = groupid == 0 ? "" : string.Format(" and c.groupid={0}", groupid);
            string name = string.IsNullOrEmpty(clientName) ? "" : string.Format(" and c.name like @name ");
            string nameParame = string.IsNullOrEmpty(clientName) ? "" : string.Format("set @name='{0}';", clientName);
            string isdoneSql = "";
            if (isdone == "false")
            {
                isdoneSql = " and ca.ActualTime is null";
            }
            sql = string.Format(sql, projectid, group, dateFrom, dateTo,name,nameParame,isdoneSql);
            using (Context db = new Context())
            {
                db.Database.Connection.Open();
                var list= db.Database.SqlQuery<ClientActivityListView>(sql).ToList();
                db.Database.Connection.Close();
                list.ForEach(o =>
                         {
                             if (o.IsDone==true)
                             {
                                 o.Done = "完成";
                             }
                             if (o.Type != "来访邀约" && o.ActualTime != null)
                             {
                                 o.Done = "到访";
                             }
                             //ToDo： 要做卡签的对应状态修改
                         });
                return list;
            }
        }

    }

    public class ClientActivityReportView
    {
        public int ProjectId { get; set; }
        public string Type { get; set; }
        //public string GroupName { get; set; }
        public int GroupId { get; set; }
        public int Num { get; set; }
        public int VisitNum { get; set; }
        public int DoneNum { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="projectid"></param>
        /// <param name="groupid">要调用方根据权限判断是否要有groupid，不要则为0</param>
        /// <param name="dateFrom"></param>
        /// <param name="dateTo"></param>
        /// <returns></returns>
        public static List<ClientActivityReportView> GetReport(int projectid, int groupid, DateTime dateFrom, DateTime dateTo)
        {
            string sql = @"select c.projectid {4}, ca.type ,count(*) as num
,sum(case  when actualtime is null then 0 else 1 end ) as VisitNum,sum(case isdone when 1 then 1 else 0 end) as donenum
from clients c join clientactivities ca on c.id=ca.clientid
join departments d on c.groupid=d.id
where 1=1  {0} {1} and ca.plantime between '{2:yyyy-MM-dd}' and '{3:yyyy-MM-dd} 23:59'
group by c.projectid {4}, ca.type";
            string project = projectid == 0 ? "" : string.Format(" and c.projectid={0}", projectid);
            string group = groupid == 0 ? "" : string.Format(" and c.groupid={0}" , groupid);
            string groupgroup = projectid == 0 ? "" : ",c.groupid";
            sql = string.Format(sql, project, group, dateFrom, dateTo,groupgroup);
            using (Context db = new Context())
            {
               return  db.Database.SqlQuery<ClientActivityReportView>(sql).ToList();
            }
        }

    }
   
}
