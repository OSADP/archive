using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace WebRole1
{
	public static class WebApiConfig
	{
		public static void Register(HttpConfiguration config)
		{
			// Web API configuration and services

			// Web API routes
			config.MapHttpAttributeRoutes();

			// Route for POST method 
			config.Routes.MapHttpRoute(
					name: "DefaultApi",
					routeTemplate: "api/{controller}/{action}/{id}",
					defaults: new { id = RouteParameter.Optional }
					);

			// Route for GET method 
			config.Routes.MapHttpRoute(
					name: "DefaultApi1",
					routeTemplate: "api/{controller}/{action}/{id}",
					defaults: new { action = "get", id = RouteParameter.Optional }
					);

			//config.Routes.MapHttpRoute(
			//	name: "DefaultApi",
			//	routeTemplate: "api/{controller}/{id}",
			//	defaults: new { id = RouteParameter.Optional }
			//	);

		}
	}
}
