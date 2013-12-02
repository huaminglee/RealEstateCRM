using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace OUDAL
{
    public class Company
    {
        public static string LogClass = "公司";
        public int Id { get; set; }
        [DisplayName("基金")]
        public int FundId { get; set; }
        [Required]
        [DisplayName("名称")]
        public string Name { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull=false)]
        [DisplayName("备注")]
        public string Remark { get; set; }
        public static string GetName(int id)
        {
            using (Context db = new Context())
            {
                var c = db.Companies.Find(id);
                if (c != null) return c.Name;
            }
            return "";
        }
    }

    public class CompanyView
    {
        public static string sql = "select c.* ,f.name as fund from companies c join funds f on c.fundid=f.id where 1=1";
        public int Id { get; set; }
        public int FundId { get; set; }
        public string Name { get; set; }
        public string Remark { get; set; }
        public string Fund { get; set; }
    }
}
