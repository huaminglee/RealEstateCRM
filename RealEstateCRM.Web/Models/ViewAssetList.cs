using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BMS.Web.Models
{
    public class ViewAssetList
    {
        // a.id,a.catalogid,a.name,a.sn,a.supplier,a.assetno,a.spec,a.price,a.buydate,a.state,a.isbad,a.remark 
//,au.Location,u.Name as username,d.Name as department
        public int id { get; set; }
        public int CatalogId { get; set; }
        public string Catalog { get; set; }
        public string Name { get; set; }
        public string Sn { get; set; }
        public string Supplier { get; set; }
        public string AssetNo { get; set; }
        public string Spec { get; set; }
        public DateTime BuyDate { get; set; }
        public decimal Price { get; set; }
        public int State { get; set; }
        public string StateName { get; set; }
        public int IsBad { get; set; }
        public string Remark { get; set; }
        public string Location { get; set; }
        public string UserName { get; set; }
        public int UserId { get; set; }
        public string Department { get; set; }


    }
}