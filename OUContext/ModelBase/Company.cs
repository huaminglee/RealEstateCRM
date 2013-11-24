using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace OUDAL
{
    public enum CompanyState { Enabled, Disabled }
    public class Company
    {
        public static string LogClass = "公司";
        public int Id { get; set; }
        [Required]
        [DisplayName("名称")]
        public string Name { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull=false)]
        [DisplayName("备注")]
        public string Remark { get; set; }
        /// <summary>
        /// 0=正常 1=禁用
        /// </summary>
        [DisplayName("状态")]
        public int State { get; set; }

        [DisplayName("账号")]
        public string AccountNumber { get; set; }
        public Company()
        {
            Remark = "";
            State = 0;
        }

        /*----BLL ----*/
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
    /// <summary>
    /// test customer valida
    /// </summary>
    public class CompanyStateValidationAttribute : ValidationAttribute
    {
        public CompanyStateValidationAttribute()
            : base("{0} must be 0 or 1")
        { }
        protected override ValidationResult IsValid(object value,ValidationContext context)
        {
            int i = (int)value;
            if (i == 0 || i == 1) return null;
            return new ValidationResult(string.Format(ErrorMessageString,context.DisplayName));
        }
    }
}
