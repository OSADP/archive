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
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PAI.FRATIS.ExternalServices.NokiaMaps.Model;
using RestSharp;

namespace PAI.FRATIS.ExternalServices.NokiaMaps
{
    public enum VehicleType
    {
        Truck,
        Car
    }

    public class NokiaMapWrapper
    {
        public MapTileZXY GetCoordinatesZXY(double latitude, double longitude, int levelOfDetail = 8)
        {
            return MapCoordinateConversionService.GetCoordinates(latitude, longitude, levelOfDetail);
        }

        public string GetCoordinatesQuadKey(double latitude, double longitude, int levelOfDetail = 8)
        {
            return MapCoordinateConversionService.GetQuadKey(latitude, longitude, levelOfDetail);
        }

        public RestClient GetNokiaRouteClient(out RestRequest request, string baseUrl = "http://route.nlp.nokia.com")
        {
            request = new RestRequest("routing/{version}/calculateroute.json", Method.POST);
            request.AddUrlSegment("version", "6.2");
            request.AddParameter("app_id", "oLPBSeIYGm6hNoyMlFWx");
            request.AddParameter("app_code", "Po9k-HXzzvllG1DaMHxR7g");
            
            return new RestClient(baseUrl);
        }

        public string GetMapTileUrl(double latitude, double longitude, int levelOfDetail)
        {
            var c = GetCoordinatesZXY(latitude, longitude, levelOfDetail);
            return string.Format("http://maps.nlp.nokia.com/maptiler/v2/maptile/newest/"
                                       + "normal.day/{2}/{3}/{4}/256/png8?app_id={0}&token={1}",
                                       "oLPBSeIYGm6hNoyMlFWx", "Po9k-HXzzvllG1DaMHxR7g", levelOfDetail, c.X, c.Y);
        }

        public string GetRouteMapTileUrl(double startLatitude, double startLongitude, double endLatitude, double endLongitude, int? width = null, int? height = null)
        {
            var result = string.Format("http://maps.nlp.nokia.com/mia/1.6/route?"
                                       + "?app_id={0}&app_code={1}&r0={2},{3},{4},{5}",
                                       "oLPBSeIYGm6hNoyMlFWx", "Po9k-HXzzvllG1DaMHxR7g", startLatitude, startLongitude, endLatitude, endLongitude);

            if (height.HasValue)
            {
                result = string.Format("{0}&h={1}", result, height.Value);
            }

            if (width.HasValue)
            {
                result = string.Format("{0}&w={1}", result, width.Value);
            }

            return result;
        }

        public string GetTrafficMapTileUrl(double latitude, double longitude, int levelOfDetail, DateTime? date = null)
        {
            var c = GetCoordinatesZXY(latitude, longitude, levelOfDetail);
            if (date.HasValue)
            {
                return
                    string.Format(
                        "http://traffic.nlp.nokia.com/traffic/6.0/tiles/" +
                        "{2}/{3}/{4}/256/png8?app_id={0}&token={1}&pattern_time={5}",
                        "oLPBSeIYGm6hNoyMlFWx",
                        "Po9k-HXzzvllG1DaMHxR7g",
                        levelOfDetail,
                        c.X,
                        c.Y,
                        GetPatternTime(date.Value));
            }

            return string.Format(
                           "http://traffic.nlp.nokia.com/traffic/6.0/tiles/" +
                           "{2}/{3}/{4}/256/png8?app_id={0}&token={1}",
                           "oLPBSeIYGm6hNoyMlFWx",
                           "Po9k-HXzzvllG1DaMHxR7g",
                           levelOfDetail,
                           c.X,
                           c.Y);
        }


        private int GetPatternTime(DateTime dt)
        {
            const int SecondsInDay = 60 * 60 * 24;
            var hourSeconds = dt.Hour * (60 * 60);
            var minuteSeconds = dt.Minute * 60;
            
            var result = (SecondsInDay * (int)dt.DayOfWeek) + hourSeconds + minuteSeconds + dt.Second;
            return result;
        }

