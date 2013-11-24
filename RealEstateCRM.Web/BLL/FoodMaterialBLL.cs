using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using OUDAL;
using System.Text;
using System.IO;
using OfficeOpenXml;
using System.Xml;
using OfficeOpenXml.Style;
using System.Diagnostics;
using System.Transactions;
namespace BMS.Web.BLL
{
    public class FoodMaterialBLL
    {
        public string RequestImportItem(OUContext db, string code, int foodId, decimal num, DateTime date, string remark, string action)
        {
            FoodMaterialRequest req = (from o in db.FoodMaterialRequests where o.Code == code select o).FirstOrDefault();
            if (action == "删除")
            {
                if (req == null) return "找不到可以删除的申请";
                if (req.TypeId != foodId) return "要删除的记录与原记录不匹配，原料不同";
                if(req.State!=(int)FoodMaterialRequestState.未提交)return "要删除的记录状态不允许删除";
                req.State = (int)FoodMaterialRequestState.作废;
                db.SaveChanges();
                return "";
            }
            if (action == "修改")
            {
                if (req == null) return "找不到可以修改的申请";
                if (req.TypeId != foodId) return "要修改的记录与原记录不匹配，原料不同";   
                if(req.State!=(int)FoodMaterialRequestState.未提交)return "要修改的记录状态不允许修改";
            }
            if (req == null)
            {
                req = new FoodMaterialRequest();
                db.FoodMaterialRequests.Add(req);
                req.Code = code;
                req.TypeId=foodId;
            }
            req.Num = num;
            req.OrderDate = date;
            req.Remark = remark;
            req.PersonId = UserInfo.CurUser.Id;
            req.RequestTime = DateTime.Now;
            db.SaveChanges();
            return "";

        }
        public string RequestImport(Stream fileStream)
        {
            using (ExcelPackage package = new ExcelPackage(fileStream))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets[1];      
                StringBuilder sbError=new StringBuilder();
                using (OUContext db = new OUContext())
                {
                    for (int i = 2; i < 10000; i++)
                    {
                        string code = ImportExcelHelper.ReadString(worksheet.Cells[i, 1]);
                        if (code == "") continue;
                        string foodName=ImportExcelHelper.ReadString(worksheet.Cells[i,2]);
                        int? _foodId=(from o in db.FoodMaterialType where o.Name==foodName select o.Id).FirstOrDefault();
                        int foodId = 0; ;
                        bool error=false;
                        if(_foodId==null)
                        {
                            sbError.AppendLine(string.Format("第{0}行错误，找不到食材{0}",i,foodName));
                            error=true;
                        }else
                        {
                            foodId=(int)_foodId;
                        }
                        decimal num=ImportExcelHelper.ReadDecimal(worksheet.Cells[i,3]);
                        if(num<=0)
                        {
                            sbError.AppendLine(string.Format("第{0}行错误，数量不能小于0",i));
                            error=true;
                        }
                        DateTime? _date= ImportExcelHelper.ReadDateEmpty(worksheet.Cells[i,4]);
                        DateTime date=DateTime.MinValue;
                        if(_date==null)
                        {
                            sbError.AppendLine(string.Format("第{0}行错误，日期格式错误",i));
                            error=true;
                        }else
                        {
                            date=(DateTime)_date;                            
                        }
                        if(!error){
                            string remark=ImportExcelHelper.ReadString(worksheet.Cells[i,5]);
                            string action=ImportExcelHelper.ReadString(worksheet.Cells[i,6]);
                            string result= RequestImportItem(db,code,foodId,num,date,remark,action);
                            if(result!="")
                            {
                                sbError.AppendLine(string.Format("第{0}行错误，{1}",i,result));
                            }
                        }
                    }
                }
                return sbError.ToString();
            }
        }
        

   

    }
}