using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;
using INCZONE.Common;
using INCZONE.Managers;
using INCZONE.Model;
using INCZONE.Repositories;
using log4net;

namespace INCZONE.Forms.Configuration
{
    public delegate void UpdateDGPSConfigDelegate(DGPSConfig dgpsConfig);

    public partial class DGPSForm : BaseForm
    {
        readonly IUnitOfWork _uow;
        private static readonly ILog log = LogManager.GetLogger(SystemConstants.Logger_Ref);
        private bool IsFirstConfig = true;

        /// <summary>
        /// Default Constructor
        /// </summary>
        public DGPSForm(Form parentForm)
        {
//            log.Debug("In DGPSForm Constructor");
            this.MdiParent = parentForm;
            
            string connectionString = ConfigurationManager.ConnectionStrings["IncZoneEntities"].ConnectionString;
            this._uow = new UnitOfWork(connectionString);
            ((IncZoneMDIParent)parentForm).RequestLabelTextChange += DGPSRequestLabelTextChange;

            InitializeComponent();
            try
            {
                _InitializeForm();
            }
            catch (Exception ex)
            {
                log.Error("DGPSForm Exception", ex);
                MessageBox.Show("The DGPS form could not be initialized, an error occured", SystemConstants.MessageBox_Caption_Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void deleteConfigBt_Click(object sender, EventArgs e)
        {
//            log.Debug("In deleteConfigBt_Click");

            try
            {
                if ((int)this.savedConfigsCb.SelectedValue != 0)
                {
                    IQueryable<DGPSConfiguration> configList = _uow.DGPSConfigurations.FindAll();
                    DGPSConfiguration entity = _uow.DGPSConfigurations.FindById((int)this.savedConfigsCb.SelectedValue);

                    if (configList.Count() < 2)
                    {                       
                        if (MessageBox.Show("The selected configuration is the last configuration.  If you continue the system will not receive DGPS data", SystemConstants.MessageBox_Caption_DeleteConfirg, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
                        {
                            _uow.DGPSConfigurations.Remove(entity);
                            _uow.Commit();
                            LogEventsManager.LogEvent("The Last DGPS Configuration Removed", LogEventTypes.DGPS_CONFIG, LogLevelTypes.INFO);
                            _InitializeForm();
                            MessageBox.Show("The selected configuration was deleted", SystemConstants.MessageBox_Caption_DeleteConfirg, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    else
                    {
                        DGPSConfiguration isDefault = _uow.DGPSConfigurations.FindWhere(m => m.IsDefault == true).FirstOrDefault();

                        if (!entity.Equals(isDefault))
                        {
                            _uow.DGPSConfigurations.Remove(entity);
                            _uow.Commit();
                            LogEventsManager.LogEvent("DGPS Configuration Removed", LogEventTypes.DGPS_CONFIG, LogLevelTypes.INFO);
                            _InitializeForm();
                            MessageBox.Show("The selected configuration was deleted", SystemConstants.MessageBox_Caption_DeleteConfirg, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            if (MessageBox.Show("The selected configuration is the default configuration.  If you continue the system will not receive DGPS data until a default configuration is selected", SystemConstants.MessageBox_Caption_DeleteConfirg, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
                            {
                                _uow.DGPSConfigurations.Remove(entity);
                                _uow.Commit();
                                LogEventsManager.LogEvent("Default DGPS Configuration Removed", LogEventTypes.DGPS_CONFIG, LogLevelTypes.INFO);
                                _InitializeForm();
                                MessageBox.Show("The selected configuration was deleted", SystemConstants.MessageBox_Caption_DeleteConfirg, MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("deleteConfigBt_Click Exception", ex);
                MessageBox.Show("The selected configuration was not deleted, an error occured", SystemConstants.MessageBox_Caption_Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void newConfigBt_Click(object sender, EventArgs e)
        {
//            log.Debug("In newConfigBt_Click");

            try
            {
                _initFieldsAndButtons();
            }
            catch (Exception ex)
            {
                log.Error("newConfigBt_Click Exception", ex);
                MessageBox.Show("Could not clear form, an error occured", SystemConstants.MessageBox_Caption_Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void testConnectionBt_Click(object sender, EventArgs e)
        {
//            log.Debug("In testConnectionBt_Click");

            try
            {
                if (_ValidTestFormValues())
                {
                    Cursor.Current = Cursors.WaitCursor;
                    NTripManager NTripManager = new NTripManager(new IPEndPoint(IPAddress.Parse(hostIPTb.Text), Convert.ToInt32(hostPortTb.Text)), usernameTb.Text, passwordTb.Text);
                    bool IsConfiguered = NTripManager.IsDGPSConfiguredCorrectly(new IPEndPoint(IPAddress.Parse(hostIPTb.Text), Convert.ToInt32(hostPortTb.Text)), usernameTb.Text, passwordTb.Text);
                    Cursor.Current = Cursors.Default;

                    if (IsConfiguered)
                    {
                        MessageBox.Show("The connection was succefull", SystemConstants.MessageBox_Caption_TestConfirm, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("The connection falied", SystemConstants.MessageBox_Caption_TestConfirm, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("EtestConnectionBt_Click xception", ex);
                MessageBox.Show("The connection test could not be completed, an error occured", SystemConstants.MessageBox_Caption_Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveConfig_Click(object sender, EventArgs e)
        {
//            log.Debug("In saveConfig_Click");

            try
            {
                int Id = (int)this.savedConfigsCb.SelectedValue;
                string saveType = string.Empty;

                if (_ValidFormValues())
                {
                    if (Id == 0)
                    {
                        DGPSConfiguration entity = new DGPSConfiguration()
                        {
                            HostIP = this.hostIPTb.Text,
                            HostPort = this.hostPortTb.Text,
                            IsDefault = this.isDefaultCheckB.Checked,
                            LocationRefreshRate = ((int)this.locationRefreshNUD.Value) * 1000,
                            Name = this.configNameTb.Text,
                            Password = this.passwordTb.Text,
                            RefreshRate = ((int)this.dgpsRefreshNUD.Value) * 1000,
                            Username = this.usernameTb.Text
                        };

                        if (this.isDefaultCheckB.Checked)
                        {
                            DGPSConfiguration defaultCheck = _uow.DGPSConfigurations.FindWhere(m => m.IsDefault == true).FirstOrDefault();
                            if (defaultCheck != null)
                            {
                                defaultCheck.IsDefault = false;
                                _uow.DGPSConfigurations.Set(defaultCheck.Id, defaultCheck);
                            }
                        }
                        _uow.DGPSConfigurations.Add(entity);
                        _uow.Commit();
                        //TODO:Start Service
                        ((IncZoneMDIParent)this.MdiParent)._DGPSConfig = new DGPSConfig(entity);
                        if (IsFirstConfig)
                        {
                            LogEventsManager.LogEvent("DGPS Configuration Added", LogEventTypes.DGPS_CONFIG_INIT, LogLevelTypes.INFO);
                        }
                        else
                        {
                            LogEventsManager.LogEvent("DGPS Configuration Added", LogEventTypes.DGPS_CONFIG, LogLevelTypes.INFO);
                        }
                        saveType = "added";
                    }
                    else
                    {
                        DGPSConfiguration entity = _uow.DGPSConfigurations.FindById((int)this.savedConfigsCb.SelectedValue);

                        entity.HostIP = this.hostIPTb.Text;
                        entity.HostPort = this.hostPortTb.Text;
                        entity.IsDefault = this.isDefaultCheckB.Checked;
                        entity.LocationRefreshRate = ((int)this.locationRefreshNUD.Value) * 1000;
                        entity.Name = this.configNameTb.Text;
                        entity.Password = this.passwordTb.Text;
                        entity.RefreshRate = ((int)this.dgpsRefreshNUD.Value) * 1000;
                        entity.Username = this.usernameTb.Text;

                        if (this.isDefaultCheckB.Checked)
                        {
                            DGPSConfiguration defaultCheck = _uow.DGPSConfigurations.FindWhere(m => m.IsDefault == true).FirstOrDefault();
                            if (defaultCheck != null && !entity.Equals(defaultCheck))
                            {
                                defaultCheck.IsDefault = false;
                                _uow.DGPSConfigurations.Set(defaultCheck.Id, defaultCheck);
                            }
                        }

                        _uow.DGPSConfigurations.Set(Id, entity);
                        _uow.Commit();
                        ((IncZoneMDIParent)this.MdiParent)._DGPSConfig = new DGPSConfig(entity);
                        saveType = "updated";
                    }
                    MessageBox.Show("The configuration was " + saveType, SystemConstants.MessageBox_Caption_SaveConfirm, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    _InitializeForm();
                }
            }
            catch (Exception ex)
            {
                log.Error("saveConfig_Click Exception", ex);
                MessageBox.Show("The configuration was not saved, an error occured", SystemConstants.MessageBox_Caption_Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cancelConfigBt_Click(object sender, EventArgs e)
        {
            (new MainForm((IncZoneMDIParent)this.MdiParent)).Show();
            this.Close();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void savedConfigsCb_SelectedIndexChanged(object sender, EventArgs e)
        {
//            log.Debug("In savedConfigsCb_SelectedIndexChanged");

            try
            {
                if ((int)savedConfigsCb.SelectedValue != 0)
                {
                    Cursor.Current = Cursors.WaitCursor;
                    deleteConfigBt.Enabled = true;
                    _PopulateFields((int)savedConfigsCb.SelectedValue);
                    Cursor.Current = Cursors.Default;
                }
                else
                {
                    _initFieldsAndButtons();
                }
            }
            catch (Exception ex)
            {
                log.Error("savedConfigsCb_SelectedIndexChanged Exception", ex);
                MessageBox.Show("The select configuration could not be displayed, an error occured", SystemConstants.MessageBox_Caption_Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #region Private Form Methods
        /// <summary>
        /// Used to Init the Form
        /// </summary>
        /// <exception cref="Exception"></exception>
        private void _InitializeForm()
        {
//            log.Debug("In _InitializeForm");
            DGPSConfig DGPSConfig = ((IncZoneMDIParent)this.MdiParent)._DGPSConfig;

            try
            {
                dgpsConnectionStatusLb.Text = ((IncZoneMDIParent)this.MdiParent).GetConnectionStatus(ConnectionType.DGPS);

                //Init the DGPS Configurations
                this.savedConfigsCb.DisplayMember = "Name";
                this.savedConfigsCb.ValueMember = "Id";
                List<DGPSConfiguration> list = _uow.DGPSConfigurations.FindAll().ToList<DGPSConfiguration>();

                if (list != null && list.Count > 0)
                {
                    IsFirstConfig = false;
                }

                DGPSConfiguration entity = new DGPSConfiguration();
                entity.Id = 0;
                entity.Name = "Please select a DGPS Configuration...";
                list.Add(entity);
                this.savedConfigsCb.DataSource = list.OrderBy(m => m.Id).ToList<DGPSConfiguration>();

                if (DGPSConfig == null)
                {
                    _initFieldsAndButtons();
                }
                else 
                {
                    _initFieldsAndButtons(DGPSConfig);
                }

            }
            catch (Exception ex)
            {
                log.Error("Exception", ex);
                throw ex;
            }
        }

        private void _initFieldsAndButtons(DGPSConfig DGPSConfig)
        {
//            log.Debug("In _initFieldsAndButtons DGPSConfig");

            try
            {
                this.deleteConfigBt.Enabled = false;

                this.configNameTb.Text = DGPSConfig.Name;
                this.hostIPTb.Text = DGPSConfig.HostIP;
                this.hostPortTb.Text = DGPSConfig.HostPort;
                this.usernameTb.Text = DGPSConfig.Username;
                this.passwordTb.Text = DGPSConfig.Password;
                this.dgpsRefreshNUD.Value = DGPSConfig.RefreshRate / 1000;
                this.locationRefreshNUD.Value = DGPSConfig.LocationRefreshRate / 1000;
                this.isDefaultCheckB.Checked = DGPSConfig.IsDefault;

                if (this.savedConfigsCb.Items != null && this.savedConfigsCb.Items.Count > 0)
                    this.savedConfigsCb.SelectedIndex = DGPSConfig.Id;
            }
            catch (Exception ex)
            {
                log.Error("Exception", ex);
                throw ex;
            }
        }

        /// <summary>
        /// Used to Init the Forms fields and buttons
        /// </summary>
        /// <exception cref="Exception"></exception>
        private void _initFieldsAndButtons()
        {
//            log.Debug("In _initFieldsAndButtons");

            try
            {
                this.deleteConfigBt.Enabled = false;

                this.configNameTb.Text = "";
                this.hostIPTb.Text = "";
                this.hostPortTb.Text = "";
                this.usernameTb.Text = "";
                this.passwordTb.Text = "";
                this.dgpsRefreshNUD.Value = 10;
                this.locationRefreshNUD.Value = 3;
                this.isDefaultCheckB.Checked = false;

                if (this.savedConfigsCb.Items != null && this.savedConfigsCb.Items.Count > 0)
                    this.savedConfigsCb.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                log.Error("Exception", ex);
                throw ex;
            }
        }

        private bool _ValidTestFormValues()
        {
//            log.Debug("In _ValidFormValues");

            try
            {
                int result;
                dgpsErrorProvider.Clear();

                if (string.IsNullOrEmpty(hostIPTb.Text))
                {
                    dgpsErrorProvider.SetError(hostIPTb, "Host Address requires a value");
                    return false;
                }
                else
                {
                    dgpsErrorProvider.SetError(hostIPTb, "");
                }
                if (!IsValidIp(hostIPTb.Text))
                {
                    dgpsErrorProvider.SetError(hostIPTb, "Host Address requires a valdi IPv4 address");
                    return false;
                }
                else
                {
                    dgpsErrorProvider.SetError(hostIPTb, "");
                }
                if (!int.TryParse(hostPortTb.Text, out result))
                {
                    dgpsErrorProvider.SetError(hostPortTb, "Port requires a integer value");
                    return false;
                }
                else
                {
                    dgpsErrorProvider.SetError(hostPortTb, "");
                }
                if (string.IsNullOrEmpty(usernameTb.Text))
                {
                    dgpsErrorProvider.SetError(usernameTb, "Username requires a value");
                    return false;
                }
                else
                {
                    dgpsErrorProvider.SetError(usernameTb, "");
                }
                if (string.IsNullOrEmpty(passwordTb.Text))
                {
                    dgpsErrorProvider.SetError(passwordTb, "Password requires a value");
                    return false;
                }
                else
                {
                    dgpsErrorProvider.SetError(passwordTb, "");
                }
            }
            catch (Exception ex)
            {
                log.Error("Exception", ex);
                throw ex;
            }

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private bool _ValidFormValues()
        {
//            log.Debug("In _ValidFormValues");

            try
            {
                int result;
                dgpsErrorProvider.Clear();

                if (string.IsNullOrEmpty(configNameTb.Text))
                {
                    dgpsErrorProvider.SetError(configNameTb, "Configuration Name requires a value");
                    return false;
                }
                else
                {
                    dgpsErrorProvider.SetError(configNameTb, "");
                }
                if (string.IsNullOrEmpty(hostIPTb.Text))
                {
                    dgpsErrorProvider.SetError(hostIPTb, "Host Address requires a value");
                    return false;
                }
                else
                {
                    dgpsErrorProvider.SetError(hostIPTb, "");
                }
                if (!IsValidIp(hostIPTb.Text))
                {
                    dgpsErrorProvider.SetError(hostIPTb, "Host Address requires a valdi IPv4 address");
                    return false;
                }
                else
                {
                    dgpsErrorProvider.SetError(hostIPTb, "");
                }
                if (!int.TryParse(hostPortTb.Text, out result))
                {
                    dgpsErrorProvider.SetError(hostPortTb, "Port requires a integer value");
                    return false;
                }
                else
                {
                    dgpsErrorProvider.SetError(hostPortTb, "");
                }
                if (string.IsNullOrEmpty(usernameTb.Text))
                {
                    dgpsErrorProvider.SetError(usernameTb, "Username requires a value");
                    return false;
                }
                else
                {
                    dgpsErrorProvider.SetError(usernameTb, "");
                }
                if (string.IsNullOrEmpty(passwordTb.Text))
                {
                    dgpsErrorProvider.SetError(passwordTb, "Password requires a value");
                    return false;
                }
                else
                {
                    dgpsErrorProvider.SetError(passwordTb, "");
                }
            }
            catch (Exception ex)
            {
                log.Error("Exception", ex);
                throw ex;
            }

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Id"></param>
        /// <exception cref="Exception"></exception>
        private void _PopulateFields(int Id)
        {
//            log.Debug("In _PopulateFields");

            try
            {
                DGPSConfiguration entity = _uow.DGPSConfigurations.FindById(Id);

                this.configNameTb.Text = entity.Name;
                this.hostIPTb.Text = entity.HostIP;
                this.hostPortTb.Text = entity.HostPort;
                this.usernameTb.Text = entity.Username;
                this.passwordTb.Text = entity.Password;
                this.dgpsRefreshNUD.Value = entity.RefreshRate / 1000;
                this.locationRefreshNUD.Value = ((int)entity.LocationRefreshRate) / 1000;
                this.isDefaultCheckB.Checked = entity.IsDefault;
            }
            catch (Exception ex)
            {
                log.Error("Exception", ex);
                throw ex;
            }
        }
        #endregion

        void DGPSRequestLabelTextChange(string newText)
        {
            dgpsConnectionStatusLb.Text = newText;
        }
    }
}
