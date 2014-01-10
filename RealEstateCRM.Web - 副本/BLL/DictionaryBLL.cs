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

        static public List<Dictionary2> GetL2ByName(string dictName, bool appendblank)
        {
            List<string> list = GetByName(dictName,false);
            List<Dictionary2> list2 = new List<Dictionary2>();
            foreach (string s in list)
            {
                string[] strs = s.Split('-');
                if (strs.Length >0 )
                {
                    Dictionary2 d2=null;
                    foreach (var d in list2)
                    {
                        if (d.L1 == strs[0]) d2 = d;
                    }
                    if (d2 == null)
                    {
                        d2 = new Dictionary2 {L1 = strs[0], L2 = new List<string>()};
                        list2.Add(d2);
                    }
                    if (strs.Length > 1)
                    {
                        d2.L2.Add(strs[1]);
                    }
                }
            }
            return list2;
        }
    }

    public class Dictionary2
    {
        public string L1{get;set;}
        public List<string> L2{get;set;}
    }
}