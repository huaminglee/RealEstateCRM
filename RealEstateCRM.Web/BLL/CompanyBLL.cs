using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BMS.Web.Models;
using OUDAL;
using System.Web.Mvc;
namespace BMS.Web.BLL
{
    public class CompanyBLL
    {
        static public List<SelectListItem> GetCompany()
        {
            List<SelectListItem> list = new List<SelectListItem>();
            using (OUContext db = new OUContext())
            {
                var query = from c in db.Companies where c.State == 0 orderby c.Name select new IntStringItem() { Value = c.Id, Text = c.Name };
                return (from c in query.ToList() select new SelectListItem { Value = c.Value.ToString(), Text = c.Text }).ToList();

            }
        }   
    }
}