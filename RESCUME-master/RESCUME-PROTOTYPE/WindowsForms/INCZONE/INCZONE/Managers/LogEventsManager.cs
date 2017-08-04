using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using INCZONE.Model;
using INCZONE.Repositories;
using INCZONE.Common;
using log4net;
using System.Windows.Forms;
using System.Configuration;

namespace INCZONE.Managers
{
    public class LogEventsManager
    {
        readonly static IUnitOfWork _uow;
        private static readonly ILog log = LogManager.GetLogger(SystemConstants.Logger_Ref);

        static LogEventsManager() 
        {
            string connectionString = ConfigurationManager.ConnectionStrings["IncZoneEntities"].ConnectionString;
            _uow = new UnitOfWork(connectionString);        
        }

        /// <summary>
        /// Used to Log all System Events
        /// </summary>
        /// <param name="message"></param>
        /// <param name="eventType"></param>
        /// <param name="loglevel"></param>
        /// <exception cref=""></exception>
        public static void LogEvent(string message, LogEventTypes eventType, LogLevelTypes loglevel)
        {
            try 
            {
                EventLog entity = new EventLog()
                {
                    EventDate = DateTime.Now,
                    EventMessage = message,
                    EventType = _uow.EventTypes.FindById((int)eventType),
                    LogLevel = _uow.LogLevels.FindById((int)loglevel),
                    EventInfo = "NONE"
                };
                _uow.EventLogs.Add(entity);
                _uow.Commit();
            }
            catch (Exception e) 
            {
                log.Error("LogEvent Exception",e);
                MessageBox.Show("The Event Log could not be completed, an error occured", SystemConstants.MessageBox_Caption_Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static void LogEvent(string message, LogEventTypes eventType, LogLevelTypes loglevel, string EventInfo)
        {
            try
            {
                EventLog entity = new EventLog()
                {
                    EventDate = DateTime.Now,
                    EventMessage = message,
                    EventType = _uow.EventTypes.FindById((int)eventType),
                    LogLevel = _uow.LogLevels.FindById((int)loglevel),
                    EventInfo = EventInfo
                };
                _uow.EventLogs.Add(entity);
                _uow.Commit();
            }
            catch (Exception e)
            {
                log.Error("LogEvent Exception", e);
                MessageBox.Show("The Event Log could not be completed, an error occured", SystemConstants.MessageBox_Caption_Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
