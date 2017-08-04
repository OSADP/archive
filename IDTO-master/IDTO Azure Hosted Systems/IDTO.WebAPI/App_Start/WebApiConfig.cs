using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace IDTO.WebAPI
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            //http://www.asp.net/web-api/overview/creating-web-apis/using-web-api-with-entity-framework/using-web-api-with-entity-framework,-part-2
            //for circular references
            var json = config.Formatters.JsonFormatter;
            json.SerializerSettings.PreserveReferencesHandling =
                Newtonsoft.Json.PreserveReferencesHandling.Objects;

            config.Formatters.Remove(config.Formatters.XmlFormatter);

            //enable trace
            config.EnableSystemDiagnosticsTracing();
        }
    }
}
