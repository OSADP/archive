using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using INCZONE.Common;
using INCZONE.Forms.Configuration;
using INCZONE.Managers;
using INCZONE.Model;
using INCZONE.Repositories;
using log4net;

namespace INCZONE.Forms.Log
{
    public partial class EventLogForm : Form
    {
        readonly IUnitOfWork _uow;
        private static readonly ILog log = LogManager.GetLogger(SystemConstants.Logger_Ref);

        /// <summary>
        /// Default Constructor
        /// </summary>
        public EventLogForm(Form form)
        {
            this.MdiParent = form;
//            log.Debug("In Error Constructor");
            string connectionString = ConfigurationManager.ConnectionStrings["IncZoneEntities"].ConnectionString;
            this._uow = new UnitOfWork(connectionString);

            InitializeComponent();
            try
            {
                _InitializeForm();
            }
            catch (Exception ex)
            {
                log.Error("Exception", ex);
                LogEventsManager.LogEvent(ex.Message, LogEventTypes.SYSTEM_ERROR, LogLevelTypes.ERROR);
                MessageBox.Show("The Event Log form could not be initialized, an error occured", SystemConstants.MessageBox_Caption_Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void showAllEventsBt_Click(object sender, EventArgs e)
        {
//            log.Debug("In showAllEventsBt_Click");
            IQueryable<EventLog> list = null;

            try
            {
                Cursor.Current = Cursors.WaitCursor;
                list = _uow.EventLogs.FindAll();
                logEventsGrid.Rows.Clear();
                foreach (EventLog entity in list)
                {
                    logEventsGrid.Rows.Add(entity.Id, entity.EventDate, entity.EventMessage, entity.EventType.Name);
                }
                Cursor.Current = Cursors.Default;
            }
            catch (Exception ex)
            {
                log.Error("showAllEventsBt_Click Exception", ex);
                LogEventsManager.LogEvent(ex.Message, LogEventTypes.SYSTEM_ERROR, LogLevelTypes.ERROR);
                MessageBox.Show("The Show All events could not be completed, an error occured", SystemConstants.MessageBox_Caption_Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void filterEventsBt_Click(object sender, EventArgs e)
        {
//            log.Debug("In filterEventsBt_Click");

            try
            {
                Cursor.Current = Cursors.WaitCursor;
                DateTime begin = beginDate.Value.Date;
                DateTime end = endDate.Value.AddDays(1).Date.AddSeconds(-1);
                int eventType = (int)eventTypeCb.SelectedValue;
                int logLevel = (int)logLevelCb.SelectedValue;

                _PopulateEventGrid(begin, end, eventType, logLevel);
                Cursor.Current = Cursors.Default;
            }
            catch (Exception ex)
            {
                log.Error("filterEventsBt_Click Exception", ex);
                LogEventsManager.LogEvent(ex.Message, LogEventTypes.SYSTEM_ERROR, LogLevelTypes.ERROR);
                MessageBox.Show("The list filter could not be completed, an error occured", SystemConstants.MessageBox_Caption_Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void beginDate_ValueChanged(object sender, EventArgs e)
        {
 //           log.Debug("In beginDate_ValueChanged");

            try
            {
                this.beginDate.MaxDate = this.endDate.Value;
                this.endDate.MinDate = this.beginDate.Value;
            }
            catch (Exception ex)
            {
                log.Error("beginDate_ValueChanged Exception", ex);
                LogEventsManager.LogEvent(ex.Message, LogEventTypes.SYSTEM_ERROR, LogLevelTypes.ERROR);
                MessageBox.Show("The Begin Date change could not be completed, an error occured", SystemConstants.MessageBox_Caption_Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void endDate_ValueChanged(object sender, EventArgs e)
        {
//            log.Debug("In beginDate_ValueChanged");

            try
            {
                this.beginDate.MaxDate = this.endDate.Value;
                this.endDate.MinDate = this.beginDate.Value;
            }
            catch (Exception ex)
            {
                log.Error("endDate_ValueChanged Exception", ex);
                LogEventsManager.LogEvent(ex.Message, LogEventTypes.SYSTEM_ERROR, LogLevelTypes.ERROR);
                MessageBox.Show("The End Date change could not be completed, an error occured", SystemConstants.MessageBox_Caption_Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #region Form Private Methods
        /// <summary>
        /// Used to intialize the Error Log Forms components
        /// </summary>
        /// <exception cref="Exception"></exception>
        private void _InitializeForm()
        {
//            log.Debug("In _InitializeForm");

            try
            {
                //Init the Event Types
                this.eventTypeCb.DisplayMember = "Name";
                this.eventTypeCb.ValueMember = "Id";
                List<EventType> listEventType = _uow.EventTypes.FindAll().ToList<EventType>();
                EventType entityEventType = new EventType();
                entityEventType.Id = 0;
                entityEventType.Name = "Please select a Event Type...";
                listEventType.Add(entityEventType);
                this.eventTypeCb.DataSource = listEventType.OrderBy(m => m.Id).ToList<EventType>();

                //Init the Log Levels
                this.logLevelCb.DisplayMember = "Name";
                this.logLevelCb.ValueMember = "Id";
                List<LogLevel> listLogLevel = _uow.LogLevels.FindAll().ToList<LogLevel>();
                LogLevel entityLogLevel = new LogLevel();
                entityLogLevel.Id = 0;
                entityLogLevel.Name = "Please select a Log Level...";
                listLogLevel.Add(entityLogLevel);
                this.logLevelCb.DataSource = listLogLevel.OrderBy(m => m.Id).ToList<LogLevel>();

                //Init the Date fields
                this.beginDate.Value = DateTime.Now.AddDays(-7);
                this.beginDate.MaxDate = DateTime.Now;
                this.endDate.Value = DateTime.Now;
                this.endDate.MinDate = this.beginDate.Value;
            }
            catch (Exception ex)
            {
                log.Error("_InitializeForm Exception", ex);
                throw ex;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <param name="eventType"></param>
        /// <param name="logLevel"></param>
        /// <exception cref="General  Exception"></exception>
        private void _PopulateEventGrid(DateTime begin, DateTime end, int eventType, int logLevel)
        {
//            log.Debug("In _PopulateEventGrid");

            IQueryable<EventLog> list = null;

            try
            {
                list = _uow.EventLogs.FindAll();

                if (eventType != 0)
                    list = list.Where(m => m.EventType.Id == eventType);

                if (logLevel != 0)
                    list = list.Where(m => m.LogLevel.Id == logLevel);

                list = list.Where(m => m.EventDate >= begin && m.EventDate <= end)
                    .OrderBy(m => m.EventDate)
                    .OrderBy(m => m.EventType.Name)
                    .OrderBy(m => m.LogLevel.Name);

                logEventsGrid.Rows.Clear();
                foreach (EventLog entity in list)
                {
                    logEventsGrid.Rows.Add(entity.Id, entity.EventDate, entity.EventMessage, entity.EventType.Name);
                }
            }
            catch (Exception ex)
            {
                log.Error("_PopulateEventGrid Exception", ex);
                throw ex;
            }
        }
        #endregion

        private void eventTypeCb_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void logLevelCb_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void logEventsGrid_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            int Id = (int)logEventsGrid.SelectedRows[0].Cells[0].Value;
            EventLog eventLog = _uow.EventLogs.FindById(Id);

            if (eventLog.EventInfo != "NONE")
            {
                (new EventInfoForm(eventLog.EventInfo)).ShowDialog();
            }
        }
    }
}
