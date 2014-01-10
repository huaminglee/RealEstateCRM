using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
namespace OUDAL
{
    //来电客户转来访后，如果确认无效，则转为无效客户
    public enum ClientStateEnum { 公共客户, 沉睡客户, 邀约客户, 来电客户, 来访客户, 办卡客户, 大定客户, 签约客户,无效客户 }
    public enum ClientViewScopeEnum { 无, 查看本组, 查看项目, 查看所有 }
    public class Client
    {
        public static string LogClass = "客户";
        public int Id { get; set; }
        [Required]
        [DisplayName("名称")]
        public string Name { get; set; }
        [DisplayName("性别")]
        public string Sex { get; set; }
        [DisplayName("项目")]
        public int ProjectId { get; set; }

        [DisplayName("销售组")]
        public int GroupId { get; set; }
        [Required]
        [DisplayName("产品类型")]
        public string RoomType { get; set; }
        [Required]
        [DisplayName("渠道")]
        public string Way { get; set; }

        [DisplayName("渠道说明")]
        public string WayExtend { get; set; }

        [DisplayName("编码")]
        public string Code { get; set; }

        [DisplayName("备注")]
        public string Remark { get; set; }

        [DisplayName("创建时间")]
        public DateTime CreateTime { get; set; }
        [DisplayName("客户状态")]
        public ClientStateEnum State { get; set; }

        [DisplayName("当期状态开始日期")]
        public DateTime StateDate { get; set; }
        //记录转入公共及沉睡客户的次数，每转入一次均加1
        public int SleepTimes { get; set; }
        public static string GetName(int id)
        {
            using (Context db = new Context())
            {
                var c = db.Clients.Find(id);
                if (c != null) return c.Name;
            }
            return "";
        }
        //保存电话及联系人时候，重新更新这个字段，将所有电话包含进来
        [Required]
        [DisplayName("电话1")]
        public string Phone1 { get; set; }
        [DisplayName("电话2")]
        public string Phone2 { get; set; }
        [DisplayName("电话")]
        public string AllPhone { get; set; }
        //ToDo: AllPhone要处理
       
        [DisplayName("电话中心经办人")]
        public string CallPerson { get; set; }
        [DisplayName("无效原因")]
        public string InvalidReason { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="date"></param>
        /// <returns>返回状态变化的log信息</returns>
        public static string StateUpdate(int id, DateTime? date)
        {
            Context db = new Context();
            Client c = db.Clients.Find(id);
            ClientStateEnum oldstate = c.State;
            while (true)
            {
                if (DepartmentBLL.GetNameById(c.GroupId).Equals("公共客户"))
                {
                    c.State = ClientStateEnum.公共客户;
                    break;
                }
                if (DepartmentBLL.GetNameById(c.GroupId).Equals("沉睡客户"))
                {
                    c.State = ClientStateEnum.沉睡客户;
                    break;
                }
                var sign = (from o in db.Orders where o.SignTime.HasValue && o.ClientId == id && !o.CancelTime.HasValue orderby o.SignTime ascending select o).FirstOrDefault();
                if (sign!=null)
                {
                    c.State = ClientStateEnum.签约客户;
                    c.StateDate = sign.SignTime.GetValueOrDefault();
                    break;
                }
                var order = (from o in db.Orders where !o.SignTime.HasValue && o.ClientId == id && !o.CancelTime.HasValue orderby o.OrderTime ascending select o).FirstOrDefault();
                if (order!=null)
                {
                    c.State = ClientStateEnum.大定客户;
                    c.StateDate = order.OrderTime;
                    break;
                }
                var card =
                    (from o in db.Cards where !o.CancelTime.HasValue && o.ClientId == id select o).FirstOrDefault();
                if (card!=null)
                {
                    c.State = ClientStateEnum.办卡客户;
                    c.StateDate = card.SmallTime;
                    break;
                }
                List<ClientActivity> all = (from o in db.ClientActivities where o.ActualTime.HasValue && o.ClientId == id  select o).ToList();
                //邀约中，非来电去电，且有完成日期的，为来访时间
                List<ClientActivity> invite = (from o in all where o.Type.Equals("来电") == false && o.Type.Equals("去电") == false orderby o.ActualTime ascending select o).ToList();//
                if (invite.Count != 0)
                {
                    c.State = ClientStateEnum.来访客户;
                    c.StateDate = invite.First().ActualTime.GetValueOrDefault();
                    break;
                }
                List<ClientActivity> call = (from o in all where o.Type.Equals("来电") orderby o.ActualTime ascending select o).ToList();
                if (call.Count != 0)
                {
                    c.State = ClientStateEnum.来电客户;
                    c.StateDate = call.First().ActualTime.GetValueOrDefault();
                    break;
                }

                ClientActivity firstinvite = (from o in db.ClientActivities where !o.ActualTime.HasValue && o.ClientId == id &&o.PlanTime.HasValue orderby o.PlanTime ascending select o).FirstOrDefault();
                c.State = ClientStateEnum.邀约客户;
                if (firstinvite != null)
                {
                    c.StateDate = firstinvite.PlanTime.Value;
                }
                
                break;

            }
            string res = "";
            if (oldstate != c.State)
            {
                c.SleepTimes = 0;
                res += string.Format("客户状态转为{0} ", c.State.ToString());
            }
            if (date.HasValue)
            {
                c.StateDate = date.GetValueOrDefault();
                res += string.Format("状态开始时间设为{0}", date.GetValueOrDefault().ToString("yyyy年MM月dd日"));
            }
            db.SaveChanges();
            return res;
        }


    }

