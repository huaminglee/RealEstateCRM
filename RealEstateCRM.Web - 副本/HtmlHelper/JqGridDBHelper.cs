using System;
using System.Data;
using System.Configuration;
using System.Text;
using System.IO;
using System.Web;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;

namespace RealEstateCRM.Web
{
    public class JqGridDB
    {
        public int page { get; set; }
        public int total { get; set; }
        public int records { get; set; }
        public List<Dictionary<string,object>> rows;   
    }
    public class JqGridDBRow
    {
         
    }
    /// <summary>
    /// GetDataBase 的摘要说明
    /// </summary>
    public  class JqGridDBHelper
    {
        public int PageSize;
        public int CurPage;
        public int StartRecord;

        public JqGridDBHelper(Models.JqGridRequest req, DataTable dt)
        {
            Dt = dt;
            PageSize = req.rows;
            if (PageSize <= 0) PageSize = 100000;
            CurPage = req.page;
            StartRecord = 0;

        }
        public JqGridDB Output()
        {
            JqGridDB d = new JqGridDB();
            d.page = CurPage;
            d.total = (Dt.Rows.Count-1) / PageSize + 1;
            d.records = Dt.Rows.Count;
            PagedDataTable();
            
            d.rows=new List<Dictionary<string,object>>();
            
                foreach (DataRow dr in Dt.Rows)
                {
                    Dictionary<string, object> item = new Dictionary<string, object>();
                    foreach (DataColumn col in Dt.Columns)
                    {
                        if (dr[col.ColumnName] == DBNull.Value)
                        {
                            item.Add(col.ColumnName, "");
                        }
                        else if (col.DataType == typeof(DateTime))
                        {
                            item.Add(col.ColumnName, ((DateTime)dr[col.ColumnName]).ToString("yyyy-MM-dd"));
                        }
                        else
                        {
                            item.Add(col.ColumnName, dr[col.ColumnName]);
                        }
                    }
                    d.rows.Add(item);
                }
            
            return d;
        }
        /// <summary>
        /// 这个函数没有用了
        /// </summary>
        /// <returns></returns>
        public string BuildOutput()
        {
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            using (JsonTextWriter jsonWriter = new JsonTextWriter(sw))
            {
                jsonWriter.WriteStartObject();
                jsonWriter.WritePropertyName("page");
                jsonWriter.WriteValue(CurPage);
                jsonWriter.WritePropertyName("total");
                jsonWriter.WriteValue((Total - 1) / PageSize + 1);
                jsonWriter.WritePropertyName("records");
                jsonWriter.WriteValue(Total);
                jsonWriter.WritePropertyName("rows");

                jsonWriter.WriteStartArray();
                if (Dt != null)
                {
                    for (int i = 0; i < Dt.Rows.Count; i++)
                    {
                        jsonWriter.WriteStartObject();
                        if (IndexName != null && IndexName != "")
                        {
                            jsonWriter.WritePropertyName("id");
                            jsonWriter.WriteValue(Dt.Rows[i][IndexName].ToString());
                        }

                        if (ColumnNameList != null && ColumnNameList.Length > 0)
                        {
                            for (int j = 0; j < ColumnNameList.Length; j++)
                            {
                                jsonWriter.WritePropertyName(ColumnNameList[j]);
                                jsonWriter.WriteValue(Dt.Rows[i][ColumnNameList[j]].ToString());
                            }
                        }
                        else
                        {
                            for (int j = 0; j < Dt.Columns.Count; j++)
                            {
                                jsonWriter.WritePropertyName(Dt.Columns[j].ColumnName.ToLower());
                                jsonWriter.WriteValue(Dt.Rows[i][j].ToString());
                            }
                        }
                        jsonWriter.WriteEndObject();
                    }
                }

                //合计
                if (DtSum != null)
                {
                    for (int i = 0; i < DtSum.Rows.Count; i++)
                    {
                        jsonWriter.WriteStartObject();
                        if (IndexName != null && IndexName != "")
                        {
                            jsonWriter.WritePropertyName("id");
                            jsonWriter.WriteValue(DtSum.Rows[i][IndexName].ToString());
                        }

                        if (ColumnNameList != null && ColumnNameList.Length > 0)
                        {
                            for (int j = 0; j < ColumnNameList.Length; j++)
                            {
                                jsonWriter.WritePropertyName(ColumnNameList[j]);
                                jsonWriter.WriteValue(DtSum.Rows[i][ColumnNameList[j]].ToString());
                            }
                        }
                        else
                        {
                            for (int j = 0; j < DtSum.Columns.Count; j++)
                            {
                                jsonWriter.WritePropertyName(DtSum.Columns[j].ColumnName.ToLower());
                                jsonWriter.WriteValue(DtSum.Rows[i][j].ToString());
                            }
                        }
                        jsonWriter.WriteEndObject();
                    }
                }


                jsonWriter.WriteEndArray();

                jsonWriter.WriteEndObject();
            }
            return sb.ToString();
        }
        public int Total;
        
