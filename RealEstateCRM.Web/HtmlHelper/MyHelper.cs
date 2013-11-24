using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Text;
namespace RealEstateCRM.Web
{
    public static class UsedExtensions
    {
        public static MvcHtmlString MyDisplayFor<TModel, TValue>(this HtmlHelper<TModel> helper, Expression<Func<TModel, TValue>> expression, string text,int col)
        {
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, helper.ViewData);

            return MvcHtmlString.Create(string.Format("<td class='tdRight'>{0}</td><td colspan='{2}'>{1}</td>", metadata.DisplayName ?? metadata.PropertyName, helper.Encode(text),col));

        }
        public static MvcHtmlString MyDisplayFor<TModel, TValue>(this HtmlHelper<TModel> helper, Expression<Func<TModel, TValue>> expression, string text)
        {
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, helper.ViewData);

            return MvcHtmlString.Create(string.Format("<td class='tdRight'>{0}</td><td>{1}</td>", metadata.DisplayName??metadata.PropertyName, helper.Encode(text)));

        }
        public static MvcHtmlString MyDisplayFor<TModel, TValue>(this HtmlHelper<TModel> helper, Expression<Func<TModel, TValue>> expression)
        {
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, helper.ViewData);
            string text = "";
            if (metadata.Model == null)
            {

            }
            else if (metadata.Model.GetType() == typeof(DateTime))
            {
                if (metadata.PropertyName.Contains("Time"))
                {
                    text = ((DateTime)metadata.Model).ToString("yyyy/MM/dd hh:mm");
                }
                else
                {
                    text = BLL.Formatter.Date((DateTime)metadata.Model);
                }
            }
            else
            {
                text = metadata.SimpleDisplayText;
                //if (text.Contains("\n")) text = text.Replace("\n", "<br/>");
            }
            return MvcHtmlString.Create(string.Format("<td class='tdRight'>{0}</td><td>{1}</td>", metadata.DisplayName, helper.Encode(text).Replace("\n", "<br/>")));

        }
        public static MvcHtmlString MyDisplayFor<TModel, TValue>(this HtmlHelper<TModel> helper, Expression<Func<TModel, TValue>> expression, int colspan)
        {
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, helper.ViewData);
            string text = "";
            if (metadata.Model == null)
            {

            }
            else if (metadata.Model.GetType() == typeof(DateTime))
            {
                if (metadata.PropertyName.Contains("Time"))
                {
                    text = ((DateTime)metadata.Model).ToString();
                }
                else
                {
                    text = BLL.Formatter.Date((DateTime)metadata.Model);
                }
            }
            else
            {
                text = metadata.SimpleDisplayText;
                //if (text.Contains("\n")) text = text.Replace("\n", "<br/>");
            }
            return MvcHtmlString.Create(string.Format("<td class='tdRight'>{0}</td><td colspan='{2}'>{1}</td>", metadata.DisplayName, helper.Encode(text).Replace("\n", "<br/>"), colspan));

        }

        public static MvcHtmlString SearchLabel(this HtmlHelper helper, string name, string title)
        {
            return MvcHtmlString.Create(string.Format("<label for='{0}' class='len60'>{1}</label>", name, title));
        }
        public static MvcHtmlString MyTextFor<TModel, TValue>(this HtmlHelper<TModel> helper, Expression<Func<TModel, TValue>> expression)
        {
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, helper.ViewData);
            string fullName = ExpressionHelper.GetExpressionText(expression);
            string str = @"<div class='form-group'>
    <label for='{0}'>{1}</label>
    
    <input id='{0}' class='text-box single-line' type='text' value='{2}' name='{0}'{3} />{4}    
    {5}
   
