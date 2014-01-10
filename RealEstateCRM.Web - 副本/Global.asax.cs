using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Security.Principal;
using OUDAL;
using RealEstateCRM.Web.BLL;

namespace RealEstateCRM.Web
{


    public class MvcApplication : System.Web.HttpApplication
    {
        public MvcApplication()
        {

        }


        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new MyFilter());
        }


        protected void Application_Start()
        {
            string configFileName = Server.MapPath("~/log.log4net");
            System.IO.FileInfo f = new System.IO.FileInfo(configFileName);
            LogHelper.LogHelper.SetConfig(f, "");
            LogHelper.LogHelper.Info("App Start");

            SqlHelper.SetConnString(System.Configuration.ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString);
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            System.Data.Entity.Database.SetInitializer<OUDAL.Context>(null);
            Application["Login"] = System.Configuration.ConfigurationManager.AppSettings["Login"];


            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            try
            {
                ClientBLL.AutoTransfer();
            }
            catch (Exception e)
            {
                
            }
            BundleMobileConfig.RegisterBundles(BundleTable.Bundles);

            //RealEstateCRM.Web.BLL.SynchronismWorkflow.SynchronishJob.Schedule();
        }
        protected void Application_Error()
        {
            Exception objExp = HttpContext.Current.Server.GetLastError();
            LogHelper.LogHelper.Error("Error", objExp.InnerException);
        }
    }
}