using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BMS.Web.Models
{
    public class ViewFoodMaterialType
    {
        public int id { get; set; }
        public int CatalogId { get; set; }
        public string Catalog { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Standard { get; set; }
        public string StoreType { get; set; }
        public string StorePlace { get; set; }
        public string BaseUnit { get; set; }
        public int GuaranteeDate { get; set; }
    }
}