using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.OleDb;
using System.Data;
using System.IO;
using Golfler.Models;
using Golfler.Repositories;
using System.Text.RegularExpressions;
using System.Configuration;

namespace Golfler.Handlers
{
    /// <summary>
    /// Summary description for ValidateExcelFile
    /// </summary>
    public class ValidateExcelFile : IHttpHandler
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

            


            strSqlString = "SELECT * FROM [Sheet1$]";
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
                if (!dt.Columns.Contains("Username"))
                    error = error + "<br>Username";
                if (!dt.Columns.Contains("password"))
                    error = error + "<br>password";
                if (!dt.Columns.Contains("Type"))
                    error = error + "<br>Type";
                if (!dt.Columns.Contains("Phone"))
                    error = error + "<br>Phone";
                if (!dt.Columns.Contains("Status"))
                    error = error + "<br>Status";

                if (error.Trim() != "")
                {
                    error = error + "<br>Column(s) not exists. please check sample file<br>";
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

                        if (dt.Rows[i]["FirstName"].ToString().Trim() == "")
                        {
                            reqError = true;

                        }
                        if (dt.Rows[i]["LastName"].ToString().Trim() == "")
                        {
                            reqError = true;

                        }
                        if (dt.Rows[i]["Email"].ToString().Trim() == "")
                        {
                            reqError = true;
                        }

                        if (dt.Rows[i]["MemberShipId"].ToString().Trim() == "")
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
                        error = error == "" ? error + "FirstName<br>LastName<br>Email<br>MemberShipId<br>Type <br> Required, please check records in following rows:<br>" + strLineNumber + "<br>" :
                                        error + "<hr>FirstName<br>LastName<br>Email<br>MemberShipId<br>Type <br> Required, please check records in following rows:<br>" + strLineNumber + "<br>";
                    }


                    #region Validations
                    string strLineNumberValidation = "";
                    
                    //for (int i = 0; i < dt.Rows.Count; i++)
                    //{
                    //    if (
                    //               !CommonFunctions.Validate(dt.Rows[i]["Email"].ToString(), RegularExp.Email)
                    //            || !CommonFunctions.Validate(dt.Rows[i]["FirstName"].ToString(), RegularExp.AlphaNumeric, 50)
                    //            || !CommonFunctions.Validate(dt.Rows[i]["LastName"].ToString(), RegularExp.AlphaNumeric, 50)
                    //            || !CommonFunctions.Validate(dt.Rows[i]["MemberShipId"].ToString(), RegularExp.UserName, 50)
                               
                                
                    //        )
                    //    {
                    //        strLineNumberValidation = strLineNumberValidation == "" ? (i + 2).ToString() : strLineNumberValidation + ", " + (i + 2).ToString();
                    //    }
                    //}
                    #endregion
                    if (strLineNumberValidation.Trim() != "")
                    {
                        error = error == "" ? error + "Invalid Data, please check records in following rows:<br>" + strLineNumberValidation + "<br>" :
                                            error + "<hr>Invalid Data, please check records in following rows:<br>" + strLineNumberValidation + "<br>";

                    }

                }

            #endregion





                //context.Response.ContentType = "text/plain";
                if (error.Trim() == "")
                {

                //    string strUserAlreadyExists = "";
                   // string strEmailAlreadyExists = "";

                    Course obj = new Course();
                //    var list = objUsers.GetUsersByUserNames(strUserNames, strEmails, orgID);
                   /* if (list.Count > 0)
                    {
                        foreach (SSAS_Users usr in list)
                        {


                            if (!strUserAlreadyExists.Contains(usr.UserName))
                            {
                                strUserAlreadyExists = strUserAlreadyExists == "" ? usr.UserName : strUserAlreadyExists + ", " + usr.UserName;
                            }
                            if (!strEmailAlreadyExists.Contains(usr.Email))
                            {
                                strEmailAlreadyExists = strEmailAlreadyExists == "" ? usr.Email : strEmailAlreadyExists + ", " + usr.Email;
                            }
                        }
                        error = error == "" ? error + "Username " + strUserAlreadyExists + "<br> or Email " + strEmailAlreadyExists + " already exists<br>" :
                            error + "<hr>Username " + strUserAlreadyExists + "<br> or Email " + strEmailAlreadyExists + " already exists<br>";
                    }*/
                  
                     //   else
                       // {*/
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
                      //  }
                   // }
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