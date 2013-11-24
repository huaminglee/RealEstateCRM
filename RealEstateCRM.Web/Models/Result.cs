using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RealEstateCRM.Web.Models
{
    public class Result
    {
        public bool success { get; set; }
        public object obj { get; set; }
    }
    public class OrderBy
    {
        public string sidx;
        public string sord;
    }
}