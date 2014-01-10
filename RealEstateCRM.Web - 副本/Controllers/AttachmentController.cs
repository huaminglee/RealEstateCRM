using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using Dapper;
using RealEstateCRM.Web.BLL;
using OUDAL;
namespace RealEstateCRM.Web.Controllers
{
    public class AttachmentController : BaseController
    {
        //
        // GET: /Attachment/
        Context db = new Context();
        public ActionResult List()
        {
            return View();
        }
        [HttpPost]
        public JsonResult ListQuery(string sidx, string sord, int page, int rows, FormCollection collection)
        {
            List<object> parameters = new List<object>();
            string sql = AttachmentView.sql;

            Utilities.AddSqlFilterLike(collection, "name", ref sql, "a.FileName", parameters);

            if (!string.IsNullOrEmpty(sidx) && !sidx.Contains(';') && !sord.Contains(';'))
            {
                //string pre = "";
                sql += string.Format(" order by {0} {1}", sidx, sord);
            }
            db.Database.Connection.Open();
            var dynamicParams = new DynamicParameters();
            parameters.ForEach(o => { var p = o as SqlParameter; dynamicParams.Add(p.ParameterName, p.Value, p.DbType); });
            var query = db.Database.Connection.Query<AttachmentView>(sql, param: dynamicParams);
            var list = query.ToList();
            int totalrow = list.Count();
            int pagenum = (totalrow - totalrow % rows) / rows + 1;
            var newquery = (from o in list
                            select o).Take(rows * page).Skip(page * rows - rows).ToList();// 这种写法是在内存中运算
            newquery.ForEach(o =>
            {

            });
            var jsonData = new
            {
                total = pagenum,
                page = page,
                records = totalrow,
                rows = newquery
            };
            return Json(jsonData);
        }

        public ActionResult Download(string fileid)
        {
            Guid id;
            if (Guid.TryParse(fileid, out id))
            {
                Attachment attachment = db.Attachments.Find(id);
                if (attachment == null)
                {
                    return View("ShowError", "", "找不到文档");
                }
                return File(attachment.Contents, attachment.ContentType, attachment.FileName);
            }
            return View("ShowError", "", "找不到文档");
        }
        public ActionResult Upload()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Upload(string mastertype, int masterid, FormCollection collection)
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
                    file.CreateTime = DateTime.Now;
                    file.Contents = new byte[postFile.ContentLength];
                    postFile.InputStream.Read(file.Contents, 0, postFile.ContentLength);
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
        public ActionResult Delete(string fileid, FormCollection collection)
        {


            Guid gid;
            Guid.TryParse(fileid, out gid);
            Attachment attachment = db.Attachments.Find(gid);
            if (attachment == null)
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