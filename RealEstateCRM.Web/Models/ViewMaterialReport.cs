using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BMS.Web.Models
{
    public class ViewMaterialReport
    {
        //public string Department { get; set; }
        public int Id;
        public string Department { get; set; }
        public string CatalogName { get; set; }
        public string Name { get; set; }
        public string Spec { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal InNum { get; set; }
        public decimal InPrice { get; set; }
        public decimal OutNum { get; set; }
        
        public decimal preNum { get; set; }
        public decimal prePrice { get; set; }
        public decimal LastNum { get; set; }
        public decimal LastPrice { get; set; }
    }
    public class _ViewMaterialReport
    {
        //public string Department { get; set; }
        public int Id;
        public string Department { get; set; }
        public int CatalogId { get; set; }
        public string Name { get; set; }
        public string Spec { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal InNum { get; set; }
        public decimal InPrice { get; set; }
        public decimal OutNum { get; set; }
    }
}