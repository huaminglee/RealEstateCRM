using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using RealEstateCRM.Web.BLL;
using RealEstateCRM.Web.Models;
using OUDAL;
namespace RealEstateCRM.Web.Controllers
{
    public class AccountController : Controller
    {   
        private Context db = new Context();
        protected override void Initialize(RequestContext requestContext)
        {
            base.Initialize(requestContext);
        }

        // **************************************
        // URL: /Account/LogOn
        // **************************************

        public ActionResult LogOn()
        {
            return View();
        }
        [HttpPost]
        public ActionResult LogOn(RealEstateCRM.Web.Models.LogOnModel model, string returnUrl)
        {
            
                SystemUser user = null;
                UserCheckResult result = SystemUser.CheckUser(model.UserName, model.Password, out user);
                switch (result)
                {
                    case UserCheckResult.验证通过:
                        FormsAuthentication.SetAuthCookie(user.Id.ToString(), false);
                        HttpContext.Session["User"] = null;
                        if (Url.IsLocalUrl(returnUrl) && returnUrl.Length > 10 && returnUrl.StartsWith("/")
                            && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))
                        {
                            return Redirect(returnUrl);
                        }
                        else
                        {
                            bool notInStore = false;
                            List<IdName> stores = new List<IdName>();
                            List<Department> departments = (from o in db.DepartmentUsers
                                                            join p in db.Departments.AsNoTracking() on o.DepartmentId
                                                                equals p.Id
                                                            where o.UserId == user.Id
                                                            select p).ToList();
                            foreach (var dept in departments)
                            {
                                if (dept.DepartmentType == "门店")
                                {
                                    stores.Add(new IdName {Id = dept.Id, Name = dept.Name});
                                }
                                else
                                {
                                    notInStore = true;
                                }
                            }
                            if (notInStore == true || departments.Count == 0)
                            {
                                return RedirectToAction("Index", "Home");
                            }
                            else if (stores.Count > 1)
                            {
                                return RedirectToAction("SelectStore", "Account");
                            }
                            else
                            {
                                return Redirect(string.Format("~/Store/{0}/Home/StoreIndex",stores[0].Id));
                            }
                        }
                        break;

                    default:
                        ModelState.AddModelError("", result.ToString());
                        break;
                }
            // If we got this far, something failed, redisplay form
            return View(model);
        }

       

        // **************************************
        // URL: /Account/LogOff
        // **************************************

        public ActionResult LogOff()
        {
            HttpContext.Session["User"] = null;
            
            FormsAuthentication.SignOut();

            return RedirectToAction("Index", "Home");
        }
        public ActionResult ChangePassword()
        {
            return View();
        }
        [HttpPost]
        public ActionResult ChangePassword(string oldpassword, string password)
        {
            int id = BLL.UserInfo.CurUser.Id;
            if (password.Length == 0)
            {
                ModelState.AddModelError("password", "密码不能为空");
            }
            using (Context db = new Context())
            {
                SystemUser user = (from o in db.SystemUsers where o.Id == id select o).First();
                if (user.CheckPassword(oldpassword) == true)
                {
                    user.Password = password;
                    user.Save(db);
                    db.SaveChanges();
                    return Redirect("ChangePasswordSuccess");
                }
                else
                {
                    ModelState.AddModelError("oldpassword", "旧密码错误");
                }
            } return View();
        }
        public ActionResult ChangePasswordSuccess()
        {
            return View();
        }

    }
}
