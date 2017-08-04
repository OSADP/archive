using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Tracing;
using System.Net.Http;

namespace IDTO.WebAPI
{
    public class CustomTracer : ITraceWriter
    {
        public void Trace(HttpRequestMessage request, string category, TraceLevel level,
            Action<TraceRecord> traceAction)
        {
            TraceRecord rec = new TraceRecord(request, category, level);
            traceAction(rec);//don't know what this line does?
            WriteTrace(rec);
        }

        protected void WriteTrace(TraceRecord rec)
        {
            var message = string.Format("{0};{1};{2}",
                rec.Operator, rec.Operation, rec.Message);

           //Can't use WriteLine for all, or else they all come out level Verbose.
            //http://www.dotnetsolutions.co.uk/blog/windows-azure-diagnostics---why-the-trace-writeline-method-only-sends-verbose-messages

            switch (rec.Level)
            {
                case TraceLevel.Error:
                    System.Diagnostics.Trace.TraceError(message);
                    break;

                case TraceLevel.Warn:
                    System.Diagnostics.Trace.TraceWarning(message);
                    break;

                case TraceLevel.Info:
                    System.Diagnostics.Trace.TraceInformation(message);
                    break;

                default:
                    System.Diagnostics.Trace.WriteLine(message);
                    break;
            }  
        }
    }
}