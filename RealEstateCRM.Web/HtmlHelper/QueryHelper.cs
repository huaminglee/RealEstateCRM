using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using System.Text;
using System.Data.SqlClient;
namespace RealEstateCRM.Web
{
    public class Query
    {
        public string Name;
        public Table MainTable;
        public List<Table> TableList = new List<Table>();
        public List<Field> FieldList = new List<Field>();
        public List<Filter> FilterList = new List<Filter>();
        public string DefaultValueStr;
        /// <summary>
        /// �� fullname=tableAlias.fieldName���ж�
        /// </summary>
        /// <param name="fullName"></param>
        /// <returns></returns>
        public Field GetFieldByFullName(string fullName)
        {
            for (int i = 0; i < FieldList.Count; i++)
            {
                if (fullName == FieldList[i].FullName)
                {
                    return FieldList[i];
                }
            }
            return null;
        }
        public Field GetFieldByName(string name)
        {
            for (int i = 0; i < FieldList.Count; i++)
            {
                if (name == FieldList[i].Name)
                {
                    return FieldList[i];
                }
            }
            return null;
        }
        public Table GetTableByName(string tablename)
        {
            for (int i = 0; i < TableList.Count; i++)
            {
                if (tablename == TableList[i].TableName)
                {
                    return TableList[i];
                }
            }
            return null;
        }
        /// <summary>
        /// ���������أ����table����ûָ������������������
        /// </summary>
        /// <param name="alias"></param>
        /// <returns></returns>
        public Table GetTableByAlias(string alias)
        {
            for (int i = 0; i < TableList.Count; i++)
            {
                if (TableList[i].Alias == "" && alias == TableList[i].TableName)
                {
                    return TableList[i];
                }
                if (alias == TableList[i].Alias)
                {
                    return TableList[i];
                }
            }
            return null;
        }
        public Filter GetFilterByName(string filtername)
        {
            foreach (Filter f in FilterList)
            {
                if (filtername == f.Name) return f;
            }
            return null;
        }
        /// <summary>
        /// ��ӱ��ų��Ѿ���ӵĺ�����,ͬʱ��ӹ�����
        /// </summary>
        /// <param name="list"></param>
        protected void AddTable(string tableAlias, List<Table> list)
        {
            if (tableAlias != MainTable.Alias && tableAlias != MainTable.TableName)
            {
                foreach (Table t in list)
                {
                    if (t.Alias == tableAlias) return;//�Ѿ�����
                }
                Table table = GetTableByAlias(tableAlias);
                if (table != null)
                {
                    //�ȼӹ�����
                    if (table.DependOnTable != null)
                    {
                        foreach (string dependT in table.DependOnTable)
                        {
                            AddTable(dependT, list);
                        }
                    }
                    list.Add(table);
                }
            }
        }
        protected void AddTable2Sql(StringBuilder sb, Table t, bool isFirst)
        {
            if (!isFirst)
            {
                if (t.Join == "")
                {
                    sb.Append(" join ");
                }
                else
                {
                    sb.Append(" ");
                    sb.Append(t.Join);
                    sb.Append(" ");
                }
            }
            if (t.Sql.Length > 0)
            {
                sb.Append("(");
                sb.Append(t.Sql);
                sb.Append(")");
            }
            else
            {
                sb.Append(t.TableName);
            }
            sb.Append(" ");
            if (t.Alias.Length > 0)
            {
                sb.Append("as ");
                sb.Append(t.Alias);
            }
            if (t.On.Length > 0)
            {
                sb.Append(" ");
                sb.Append(t.On);
            }
            sb.AppendLine();
        }
        //������dependonfield
        protected void AddField2Sql(StringBuilder sb, Field f)
        {
            if (sb.Length == 0)
            {
                sb.Append("select ");
            }
            else
            {
                sb.Append(",");
            }
            if (f.FunctionStr == "")
            {
                sb.Append(f.FullName);
            }
            else
            {
                sb.Append(f.FunctionStr);
            }
            if (f.Alias.Length > 0)
            {
                sb.Append(" as ");
                sb.Append(f.Alias);
            }
        }
        protected void AddSumField2Sql(StringBuilder sb, Field f)
        {
            if (sb.Length == 0)
            {
                sb.Append("select ");
            }
            else
            {
                sb.Append(",");
            }
            sb.Append("sum(");
            sb.Append(f.FullName);
            sb.Append(") as ");
            if (f.Alias.Length > 0)
            {
                sb.Append(f.Alias);
            }
            else
            {
                sb.Append(f.Name);
            }
        }
        protected void AddGroup2Sql(StringBuilder sb, Field f)
        {
            if (sb.Length == 0)
            {
                sb.Append(" group by ");
            }
            else
            {
                sb.Append(",");
            }
            sb.Append(f.FullName);
        }
        /// <summary>
        /// showField ʹ�� tableAlias.fieldName ����ʽ
        /// </summary>
        /// <param name="showField"></param>
        /// <returns></returns>
        public string BuildSql(string[] showField, FilterBuilder fb, string order)
        {
            //ȷ����Ҫ��ӵı�,����ʾ��Ŀ�Ͳ�ѯ�����б���
            List<Table> tablelist = new List<Table>();
            foreach (Table t in TableList)
            {
                if (t.IsDefault)
                {
                    AddTable(t.Alias, tablelist);//���ﲻֱ�� list.add(t),����Ҫͬʱ���dependOn��
                }
            }
            foreach (string s in showField)
            {
                AddField2Table(s, tablelist);
            }
            foreach (Filter filter in fb.FilterList)
            {
                AddField2Table(filter.FieldFullName, tablelist);
            }
            StringBuilder fie = new StringBuilder();

            foreach (string s in showField)
            {
                Field f = GetFieldByFullName(s);
                if (f != null)
                {
                    if (f.DependOnField != null)
                    {
                        foreach (string df in f.DependOnField)
                        {
                            Field f2 = GetFieldByFullName(df);
                            if (f2 != null)
                            {
                                AddField2Sql(fie, f2);
                            }
                        }
                    }
                    else
                    {
                        AddField2Sql(fie, f);
                    }
                }
            }

            StringBuilder tab = new StringBuilder(" from ");
            AddTable2Sql(tab, MainTable, true);
            foreach (Table table in tablelist)
            {
                AddTable2Sql(tab, table, false);
            }
            StringBuilder fullsb = new StringBuilder();
            fullsb.Append(fie.ToString());
            fullsb.Append(tab.ToString());
            fullsb.Append(fb.BuildWhere());
            if (order.Length > 0)
            {
                fullsb.Append(" order by ");
                fullsb.Append(order);
            }
            return fullsb.ToString();
        }
        public void AddField2Table(string fullFieldName, List<Table> tablelist)
        {
            Field f = GetFieldByFullName(fullFieldName);
            if (f != null)
            {
                if (f.DependOnField == null)// �Ǽ����ֶ�
                {
                    AddTable(f.TableAlias, tablelist);
                }
                else
                {
                    foreach (string df in f.DependOnField)
                    {
                        Field f2 = GetFieldByFullName(df);
                        if (f2 != null)
                        {
                            AddTable(f2.TableAlias, tablelist);
                        }
                    }
                }
            }
        }
        public string BuildGroupSql(string[] groupField, string[] sumField, FilterBuilder fb, string order)
        {
            //ȷ����Ҫ��ӵı�,����ʾ��Ŀ�Ͳ�ѯ�����б���
            List<Table> tablelist = new List<Table>();
            foreach (string s in groupField)
            {
                AddField2Table(s, tablelist);
            }
            foreach (string s in sumField)
            {
                AddField2Table(s, tablelist);
            }
            foreach (Filter filter in fb.FilterList)
            {
                AddTable(filter.FieldFullName, tablelist);
            }
            //׼��select �ֶ�
            StringBuilder fie = new StringBuilder();
            foreach (string s in groupField)
            {
                Field f = GetFieldByFullName(s);
                if (f != null)
                {
                    AddField2Sql(fie, f);//group�ֶβ�����depend on                     
                }
            }
            foreach (string s in sumField)
            {
                Field f = GetFieldByFullName(s);
                if (f != null)
                {
                    if (f.DependOnField != null)
                    {
                        foreach (string df in f.DependOnField)
                        {
                            Field f2 = GetFieldByFullName(df);
                            if (f2 != null)
                            {
                                AddSumField2Sql(fie, f2);
                            }
                        }
                    }
                    else
                    {
                        AddField2Sql(fie, f);
                    }
                }
            }
            // ׼�� groupby 
            StringBuilder groupStr = new StringBuilder();
            foreach (string s in groupField)
            {
                Field f = GetFieldByFullName(s);
                if (f != null)
                {
                    AddGroup2Sql(groupStr, f);
                }
            }

            StringBuilder tab = new StringBuilder(" from ");
            AddTable2Sql(tab, MainTable, true);
            foreach (Table table in tablelist)
            {
                AddTable2Sql(tab, table, false);
            }
            StringBuilder fullsb = new StringBuilder();
            fullsb.Append(fie.ToString());
            fullsb.Append(tab.ToString());
            fullsb.Append(fb.BuildWhere());
            fullsb.Append(groupStr.ToString());
            if (order.Length > 0)
            {
                fullsb.Append(" order by ");
                fullsb.Append(order);
            }
            return fullsb.ToString();
        }

