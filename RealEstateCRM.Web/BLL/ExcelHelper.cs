using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OfficeOpenXml;
using System.Xml;
using OfficeOpenXml.Style;
using OUDAL;
using System.Diagnostics;
using System.Web;
using System.IO;
using System.Reflection;
using System.Web.Mvc;
namespace RealEstateCRM.Web.BLL
{
    public  class ExcelHelper
    {
        static public string ReadString(ExcelRange cell)
        {
            if (cell.Value != null) return cell.Value.ToString().Trim();
            return "";
        }
        static public decimal ReadDecimal(ExcelRange cell)
        {
            decimal d = 0;
            try
            {
                if (cell.Value != null) return Convert.ToDecimal((double)(cell.Value)); ;
            }
            catch (Exception exp)
            {
                Debug.WriteLine("错误" + cell.Address + " " + exp.Message);
            }
            return d;
        }
        
        static public DateTime? ReadDateEmpty(ExcelRange cell)
        {
            if (cell.Value == null) return null;
            long serialDate = 0;

            if(long.TryParse(cell.Value.ToString(),out serialDate))
            {
                return DateTime.FromOADate(serialDate);
            }
            DateTime d1 = DateTime.MinValue;
            if (DateTime.TryParse(cell.Value.ToString(), out d1)) return d1;
            return null;

        }
        static public void Output<T>(HttpResponseBase response,List<T> data,string templateName)
        {
            //LogHelper.LogHelper.Info("begin");
            DirectoryInfo di=new DirectoryInfo(System.IO.Path.Combine(HttpRuntime.AppDomainAppPath, "App_Data\\Excel"));
            if(!di.Exists)
            {
                di.Create();
            }
            FileInfo fi=new FileInfo(System.IO.Path.Combine(HttpRuntime.AppDomainAppPath, "App_Data\\Excel\\"+templateName));
            if(!fi.Exists)
            {
                throw new Exception(string.Format("模板文件 {0} 不存在",templateName)); 
            }
            using (ExcelPackage package = new ExcelPackage(fi))
            {
                var sheet=package.Workbook.Worksheets.First();
                int headrow=3;
                if (data.Count == 0)
                {
                    for (int i = 1; i < 100; i++)
                    {
                        var headcell = sheet.Cells[headrow, i];
                        headcell.Value = null;
                    }
                }
                else
                {
                    List<PropertyInfo> properties = new List<PropertyInfo>();
                    List<int> indexs = new List<int>();
                    for (int i = 1; i < 100; i++)
                    {
                        var headcell = sheet.Cells[headrow, i];
                        if (headcell != null && headcell.Value != null && headcell.Value.ToString() != "")
                        {
                            PropertyInfo pi = typeof(T).GetProperty(headcell.Value.ToString().Trim());
                            if (pi != null)
                            {
                                properties.Add(pi); indexs.Add(i);
                            }
                        }
                    }
                    int row = headrow;
                    foreach (T rowData in data)
                    {
                        for (int i = 0; i < indexs.Count; i++)
                        {
                            sheet.Cells[row, indexs[i]].Value = properties[i].GetValue(rowData, null);
                        }
                        row++;
                    }
                }
                //LogHelper.LogHelper.Info("end stream");
                package.SaveAs(response.OutputStream);
                response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                response.AddHeader("content-disposition", "attachment;  filename=Export.xlsx");
                response.AddHeader("Content-Length", package.Stream.Length.ToString());
                //LogHelper.LogHelper.Info("end output");
            }
        }
    }
}