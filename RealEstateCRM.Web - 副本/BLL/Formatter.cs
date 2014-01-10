using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RealEstateCRM.Web.BLL
{
    public class Formatter
    {
        public static string Date(DateTime? d)
        {
            if (d == null) return "";
            return ((DateTime)d).ToString("yyyy-MM-dd");
        }
        public static string  String(string s)
        {
            if (string.IsNullOrEmpty(s))return string.Empty;
            var a=  s.Replace("\n", "<br/>");
            return a;
        }
        public static string Currency(decimal num)
        {
            return num.ToString("N2");
        }
    }
}