        public string FormatOrder(string orderAlias, string sord)
        {
            foreach (Field f in FieldList)//���ж�alias ���Ƿ���ƥ��
            {
                if (f.Alias != null && f.Alias != "")
                {
                    if (orderAlias == f.Alias) return f.FullName + " " + sord;
                }
            }
            foreach (Field f in FieldList)//���ж�fieldname�Ƿ���ƥ��
            {
                if (orderAlias == f.FieldName)
                {
                    if (f.DependOnField != null)
                    {
                        string strorder = "";
                        //����ֶ�
                        foreach (string df in f.DependOnField)
                        {
                            Field f2 = GetFieldByFullName(df);
                            if (f2 != null)
                            {
                                if (strorder == "")
                                {
                                    strorder = f2.FullName + " " + sord;
                                }
                                else
                                {
                                    strorder = f2.FullName + " " + sord + "," + strorder;
                                }
                            }
                        }
                        return strorder;

                    }
                    else
                    {
                        return f.FullName + " " + sord;
                    }
                }
            }
            return "";
        }

        public string FormatOrder(string orderAlias, string sord, string customOrderBy)
        {
            if (String.IsNullOrEmpty(orderAlias))
                return customOrderBy;
            else
                return FormatOrder(orderAlias, sord);
        }

        protected string CookieName
        {
            get
            {
                return "QueryField" + HttpUtility.UrlEncode(Name);
            }
        }

