using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RealEstateCRM.Web.Models
{
    public class JqGridDate
    {
        public int page { get; set; }
        public int total { get; set; }
        public int records { get; set; }
        public object rows { get; set; }
    }
}