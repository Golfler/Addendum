using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace CourseWebApi.Models
{
    public class LogClass
    {
        public static string RequestFilePath { get; set; }
        public static bool RequestLogMode { get; set; }

        /// <summary>
        /// Write Log for Mass Messages
        /// </summary>
        /// <param name="msgs"></param>
        /// <returns></returns>
        public static string WriteLog(List<string> msgs, string strfilename = "")
        {
            string strResult = "";
            try
            {
                //if (Convert.ToString(System.Web.Configuration.WebConfigurationManager.AppSettings["ERROR_LOG_MODE"]) == "1")
                //{
                RequestLogMode = true;
                //}
                //else
                //{
                //    RequestLogMode = false;
                //}
                RequestFilePath = Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["PhysicalApplicationPath"]) + Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["LOG_DIR"]);
                if (RequestLogMode)
                {
                    string fileName = "";
                    if (strfilename == "")
                    {
                        fileName = "braintree.txt";
                    }
                    else
                    {
                        fileName = strfilename;
                    }

                    string filePath = RequestFilePath + fileName;
                    StreamWriter w;

                    w = File.Exists(filePath) ? File.AppendText(filePath) : File.CreateText(filePath);

                    w.WriteLine("---------------------------------------------------------------------");
                    w.WriteLine(DateTime.Now);
                    w.WriteLine("---------------------------------------------------------------------");
                    foreach (var objMsg in msgs)
                    {
                        w.WriteLine(objMsg);
                    }
                    w.WriteLine("---------------------------------------------------------------------");
                    w.Flush();
                    w.Close();
                    strResult = "Log file created successfully";
                }
                else
                {
                    strResult = "Log file disabled";
                }
            }
            catch (Exception ex)
            {
                strResult = "Exception: " + ex.InnerException;
                strResult += "Message: " + ex.Message;
            }
            return strResult;
        }

    }
}