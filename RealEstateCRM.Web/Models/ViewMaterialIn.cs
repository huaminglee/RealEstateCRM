using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BMS.Web.Models
{
    public class ViewMaterialIn
    {
        public int id { get; set; }
        public string Department { get; set; }
        public string UserName { get; set; }
        public string Code { get; set; }
        public string InType { get; set; }
        public string Supplier { get; set; }
        public DateTime InDate { get; set; }
        public string Remark { get; set; }
        public string Name { get; set; }
        public string Spec { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Num { get; set; }
        public decimal Price { get; set; }
    }
}