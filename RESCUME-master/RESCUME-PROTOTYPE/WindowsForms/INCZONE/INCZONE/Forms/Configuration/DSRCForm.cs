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
using INCZONE.Managers;
using INCZONE.Model;
using INCZONE.Repositories;
using log4net;

namespace INCZONE.Forms.Configuration
{
    public partial class DSRCForm : BaseForm
    {
        readonly IUnitOfWork _uow;
        private static readonly ILog log = LogManager.GetLogger(SystemConstants.Logger_Ref);
        private bool IsFirstConfig = true;

        public DSRCForm(Form form)
        {
            this.MdiParent = form;
//            log.Debug("In DSRCForm Constructor");

            string connectionString = ConfigurationManager.ConnectionStrings["IncZoneEntities"].ConnectionString;
            this._uow = new UnitOfWork(connectionString);

            InitializeComponent();
            try
            {
                _InitializeForm();
            }
            catch (Exception ex)
            {
                log.Error("DSRCForm Exception", ex);
                MessageBox.Show("The DSRC form could not be initialized, an error occured", SystemConstants.MessageBox_Caption_Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void saveConfigBt_Click(object sender, EventArgs e)
        {
//            log.Debug("In saveConfigBt_Click");

            string saveType = string.Empty;
            DSRCConfiguration entity = null;

            try
            {
                if (_ValidFormValues())
                {
                    if (IsFirstConfig)
                    {
                        entity = new DSRCConfiguration()
                        {
                            ACM = (int)acmNUD.Value,
                            BSM = (int)bsmNUD.Value,
                            EVA = (int)evaNUD.Value,
                            TIM = (int)timNUD.Value
                        };

                        _uow.DSRCConfigurations.Add(entity);
                        _uow.Commit();
                        LogEventsManager.LogEvent("DSRC Configuration Added", LogEventTypes.DGPS_CONFIG_INIT, LogLevelTypes.INFO);
                        saveType = "added";
                    }
                    else
                    {
                        entity = _uow.DSRCConfigurations.FindAll().FirstOrDefault();

                        if (entity != null)
                        {
                            entity.ACM = (int)acmNUD.Value;
                            entity.BSM = (int)bsmNUD.Value;
                            entity.EVA = (int)evaNUD.Value;
                            entity.TIM = (int)timNUD.Value;

                            _uow.DSRCConfigurations.Set(entity.Id, entity);
                            _uow.Commit();
                            LogEventsManager.LogEvent("DSRC Configuration updated", LogEventTypes.DSRC_CONFIG, LogLevelTypes.INFO);
                            saveType = "updated";
                        }
                        else
                        {
                            throw new Exception("The DSRC configuration could not be found to update");
                        }
                    }

                    ((IncZoneMDIParent)this.MdiParent)._DSRCConfig = new DSRCConfig(entity);
                    MessageBox.Show("The configuration was " + saveType, SystemConstants.MessageBox_Caption_SaveConfirm, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    _InitializeForm();
                }
            }
            catch (Exception ex)
            {
                log.Error("saveConfigBt_Click Exception", ex);
                MessageBox.Show("The configuration was not saved, an error occured", SystemConstants.MessageBox_Caption_Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void cancelConfigBt_Click(object sender, EventArgs e)
        {
            (new MainForm((IncZoneMDIParent)this.MdiParent)).Show();
            this.Close();
        }

        #region Private Class Methods
        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="Exception"></exception>
        private void _InitializeForm()
        {
//            log.Debug("In _InitializeForm");

            try
            {
                dsrcConnectStatusLb.Text = ((IncZoneMDIParent)this.MdiParent).GetConnectionStatus(ConnectionType.DSCR);

                DSRCConfiguration entity = _uow.DSRCConfigurations.FindAll().FirstOrDefault();
                if (entity == null)
                {
                    this.acmNUD.Value = 10;
                    this.bsmNUD.Value = 10;
                    this.evaNUD.Value = 10;
                    this.timNUD.Value = 10;
                }
                else
                {
                    IsFirstConfig = false;
                    this.acmNUD.Value = entity.ACM;
                    this.bsmNUD.Value = entity.BSM;
                    this.evaNUD.Value = entity.EVA;
                    this.timNUD.Value = entity.TIM;
                }
            }
            catch (Exception ex)
            {
                log.Error("Exception", ex);
                throw ex;
            }
        }

        private bool _ValidFormValues()
        {
            return true;
        }
        #endregion
    }
}
