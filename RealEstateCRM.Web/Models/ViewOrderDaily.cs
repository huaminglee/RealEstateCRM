using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BMS.Web.Models
{
    public class ViewOrderDaily
    {
        public int id { get; set; }
        public string Store { get; set; }
        public DateTime ModifyDate { get; set; }
        public string Remark { get; set; }
        public Dictionary<int, int> Details = new Dictionary<int, int>();
    }
}