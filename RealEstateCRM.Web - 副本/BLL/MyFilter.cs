using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RealEstateCRM.Web.Models;
using OUDAL;
namespace RealEstateCRM.Web
{
    public class MyFilter : FilterAttribute, IActionFilter, IResultFilter, IExceptionFilter, IAuthorizationFilter
    {
        #region IActionFilter 成员

        public void OnActionExecuted(ActionExecutedContext filterContext)
        {
            //filterContext.RequestContext.HttpContext.Response.Write(string.Format("Action({0})已经执行了!<br />"
            //    ,filterContext.ActionDescriptor.ActionName));
        }

        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
            //filterContext.RequestContext.HttpContext.Response.Write(string.Format("Action({0})执行之前!<br />"
            //    ,filterContext.ActionDescriptor.ActionName));
        }

        #endregion


        #region IResultFilter 成员

        public void OnResultExecuted(ResultExecutedContext filterContext)
        {
            //filterContext.RequestContext.HttpContext.Response.Write("Result已经执行了!");
        }

        public void OnResultExecuting(ResultExecutingContext filterContext)
        {
            //filterContext.RequestContext.HttpContext.Response.Write("Result执行之前!");
        }

        #endregion

        #region IExceptionFilter 成员

        public void OnException(ExceptionContext filterContext)
        {
            //string controller = filterContext.RouteData.Values["controller"] as string;
            //string action = filterContext.RouteData.Values["action"] as string;

            //filterContext.RequestContext.HttpContext.Response.Write(string.Format("{0}:{1}发生异常!{2}",
            //    controller,action, filterContext.Exception.Message));
            //filterContext.ExceptionHandled = true;
        }

        #endregion

        #region IAuthorizationFilter 成员

        public void OnAuthorization(AuthorizationContext filterContext)
        {
            //if (filterContext.HttpContext.User.Identity.IsAuthenticated == false)
            //{
            //    if(filterContext.HttpContext.Application["Login"] as string =="password")
            //    { 
            //        if (filterContext.ActionDescriptor.ActionName != "LogOn")
            //        {
            //            filterContext.HttpContext.Response.Redirect("~/Account/LogOn?ReturnUrl=" + filterContext.HttpContext.Request.Url.PathAndQuery);
            //        }
            //    }else
            //    {
            //        filterContext.HttpContext.Response.Redirect("~/Content/AccessDeny.htm");
            //    }
                
            //    //
            //}
        }

        #endregion
    }
    public enum MyAuthorizeResultEnum{ ActionResultType,JsonResultType}
    public class MyAuthorizeAttribute:AuthorizeAttribute
    {
        private string rightName;
        private int unitscope;
        private MyAuthorizeResultEnum resultType; 
        public MyAuthorizeAttribute(string _rightName)
        {
            rightName = _rightName;
            resultType = MyAuthorizeResultEnum.ActionResultType;
        }

        public MyAuthorizeAttribute(string _rightName,int unit)
        {
            rightName = _rightName;
            unitscope = unit;
            resultType = MyAuthorizeResultEnum.ActionResultType;
        }
        public MyAuthorizeAttribute(MyAuthorizeResultEnum resultEnum,string _rightName)
        {
            rightName = _rightName;
            resultType = resultEnum;
        }

        public MyAuthorizeAttribute(MyAuthorizeResultEnum resultEnum,string _rightName, int unit)
        {
            rightName = _rightName;
            unitscope = unit;
            resultType = resultEnum;
        }
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            base.OnAuthorization(filterContext);
            bool authorized = true;
            if(rightName!=null)
            {
                if(unitscope!=0)
                {
                    if(!BLL.UserInfo.CurUser.HasRight(rightName,unitscope))
                    {
                        authorized = false;
                    }
                }else if(!BLL.UserInfo.CurUser.HasRight(rightName))
                {
                    authorized = false;
                }
            }
            if(!authorized)
            {
                    if(resultType==MyAuthorizeResultEnum.JsonResultType)
                        {
                            Result result = new Result { obj = "没有权限" };
                            JsonResult re = new JsonResult { Data = result,JsonRequestBehavior = JsonRequestBehavior.AllowGet};
                            filterContext.Result = re;
                        }else
                        {
                            filterContext.HttpContext.Response.Redirect("~/Content/AccessDeny.htm");
                        }
            }
            
        } 
    }
}