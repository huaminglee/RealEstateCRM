using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using OUDAL;

namespace RealEstateCRM.Web
{
    public static class OtherHelpers
    {
        public static List<SelectListItem> ToSelectLists(this List<IdName> list, bool appendBlank)
        {
            List<SelectListItem> items = new List<SelectListItem>();
            if (appendBlank)
            {
                items.Add(new SelectListItem {});
            }
            foreach (var i in list)
            {
                items.Add(new SelectListItem {Value = i.Id.ToString(), Text = i.Name});
            }
            return items;
        }
    }

}