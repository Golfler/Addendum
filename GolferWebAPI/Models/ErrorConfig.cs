using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Linq;

namespace GolferWebAPI.Models
{
    public class ErrorConfig
    {
        /// <summary>
        /// Created by: Arun
        /// Created Date: 05 Nov. 2014
        /// Purpose: Get error messages
        /// </summary>
        /// <param name="nodename">Error message name (node name in xml file) </param>
        /// <param name="message">string to customize the message.</param>
        /// <returns></returns>
        public static string GetErrorMessage(string nodename, string message = "")
        {
            XmlDocument xmldoc = null;
            try
            {

                xmldoc = new XmlDocument();
                XmlNodeList xmlnode;
                xmldoc.Load(HttpContext.Current.Server.MapPath(ConfigClass.ErrorMessagesFilePath));

                xmlnode = xmldoc.GetElementsByTagName(nodename);
                var text = xmlnode[0].ChildNodes.Item(0).InnerText;
                if (message != "")
                {
                    var msg = message.Split('-');
                    if (msg.Length > 1)
                        text = text.Replace(msg[0], msg[1]);
                }
                return text;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public static IList<dynamic> GetExpandoFromXml(string category)
        {
            var persons = new List<dynamic>();

            var doc = XDocument.Load(HttpContext.Current.Server.MapPath(ConfigClass.ErrorMessagesFilePath));
            //var nodes = from node in doc
            //            where(x => x.Attribute("category").Value.Equals(category.ToLower()))
            //            select node;
            //foreach (var n in nodes)
            //{
            //    dynamic person = new ExpandoObject();
            //    foreach (var child in n.Descendants())
            //    {
            //        var p = person as IDictionary<String, object>;
            //        p[child.Name.ToString()] = child.Value.Trim();
            //    }

            //    persons.Add(person);
            //}



            return persons;
        }
    }
}