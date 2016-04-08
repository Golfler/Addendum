using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Xml;
using Newtonsoft.Json;

namespace CourseWebApi.Models
{
    public partial class weather
    {
        private GolflerEntities _db = null;

        /// <summary>
        /// Created By: Veera Verma
        /// Created Date: 14 April 2015
        /// Purpose: Add weatherinfo
        /// </summary>
        /// <param name="feedback"></param>
        /// <returns></returns>
        public Result AddWeather(GF_WeatherDetails objWeather)
        {
            try
            {
                _db = new GolflerEntities();
                var lstCourse = _db.GF_Order.FirstOrDefault(x => x.ID == objWeather.OrderID && x.GolferID == objWeather.GolferID);
                if (lstCourse == null)
                {
                    return new Result { Id = 0, Status = 0, record = null, Error = "Invalid Order." };
                }
                objWeather.CreatedDate = DateTime.UtcNow;
                string temp = "";
                string temp_min = "";
                string temp_max = "";
                string wind = "";
                string pressure = "";
                string humidity = "";
                string description = "";

                if (Getweather(objWeather.Latitude, objWeather.Longitude, ref temp, ref temp_min, ref temp_max, ref wind, ref pressure, ref humidity, ref description))
                {
                    objWeather.Status = "A";
                    objWeather.Temp = temp;
                    objWeather.TempMax = temp_max;
                    objWeather.TempMin = temp_min;
                    objWeather.Humidity = humidity;
                    objWeather.Pressure = pressure;
                    objWeather.WeatherDescription = description;
                    objWeather.WindSpeed = wind;

                    _db.GF_WeatherDetails.Add(objWeather);
                    _db.SaveChanges();
                }
                else
                {
                    return new Result { Id = 0, Status = 0, record = null, Error = "Some error occurs while saving the data.please try again later." };
                }
                return new Result
                {
                    Id = objWeather.ID,
                    Status = 1,
                    Error = "Success",
                    record = new
                    {
                        objWeather.ID,
                        objWeather.GolferID,
                        objWeather.Temp,
                        objWeather.TempMax,
                        objWeather.TempMin,
                        objWeather.WeatherDescription,
                        objWeather.Pressure,
                        objWeather.Humidity,
                        objWeather.WindSpeed

                    }
                };

            }
            catch (Exception ex)
            {
                return new Result { Id = 0, Status = 0, record = null, Error = ex.Message };
            }
        }
        public bool Getweather(string latitude, string longitude, ref string temp, ref string temp_min, ref string temp_max, ref string wind, ref string pressure, ref string humidity, ref string description)
        {
            bool result = false;
            bool intermediateresult1 = false;
            bool intermediateresult2 = false;
            bool intermediateresult3 = false;
            try
            {
                string url = "http://api.openweathermap.org/data/2.5/weather?lat=" + latitude + "&lon=" + longitude;
                var json = new System.Net.WebClient().DownloadString(url);
                DataSet dsRecognize = new DataSet();
                string jsonStringRecognize = json;
                XmlDocument xdRecognize = new XmlDocument();
                jsonStringRecognize = "{ \"rootNode\": {" + jsonStringRecognize.Trim().TrimStart('{').TrimEnd('}') + "} }";
                xdRecognize = (XmlDocument)JsonConvert.DeserializeXmlNode(jsonStringRecognize);
                dsRecognize.ReadXml(new XmlNodeReader(xdRecognize));

                if (dsRecognize != null && dsRecognize.Tables.Count > 0)
                {
                    DataTable main = dsRecognize.Tables["main"];
                    if (main != null && main.Rows.Count > 0)
                    {
                        temp = main.Rows[0]["temp"].ToString();//coming in kelvin
                         // the formula for converting Kelvin to Celsius. The formula is: C = K - 273. 
                        temp_min = main.Rows[0]["temp_min"].ToString();////coming in kelvin
                        temp_max = main.Rows[0]["temp_max"].ToString();//coming in kelvin
                        pressure = main.Rows[0]["pressure"].ToString();//hpa
                        humidity = main.Rows[0]["humidity"].ToString();// %
                        intermediateresult1 = true;
                    }
                    else
                    {
                        temp = "";
                        temp_min = "";
                        temp_max = "";
                        pressure = "";
                        humidity = "";
                        intermediateresult1 = false;
                    }
                    DataTable windspeed = dsRecognize.Tables["wind"];
                    if (windspeed != null && windspeed.Rows.Count > 0)
                    {
                        wind = windspeed.Rows[0]["speed"].ToString();//m/s
                        intermediateresult2 = true;
                    }
                    else
                    {
                        wind = "";
                        intermediateresult2 = false;
                    }

                    DataTable weather = dsRecognize.Tables["weather"];
                    if (weather != null && weather.Rows.Count > 0)
                    {
                        description = weather.Rows[0]["description"].ToString();
                        intermediateresult3 = true;
                    }
                    else
                    {
                        description = "";
                        intermediateresult3 = false;

                    }
                }
                if (intermediateresult1 == true && intermediateresult2 == true && intermediateresult3 == true)
                {
                    result = true;
                }
                else
                {
                    result = false;
                }

            }
            catch
            {
                result = false;
            }
            return result;
        }
    }
}