</div>";
            MvcHtmlString m3 = ValidationExtensions.ValidationMessageFor<TModel, TValue>(helper, expression);
          string fullStr=  string.Format(str,fullName,metadata.DisplayName ?? metadata.PropertyName,metadata.Model
              ,metadata.IsRequired == true?string.Format(" data-val='true' data-val-required='{0} 字段是必需的'",metadata.DisplayName ?? metadata.PropertyName,metadata):""
                ,metadata.IsRequired == true?" <span style='color:red'>*</span>":"",m3.ToHtmlString()
                );
            return MvcHtmlString.Create(fullStr);
        }
        public static MvcHtmlString MyTextAreaFor<TModel, TValue>(this HtmlHelper<TModel> helper, Expression<Func<TModel, TValue>> expression,int row)
        {
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, helper.ViewData);
            MvcHtmlString m3 = ValidationExtensions.ValidationMessageFor<TModel, TValue>(helper, expression);
            string fullName = ExpressionHelper.GetExpressionText(expression);
            string str = @"<div class='form-group'>
    <label for='{0}'>{1}</label>
   
    <textarea id='{0}' class='form-control textMiddle' row='{5}' name='{0}'{3} >{2}</textarea>{4} {6}   
    
    
</div>";
            string fullStr = string.Format(str, fullName, metadata.DisplayName ?? metadata.PropertyName, metadata.Model
                , metadata.IsRequired == true ? string.Format(" data-val='true' data-val-required='{0} 字段是必需的'", metadata.DisplayName ?? metadata.PropertyName, metadata) : ""
                  , metadata.IsRequired == true ? " <span style='color:red'>*</span>" : ""
                  ,row     ,m3.ToHtmlString()             );
            return MvcHtmlString.Create(fullStr);
        }
        public static MvcHtmlString MyDateFor<TModel, TValue>(this HtmlHelper<TModel> helper, Expression<Func<TModel, TValue>> expression)
        {
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, helper.ViewData);
            MvcHtmlString m3 = ValidationExtensions.ValidationMessageFor<TModel, TValue>(helper, expression);
            string fullName = ExpressionHelper.GetExpressionText(expression);
            string str = @"<div class='form-group'>
    <label for='{0}'>{1}</label>
   
    <input id='{0}' class='form-control datepicker text150' type='text' value='{2}' name='{0}' {3} />{4}    
    {5}
   
</div>";
            string valueStr="";
            if (metadata.Model != null)
            {
                DateTime d = (DateTime)metadata.Model;
                if (d != DateTime.MinValue)
                {
                    valueStr = d.ToString("yyyy-MM-dd");
                }
            }
            string fullStr = string.Format(str, fullName, metadata.DisplayName ?? metadata.PropertyName, valueStr
              , metadata.IsRequired == true ? string.Format(" data-val='true' data-val-required='{0} 字段是必需的'", metadata.DisplayName ?? metadata.PropertyName, metadata) : ""
                , metadata.IsRequired == true ? " <span style='color:red'>*</span>" : "",m3.ToHtmlString()
                );
            return MvcHtmlString.Create(fullStr);
            
        }
        public static MvcHtmlString MyEnumFor<TModel, TValue>(this HtmlHelper<TModel> helper, Expression<Func<TModel, TValue>> expression, Type enumType)
        {
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, helper.ViewData);
            string fullName = ExpressionHelper.GetExpressionText(expression);
            string str = @"<div class='form-group'>
    <label control-label' for='{0}'>{1}</label>
   
    {5} {4}    
    <span class='field-validation-valid' data-valmsg-replace='true' data-valmsg-for='{0}'></span>
   