        public string IndexName;
        public string[] ColumnNameList;
        public DataTable Dt;
        public DataTable DtSum;

        public class SumField
        {
            public string FieldName = "";
            public string SumFormula = "";
            public SumField(string fieldName, string sumFormula)
            {
                FieldName = fieldName;
                SumFormula = sumFormula;
            }
        }
        public List<SumField> ListSumField = new List<SumField>();
        public void SetListSumField(string[] fieldList, Query query)
        {
            foreach (string fieldName in fieldList)
            {
                Field field = query.GetFieldByFullName(fieldName);
                if (field != null && !String.IsNullOrEmpty(field.SumFormula))
                {
                    ListSumField.Add(new SumField(field.FieldName, field.SumFormula));
                }
            }
        }

        //protected void Init(HttpContext context)
        //{
        //    Sidx = context.Request["sidx"] + "";
        //    Sord = context.Request["sord"] + "";
        //    if (Sidx == "") { Order = ""; }
        //    else { Order = string.Format("{0} {1}", Sidx, Sord); }
        //    if (int.TryParse(context.Request["rows"] + "", out PageSize) == false) PageSize = 1000;
        //    int.TryParse(context.Request["page"] + "", out CurPage);
        //    StartRecord = (CurPage - 1) * PageSize;
        //}
        /// <summary>
        /// 将 Dt 的多余记录删除，只保留该页需要的记录集
        /// </summary>
        public void PagedDataTable()
        {
            if (Dt != null)
            {
                Total = Dt.Rows.Count;
                int endRecord = CurPage * PageSize - 1;
                int startRecord = PageSize * (CurPage - 1);

                //计算合计
                DataRow drSum1 = null;
                DataRow drSum2 = null;
                if (ListSumField.Count > 0)
                {
                    DtSum = Dt.Clone();

                    drSum1 = DtSum.NewRow();
                    if (!String.IsNullOrEmpty(IndexName))
                        drSum1[IndexName] = "-1";
                    DtSum.Rows.Add(drSum1);

                    drSum2 = DtSum.NewRow();
                    if (!String.IsNullOrEmpty(IndexName))
                        drSum2[IndexName] = "-2";
                    DtSum.Rows.Add(drSum2);

                    foreach (DataRow dr in Dt.Rows)
                    {
                        foreach (SumField sumField in ListSumField)
                        {
                            if (Dt.Columns.Contains(sumField.FieldName) && sumField.SumFormula.ToLower().StartsWith("sum"))
                            {
                                drSum2[sumField.FieldName] = Convert.ToDecimal(drSum2[sumField.FieldName]) + Convert.ToDecimal(dr[sumField.FieldName]);
                            }
                        }
                    }
                }


                for (int i = Dt.Rows.Count - 1; i > endRecord; i--)
                {
                    Dt.Rows.RemoveAt(i);
                }
                if (startRecord >= Total)
                {
                    Dt.Rows.Clear();
                }
                for (int i = 0; i < startRecord; i++)
                {
                    if (startRecord > Total)
                    {
                        CurPage = 1;
                        break;
                    }
                    Dt.Rows.RemoveAt(0);
                }
                Total = Dt.Rows.Count;
                //计算本页小计
                if (ListSumField.Count > 0)
                {
                    foreach (DataRow dr in Dt.Rows)
                    {
                        foreach (SumField sumField in ListSumField)
                        {
                            if (Dt.Columns.Contains(sumField.FieldName) && sumField.SumFormula.ToLower().StartsWith("sum"))
                            {
                                drSum1[sumField.FieldName] = Convert.ToDecimal(drSum1[sumField.FieldName]) + Convert.ToDecimal(dr[sumField.FieldName]);
                            }
                        }
                    }
                }

                //求和之外的其它公式
                foreach (SumField sumField in ListSumField)
                {
                    string[] formula = sumField.SumFormula.Split(',');
                    if (formula.Length > 0)
                    {
                        if (!DtSum.Columns.Contains(sumField.FieldName))
                            DtSum.Columns.Add(sumField.FieldName);

                        switch (formula[0].ToLower())
                        {
                            case "divide": //除法
                                if (formula.Length >= 3)
                                {
                                    if (DtSum.Columns.Contains(formula[1]) && DtSum.Columns.Contains(formula[2]))
                                    {
                                        foreach (DataRow drSum in DtSum.Rows)
                                        {
                                            drSum[sumField.FieldName] = Convert.ToDecimal(drSum[formula[2]]) == 0 ? 0 : Convert.ToDecimal(drSum[formula[1]]) / Convert.ToDecimal(drSum[formula[2]]);
                                        }
                                    }
                                }
                                break;
                        }
                    }
                }
            }
        }
    }
}