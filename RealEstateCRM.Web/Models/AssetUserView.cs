using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BMS.Web.Models
{
    public class AssetUserView
    {
        public int Id { get; set; }
        public int AssetId { get; set; }
        public int? UserId { get; set; }
        public string Location { get; set; }
        public DateTime BeginDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string UserName { get; set; }
        public string UseType { get; set; }
    }
}