</div>";
           
            StringBuilder sb = new StringBuilder();
            
                foreach (var val in Enum.GetValues(enumType))
                {
                    sb.Append(InputExtensions.RadioButtonFor(helper, expression, val,new {@class="form-control"}));
                    sb.Append(val);
                    sb.Append("&nbsp;&nbsp;");
                }
            string fullStr = string.Format(str, fullName, metadata.DisplayName ?? metadata.PropertyName, metadata.Model
                , metadata.IsRequired == true ? string.Format(" data-val='true' data-val-required='{0} 字段是必需的'", metadata.DisplayName ?? metadata.PropertyName, metadata) : ""
                  , metadata.IsRequired == true ? " <span style='color:red'>*</span>" : ""
                  ,sb.ToString()
                  );
            return MvcHtmlString.Create(fullStr);

        }
        /// <summary>
        /// 支持 string int enum 3类型的属性
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="helper"></param>
        /// <param name="expression"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static MvcHtmlString MyIntSelectFor<TModel, TValue>(this HtmlHelper<TModel> helper, Expression<Func<TModel, TValue>> expression, Dictionary<int,string> options,bool IsAppendBlank)
        {
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, helper.ViewData);
            string fullName = ExpressionHelper.GetExpressionText(expression);
            string str = @"<div class='form-group'>
    <label control-label' for='{0}'>{1}</label>
   <select id='{0}' name='{0}'>{3}
</select>
    {5} {4}    
    <span class='field-validation-valid' data-valmsg-replace='true' data-valmsg-for='{0}'></span>
   
</div>";
          
            StringBuilder sb = new StringBuilder();
            foreach(var val in options)
            {
                sb.AppendLine(string.Format("<option value='{0}' {3}>{1}</option>", val.Key,Convert.ToInt32(metadata.Model)==val.Key?"selected":
                    ""));
            }
            string fullStr = string.Format(str, fullName, metadata.DisplayName ?? metadata.PropertyName, metadata.Model
                , metadata.IsRequired == true ? string.Format(" data-val='true' data-val-required='{0} 字段是必需的'", metadata.DisplayName ?? metadata.PropertyName, metadata) : ""
                  , metadata.IsRequired == true ? " <span style='color:red'>*</span>" : ""
                  ,sb.ToString()
                  );
            return MvcHtmlString.Create(fullStr);
        }
        public static MvcHtmlString MyStringSelectFor<TModel, TValue>(this HtmlHelper<TModel> helper, Expression<Func<TModel, TValue>> expression, List<string> options)
        {
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, helper.ViewData);
            MvcHtmlString m3 = ValidationExtensions.ValidationMessageFor<TModel, TValue>(helper, expression);
            string fullName = ExpressionHelper.GetExpressionText(expression);
            string str = @"<div class='form-group'>
    <label control-label' for='{0}'>{1}</label>
   <select id='{0}' name='{0}'>{2}</select>{3}
  {4}
   
</div>";

            StringBuilder sb = new StringBuilder();
            foreach (var val in options)
            {
                sb.AppendLine(string.Format("<option value='{0}' {1}>{0}</option>", val,metadata.Model==null?"": metadata.Model.ToString() == val ? "selected='selected'" :""));
            }
            string fullStr = string.Format(str
                , fullName
                , metadata.DisplayName ?? metadata.PropertyName
                , sb.ToString()
                  , metadata.IsRequired == true ? " <span style='color:red'>*</span>" : ""
                  ,m3.ToHtmlString()
                  );
            return MvcHtmlString.Create(fullStr);
        }
        public static MvcHtmlString MyDropdown(this HtmlHelper helper, string name, List<SelectListItem> list)
        {

            return MyDropdown(helper, name, list, null);
        }
        public static MvcHtmlString MyDropdown(this HtmlHelper helper, string name, List<SelectListItem> list, string defaultValue)
        {

            if (list.Count == 0 || list.Count > 0 && list[0].Value != "") list.Insert(0, new SelectListItem() { Value = "", Text = "----" });
            if (!string.IsNullOrEmpty(defaultValue))
            {
                foreach (var selectListItem in list)
                {
                    if (selectListItem.Value == defaultValue) selectListItem.Selected = true;
                }
            }
            MvcHtmlString m2 = SelectExtensions.DropDownList(helper, name, list);
            return MvcHtmlString.Create(m2.ToHtmlString());

        }
        
        public static MvcHtmlString MyDropdownFor<TModel, TValue>(this HtmlHelper<TModel> helper, Expression<Func<TModel, TValue>> expression, List<SelectListItem> list, string defaultValue="")
        {

            if (list.Count == 0 || list.Count > 0 && list[0].Value != "") list.Insert(0, new SelectListItem() { Value = "", Text = "----" });
            if (!string.IsNullOrEmpty(defaultValue))
            {
                foreach (var selectListItem in list)
                {
                    if (selectListItem.Value == defaultValue) selectListItem.Selected = true;
                }
            }
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, helper.ViewData);
            MvcHtmlString m3 = ValidationExtensions.ValidationMessageFor<TModel, TValue>(helper, expression);
            string fullName = ExpressionHelper.GetExpressionText(expression);
            string str = @"<div class='form-group'>
    <label control-label' for='{0}'>{1}</label>{2} {3} 
    {4}
   