        protected string CookieNameOrderBy
        {
            get
            {
                return "QueryFieldOrderBy" + HttpUtility.UrlEncode(Name);
            }
        }

        public string GetFieldList(HttpContextBase context)
        {
            if (context.Request.Cookies[CookieName] != null)
            {
                return (context.Request.Cookies[CookieName].Value);
            }
            if (defaultfield == null)
            {
                StringBuilder sb = new StringBuilder();
                foreach (Field f in FieldList)
                {
                    if (f.IsDefault || f.IsHidden || f.IsForce)
                    {
                        if (sb.Length > 0) sb.Append(",");
                        sb.Append(f.FullName);
                    }
                }
                defaultfield = sb.ToString();
            }
            //HttpCookie cookie = new HttpCookie(CookieName, defaultfield);
            //cookie.Expires = new DateTime(2100, 1, 1);
            //context.Response.Cookies.Add(cookie);

            return defaultfield;
        }
        /// <summary>
        /// ��ȡ�������� add by jinjie
        /// </summary>
        /// <returns></returns>
        public string GetFieldListName()
        {
            StringBuilder sb = new StringBuilder();
            foreach (Field f in FieldList)
            {
                if (!f.IsHidden)
                {
                    if (sb.Length > 0) sb.Append(",");
                    sb.Append(f.Name);
                }
            }
            defaultfield = sb.ToString();
            return defaultfield;
        }

