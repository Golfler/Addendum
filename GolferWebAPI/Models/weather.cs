using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Xml;
using Newtonsoft.Json;
using System.Net;
using Newtonsoft.Json.Linq;

namespace GolferWebAPI.Models
{
    public partial class weather
    {
        private static GolflerEntities _db = null;

        #region Async Thread for Order Weather Api

        public static void callingWeatherApi(GF_WeatherDetails objWeather)
        {
            invProfileApplicationCompliance = new delProfileApplicationCompliance(AddWeather);
            invProfileApplicationCompliance.BeginInvoke(objWeather, new AsyncCallback(AddWeather), null);
        }

        public delegate void delProfileApplicationCompliance(GF_WeatherDetails objWeather);
        public static delProfileApplicationCompliance invProfileApplicationCompliance;

        public static void AddWeather(IAsyncResult t)
        {
            List<string> objList = new List<string>();
            try
            {
                invProfileApplicationCompliance.EndInvoke(t);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region Async Thread for Game Play Round Weather Api

        public static void callingRoundWeatherApi(GF_GamePlayRound objRound)
        {
            invGamePlayRound = new delGamePlayRound(AddRoundWeatherInfo);
            invGamePlayRound.BeginInvoke(objRound, new AsyncCallback(AddRoundWeatherInfo), null);
        }

        public delegate void delGamePlayRound(GF_GamePlayRound objRound);
        public static delGamePlayRound invGamePlayRound;

        public static void AddRoundWeatherInfo(IAsyncResult t)
        {
            List<string> objList = new List<string>();
            try
            {
                invGamePlayRound.EndInvoke(t);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        /// <summary>
        /// Created By: Veera Verma
        /// Created Date: 14 April 2015
        /// Purpose: Add weatherinfo
        /// </summary>
        /// <param name="feedback"></param>
        /// <returns></returns>
        public static void AddWeather(GF_WeatherDetails objWeather)
        {
            try
            {
                _db = new GolflerEntities();
                var lstCourse = _db.GF_Order.FirstOrDefault(x => x.ID == objWeather.OrderID && x.GolferID == objWeather.GolferID);
                if (lstCourse == null)
                {
                    return;
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
                    return;
                }
            }
            catch
            {
                return;
            }
        }
        
        /// <summary>
        /// Created By: Amit Kumar
        /// Created Date: 29 April 2015
        /// Purpose: Add weather information in game play round
        /// </summary>
        /// <param name="feedback"></param>
        /// <returns></returns>
        public static void AddRoundWeatherInfo(GF_GamePlayRound objRound)
        {
            try
            {
                _db = new GolflerEntities();
                var lstRound = _db.GF_GamePlayRound.FirstOrDefault(x => x.ID == objRound.ID);
                if (lstRound == null)
                {
                    return;
                }

                string temp = "";
                string temp_min = "";
                string temp_max = "";
                string wind = "";
                string pressure = "";
                string humidity = "";
                string description = "";
                string rain = "";
                string snow = "";

                if (GetweatherNew(lstRound.GF_CourseInfo.LATITUDE, lstRound.GF_CourseInfo.LONGITUDE, ref temp, ref temp_min, ref temp_max,
                    ref wind, ref pressure, ref humidity, ref description, ref rain, ref snow))
                {
                    lstRound.Temperature = temp;
                    lstRound.TempMax = temp_max;
                    lstRound.TempMin = temp_min;
                    lstRound.Humidity = humidity;
                    lstRound.Pressure = pressure;
                    lstRound.WeatherDescription = description;
                    lstRound.WindSpeed = wind;
                    lstRound.Rain = rain;

                    _db.SaveChanges();
                }
                else
                {
                    return;
                }
            }
            catch(System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                string msg = ex.Message;
                return;
            }
        }

        public static bool Getweather(string latitude, string longitude, ref string temp, ref string temp_min, ref string temp_max, ref string wind, ref string pressure, ref string humidity, ref string description)
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

        public static bool GetweatherNew(string latitude, string longitude, ref string temp, ref string temp_min, ref string temp_max,
            ref string wind, ref string pressure, ref string humidity, ref string description, ref string rain, ref string snow)
        {
            bool result = false;
            bool intermediateresult1 = false;
            bool intermediateresult2 = false;
            bool intermediateresult3 = false;
            try
            {
                string url = "http://api.openweathermap.org/data/2.5/weather?lat=" + latitude + "&lon=" + longitude;
                MyWebRequest myRequest = new MyWebRequest(url);
                var jsonString = myRequest.GetResponse();
                WeatherInformation weatherInformation = JsonHelper.JsonDeserialize<WeatherInformation>(jsonString);

                JObject json = JObject.Parse(jsonString);
                if (jsonString.Contains("rain"))
                {
                    weatherInformation.rain._3h = Convert.ToDouble((json["rain"]["3h"]).ToString());
                    rain = weatherInformation.rain._3h.ToString();
                }
                else
                {
                    rain = "";
                }

                if (jsonString.Contains("snow"))
                {
                    weatherInformation.snow._3h = Convert.ToDouble((json["snow"]["3h"]).ToString());
                    snow = weatherInformation.snow._3h.ToString();
                }
                else
                {
                    snow = "";
                }
                
                if (weatherInformation != null)
                {
                    if (weatherInformation.main != null)
                    {
                        temp = weatherInformation.main.temp.ToString();//coming in kelvin
                        // the formula for converting Kelvin to Celsius. The formula is: C = K - 273. 
                        temp_min = weatherInformation.main.temp_min.ToString();//coming in kelvin
                        temp_max = weatherInformation.main.temp_max.ToString();//coming in kelvin
                        pressure = weatherInformation.main.pressure.ToString();//hpa
                        humidity = weatherInformation.main.humidity.ToString();//%
                        
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

                    if (weatherInformation.wind != null && weatherInformation.wind.speed != null)
                    {
                        wind = weatherInformation.wind.speed.ToString();//m/s
                        intermediateresult2 = true;
                    }
                    else
                    {
                        wind = "";
                        intermediateresult2 = false;
                    }

                    if (weatherInformation.weather != null)
                    {
                        if (weatherInformation.weather.FirstOrDefault() != null)
                            description = weatherInformation.weather.FirstOrDefault().description;
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

    #region Weather Info Class

    public class Coord
    {
        public double lon { get; set; }
        public double lat { get; set; }
    }

    public class Sys
    {
        public double message { get; set; }
        public string country { get; set; }
        public int sunrise { get; set; }
        public int sunset { get; set; }
    }

    public class Weather
    {
        public int id { get; set; }
        public string main { get; set; }
        public string description { get; set; }
        public string icon { get; set; }
    }

    public class Main
    {
        public double temp { get; set; }
        public double temp_min { get; set; }
        public double temp_max { get; set; }
        public double pressure { get; set; }
        public double sea_level { get; set; }
        public double grnd_level { get; set; }
        public int humidity { get; set; }
    }

    public class Wind
    {
        public double speed { get; set; }
        public double deg { get; set; }
    }

    public class Clouds
    {
        public int all { get; set; }
    }

    public class Rain
    {
        public double _3h { get; set; }
    }

    public class Snow
    {
        public double _3h { get; set; }
    }

    public class WeatherInformation
    {
        public Coord coord { get; set; }
        public Sys sys { get; set; }
        public List<Weather> weather { get; set; }
        public string @base { get; set; }
        public Main main { get; set; }
        public Wind wind { get; set; }
        public Clouds clouds { get; set; }
        public Rain rain { get; set; }
        public Snow snow { get; set; }
        public int dt { get; set; }
        public int id { get; set; }
        public string name { get; set; }
        public int cod { get; set; }
    }

    #endregion

}