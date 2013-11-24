using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
namespace BMS.Web.Models
{
    
    public class SiteSaleReportModel
    {
        public int Id { get; set; }
        public string CardId { get; set; }
        public string Department { get; set; }
        public string Company { get; set; }
        public string SaleDepartment { get; set; }
        public string UserName { get; set; }
        public string Sales { get; set; }
        public string SaleTime { get; set; }
        public decimal Money { get; set; }
        public string Remark { get; set; }
        
    }
}
