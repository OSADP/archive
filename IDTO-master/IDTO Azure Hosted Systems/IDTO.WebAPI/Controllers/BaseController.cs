using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Repository;
using Microsoft.WindowsAzure.ServiceRuntime;
using System.Diagnostics;
using Ninject;
using Repository.Providers.EntityFramework;
using IDTO.Data;
using IDTO.Entity.Models;
using System.Web.Http.Tracing;

namespace IDTO.WebAPI.Controllers
{
    public abstract class BaseController : ApiController
    {
        protected IDbContext db;//= new IDTOContext();
        public readonly IUnitOfWork Uow;
        //protected BaseController()
        //{
        //    Uow = WebApiApplication.Kernel.Get<IUnitOfWork>();
        //}

        //protected BaseController(IUnitOfWork repository)
        //{
        //    Uow = repository;

        //}
        protected BaseController(IDbContext context)
        {
            db = context;
            Uow = new UnitOfWork(context);
        }

        protected string RecordException(Exception ex, string methodName)
        {

            string exMessage = "Message: " + ex.Message;
            exMessage += Environment.NewLine + "Source: " + ex.Source;
            if (ex.InnerException != null)
            {

                exMessage += Environment.NewLine + "Inner Exception: ";
                exMessage += ex.InnerException.Message;
                if (ex.InnerException.InnerException != null)
                {
                    exMessage += Environment.NewLine;
                    exMessage += ex.InnerException.InnerException.Message;
                }
            }
            exMessage += Environment.NewLine + "StackTrace: " + ex.StackTrace;

            //Log it
              Log(System.Web.Http.Tracing.TraceLevel.Error,methodName, exMessage);

            return exMessage;


        }
        /// <summary>
        /// Logs to the TraceWriter, if available.
        /// </summary>
        protected void Log(System.Web.Http.Tracing.TraceLevel level, string methodName, string msg)
        {

            msg = "Method: " + methodName + ".  " + msg;//prepend method name
            ITraceWriter tracewriter = Configuration.Services.GetTraceWriter();
            if (tracewriter != null)
            {
               
                switch (level)
                {
                    case System.Web.Http.Tracing.TraceLevel.Error:
                        tracewriter.Error(Request, "", msg);
                        break;

                    case System.Web.Http.Tracing.TraceLevel.Warn:
                        tracewriter.Warn(Request, "", msg);
                        break;

                    case System.Web.Http.Tracing.TraceLevel.Info:
                        tracewriter.Info(Request, "", msg);
                        break;

                    default:
                        tracewriter.Debug(Request, "", msg);
                        break;
                }  
            }
        }
    }
}
