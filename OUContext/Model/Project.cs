using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace OUDAL
{
    public class Project
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int DepartmentId { get; set; }

        [DisplayName("编码")]
        public string Code { get; set; }

        public static int GetGroupByName(int projectid, string name)
        {
            List<Department> list = DepartmentBLL.GetDepartmentByParent(projectid);
            foreach (Department d in list)
            {
                if (d.Name == name) return d.Id;
            }
            return 0;
        }

        public static List<IdName> GetSalesGroup(int projectid)
        {
            List<IdName> groups = new List<IdName>();
            List<Department> list = DepartmentBLL.GetDepartmentByParent(projectid);
            foreach (Department d in list)
            {
                if (d.DepartmentType == "小组" && d.Name != "前台" && d.Name != "公共客户" && d.Name != "沉睡客户")
                {
                    groups.Add(new IdName {Id = d.Id, Name = d.Name});
                }
            }
            return groups;
        }

        [NotMapped]
        public List<RoomType> RoomTypes { get; set; }

        public static Project Get(int id)
        {
            using (Context db = new Context())
            {
                Project p = (from o in db.Projects.AsNoTracking() where o.DepartmentId == id select o).FirstOrDefault();
                if (p == null)
                {
                    p = new Project();
                    p.DepartmentId = id;
                }
                p.RoomTypes = (from o in db.RoomTypes.AsNoTracking() where o.DepartmentId == id select o).ToList();
                return p;
            }
        }

        public List<ClientTransferAlertReport> GetOutTimeAlertNum(int project, int group)
        {
            List<ClientTransferAlertReport> list = new List<ClientTransferAlertReport>();
            list.Add(new ClientTransferAlertReport
                     {
                         TransferType = "电转访超期预警",Num = GetOutTimeAlert(project, group, ClientStateEnum.来电客户).Count
                     });
            list.Add(new ClientTransferAlertReport
                     {
                         TransferType = "访转卡超期预警",Num= GetOutTimeAlert(project, group, ClientStateEnum.来访客户).Count});
   
            list.Add (new ClientTransferAlertReport
                     {
                         TransferType = "卡转定超期预警",Num= GetOutTimeAlert(project , group , ClientStateEnum.办卡客户 ).Count });
        return list;

}
        public List<OutTimeClient> GetOutTimeAlert(int project,int group, ClientStateEnum state)
        {
            string field="";
            string alertField = "";
            string type = "";
            switch (state)
            {
                case ClientStateEnum.来电客户:
                    field = "TelToVisitDays";
                    alertField = "TelToVisitAheads";
                    type = "电转访超期预警";
                    break;
                    case ClientStateEnum.来访客户:
                    field = "VisitToCardDays";
                    alertField = "VisitToCardAheads";
                    type = "访转卡超期预警";
                    break;
                    case ClientStateEnum.办卡客户:
                    field = "CardToOrderDays";
                    alertField = "CardToOrderAheads";
                    type = "卡转定超期预警";
                    break;
            }
            string sql = @"select c.id as clientid,c.name,c.state,d.name as groupname ,'{5}' as type from clients c join departments d on c.groupid=d.id 
join roomtypes t on t.departmentid=c.projectid and t.name=c.roomtype
where c.projectid={0} and  c.state={1} {2} and t.{3} >0 and  DATEADD(dd,t.{3}-t.{4},c.statedate)<=getdate() 
order by DATEADD(dd,t.{3}-t.{4},c.statedate)-getdate()";
            string groupSql = "";
            if (group != 0)
            {
                groupSql = "and c.groupid=" + group.ToString();
            }
            sql = string.Format(sql, project, (int) state, groupSql, field, alertField,type);
            using (Context db = new Context())
            {
                List<OutTimeClient> list = db.Database.SqlQuery<OutTimeClient>(sql).ToList();
                return list;
            }
            
        }
        static public List<SelectListItem> GetOutTimeType(bool appendblank)
        {
            List<SelectListItem> list = new List<SelectListItem>();
            if (appendblank == true) list.Add(new SelectListItem { Value = "", Text = "---" });
            list.Add(new SelectListItem { Value = "电转访超期预警", Text = "电转访超期预警" });
            list.Add(new SelectListItem { Value = "访转卡超期预警", Text = "访转卡超期预警" });
            list.Add(new SelectListItem { Value = "卡转定超期预警", Text = "卡转定超期预警" });
            return list;
        }
    }

    

    public class OutTimeClient
    {
        public int ClientId { get; set; }
        public ClientStateEnum State { get; set; }
        public string Name { get; set; }
        public string GroupName { get; set; }
        public string Type { get; set; }
    }
}
