using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Text;
using System.Web;

namespace Golfler.Helper
{
    public static class HelperAddOn
    {
        public static string RadioButtonList(this HtmlHelper helper, string name, IEnumerable<string> items)
        {
            var selectList = new SelectList(items);
            return helper.RadioButtonList(name, selectList);
        }

        public static string RadioButtonList(this HtmlHelper helper, string name, IEnumerable<SelectListItem> items)
        {
            TagBuilder tableTag = new TagBuilder("table");
            tableTag.AddCssClass("radio-main");

            var trTag = new TagBuilder("tr");
            foreach (var item in items)
            {
                var tdTag = new TagBuilder("td");
                var rbValue = item.Value ?? item.Text;
                var rbName = name + "_" + rbValue;
                var radioTag = helper.RadioButton(rbName, rbValue, item.Selected, new { name = name });

                var labelTag = new TagBuilder("label");
                labelTag.MergeAttribute("for", rbName);
                labelTag.MergeAttribute("id", rbName + "_label");
                labelTag.InnerHtml = rbValue;

                tdTag.InnerHtml = radioTag.ToString() + labelTag.ToString();

                trTag.InnerHtml += tdTag.ToString();
            }
            tableTag.InnerHtml = trTag.ToString();

            return tableTag.ToString();
        }
        public static MvcHtmlString RadioButtonListFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, IEnumerable<SelectListItem> items)
        {
            return htmlHelper.RadioHelper(ExpressionHelper.GetExpressionText(expression), items, null);
        }

