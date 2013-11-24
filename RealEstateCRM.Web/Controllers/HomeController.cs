using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RealEstateCRM.Web.BLL;
using OUDAL;
using RealEstateCRM;
using System.Data.SqlClient;
namespace RealEstateCRM.Web.Controllers
{
    public class HomeController : BaseController
    {
        private Context db = new Context();
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
        public ActionResult Index()
        {
            return View(Request.Browser);
            
        }
        public ActionResult ProjectIndex(int projectid)
        {
           
            return View(projectid);

        }
        [MyAuthorize(MyAuthorizeResultEnum.JsonResultType,"某个权限",1)]
        public ActionResult About()
        {
            return View();
        }

        public ActionResult Report()
        {
            return View();
        }
        
    }
}
