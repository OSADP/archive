using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace IDTO.TravelerPortal.Common
{
    public static class Config
    {

        public static int MaxWalkDistanceDefault
        {
            get { return int.Parse(ConfigurationManager.AppSettings["MaxWalkDistanceDefault"]); }
        }
        public static string IDTOWebApiBaseUrl
        {
            get
            {
                return ConfigurationManager.AppSettings["IDTOWebApiBaseUrl"];
            }
        }

        public static string MobileServiceUrl { get { return ConfigurationManager.AppSettings["MobileServiceURL"]; } }

        public static string MobileAppKey { get { return ConfigurationManager.AppSettings["MobileAppKey"]; } }

        public static string OpenWeatherMapDataUrl { get { return ConfigurationManager.AppSettings["OpenWeatherMapData"]; } }

        public static string OpenWeatherDataIconBaseUrl { get { return ConfigurationManager.AppSettings["OpenWeatherDataIconBaseUrl"]; } }

        public static string GoogleMapLibraryUrl { get { return ConfigurationManager.AppSettings["GoogleMapLibraryUrl"]; } }

        public static string GoogleMapPinLibraryUrl { get { return ConfigurationManager.AppSettings["GoogleMapPinLibraryUrl"]; } }
        
    }
}