        public RestClient GetMapTileClient(out RestRequest request, MapTileZXY coordinates, string baseUrl = "http://maps.nlp.nokia.com")
        {
            request = new RestRequest(string.Format("maptiler/{{version}}/maptile/newest/normal.day/{0}/{1}/{2}/256/png8", coordinates.Z, coordinates.X, coordinates.Y), Method.GET);
            request.AddUrlSegment("version", "v2");
            request.AddParameter("app_id", "oLPBSeIYGm6hNoyMlFWx");
            request.AddParameter("token", "Po9k-HXzzvllG1DaMHxR7g");
            request.AddParameter("c", "US");
            request.AddParameter("lg", "en");
            request.AddParameter("i18n", "true");
            request.AddParameter("localtime", "true");
            request.AddParameter("criticality", "0,1");

            return new RestClient(baseUrl);
        }
        public RestClient GetNokiaTrafficClient(out RestRequest request, MapTileZXY coordinates, string baseUrl = "http://traffic.nlp.nokia.com")
        {
            request = new RestRequest(string.Format("traffic/{{version}}/incidents/json/{0}/{1}/{2}", coordinates.Z, coordinates.X, coordinates.Y), Method.GET);
            request.AddUrlSegment("version", "6.0");
            request.AddParameter("app_id", "oLPBSeIYGm6hNoyMlFWx");
            request.AddParameter("token", "Po9k-HXzzvllG1DaMHxR7g");
            request.AddParameter("c", "US");
            request.AddParameter("lg", "en");
            request.AddParameter("i18n", "true");
            request.AddParameter("localtime", "true");
            request.AddParameter("criticality", "0,1");

            return new RestClient(baseUrl);
        }

        public RouteSummary GetRouteSummary(double pos1Lat, double pos1Long, double pos2Lat, double pos2Long, DateTime? departureTime = null, VehicleType vehicleType = VehicleType.Truck)
        {            
            RestRequest request = null;
            var client = GetNokiaRouteClient(out request);

            request.AddParameter("waypoint0", string.Format("geo!{0},{1}", pos1Lat, pos1Long));
            request.AddParameter("waypoint1", string.Format("geo!{0},{1}", pos2Lat, pos2Long));
            request.AddParameter("mode", "fastest;" + vehicleType.ToString().ToLower() + ";traffic:enabled");
            request.AddParameter("routepresentationoptions", "routeattributes:summary");
            if (departureTime.HasValue)
            {
                request.AddParameter("departure", departureTime.Value.ToString("o"));
            }
            else
            {
                request.AddParameter("departure", DateTime.UtcNow.ToString("o"));
            }
            

            IRestResponse r = client.Execute(request);
            var content = r.Content; // raw content as strin

            client.ExecuteAsync(request, response => { });

            if (r.Content.Length > 1)
            {
                var jo = JObject.Parse(r.Content);
                var resp = jo["Response"];

                if (resp == null)
                {
                    return new RouteSummary();
                }

                var routeValues = resp["Route"].First;
                var summaryString = routeValues["Summary"].ToString();

                var summary = JsonConvert.DeserializeObject<RouteSummary>(summaryString);
                return summary;
            }

            Console.WriteLine("Empty response received!");
            return new RouteSummary();
        }

        public TrafficItemList GetIncidents(double latitude, double longitude)
        {
            RestRequest request = null;
            var client = GetNokiaTrafficClient(out request, GetCoordinatesZXY(latitude, longitude));

            IRestResponse r = client.Execute(request);
            var content = r.Content; // raw content as strin

            client.ExecuteAsync(
                request,
                response =>
                {

                });

            if (r.Content.Length > 1)
            {
                var jo = JObject.Parse(r.Content);
                var resp = jo["TRAFFICITEMS"];

                if (resp == null)
                {
                    return new TrafficItemList();
                }

                var summary = JsonConvert.DeserializeObject<TrafficItemList>(resp.ToString());
                return summary;
            }

            Console.WriteLine("Empty response received!");
            return new TrafficItemList();
        }
    }
}