</div>";
            MvcHtmlString m2 = SelectExtensions.DropDownList(helper, fullName, list);
            string fullStr = string.Format(str, fullName, metadata.DisplayName ?? metadata.PropertyName, m2.ToHtmlString()
                 , metadata.IsRequired == true ? " <span style='color:red'>*</span>" : ""
            ,m3.ToHtmlString()
             
              );
            return MvcHtmlString.Create(fullStr);

        }
    }
    public static class MyExtensions
    {
        public static MvcHtmlString ToJson(this HtmlHelper helper, object obj)
        {
            Web.BLL.JsonFormatter formatter = new BLL.JsonFormatter(Newtonsoft.Json.JsonConvert.SerializeObject(obj));
            //return formatter.Format();
            return MvcHtmlString.Create(formatter.Format());
        }
        public static MvcHtmlString MyValidation(this HtmlHelper helper, string name)
        {
            return MvcHtmlString.Create(string.Format("<span class=\"field-validation-valid\" data-valmsg-for=\"{0}\" data-valmsg-replace=\"true\"></span>", name));
        }
        public static MvcHtmlString MyInt(this HtmlHelper helper, string name, int? Value)
        {
            string s = "<input id='{0}' name='{0}' value='{1}' onkeyup=\"this.value=this.value.replace(/[^0-9]/g,'')\" />";

            return MvcHtmlString.Create(string.Format(s, name, Value == null ? "" : ((int)Value).ToString()));

        }
        public static MvcHtmlString MyDecimal(this HtmlHelper helper, string name, decimal? Value)
        {
            string s = "<input class=\"text-box single-line\" data-val=\"true\" data-val-number=\"请输入数值\" data-val-required=\"请输入数值。\" id=\"{0}\" name=\"{0}\"  type=\"text\" value=\"{1}\" onkeyup=\"this.value=this.value.replace(/[^0-9.]/g,'')\"/>";
            return MvcHtmlString.Create(string.Format(s, name, Value == null ? "" : ((decimal)Value).ToString()));

        }
        //onkeyup="this.value=this.value.replace(/[^0-9]/g,'')"
        public static MvcHtmlString MyTextForTR<TModel, TValue>(this HtmlHelper<TModel> helper, Expression<Func<TModel, TValue>> expression, bool isReadonly, string remark)
        {

            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, helper.ViewData);
            string isreadonly = "";
            if (isReadonly)
            {
                isreadonly = "readonly='readonly'";
                //return MvcHtmlString.Create(string.Format("<td class='tdRight'>{0}</td><td>{1}</td>", metadata.DisplayName, helper.Encode(metadata.SimpleDisplayText)));
            }
            MvcHtmlString m1 = LabelExtensions.LabelFor<TModel, TValue>(helper, expression);
            MvcHtmlString m2 = EditorExtensions.EditorFor<TModel, TValue>(helper, expression, isreadonly);
            MvcHtmlString m3 = ValidationExtensions.ValidationMessageFor<TModel, TValue>(helper, expression);
            string isrequest = "";
            if (m3 != null && metadata.IsRequired == true) isrequest = "<span style='color:red'>*</span>";
            return MvcHtmlString.Create(string.Format("<td class='tdRight'>{0}</td><td>{1} {3} {2} {4}</td>", m1.ToHtmlString(), m2.ToHtmlString(), m3 == null ? "" : m3.ToHtmlString(), isrequest, remark));

        }
        public static MvcHtmlString MyTextForTR<TModel, TValue>(this HtmlHelper<TModel> helper, Expression<Func<TModel, TValue>> expression, bool isReadonly)
        {
            return MyTextForTR(helper, expression, isReadonly, "");
        }
        public static MvcHtmlString MyTextForTR<TModel, TValue>(this HtmlHelper<TModel> helper, Expression<Func<TModel, TValue>> expression, string remark)
        {
            return MyTextForTR(helper, expression, false, remark);
        }
        public static MvcHtmlString MyTextForTR<TModel, TValue>(this HtmlHelper<TModel> helper, Expression<Func<TModel, TValue>> expression)
        {
            return MyTextForTR(helper, expression, false);
        }
        public static MvcHtmlString MyTextAreaFor<TModel, TValue>(this HtmlHelper<TModel> helper, Expression<Func<TModel, TValue>> expression, bool isReadonly, string remark)
        {

            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, helper.ViewData);
            string isreadonly = "";
            if (isReadonly)
            {
                isreadonly = "readonly='readonly'";
                //return MvcHtmlString.Create(string.Format("<td class='tdRight'>{0}</td><td>{1}</td>", metadata.DisplayName, helper.Encode(metadata.SimpleDisplayText)));
            }
            MvcHtmlString m1 = LabelExtensions.LabelFor<TModel, TValue>(helper, expression);
            MvcHtmlString m2 = TextAreaExtensions.TextAreaFor<TModel, TValue>(helper, expression, isreadonly);
            MvcHtmlString m3 = ValidationExtensions.ValidationMessageFor<TModel, TValue>(helper, expression);
            string isrequest = "";
            if (m3 != null && metadata.IsRequired == true) isrequest = "<span style='color:red'>*</span>";
            return MvcHtmlString.Create(string.Format("<td class='tdRight'>{0}</td><td>{1} {3} {2} {4}</td>", m1.ToHtmlString(), m2.ToHtmlString(), m3 == null ? "" : m3.ToHtmlString(), isrequest, remark));

        }
        public static MvcHtmlString MyEnumFor<TModel, TValue>(this HtmlHelper<TModel> helper, Expression<Func<TModel, TValue>> expression, Type enumType, bool isReadonly)
        {
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, helper.ViewData);
            StringBuilder sb = new StringBuilder();
            sb.Append(string.Format("<td class='tdRight'>{0}</td>", LabelExtensions.LabelFor<TModel, TValue>(helper, expression).ToHtmlString()));
            sb.Append("<td>");
            if (isReadonly)
            {
                sb.Append(metadata.SimpleDisplayText);
                //sb.Append(helper.Encode(Enum.GetName(enumType, int.Parse(metadata.SimpleDisplayText))));
            }
            else
            {
                foreach (var val in Enum.GetValues(enumType))
                {
                    sb.Append(InputExtensions.RadioButtonFor(helper, expression, val));
                    sb.Append(val);
                }
            }
            sb.Append("</td>");
            return MvcHtmlString.Create(sb.ToString());
        }
        
        //以下两个函数，处理model里用int来表示enum字段的情况 ，为ef不支持enum时的遗留代码
        public static MvcHtmlString MyEnumIntForTR<TModel, TValue>(this HtmlHelper<TModel> helper, Expression<Func<TModel, TValue>> expression, Type enumType, bool isReadonly)
        {
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, helper.ViewData);
            StringBuilder sb = new StringBuilder();
            sb.Append(string.Format("<td class='tdRight'>{0}</td>", LabelExtensions.LabelFor<TModel, TValue>(helper, expression).ToHtmlString()));
            sb.Append("<td>");
            if (isReadonly)
            {
                sb.Append(helper.Encode(Enum.GetName(enumType, int.Parse(metadata.SimpleDisplayText))));
            }
            else
            {
                foreach (var val in Enum.GetValues(enumType))
                {
                    sb.Append(InputExtensions.RadioButtonFor(helper, expression, val));
                    sb.Append(val);
                }
            }
            sb.Append("</td>");
            return MvcHtmlString.Create(sb.ToString());
        }
        public static MvcHtmlString MyEnumIntFor<TModel, TValue>(this HtmlHelper<TModel> helper, Expression<Func<TModel, TValue>> expression, Type enumType, bool isReadonly)
        {
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, helper.ViewData);
            StringBuilder sb = new StringBuilder();
            if (isReadonly)
            {
                sb.Append(helper.Encode(Enum.GetName(enumType, int.Parse(metadata.SimpleDisplayText))));
            }
            else
            {
                foreach (string s in Enum.GetNames(enumType))
                {

                    sb.Append(InputExtensions.RadioButtonFor(helper, expression, (int)Enum.Parse(enumType, s)));
                    sb.Append(s);
                    sb.Append("&nbsp;&nbsp;");

                }
            }

            return MvcHtmlString.Create(sb.ToString());
        }
        public static MvcHtmlString MyDateForTr<TModel, TValue>(this HtmlHelper<TModel> helper, Expression<Func<TModel, TValue>> expression, bool isReadonly)
        {
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, helper.ViewData);
            StringBuilder sb = new StringBuilder();
            sb.Append(string.Format("<td class='tdRight'>{0}</td>", LabelExtensions.LabelFor<TModel, TValue>(helper, expression).ToHtmlString()));

            sb.Append("<td>");
            if (isReadonly)
            {
                sb.Append(helper.Encode(((DateTime)metadata.Model).ToString("yyyy-MM-dd")));
            }
            else
            {
                TagBuilder tagBuilder = new TagBuilder("input");
                tagBuilder.MergeAttribute("class", "text100 datepicker");
                tagBuilder.MergeAttribute("type", "text");
                string name = ExpressionHelper.GetExpressionText(expression);
                tagBuilder.MergeAttribute("name", name);
                string fullname = helper.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(name);
                tagBuilder.MergeAttribute("id", fullname);
                tagBuilder.MergeAttribute("value", metadata.Model == null ? "" : ((DateTime)metadata.Model).ToString("yyyy-MM-dd"));
                sb.Append(tagBuilder.ToString(TagRenderMode.Normal));
                if (metadata.IsRequired == true)
                {
                    sb.Append("<span style='color:red'>*</span>");
                }
                MvcHtmlString m3 = ValidationExtensions.ValidationMessageFor<TModel, TValue>(helper, expression);

                sb.Append(m3 == null ? "" : m3.ToHtmlString());
            }
            sb.Append("</td>");
            return MvcHtmlString.Create(sb.ToString());
        }
       
        public static MvcHtmlString MyDropdownForTR<TModel, TValue>(this HtmlHelper<TModel> helper, Expression<Func<TModel, TValue>> expression, bool isReadonly, List<SelectListItem> list)
        {
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, helper.ViewData);
            if (isReadonly)
            {
                return MvcHtmlString.Create(string.Format("<td class='tdRight'>{0}</td><td>{1}</td>", metadata.DisplayName, helper.Encode(metadata.SimpleDisplayText)));
            }
            MvcHtmlString m1 = LabelExtensions.LabelFor<TModel, TValue>(helper, expression);
            if (list == null) list = new List<SelectListItem>();
            if (list.Count > 0 && list[0].Value != "") list.Insert(0, new SelectListItem() { Value = "", Text = "----" });
            MvcHtmlString m2 = SelectExtensions.DropDownListFor<TModel, TValue>(helper, expression, list);
            MvcHtmlString m3 = ValidationExtensions.ValidationMessageFor<TModel, TValue>(helper, expression);
            string isrequest = "";
            if (metadata.IsRequired == true) isrequest = "<span style='color:red'>*</span>";
            return MvcHtmlString.Create(string.Format("<td class='tdRight'>{0}</td><td>{1} {3} {2}</td>", m1.ToHtmlString(), m2.ToHtmlString(), m3 == null ? "" : m3.ToHtmlString(), isrequest));

        }
        

        //static List<SelectListItem> GetEnumSelectList(int val, Type enumType)
        //{
        //    List<SelectListItem> list = new List<SelectListItem>();
        //    foreach (string s in Enum.GetNames(enumType))
        //    {
        //        SelectListItem item = new SelectListItem();
        //        item.Text = s;
        //        item.Value = ((int)Enum.Parse(enumType, s)).ToString();
        //        list.Add(item);
        //    }
        //    return list;
        //}
        public static MvcHtmlString MyRadioBoxFor<TModel, TValue>(this HtmlHelper<TModel> helper, Expression<Func<TModel, TValue>> expression, bool isReadonly, List<SelectListItem> list)
        {
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, helper.ViewData);
            StringBuilder sb = new StringBuilder();
            sb.Append(string.Format("<td class='tdRight'>{0}</td>", LabelExtensions.LabelFor<TModel, TValue>(helper, expression).ToHtmlString()));
            sb.Append("<td>");
            if (isReadonly)
            {
                foreach (SelectListItem item in list)
                {
                    if (metadata.SimpleDisplayText == item.Value)
                    {
                        sb.Append(item.Text); break;
                    }
                }
            }
            else
            {
                foreach (SelectListItem item in list)
                {

                    sb.Append(InputExtensions.RadioButtonFor(helper, expression, item.Value));
                    sb.Append(item.Text);
                }
            }
            sb.Append("</td>");
            return MvcHtmlString.Create(sb.ToString());
        }
        public static MvcHtmlString MyBoolFor<TModel, TValue>(this HtmlHelper<TModel> helper, Expression<Func<TModel, TValue>> expression, bool isReadonly)
        {
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, helper.ViewData);
            StringBuilder sb = new StringBuilder();
            sb.Append(string.Format("<td class='tdRight'>{0}</td>", LabelExtensions.LabelFor<TModel, TValue>(helper, expression).ToHtmlString()));
            sb.Append("<td>");
            if (isReadonly)
            {
                if (metadata.SimpleDisplayText == "1")
                {
                    sb.Append("是");
                }
                else
                {
                    sb.Append("否");
                }

            }
            else
            {
                sb.Append(InputExtensions.RadioButtonFor(helper, expression, 1));
                sb.Append("是");
                sb.Append(InputExtensions.RadioButtonFor(helper, expression, 0));
                sb.Append("否");
            }
            sb.Append("</td>");
            return MvcHtmlString.Create(sb.ToString());
        }
        public static MvcHtmlString MyDate(this HtmlHelper helper, string name, string value)
        {
            string temp =
                "<input class=\"text100 datepicker\" id=\"{0}\" name=\"{0}\" type=\"text\" value=\"{1}\" />";
            return MvcHtmlString.Create(string.Format(temp, name, value));
        }
        /*-----------------------------------------------------------------------------*/
        public static MvcHtmlString ShortInput(this HtmlHelper helper, string name, string label)
        {
            string temp =
                "<label class=\"len60 right\" for=\"{0}\">{1}</label><input class=\"text60\" id=\"{0}\" name=\"{0}\" type=\"text\" value=\"{2}\" />";
            return MvcHtmlString.Create(string.Format(temp, name, label, helper.ViewData[name] ?? ""));
        }
        public static MvcHtmlString SearchDate(this HtmlHelper helper, string name, string label)
        {
            string temp =
                "<label class=\"len60 right\" for=\"Date{0}\">{1}</label><input class=\"text60 datepicker\" id=\"Date{0}\" name=\"Date{0}\" type=\"text\" {2} />";
            return MvcHtmlString.Create(string.Format(temp, name, label
                , helper.ViewData["Date" + name] == null ? "" : string.Format("value='{0}'", helper.ViewData["Date" + name])));

        }
        public static MvcHtmlString SearchDateRange(this HtmlHelper helper, string name, string label)
        {
            string temp =
                "<label class=\"len60 right\" for=\"DateFrom{0}\">{1}</label><input class=\"text60 datepicker\" id=\"DateFrom{0}\" name=\"DateFrom{0}\" type=\"text\" {2} /> - <input class=\"text60 datepicker\" id=\"DateTo{0}\" name=\"DateTo{0}\" type=\"text\" {3} />";
            return MvcHtmlString.Create(string.Format(temp, name, label
                , helper.ViewData["DateFrom" + name] == null ? "" : string.Format("value='{0}'", helper.ViewData["DateFrom" + name])
                , helper.ViewData["DateTo" + name] == null ? "" : string.Format("value='{0}'", helper.ViewData["DateTo" + name])));

        }
        public static MvcHtmlString SearchEnum(this HtmlHelper helper, string name, string label, Type enumType)
        {
            string temp = "<input type='checkbox' name='{0}' value='{1}' {3} /><span class='len60'>{2}</span>";
            StringBuilder sb = new StringBuilder(label + ": ");
            string[] values = null;
            if (helper.ViewData["Enum" + name] != null)
            {
                values = (helper.ViewData["Enum" + name] as string).Split(',');
            }
            foreach (var val in Enum.GetValues(enumType))
            {
                string strChecked = "";
                if (values != null)
                {
                    foreach (var value in values)
                    {
                        if (value == ((int)val).ToString()) strChecked = "checked='checked'";
                    }
                }
                sb.Append(string.Format(temp, name, (int)val, val, strChecked));
            }
            return MvcHtmlString.Create(sb.ToString());

        }
        public static MvcHtmlString RendYear(this HtmlHelper helper, string id)
        {
            string s = "<select id='{0}' name='{0}'>";
            StringBuilder sb = new StringBuilder();
            sb.Append(string.Format(s, id));
            int year = DateTime.Today.Year;
            for (int i = year - 4; i < year; i++)
            {
                sb.Append("<option value='"); sb.Append(i); sb.Append("'>"); sb.Append(i); sb.Append("</option>");
            }
            sb.Append("<option value='"); sb.Append(year); sb.Append("' selected>"); sb.Append(year); sb.Append("</option>");
            year++;
            sb.Append("<option value='"); sb.Append(year); sb.Append("'>"); sb.Append(year); sb.Append("</option>");
            sb.Append("</select>");
            return MvcHtmlString.Create(sb.ToString());
        }
        public static MvcHtmlString RendMonth(this HtmlHelper helper, string id)
        {
            string s = "<select id='{0}' name='{0}'>";
            StringBuilder sb = new StringBuilder();
            sb.Append(string.Format(s, id));
            int month = DateTime.Today.Month;
            for (int i = 1; i <= 12; i++)
            {
                string s1 = "";
                if (i == month) s1 = " selected";
                sb.Append(string.Format("<option value={0}{1}>{0}</option>", i, s1));
            }
            sb.Append("</select>");
            return MvcHtmlString.Create(sb.ToString());
        }
        public static MvcHtmlString RendSeason(this HtmlHelper helper, string id)
        {
            string s = "<select id='{0}' name='{0}'>";
            StringBuilder sb = new StringBuilder();
            sb.Append(string.Format(s, id));
            int month = DateTime.Today.Month;
            for (int i = 1; i <= 4; i++)
            {
                string s1 = "";
                if (i * 3 >= month && (i - 1) * 3 < month) s1 = " selected";
                sb.Append(string.Format("<option value={0}{1}>{0}</option>", i, s1));
            }
            sb.Append("</select>");
            return MvcHtmlString.Create(sb.ToString());
        }
    }
    
}