        /// <summary>
        /// f.FieldName+"_"+f.TableAlias �����
        /// </summary>
        /// <returns></returns>
        public string GetFildListJsonName()
        {
            StringBuilder sb = new StringBuilder();
            foreach (Field f in FieldList)
            {
                if (!f.IsHidden)
                {
                    if (sb.Length > 0) sb.Append(",");
                    sb.Append(f.Name + "$" + f.FieldName + "_" + f.TableAlias);
                }
            }
            defaultfield = sb.ToString();
            return defaultfield;
        }

        public void SetFieldList(HttpContext context, string list)
        {
            HttpCookie cookie = new HttpCookie(CookieName, list);
            cookie.Expires = new DateTime(2100, 1, 1);
            context.Response.Cookies.Add(cookie);
        }

        public void SetOrderBy(HttpContext context, string orderBy)
        {
            HttpCookie cookie = new HttpCookie(CookieNameOrderBy, orderBy);
            cookie.Expires = new DateTime(2100, 1, 1);
            context.Response.Cookies.Add(cookie);
        }

        public string GetOrderBy(HttpContext context)
        {
            if (context.Request.Cookies[CookieNameOrderBy] != null)
            {
                return (context.Request.Cookies[CookieNameOrderBy].Value);
            }

            return null;
        }
        public string GetFieldList(HttpContext context)
        {
            if (context.Request.Cookies[CookieName] != null)
            {
                return (context.Request.Cookies[CookieName].Value);
            }
            if (defaultfield == null)
            {
                StringBuilder sb = new StringBuilder();
                foreach (Field f in FieldList)
                {
                    if (f.IsDefault || f.IsHidden || f.IsForce)
                    {
                        if (sb.Length > 0) sb.Append(",");
                        sb.Append(f.FullName);
                    }
                }
                defaultfield = sb.ToString();
            }
            //HttpCookie cookie = new HttpCookie(CookieName, defaultfield);
            //cookie.Expires = new DateTime(2100, 1, 1);
            //context.Response.Cookies.Add(cookie);

            return defaultfield;
        }
        public void GetFilterListFromRequest(List<Filter> list, HttpContext context)
        {

        }
        [NonSerialized]
        private string defaultfield = null;

    }
    public class Table
    {
        public string Name;
        public string TableName;
        public string Alias = "";
        public string Sql = "";
        public string On = "";
        /// <summary>
        /// join Ϊ ���� ��Ĭ�� join ,����ʹ��join��ʵ��ֵ
        /// </summary>
        public string Join = "";
        public bool IsDefault = false;
        public string[] DependOnTable;
    }
    public class Field
    {
        public string Name;
        public string FieldName;
        public string Alias = "";
        public string TableAlias = "";
        public bool IsDefault = false;
        public bool IsHidden = false;
        public bool IsForce = false;
        public bool Sortable = true;
        public string SumFormula = "";
        public string[] DependOnField;//�����ֶΣ������ room.fullname �����ĸ�ʽ�� �����ֶβ���Ҫ��tablealias
        public int width = 0;
        public string FunctionStr = "";//�� case when ����������ʹ��
        /// <summary>
        /// ��=�ַ��� ��������jqgrid ��fromatter :
        /// integer  , number , area , rate , exchangerate ,  currency , date 
        /// </summary>
        public string FieldType = "";
        /// <summary>
        /// �������ʡ�ԣ�ʡ�����fieldtype���Զ�ȡ
        /// </summary>
        public string Align = "";

