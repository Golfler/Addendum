using System.Web.Mvc;

namespace SSASWeb
{
    /// <summary>
    /// Created By Renuka Hira
    /// This is to return file according to specified file extension
    /// </summary>
    public class PDFActionResult : ActionResult
    {
        // public HtmlToPdfBuilder HtmlPdfBuilder { get; set; }
        public System.Text.StringBuilder stringBuilder { get; set; }
        public string FileName { get; set; }
        public string ExportContentType { get; set; }
        public System.IO.StringWriter ModelData = new System.IO.StringWriter();

        public override void ExecuteResult(ControllerContext context)
        {
            // output 

            if (ExportContentType == "PDF")
            {
                //set the output to pdf
                //context.HttpContext.Response.ContentType = "application/PDF";
                //next write out the pdf to the response
                //context.HttpContext.Response.BinaryWrite(HtmlPdfBuilder.RenderPdf());
            }
            else if (ExportContentType == "XLS" || ExportContentType == "CSV")
            {
                //clear the output
                context.HttpContext.Response.Clear();
                context.HttpContext.Response.AddHeader("Content-Disposition", "attachment;filename=" + System.Text.RegularExpressions.Regex.Replace(FileName, @"[-\s()/]", ""));

                if (ExportContentType == "XLS")
                {
                    context.HttpContext.Response.ContentType = "application/excel";
                    context.HttpContext.Response.Write(System.Text.RegularExpressions.Regex.Replace(ModelData.ToString(), @"a[^>]+href[^>]", "span"));
                    context.HttpContext.Response.End();
                }
                else if (ExportContentType == "CSV")
                {
                    context.HttpContext.Response.ContentType = "application/csv";
                    context.HttpContext.Response.Write(ModelData.ToString());
                    context.HttpContext.Response.End();
                }
            }
        }
    }
}