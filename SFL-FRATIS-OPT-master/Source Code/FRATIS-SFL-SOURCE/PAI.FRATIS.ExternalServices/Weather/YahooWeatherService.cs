//    Copyright 2014 Productivity Apex Inc.
//        http://www.productivityapex.com/
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

using System;
using System.IO;
using System.Net;
using System.Xml;

namespace PAI.FRATIS.ExternalServices.Weather
{
    public class YahooWeatherService : IWeatherService
    {
        public string GetCityCode(string city, string state)
        {
            string result;

            try
            {
                const string appId = "3XMGfbfV34FeiiK4MZ6xsFxSLbXbNCv4tBBnUQRQ2wBthiimvINHsj3UmX4Wlky9s7TB4iTz7tJtYnj7lFkae7BCjqUl1fY-";
                var url = string.Format("http://where.yahooapis.com/v1/places.q('{0} {1}')?appid={2}", city, state, appId);

                // Create the web request & get response
                var request = WebRequest.Create(url) as HttpWebRequest;
                var responseString = string.Empty;
                using (var response = request.GetResponse() as HttpWebResponse)
                {
                    // Get the response stream  
                    var reader = new StreamReader(response.GetResponseStream());

                    // Read the whole contents and return as a string  
                    responseString = reader.ReadToEnd();
                }

                using (XmlReader reader = XmlReader.Create(new StringReader(responseString)))
                {
                    reader.MoveToContent();
                    //reader.ReadToFollowing("places");
                    reader.ReadToFollowing("place");
                    reader.ReadToFollowing("woeid");

                    result = reader.ReadElementString();
                }
            }
            catch (Exception e)
            {
                result = string.Empty;
            }
            return result;
        }

        public WeatherResponse GetWeather(string locationCode)
        {
            var result = new WeatherResponse() { LocationCode = locationCode };

            var url = string.Format("http://weather.yahooapis.com/forecastrss?w={0}", locationCode);
            var responseString = string.Empty;

            // Create the web request & get response
            var request = WebRequest.Create(url) as HttpWebRequest;
            using (var response = request.GetResponse() as HttpWebResponse)
            {
                // Get the response stream  
                var reader = new StreamReader(response.GetResponseStream());

                // Read the whole contents and return as a string  
                responseString = reader.ReadToEnd();
            }

            using (XmlReader reader = XmlReader.Create(new StringReader(responseString)))
            {
                reader.MoveToContent();
                reader.ReadToFollowing("channel");
                reader.ReadToFollowing("title");

                result.Title = reader.ReadElementString();

                reader.ReadToFollowing("link");
                result.Link = reader.ReadElementString();

                reader.ReadToFollowing("item");
                reader.ReadToFollowing("description");
                result.CurrentWeather = reader.ReadElementString();

                var pos = result.CurrentWeather.IndexOf("(provided");
                if (pos > 0)
                {
                    result.CurrentWeather = result.CurrentWeather.Substring(0, pos);
                }
            }

            return result;
        }
    }
}