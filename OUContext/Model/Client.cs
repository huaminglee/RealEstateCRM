using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
namespace OUDAL
{
    public enum ClientStateEnum {公共客户,沉睡客户, 邀约客户, 来电客户, 来访客户,办卡客户,大定客户,签约客户 }
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

        [DisplayName("产品类型")]
        public string RoomType { get; set; }

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
        [DisplayName("电话")]
        public string AllPhone { get; set; }
        //ToDo: AllPhone要处理

    }
    //ToDo:这里不需要displayname
    public class ClientView
    {
        public static string sql =
               @"select c.*,d1.name as projectname,d2.name as groupname from Clients c join departments d1 on c.projectid=d1.id join departments d2 on c.groupid=d2.id where 1=1";
        public int Id { get; set; }

        [DisplayName("名称")]
        public string Name { get; set; }

        [DisplayName("性别")]
        public string Sex { get; set; }

        [DisplayName("所属项目")]
        public int ProjectId { get; set; }

        [DisplayName("所属小组")]
        public int GroupId { get; set; }

        [DisplayName("产品类型")]
        public string RoomType { get; set; }
        [DisplayName("所属项目")]
        public string ProjectName { get; set; }

        [DisplayName("所属小组")]
        public string GroupName { get; set; }

        [DisplayName("渠道")]
        public string Way { get; set; }

        [DisplayName("渠道说明")]
        public string WayExtend { get; set; }

        [DisplayName("编码")]
        public string Code { get; set; }

        //[DisplayName("备注")]
        //public string Remark { get; set; }

        [DisplayName("创建时间")]
        public DateTime CreateTime { get; set; }
        //保存电话及联系人时候，重新更新这个字段，将所有电话包含进来
        [DisplayName("电话")]
        public string AllPhone { get; set; }
        public ClientStateEnum State { get; set; }
        public DateTime StateDate { get; set; }
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

        [DisplayName("产品类型")]
        public string RoomType { get; set; }

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
        [DisplayName("电话")]
        public string AllPhone { get; set; }
        [DisplayName("联系类型")]
        [Required]
        public string ContactType { get; set; }
        [DisplayName("联系时间")]
        public DateTime? ContactActualTime { get; set; }
        [DisplayName("联系情况说明")]
        public string ContactDetail { get; set; }
        [DisplayName("邀约类型")]
        public string AppointmentType { get; set; }
        [DisplayName("邀约时间")]
        public DateTime? AppointmentPlanTime { get; set; }
        
        [DisplayName("邀约情况说明")]
        public string AppointmentDetail { get; set; }
    }

    
}