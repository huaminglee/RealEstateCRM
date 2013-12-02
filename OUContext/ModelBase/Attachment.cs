using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
namespace OUDAL
{
   
    public class Attachment
    {
        public static string LogClass = "附件";
        public Guid Id { get; set; }
        [Required]
        [MaxLength(50)]
        public string MasterType { get; set; }
        public int MasterId { get; set; }
        [Required]
        [MaxLength(100)]
        [DisplayName("文档名")]
        public string FileName { get; set; }
        [DisplayName("文档类型")]
        public string ContentType { get; set; }
        [DisplayName("文档大小")]
        public int Length { get; set; }
        [DisplayName("创建时间")]
        public DateTime CreateTime { get; set; }
        [Required]

        public byte[] Contents { get; set; }
    }

    public class AttachmentVM
    {
        public static string sql =
            @"select a.id,a.filename,a.length,a.createtime,a.mastertype
from Attachments a where a.masterid={0} and a.mastertype='{1}'";
        public Guid Id { get; set; }
        public string MasterType { get; set; }
        public int MasterId { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public int Length { get; set; }
        public DateTime CreateTime { get; set; }

        public static List<AttachmentVM> Get(string _type,int id)
        {
            Context db = new Context();
            return db.Database.SqlQuery<AttachmentVM>(string.Format(sql, id, _type)).ToList();
        }
    }
    public class AttachmentView
    {
        public static string sql =
            @"select a.id,a.filename,a.length,a.createtime,a.mastertype,f.name as fund 
from Attachments a left outer join funds f on a.masterid=f.id and a.mastertype='基金'";
        public Guid Id { get; set; }
        public string MasterType { get; set; }
        public int MasterId { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public int Length { get; set; }
        public DateTime CreateTime { get; set; }
        public string Fund { get; set; }
    }
}