    //ToDo:这里不需要displayname
    public class ClientView
    {
        public static string sql =
               @"select c.*,d1.name as projectname,d2.name as groupname from Clients c join departments d1 on c.projectid=d1.id join departments d2 on c.groupid=d2.id where 1=1";
        public int Id { get; set; }

        public string Name { get; set; }
        public string Phone1 { get; set; }
        public string Phone2 { get; set; }

        public string Sex { get; set; }

        public int ProjectId { get; set; }

        public int GroupId { get; set; }

        public string RoomType { get; set; }
        public string ProjectName { get; set; }

        public string GroupName { get; set; }

        public string Way { get; set; }

        public string WayExtend { get; set; }

        public string Code { get; set; }


        public DateTime CreateTime { get; set; }
        public string AllPhone { get; set; }
        public ClientStateEnum State { get; set; }
        public DateTime StateDate { get; set; }
        public string InvalidReason { get; set; }
        public string CallPerson { get; set; }

        public string MarkedPhone1
        {
            get
            {
                if (Phone1 == null) return "";
                if (Phone1.Length >= 11)
                {
                    return Phone1.Substring(0, 3) + "****" + Phone1.Substring(7);
                }
                else if (Phone1.Length >= 8)
                {
                    return "****" + Phone1.Substring(4);
                }
                else
                {
                    return Phone1;
                }
            }
        }
        public string MarkedPhone2
        {
            get
            {
                if (Phone2 == null) return "";
                if (Phone2.Length >= 11)
                {
                    return Phone2.Substring(0, 3) + "****" + Phone2.Substring(7);
                }
                else if (Phone2.Length >= 8)
                {
                    return "****" + Phone2.Substring(4);
                }
                else
                {
                    return Phone2;
                }
            }
        }
    }

    public class ClientCreate
    {
        public int Id { get; set; }
        [Required]
        [DisplayName("名称")]
        public string Name { get; set; }
        [DisplayName("性别")]
        public string Sex { get; set; }
        [DisplayName("所属项目")]
        public int ProjectId { get; set; }

        [DisplayName("所属小组")]
        public int GroupId { get; set; }
        [Required]
        [DisplayName("产品类型")]
        public string RoomType { get; set; }
        [Required]
        [DisplayName("渠道")]
        public string Way { get; set; }

        [DisplayName("渠道说明")]
        public string WayExtend { get; set; }

        [DisplayName("编码")]
        public string Code { get; set; }

        [DisplayName("备注")]
        public string Remark { get; set; }

        [DisplayName("创建时间")]
        public DateTime CreateTime { get; set; }
        //保存电话及联系人时候，重新更新这个字段，将所有电话包含进来
        [Required]
        [DisplayName("电话1")]
        public string Phone1 { get; set; }
        [DisplayName("电话2")]
        public string Phone2 { get; set; }


        [DisplayName("联系时间")]
        public DateTime? ContactActualTime { get; set; }

        [DisplayName("邀约类型")]
        public string AppointmentType { get; set; }
        [DisplayName("邀约时间")]
        public DateTime? AppointmentPlanTime { get; set; }

        [DisplayName("邀约情况说明")]
        public string AppointmentDetail { get; set; }
        [DisplayName("电话中心经办人")]
        public string CallPerson { get; set; }
    }



}