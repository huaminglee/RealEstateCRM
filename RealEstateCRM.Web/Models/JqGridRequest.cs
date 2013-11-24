using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RealEstateCRM.Web.Models
{
    public class JqGridRequest
    {
        public int page { get; set; }
        public int rows { get; set; }
        public string sidx { get; set; }
        public string sord { get; set; }
        public int StartRecord;
    }
    
}