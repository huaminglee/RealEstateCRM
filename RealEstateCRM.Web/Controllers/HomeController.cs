﻿using System;
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
            var projects = UserInfo.CurUser.GetProjects();
            if (projects.Count == 1)
            {
                return Redirect("~/Project/" + projects[0].Id.ToString() + "/Home/ProjectIndex");
            }
            return View(Request.Browser);
            
        }
        public ActionResult ProjectIndex(int projectid)
        {
            int groupid = 0;
            if (UserInfo.CurUser.GetClientRight(projectid) <= ClientViewScopeEnum.查看本组)
            {
                groupid = UserInfo.CurUser.GetGroup(projectid);
            }

            List<ClientActivityReportView> inviteList = new List<ClientActivityReportView>();
            List<string> yaoyueTypes = DictionaryBLL.GetByName("邀约类型",false);
            yaoyueTypes.ForEach(o =>
                                {
                                    inviteList.Add(new ClientActivityReportView {Type = o});
                                });
            List<ClientActivityReportView> caList = ClientActivityReportView.GetReport(projectid, groupid,
                DateTime.Today, DateTime.Today);
            foreach (var i in caList)
            {
                foreach (var j in inviteList)
                {
                    if (i.Type == j.Type)
                    {
                        j.Num += i.Num;
                        j.VisitNum += i.VisitNum;
                        j.DoneNum += i.DoneNum;
                    }
                }
            }
            ViewBag.InviteList = inviteList;
            Project project = Project.Get(projectid);

            List<ClientTransferAlertReport> transferAlerts = project.GetOutTimeAlertNum(projectid, groupid);
            List<ClientTransferAlertReport> transferOuts = new List<ClientTransferAlertReport>
                                                             {
                                                                 new ClientTransferAlertReport {TransferType = "电转访超期", Num = 1},
                                                                 new ClientTransferAlertReport {TransferType = "办卡超期", Num = 1}
                                                             };
            List<ClientTransferAlertReport> transferIns = new List<ClientTransferAlertReport>
                                                             {
                                                                 new ClientTransferAlertReport {TransferType = "来电客户", Num = 1},
                                                                 new ClientTransferAlertReport {TransferType = "来访客户", Num = 1}
                                                             };
            ViewBag.transferAlerts = transferAlerts;
            ViewBag.transferOuts = transferOuts;
            ViewBag.transferIns = transferIns;
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
