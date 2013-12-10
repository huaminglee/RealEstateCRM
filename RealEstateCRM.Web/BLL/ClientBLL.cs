using OUDAL;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RealEstateCRM.Web.BLL
{
    public class ClientBLL
    {
        public static void AutoTransfer()
        {
            using (Context db = new Context())
            {
                List<int> projectids = (from o in db.Departments where o.DepartmentType.Equals("项目") select o.Id).ToList();
                foreach (int projectid in projectids)
                {
                    int gonggong = Project.GetGroupByName(projectid, "公共客户");
                    int chenshui = Project.GetGroupByName(projectid, "沉睡客户");
                    AutoTransfer(projectid, ClientStateEnum.来电客户, gonggong, chenshui);
                    AutoTransfer(projectid, ClientStateEnum.来访客户, gonggong, chenshui);
                    AutoTransfer(projectid, ClientStateEnum.办卡客户, gonggong, chenshui);
                }
            }
        }
        public static void AutoTransfer(int project, ClientStateEnum state, int gonggong, int chenshui)
        {
            string field = "";
            string type = "";
            switch (state)
            {
                case ClientStateEnum.来电客户:
                    field = "TelToVisitDays";
                    type = "电转访超期转移";
                    break;
                case ClientStateEnum.来访客户:
                    field = "VisitToCardDays";
                    type = "访转卡超期转移";
                    break;
                case ClientStateEnum.办卡客户:
                    field = "CardToOrderDays";
                    type = "卡转定超期转移";
                    break;
            }
            string sql = @"select c.* from clients c join departments d on c.groupid=d.id join roomtypes t on t.departmentid=c.projectid and t.name=c.roomtype where c.projectid={0} and  c.state={1} and t.{2} >0 and  DATEADD(dd,t.{2},c.statedate)<getdate()";
            sql = string.Format(sql, project, (int)state, field, type);
            using (Context db = new Context())
            {
                List<Client> list = db.Database.SqlQuery<Client>(sql).ToList();
                foreach (Client client in list)
                {
                    Client c = db.Clients.Find(client.Id);
                    db.ClientTransfers.Add(new ClientTransfer()
                    {
                        ClientId = c.Id,
                        InGroup = c.SleepTimes == 0 ? gonggong : chenshui,
                        OutGroup = c.GroupId,
                        Reason = type,
                        Person = 1,
                        TransferDate = DateTime.Today
                    });
                    if (c.SleepTimes == 0)
                    {
                        c.GroupId = gonggong;
                        c.State = ClientStateEnum.公共客户;
                    }
                    else
                    {
                        c.GroupId = chenshui;
                        c.State = ClientStateEnum.沉睡客户;
                    }
                    c.SleepTimes++;
                    c.StateDate = DateTime.Today;
                    db.SaveChanges();
                    //Client.StateUpdate(client.Id, DateTime.Today);
                }
            }
        }
    }
}