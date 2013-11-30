using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
namespace OUDAL
{
    //public enum ClientTypeEnum { 公司客户, 个人客户 }
    public class Client
    {
        public static string LogClass = "客户";
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

    }
    public class ClientView
    {
        public static string sql =
               @"select c.* from Clients c where 1=1";
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

        [DisplayName("备注")]
        public string Remark { get; set; }

        [DisplayName("创建时间")]
        public DateTime CreateTime { get; set; }
        //保存电话及联系人时候，重新更新这个字段，将所有电话包含进来
        [DisplayName("电话")]
        public string AllPhone { get; set; }
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

    public class ClientTransformLog
    {
        [DisplayName("客户姓名")]
        public string ClientName { get; set; }
        [DisplayName("转移时间")]
        public DateTime TransferTime { get; set; }
        [DisplayName("转移原因")]
        public string Reason { get; set; }
        [DisplayName("转出销售组")]
        public string OutGroup { get; set; }
        [DisplayName("转入销售组")]
        public string InGroup { get; set; }
        [DisplayName("操作人")]
        public string Person { get; set; }

        public static string sqlIn = @"select Clients.Name as ClientName,AccessTime as TransferTime,SUBSTRING(AccessInfo,(CHARINDEX('从',AccessInfo)+1),(CHARINDEX('转移到',AccessInfo)-CHARINDEX('从',AccessInfo)-1)) as OutGroup,SUBSTRING(AccessInfo,PATINDEX('%转移到%',AccessInfo)+3,LEN(AccessInfo)) as InGroup, AccessPerson as Person from AccessLogs join Clients on KeyId=Clients.Id where AccessClass='客户' and AccessAction='客户转移' and AccessInfo LIKE '从%转移到{0}'";
        public static string sqlOut = @"select Clients.Name as ClientName,AccessTime as TransferTime,SUBSTRING(AccessInfo,(CHARINDEX('从',AccessInfo)+1),(CHARINDEX('转移到',AccessInfo)-CHARINDEX('从',AccessInfo)-1)) as OutGroup,SUBSTRING(AccessInfo,PATINDEX('%转移到%',AccessInfo)+3,LEN(AccessInfo)) as InGroup, AccessPerson as Person from AccessLogs join Clients on KeyId=Clients.Id where AccessClass='客户' and AccessAction='客户转移' and AccessInfo LIKE '从{0}转移到%'";
    }
}