using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RealEstateCRM.Web.Controllers
{
    public class MenuController : Controller
    {
        //
        // GET: /Menu/
        public ActionResult GetMenu(bool isproject,int? projectid)
        {
            ViewBag.IsProject = isproject;
            if(projectid.HasValue)
            {
                ViewBag.ProjectId = projectid.Value;
            }
            else
            {
                ViewBag.ProjectId = 0;
            }
            return View();
        }
	}
}