namespace IDTO.TravelerPortal.Common
{
    using System.Threading.Tasks;
    using IDTO.TravelerPortal.Common.Models;

    using RestSharp;

    public class WeatherManager
    {
        public readonly string uriString = Config.OpenWeatherMapDataUrl;

        public WeatherManager()
        {
        }

        public WeatherInfo GetWeatherInfo(double lat, double lon)
        {
            WeatherReport weatherReport = new WeatherReport();

            var client = new RestClient(uriString);

            var request = new RestRequest("/weather", Method.GET);
            request.RequestFormat = DataFormat.Json;

            request.AddParameter("lat", lat);
            request.AddParameter("lon", lon);

            IRestResponse<WeatherReport> response = client.Execute<WeatherReport>(request);

            weatherReport = response.Data;

            WeatherInfo weatherInfo = new WeatherInfo();

            if (weatherReport != null)
            {
                weatherInfo.TemperatureDegF = weatherReport.KelvinToDegF(weatherReport.main.temp);
                weatherInfo.LastUpdate = weatherReport.LongToDateTime(weatherReport.dt);
                if (weatherReport.weather.Count > 0)
                {
                    weatherInfo.Conditions = weatherReport.weather[0].description;
                    weatherInfo.IconName = weatherReport.IconToIconName(weatherReport.weather [0].icon);
                    weatherInfo.IconURL = weatherReport.IconToUrl(weatherReport.weather[0].icon);
                    weatherInfo.LocationName = weatherReport.name;
                }
            }

            return weatherInfo;
        }
    }
}