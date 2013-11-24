using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using RealEstateCRM.Web.BLL;
using OUDAL;
namespace RealEstateCRM.Web.Controllers
{
    public class AttachmentController : BaseController
    {
        //
        // GET: /Attachment/
        Context db=new Context();
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Download(string fileid)
        {
            //Guid id;
            //if(Guid.TryParse(fileid,out id))
            //{
            //    Attachment attachment = db.Attachments.Find(id);
            //    if(attachment==null)
            //    {
            //        return View("ShowError", "", "找不到文档");
            //    }
            //    return File(attachment.Contents, attachment.ContentType, attachment.FileName);
            //}
            return View("ShowError", "", "找不到文档");
        }
        public ActionResult Upload()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Upload(string mastertype,int masterid,FormCollection collection)
        {
            int fileNum = 0;
            int.TryParse(Request["fileNum"], out fileNum);
            List<HttpPostedFileBase> postFiles = new List<HttpPostedFileBase>();
            for (int i = 0; i < fileNum; i++)
            {

                HttpPostedFileBase postFile = Request.Files["file" + i.ToString()];
                if (postFile != null && postFile.ContentLength > 0)
                {
                    Attachment file = new Attachment();
                    file.FileName = Path.GetFileName(postFile.FileName);
                    file.ContentType = postFile.ContentType;
                    file.Length = postFile.ContentLength;
                    file.Id = Guid.NewGuid();
                    file.MasterId = masterid;
                    file.MasterType = mastertype;
                    //file.Contents=new byte[postFile.ContentLength];
                    //postFile.InputStream.Read(file.Contents, 0, postFile.ContentLength);
                    db.Attachments.Add(file);
                }
            }
            db.SaveChanges();

            return Redirect("~/content/close.htm");
        }
        public ActionResult Delete(string fileid)
        {
            return View();
        }
        [HttpPost]
        public ActionResult Delete(string fileid,FormCollection collection)
        {

            
            Guid gid;
            Guid.TryParse(fileid, out gid);
            Attachment attachment = db.Attachments.Find(gid);
            if(attachment==null)
            {
                return View("ShowError", "", "找不到附件");
            }
            int id = 0;
           
           
            db.Attachments.Remove(attachment);
            db.SaveChanges();
            return Redirect("~/content/close.htm");
        }

    }
}
