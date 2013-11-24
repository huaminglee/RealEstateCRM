using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RealEstateCRM.Web.Models;
using OUDAL;
using System.Web.Mvc;
namespace RealEstateCRM.Web.BLL
{
    public class DictionaryBLL
    {
        static public List<SelectListItem> GetList(string dictName,bool appendblank)
        {
            List<SelectListItem> list = new List<SelectListItem>();
            if(appendblank==true)list.Add(new SelectListItem{Value="",Text="---"});
            using (Context db = new Context())
            {
                var query=from i in db.DictionaryItems join d in db.Dictionaries on i.DictId equals d.Id where d.Name==dictName orderby i.IndexNum ,i.Name select i;
                foreach (DictionaryItem c in query)
                {
                    list.Add(new SelectListItem { Value = c.Name.ToString(), Text = c.Name });
                }
                return list;
            }
        }
        static public List<string> GetByName(string dictName, bool appendblank)
        {
            List<string> list = new List<string>();
            if (appendblank == true) list.Add("");
            using (Context db = new Context())
            {
                var query = from i in db.DictionaryItems join d in db.Dictionaries on i.DictId equals d.Id where d.Name == dictName orderby i.IndexNum, i.Name select i;
                foreach (DictionaryItem c in query)
                {
                    list.Add(c.Name);
                }
                return list;
            }
        } 
    }
}