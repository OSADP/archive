using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.Diagnostics.Management;
using Microsoft.WindowsAzure.ServiceRuntime;
using Ninject;
using Repository;
using IDTO.Data;
using System.Web.Http.Tracing;
namespace IDTO.WebAPI
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            Register(GlobalConfiguration.Configuration);

            ///This line provides better error messages for "internal server error" for very low errors like missing libraries.
            GlobalConfiguration.Configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;

            BasicAuthentication.BasicAuthentication.Init();
        }
        /// <summary>
        /// Setting the Trace Writer
        /// To enable tracing, you must configure Web API to use your ITraceWriter implementation. You do this through the HttpConfiguration object,
        /// </summary>
        /// <param name="config"></param>
        private static void Register(HttpConfiguration config)
        {
            config.Services.Replace(typeof(ITraceWriter), new CustomTracer());
        }
    }
}
