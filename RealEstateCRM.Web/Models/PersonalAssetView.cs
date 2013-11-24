using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BMS.Web.Models
{
   
    public class PersonalAssetView
    {
        public int AssetId { get; set; }
        public string AssetName { get; set; }
        public string AssetNo { get; set; }
        public string AssetBrand { get; set; }
        public string SN { get; set; }
        public DateTime BeginDate { get; set; }
        public string Location { get; set; }
    }
}