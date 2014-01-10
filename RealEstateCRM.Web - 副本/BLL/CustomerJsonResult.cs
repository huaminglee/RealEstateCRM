using System;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;

namespace BMS.Web.BLL
{
    public class CustomJsonResult : JsonResult
    {
        public override void ExecuteResult(ControllerContext context)
        {
            //MaxJsonLength = 40000000;
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            HttpResponseBase response = context.HttpContext.Response;

            if (!String.IsNullOrEmpty(ContentType))
            {
                response.ContentType = ContentType;
            }

            else
            {
                response.ContentType = "application/json";
            }

            if (ContentEncoding != null)
            {
                response.ContentEncoding = ContentEncoding;
            }

            if (Data != null)
            {
#pragma warning disable 0618

                response.Write(JsonConvert.SerializeObject(Data));

#pragma warning restore 0618
            }
        }
    }
}