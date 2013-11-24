using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BMS.Web.Models
{
    public class ViewMaterialOutListReport
    {
        public int Id { get; set; }
        public DateTime OutDate { get; set; }
        public string Department { get; set; }
        public int DepartmentId { get; set; }
        public string UserName { get; set; }
        public int MaterialId;
        public int CatalogId { get; set; }
        public string CatalogName { get; set; }
        public string Name { get; set; }
        public string Spec { get; set; }
        public decimal OutNum { get; set; }
        public string Remark { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal UncountedNum { get; set; }
    }

    public class ViewMaterialOutReport
    {
        public int DepartmentId { get; set; }
        public string Department { get; set; }
        public int MaterialId;
        public int CatalogId { get; set; }
        public string CatalogName { get; set; }
        public string Name { get; set; }
        public string Spec { get; set; }
        public decimal OutNum { get; set; }
        public decimal TotalPrice { get; set; }
        
    }
    /// <summary>
    /// 先进先出统计辅助类
    /// </summary>
    public class ViewMaterialInOut
    {
        public int MaterialId { get; set; }
        public DateTime InDate { get; set; }
        public decimal Num { get; set; }
        public decimal Price { get; set; }
        public decimal OutNum { get; set; }
    }
    //public class ViewMaterialOutDetailReport
    //{
    //    public int Id;
    //    public string UserName { get; set; }
    //    public string CatalogName { get; set; }
    //    public string Name { get; set; }
    //    public string Spec { get; set; }
    //    public decimal Out { get; set; }
    //}

}