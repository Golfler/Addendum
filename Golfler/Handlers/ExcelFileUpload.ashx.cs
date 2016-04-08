using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Web;
using Golfler.Models;

namespace Golfler.Handlers
{
    /// <summary>
    /// Summary description for ExcelFileUpload
    /// </summary>
    public class ExcelFileUpload : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            #region Validate

            string msg = "";
            var file = context.Request.Files[0];
            var courseID = Convert.ToInt32(context.Request.Form["courseID"]);
            var courseAdminID = Convert.ToInt32(context.Request.Form["courseAdminID"]);

            var newFileName = "";
            newFileName = "Membership_" + DateTime.Now.Ticks + Path.GetExtension(file.FileName);
            string path = HttpContext.Current.Server.MapPath("~/Uploads/Excel/") + newFileName;
            file.SaveAs(path);

            OleDbDataAdapter adpImportFile;
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();
            string strSqlString = "";
            string conn = "";
            string error = "";

            strSqlString = "SELECT * FROM [Membership$]";
            conn = string.Format(ConfigurationManager.ConnectionStrings["OleDbConnectionString"].ToString(), path);

            adpImportFile = new OleDbDataAdapter(strSqlString, conn);
            adpImportFile.MissingSchemaAction = MissingSchemaAction.AddWithKey;
            adpImportFile.Fill(ds, "Memberships");
            dt = ds.Tables[0];

            if (dt.Rows.Count > 0)
            {
                #region File should Contains All columns

                if (!dt.Columns.Contains("FirstName"))
                    error = error + "<br>FirstName";
                if (!dt.Columns.Contains("LastName"))
                    error = error + "<br>LastName";
                if (!dt.Columns.Contains("Email"))
                    error = error + "<br>Email";
                if (!dt.Columns.Contains("MemberShipId"))
                    error = error + "<br>MemberShipId";

                if (error.Trim() != "")
                {
                    error = error + "<br>Column(s) does not exists. Please check sample file<br>";
                }

                #endregion

                string strUserNames = "";
                string strEmails = "";

                if (error.Trim() == "")
                {
                    #region validation for Primarykey
                    //try
                    //{
                    //    //DataColumn[] columns = new DataColumn[1];
                    //    //columns[0] = dt.Columns["Username"];
                    //    //dt.PrimaryKey = columns;



                    //    //UniqueConstraint custUnique = new UniqueConstraint(new DataColumn[] { dt.Columns["email"] });


                    //    //dt.Constraints.Add(custUnique);

                    //}
                    //catch (Exception ex)
                    //{
                    //    error = "File Contains Duplicate/Blank UserNames or Email<br>";
                    //}
                    #endregion

                    #region Required fields

                    Boolean reqError = false;
                    string strLineNumber = "";

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        reqError = false;

                        if (string.IsNullOrEmpty(dt.Rows[i]["FirstName"].ToString().Trim()) &&
                            string.IsNullOrEmpty(dt.Rows[i]["LastName"].ToString().Trim()) &&
                            string.IsNullOrEmpty(dt.Rows[i]["Email"].ToString().Trim()) &&
                            string.IsNullOrEmpty(dt.Rows[i]["MemberShipId"].ToString().Trim()))
                        {
                            continue;
                        }

                        if (string.IsNullOrEmpty(dt.Rows[i]["FirstName"].ToString().Trim()))
                        {
                            reqError = true;
                        }

                        if (string.IsNullOrEmpty(dt.Rows[i]["LastName"].ToString().Trim()))
                        {
                            reqError = true;
                        }

                        if (string.IsNullOrEmpty(dt.Rows[i]["Email"].ToString().Trim()))
                        {
                            reqError = true;
                        }

                        if (string.IsNullOrEmpty(dt.Rows[i]["MemberShipId"].ToString().Trim()))
                        {
                            reqError = true;
                        }

                        if (reqError)
                        {
                            // i+2 One extra for Heading in excel sheet : varinder
                            strLineNumber = strLineNumber == "" ? (i + 2).ToString() : strLineNumber + ", " + (i + 2).ToString();
                        }

                        strEmails = strEmails == "" ? dt.Rows[i]["Email"].ToString().Trim() : strEmails + ", " + dt.Rows[i]["Email"].ToString().Trim();
                    }



                    #endregion

                    if (strLineNumber.Trim() != "")
                    {
                        error = error == "" ? error + "FirstName<br>LastName<br>Email<br>MemberShipId<br>Required fields, please check records in following rows:<br>" + strLineNumber + "<br>" :
                                          error + "<hr>FirstName<br>LastName<br>Email<br>MemberShipId<br>Required fields, please check records in following rows:<br>" + strLineNumber + "<br>";
                    }


                    #region Validations

                    string strLineNumberValidation = "";

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (string.IsNullOrEmpty(dt.Rows[i]["FirstName"].ToString().Trim()) &&
                            string.IsNullOrEmpty(dt.Rows[i]["LastName"].ToString().Trim()) &&
                            string.IsNullOrEmpty(dt.Rows[i]["Email"].ToString().Trim()) &&
                            string.IsNullOrEmpty(dt.Rows[i]["MemberShipId"].ToString().Trim()))
                        {
                            continue;
                        }

                        if (!CommonFunctions.Validate(dt.Rows[i]["Email"].ToString(), RegularExp.Email))
                        {
                            strLineNumberValidation = strLineNumberValidation == "" ? (i + 2).ToString() : strLineNumberValidation + ", " + (i + 2).ToString();
                        }
                    }

                    #endregion

                    if (strLineNumberValidation.Trim() != "")
                    {
                        error = error == "" ? error + "Invalid Email(s), please check records in following rows:<br>" + strLineNumberValidation + "<br>" :
                                          error + "<hr>Invalid Email(s), please check records in following rows:<br>" + strLineNumberValidation + "<br>";
                    }
                }

                //context.Response.ContentType = "text/plain";
                if (error.Trim() == "")
                {
                    Course obj = new Course();

                    List<GF_CourseMemberShip> listAll = new List<GF_CourseMemberShip>();
                    GF_CourseMemberShip _objMemberShip;
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        _objMemberShip = new GF_CourseMemberShip();
                        _objMemberShip.FirstName = dt.Rows[i]["FirstName"].ToString().Trim();
                        _objMemberShip.LastName = dt.Rows[i]["LastName"].ToString().Trim();
                        _objMemberShip.Email = dt.Rows[i]["Email"].ToString().Trim();
                        _objMemberShip.MemberShipId = dt.Rows[i]["MemberShipId"].ToString().Trim();

                        listAll.Add(_objMemberShip);
                    }

                    obj.InsertMulipleMembership(listAll, courseID, courseAdminID);
                }
            }
            else
            {
                error = "No record exists in excel file";
            }

            if (error.Trim() == "")
            {
                msg = "{";
                msg += string.Format("error:'{0}',\n", string.Empty);
                msg += string.Format("msg:'{0}'\n", "success");
                msg += "}";

                context.Response.Write(msg);
            }
            else
            {
                msg = "{";
                msg += string.Format("error:'{0}',\n", error);
                msg += string.Format("msg:'{0}'\n", "Error");
                msg += "}";

                context.Response.Write(msg);
            }

            #endregion
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}