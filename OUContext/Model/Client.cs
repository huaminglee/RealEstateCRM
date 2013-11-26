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
}