        [NonSerialized]
        string fullName = "";
        public string FullName
        {
            get
            {
                if (fullName == "")
                {
                    if (TableAlias == "")
                    {
                        fullName = FieldName;
                    }
                    else
                    {
                        fullName = TableAlias + "." + FieldName;
                    }
                }
                return fullName;
            }
        }


    }
    /// <summary>
    /// ��ѯ������
    /// 
    /// </summary>
    public class Filter
    {
        /// <summary>
        /// �����������ã�����������鿴
        /// </summary>
        public string Title = "";
        /// <summary>
        /// ����������postdata���ύ�Ĳ�ѯ����ͬ����Ψһ
        /// </summary>
        public string Name;
        /// <summary>
        /// ��Ӧ�ֶ������ֶ������� contract.cid ���� alias.fieldname ����
        /// </summary>
        public string FieldFullName;
        /// <summary>
        /// ��ѯ���ͣ� �� ��������ѯ= P���ַ���= S ���ֶ���� =N 3��
        /// </summary>
        public string BuildType = "P";

        /// <summary>
        /// 
        /// </summary>
        public string ReferenceType = "";
        /// <summary>
        /// 
        /// </summary>
        public string ReferenceName = "";

        public bool IsIncludeAnd = false;

        /// <summary>
        /// ��ѯ���� 
        /// ��� type=P ,ʹ�� client.cname like {0} ������ʽ������ @projectid �滻{0}���ֵ�����ڴ���like�Ĳ���ʱ���Լ����%
        /// ��� type=S ,ʹ�� contract.projectid={0}�� client.cname like '{0}%' �����ĸ�ʽ ,����string.format �滻{0}���ֵ
        /// </summary>
        public string Sql;
        [NonSerialized]
        public string Value;

    }
    public class FilterBuilder
    {
        protected FilterBuilder() { }
        public static FilterBuilder GetBuilder(Query query, SqlParameterCollection parameterlist)
        {
            FilterBuilder fb = new FilterBuilder();
            fb.query = query;
            fb.parameterList = parameterlist;
            return fb;
        }
        private int filterAdded = 0;
        private bool whereStringAdded = false;
        StringBuilder sb = new StringBuilder();
        public Query query;
        public List<Filter> FilterList = new List<Filter>();
        public SqlParameterCollection parameterList;
        /// <summary>
        /// ��� Value="" ���� null ������ʵ�����
        /// </summary>
        /// <param name="filtername"></param>
        /// <param name="Value"></param>
        /// <returns></returns>
        public Filter AddFilter(string filtername, string Value)
        {
            if (Value == null || Value == "" || Value == "%%" || Value == "%") return null;
            Filter f = query.GetFilterByName(filtername);
            if (f != null)
            {
                f.Value = Value;
                FilterList.Add(f);
            }
            return f;
        }
        public Filter AddFilterLike(string filtername, string Value)
        {
            if (Value == null || Value == "" || Value == "%%" || Value == "%") return null;
            Filter f = query.GetFilterByName(filtername);
            if (f != null)
            {
                f.Value ="%"+ Value+"%";
                FilterList.Add(f);
            }
            return f;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <param name="oper"></param>
        /// <returns></returns>
        public Filter AddFilter(string filterName, string fieldName, string value, string oper, string andor, string leftParen, string rightParen)
        {
            if (value == null || value == "") return null;

            Filter f = new Filter();
            f.Name = filterName;
            f.FieldFullName = fieldName;

            if (oper.ToLower() == "in")
            {
                f.BuildType = "S";
                f.Sql = string.Format("{0} {1} {2}", fieldName, oper, value);
            }
            else
            {
                f.BuildType = "P";
                f.Sql = string.Format("{0} {1} @{2}", fieldName, oper, f.Name);
            }

            if (andor != "")
            {
                f.Sql = andor + " " + f.Sql;
                f.IsIncludeAnd = true;
            }

            f.Sql = leftParen + f.Sql + rightParen;

            f.Value = value;
            FilterList.Add(f);

            return f;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="conditions"></param>
        public void AddCustomFilter(string conditions)
        {
            string[] conditionList = conditions.Split('��');
            int i = 0;
            foreach (string condition in conditionList)
            {
                string[] arr = condition.Split('��');
                if (arr.Length < 3) continue;

                string andor = "";
                if (i > 0)
                {
                    if (arr.Length > 3)
                        andor = arr[3];
                }

                //������
                string leftParen = "";
                if (arr.Length > 4)
                    leftParen = arr[4];

                //������
                string rightParen = "";
                if (arr.Length > 5)
                    rightParen = arr[5];

                //������ѯ����ǰ���������
                if (i == 0)
                    leftParen += "(";
                if (i == conditionList.Length - 1)
                    rightParen += ")";

                Filter f = AddFilter("CustomField" + i.ToString(), arr[0], arr[2], arr[1], andor, leftParen, rightParen);

                i++;
            }
        }

        /// <summary>
        /// ����where ���,���� sqlParamater add �� paramaterList�С�
        /// </summary>
        /// <returns></returns>
        public string BuildWhere()
        {
            foreach (Filter f in FilterList)
            {
                sb.Append(" ");
                switch (f.BuildType.ToUpper())
                {
                    case "P":
                        filterAdded++;
                        if (filterAdded > 1 && !f.IsIncludeAnd)
                        {
                            sb.Append("and ");
                        }
                        string s = "@" + f.Name;
                        sb.Append(string.Format(f.Sql, s));
                        SqlParameter p = new SqlParameter(s, f.Value);
                        parameterList.Add(p);
                        break;
                    case "S":
                        filterAdded++;
                        if (filterAdded > 1 && !f.IsIncludeAnd)
                        {
                            sb.Append("and ");
                        }
                        sb.Append(string.Format(f.Sql, f.Value));
                        break;
                    case "N":
                    default:
                        break;
                }
            }
            if (whereStringAdded == false && filterAdded > 0)
            {
                sb.Insert(0, " where ");
            }
            return sb.ToString();
        }
        public void AddOtherFilter(string sql, params SqlParameter[] list)
        {

            if (whereStringAdded == false)
            {
                sb.Append(" where ");
                whereStringAdded = true;
                filterAdded++;
            }
            else
            {
                sb.Append(" and ");
            }
            sb.Append(sql);
            if (list != null)
            {
                parameterList.AddRange(list);
            }
        }
    }
    public class QueryBuilder
    {
        protected static Query GetQueryByFile(string filename)
        {

            string config = "";
            if (File.Exists(filename))
            {
                config = File.ReadAllText(filename, Encoding.GetEncoding("gb2312"));
            }
            Query q2 = (Query)Newtonsoft.Json.JsonConvert.DeserializeObject(config, typeof(Query));
            return q2;
        }
        Dictionary<string, Query> QueryCache = new Dictionary<string, Query>();

        public static Query GetQuery(string queryName)
        {
            if (HttpRuntime.Cache["Query" + queryName] == null)
            {
                string filename = Path.Combine(HttpRuntime.AppDomainAppPath, "app_data\\query\\" + queryName + ".txt");
                Query q = GetQueryByFile(filename);
                q.Name = queryName;//���ļ�����query������,�����ļ����������ʵ������
                if (q != null)
                {
                    System.Web.Caching.CacheDependency dep = new System.Web.Caching.CacheDependency(filename);
                    HttpRuntime.Cache.Add("Query" + queryName, q, dep, new DateTime(2100, 1, 1), System.Web.Caching.Cache.NoSlidingExpiration, System.Web.Caching.CacheItemPriority.Default, null);
                }
            }
            return (Query)HttpRuntime.Cache["Query" + queryName];
        }
    }
}