        /// <summary>
        /// Returns a file input element by using the specified HTML helper, the name of the form field, and the HTML attributes.
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="htmlHelper">The HTML helper instance that this method extends.</param>
        /// <param name="expression">The expression.</param>
        /// <param name="htmlAttributes">An object that contains the HTML attributes for the element. The attributes are retrieved through reflection by examining the properties of the object. The object is typically created by using object initializer syntax.</param>
        /// <returns>
        /// An input element that has its type attribute set to "file".
        /// </returns>
        public static MvcHtmlString RadioButtonListFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, IEnumerable<SelectListItem> items, object htmlAttributes)
        {
            return htmlHelper.RadioHelper(ExpressionHelper.GetExpressionText(expression), items, HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        /// <summary>
        /// Returns a file input element by using the specified HTML helper, the name of the form field, and the HTML attributes.
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="htmlHelper">The HTML helper instance that this method extends.</param>
        /// <param name="expression">The expression.</param>
        /// <param name="htmlAttributes">An object that contains the HTML attributes for the element. The attributes are retrieved through reflection by examining the properties of the object. The object is typically created by using object initializer syntax.</param>
        /// <returns>
        /// An input element that has its type attribute set to "file".
        /// </returns>
        public static MvcHtmlString RadioButtonListFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, IEnumerable<SelectListItem> items, IDictionary<string, object> htmlAttributes)
        {
            return htmlHelper.RadioHelper(ExpressionHelper.GetExpressionText(expression), items, htmlAttributes);
        }
        public static MvcHtmlString RadioButtonListDivFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, IEnumerable<SelectListItem> items)
        {
            return htmlHelper.RadioHelperDiv(ExpressionHelper.GetExpressionText(expression), items, null);
        }

        /// <summary>
        /// Returns a file input element by using the specified HTML helper, the name of the form field, and the HTML attributes.
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="htmlHelper">The HTML helper instance that this method extends.</param>
        /// <param name="expression">The expression.</param>
        /// <param name="htmlAttributes">An object that contains the HTML attributes for the element. The attributes are retrieved through reflection by examining the properties of the object. The object is typically created by using object initializer syntax.</param>
        /// <returns>
        /// An input element that has its type attribute set to "file".
        /// </returns>
        public static MvcHtmlString RadioButtonListDivFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, IEnumerable<SelectListItem> items, object htmlAttributes)
        {
            return htmlHelper.RadioHelperDiv(ExpressionHelper.GetExpressionText(expression), items, HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
        }
        public static MvcHtmlString RadioButtonListDivDesignFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, IEnumerable<SelectListItem> items, object htmlAttributes)
        {
            return htmlHelper.RadioHelperDivDesign(ExpressionHelper.GetExpressionText(expression), items, HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
        }
        /// <summary>
        /// Returns a file input element by using the specified HTML helper, the name of the form field, and the HTML attributes.
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="htmlHelper">The HTML helper instance that this method extends.</param>
        /// <param name="expression">The expression.</param>
        /// <param name="htmlAttributes">An object that contains the HTML attributes for the element. The attributes are retrieved through reflection by examining the properties of the object. The object is typically created by using object initializer syntax.</param>
        /// <returns>
        /// An input element that has its type attribute set to "file".
        /// </returns>
        public static MvcHtmlString RadioButtonListDivFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, IEnumerable<SelectListItem> items, IDictionary<string, object> htmlAttributes)
        {
            return htmlHelper.RadioHelperDiv(ExpressionHelper.GetExpressionText(expression), items, htmlAttributes);
        }
        private static MvcHtmlString RadioHelper(this HtmlHelper htmlHelper, string name, IEnumerable<SelectListItem> items, IDictionary<string, object> htmlAttributes)
        {
            string fieldName = htmlHelper.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(name);
            var tagBuilder = new TagBuilder("input");
            tagBuilder.MergeAttributes(htmlAttributes);
            tagBuilder.MergeAttribute("name", fieldName, true);
            tagBuilder.GenerateId(fieldName);

            TagBuilder tableTag = new TagBuilder("table");
            tableTag.AddCssClass("radio-main");


            foreach (var item in items)
            {
                var trTag = new TagBuilder("tr");
                var tdTag = new TagBuilder("td");
                tdTag.MergeAttribute("align", "right");
                tdTag.MergeAttribute("width", "20px");
                var rbValue = item.Value ?? item.Text;
                var rbName = name;// +"_" + rbValue;
                //var radioTag = htmlHelper.RadioButton(rbName, rbValue, item.Selected, new { name = name, @class = "inp_new", @onclick = "getdetails(" + item.Value + ")" });
                var radioTag = htmlHelper.RadioButton(rbName, rbValue, item.Selected, new { name = name, @class = "inp_new" });
                tdTag.InnerHtml = radioTag.ToString();

                var tdTag1 = new TagBuilder("td");
                tdTag1.MergeAttribute("align", "left");
                tdTag1.MergeAttribute("valign", "top");
                var labelTag = new TagBuilder("label");
                labelTag.MergeAttribute("for", rbName);
                labelTag.AddCssClass("lab1");
                labelTag.MergeAttribute("id", rbName + "_label");
                labelTag.InnerHtml = item.Text;

                tdTag1.InnerHtml = labelTag.ToString();

                trTag.InnerHtml += tdTag.ToString() + tdTag1.ToString();
                tableTag.InnerHtml += trTag.ToString();
            }


            //return tableTag.ToString();

            //ModelState modelState;
            //if (htmlHelper.ViewData.ModelState.TryGetValue(name, out modelState))
            //{
            //    if (modelState.Errors.Count > 0)
            //    {
            //        tagBuilder.AddCssClass(HtmlHelper.ValidationInputCssClassName);
            //    }
            //}
            var s = MvcHtmlString.Create(tableTag.ToString() + "\r");
            return s;//MvcHtmlString.Create(tableTag.ToString(TagRenderMode.SelfClosing));
        }

        private static MvcHtmlString RadioHelperDiv(this HtmlHelper htmlHelper, string name, IEnumerable<SelectListItem> items, IDictionary<string, object> htmlAttributes)
        {
            string fieldName = htmlHelper.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(name);
            var tagBuilder = new TagBuilder("input");
            tagBuilder.MergeAttributes(htmlAttributes);
            tagBuilder.MergeAttribute("name", fieldName, true);
            tagBuilder.GenerateId(fieldName);

            TagBuilder divTag = new TagBuilder("div");
            divTag.AddCssClass("radio-main");


            foreach (var item in items)
            {
                var idivTag = new TagBuilder("div");
                idivTag.AddCssClass("radiioImage");

                var labelTag = new TagBuilder("label");
                var rbValue = item.Value ?? item.Text;
                var rbName = name;
                var radioTag = htmlHelper.RadioButton(rbName, rbValue, item.Selected, new { name = name, @class = "inp_new" });
                labelTag.InnerHtml = radioTag.ToString();
                labelTag.InnerHtml += item.Text;

                idivTag.InnerHtml = labelTag.ToString();

                var clrDiv = new TagBuilder("div");
                clrDiv.AddCssClass("clear");
                idivTag.InnerHtml += clrDiv.ToString();

                divTag.InnerHtml += idivTag.ToString();
            }
            var s = MvcHtmlString.Create(divTag.ToString() + "\r");
            return s;
        }


        private static MvcHtmlString RadioHelperDivDesign(this HtmlHelper htmlHelper, string name, IEnumerable<SelectListItem> items, IDictionary<string, object> htmlAttributes)
        {
            string fieldName = htmlHelper.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(name);
            var tagBuilder = new TagBuilder("input");
            tagBuilder.MergeAttributes(htmlAttributes);
            tagBuilder.MergeAttribute("name", fieldName, true);
            tagBuilder.GenerateId(fieldName);

            string strReturn = "";


            foreach (var item in items)
            {
                var idivTag = new TagBuilder("div");
                idivTag.AddCssClass("template_1");

                var labelTag = new TagBuilder("label");
                var rbValue = item.Value ?? item.Text;
                var rbName = name;
                var radioTag = htmlHelper.RadioButton(rbName, rbValue, item.Selected, new { name = name, @class = "inp_new" });
                labelTag.InnerHtml = radioTag.ToString();
                labelTag.InnerHtml += item.Text;

                idivTag.InnerHtml = labelTag.ToString();

                var clrDiv = new TagBuilder("div");
                clrDiv.AddCssClass("clear");
                idivTag.InnerHtml += clrDiv.ToString();

                strReturn += idivTag.ToString();
            }

            var s = MvcHtmlString.Create(strReturn.ToString() + "\r");
            return s;
        }
    }

    public static class HtmlExtensions
    {
        public static MvcHtmlString RadioButtonForSelectList<TModel, TProperty>(
            this HtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, TProperty>> expression,
            IEnumerable<SelectListItem> listOfValues)
        {
            var metaData = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
            var sb = new StringBuilder();

            if (listOfValues != null)
            {
                // Create a radio button for each item in the list
                foreach (SelectListItem item in listOfValues)
                {
                    // Generate an id to be given to the radio button field
                    var id = string.Format("{0}_{1}", metaData.PropertyName, item.Value);

                    // Create and populate a radio button using the existing html helpers
                    var label = htmlHelper.Label(id, HttpUtility.HtmlEncode(item.Text));
                    var radio = htmlHelper.RadioButtonFor(expression, item.Value, new { id = id }).ToHtmlString();

                    // Create the html string that will be returned to the client
                    // e.g. <input data-val="true" data-val-required="You must select an option" id="TestRadio_1" name="TestRadio" type="radio" value="1" /><label for="TestRadio_1">Line1</label>
                    sb.AppendFormat("<div class=\"RadioButton\">{0}{1}</div>", radio, label);
                }
            }

            return MvcHtmlString.Create(sb.ToString());
        }
    }


    /// <summary>
    /// File Extensions.
    /// </summary>
    /// <author>Anwar</author>
    /// <datetime>1/23/2011 12:00 AM</datetime>
    public static class FileExtensions
    {
        /// <summary>
        /// Returns a file input element by using the specified HTML helper and the name of the form field.
        /// </summary>
        /// <param name="htmlHelper">The HTML helper instance that this method extends.</param>
        /// <param name="name">The name of the form field.</param>
        /// <returns>An input element that has its type attribute set to "file".</returns>
        public static MvcHtmlString File(this HtmlHelper htmlHelper, string name)
        {
            return htmlHelper.FileHelper(name, null);
        }

        /// <summary>
        /// Returns a file input element by using the specified HTML helper, the name of the form field, and the HTML attributes.
        /// </summary>
        /// <param name="htmlHelper">The HTML helper instance that this method extends.</param>
        /// <param name="name">The name of the form field.</param>
        /// <param name="htmlAttributes">An object that contains the HTML attributes for the element. The attributes are retrieved through reflection by examining the properties of the object. The object is typically created by using object initializer syntax.</param>
        /// <returns>An input element that has its type attribute set to "file".</returns>
        public static MvcHtmlString File(this HtmlHelper htmlHelper, string name, object htmlAttributes)
        {
            return htmlHelper.FileHelper(name, HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        /// <summary>
        /// Returns a file input element by using the specified HTML helper, the name of the form field, and the HTML attributes.
        /// </summary>
        /// <param name="htmlHelper">The HTML helper instance that this method extends.</param>
        /// <param name="name">The name of the form field.</param>
        /// <param name="htmlAttributes">An object that contains the HTML attributes for the element. The attributes are retrieved through reflection by examining the properties of the object. The object is typically created by using object initializer syntax.</param>
        /// <returns>
        /// An input element that has its type attribute set to "file".
        /// </returns>
        public static MvcHtmlString File(this HtmlHelper htmlHelper, string name, IDictionary<string, object> htmlAttributes)
        {
            return htmlHelper.FileHelper(name, htmlAttributes);
        }

        /// <summary>
        /// Returns a file input element by using the specified HTML helper and the name of the form field.
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="htmlHelper">The HTML helper instance that this method extends.</param>
        /// <param name="expression">The expression.</param>
        /// <returns>
        /// An input element that has its type attribute set to "file".
        /// </returns>
        public static MvcHtmlString FileFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression)
        {
            return htmlHelper.FileHelper(ExpressionHelper.GetExpressionText(expression), null);
        }

        /// <summary>
        /// Returns a file input element by using the specified HTML helper, the name of the form field, and the HTML attributes.
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="htmlHelper">The HTML helper instance that this method extends.</param>
        /// <param name="expression">The expression.</param>
        /// <param name="htmlAttributes">An object that contains the HTML attributes for the element. The attributes are retrieved through reflection by examining the properties of the object. The object is typically created by using object initializer syntax.</param>
        /// <returns>
        /// An input element that has its type attribute set to "file".
        /// </returns>
        public static MvcHtmlString FileFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, object htmlAttributes)
        {
            return htmlHelper.FileHelper(ExpressionHelper.GetExpressionText(expression), HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        /// <summary>
        /// Returns a file input element by using the specified HTML helper, the name of the form field, and the HTML attributes.
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="htmlHelper">The HTML helper instance that this method extends.</param>
        /// <param name="expression">The expression.</param>
        /// <param name="htmlAttributes">An object that contains the HTML attributes for the element. The attributes are retrieved through reflection by examining the properties of the object. The object is typically created by using object initializer syntax.</param>
        /// <returns>
        /// An input element that has its type attribute set to "file".
        /// </returns>
        public static MvcHtmlString FileFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, IDictionary<string, object> htmlAttributes)
        {
            return htmlHelper.FileHelper(ExpressionHelper.GetExpressionText(expression), htmlAttributes);
        }

        private static MvcHtmlString FileHelper(this HtmlHelper htmlHelper, string name, IDictionary<string, object> htmlAttributes)
        {
            string fieldName = htmlHelper.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(name);
            var tagBuilder = new TagBuilder("input");
            tagBuilder.MergeAttributes(htmlAttributes);
            tagBuilder.MergeAttribute("type", "file", true);
            tagBuilder.MergeAttribute("name", fieldName, true);
            tagBuilder.GenerateId(fieldName);

            ModelState modelState;
            if (htmlHelper.ViewData.ModelState.TryGetValue(name, out modelState))
            {
                if (modelState.Errors.Count > 0)
                {
                    tagBuilder.AddCssClass(HtmlHelper.ValidationInputCssClassName);
                }
            }

            return MvcHtmlString.Create(tagBuilder.ToString(TagRenderMode.SelfClosing));
        }
    }
}