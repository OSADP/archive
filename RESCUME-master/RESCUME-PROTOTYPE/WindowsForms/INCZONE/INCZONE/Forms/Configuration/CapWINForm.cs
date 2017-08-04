using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using INCZONE.Common;
using INCZONE.Managers;
using INCZONE.Model;
using INCZONE.Repositories;
using log4net;

namespace INCZONE.Forms.Configuration
{
    public partial class CapWINForm : BaseForm
    {
        readonly IUnitOfWork _uow;
        private static readonly ILog log = LogManager.GetLogger(SystemConstants.Logger_Ref);
        private bool IsFirstConfig = true;

        /// <summary>
        /// 
        /// </summary>
        public CapWINForm(Form parentForm)
        {
//            log.Debug("In CapWINForm Constructor");
            string connectionString = ConfigurationManager.ConnectionStrings["IncZoneEntities"].ConnectionString;
            this._uow = new UnitOfWork(connectionString);

            InitializeComponent();
            this.MdiParent = parentForm;

            ((IncZoneMDIParent)parentForm).RequestLabelTextChange += CapWINRequestLabelTextChange;

            try
            {
                _InitializeForm();
            }
            catch (Exception ex)
            {
                log.Error("CapWINForm Exception", ex);
                MessageBox.Show("The Cap WIN form could not be initialized, an error occured", SystemConstants.MessageBox_Caption_Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    XmlDocument test = CapWINManager.TestCapWINSettings(hostURLTb.Text, usernameTb.Text, passwordTb.Text);
                    Cursor.Current = Cursors.Default;
                    if (test != null)
                    {
                        MessageBox.Show("The connection was succefull", "Test Confirmation", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("The connection falied", "Test Confirmation", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                }
            }
            catch (Exception ex)
            {
                Cursor.Current = Cursors.Default;
                log.Error("testConnectionBt_Click Exception", ex);
                MessageBox.Show("The connection test could not be completed, an error occured", SystemConstants.MessageBox_Caption_Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveConfigurationBt_Click(object sender, EventArgs e)
        {
//            log.Debug("In saveConfigurationBt_Click");

            string saveType = string.Empty;

            try
            {
                if (_ValidFormValues())
                {
                    if (IsFirstConfig)
                    {
                        Cursor.Current = Cursors.WaitCursor;

                        CapWINConfiguration entity = new CapWINConfiguration()
                        {
                            HostURL = hostURLTb.Text,
                            Password = passwordTb.Text,
                            Username = usernameTb.Text,
                            BaudRate = baudRateTb.Text,
                            ComPort = comPortCb.SelectedText,
                            DistanceToIncident = (int)distanceNUD.Value,
                            LaneData = (int)laneDataNUD.Value
                        };

                        _uow.CapWINConfigurations.Add(entity);
                        _uow.Commit();
                        ((IncZoneMDIParent)this.MdiParent)._CapWinConfig = new CapWINConfig(entity);
                        LogEventsManager.LogEvent("CapWIN Configuration Added", LogEventTypes.CAPWIN_CONFIG_INIT, LogLevelTypes.INFO);
                        saveType = "added";
                        Cursor.Current = Cursors.Default;
                    }
                    else
                    {
                        Cursor.Current = Cursors.WaitCursor;
                        CapWINConfiguration entity = _uow.CapWINConfigurations.FindAll().FirstOrDefault();

                        if (entity != null)
                        {
                            entity.HostURL = hostURLTb.Text;
                            entity.Password = passwordTb.Text;
                            entity.Username = usernameTb.Text;
                            entity.BaudRate = baudRateTb.Text;
                            entity.ComPort = (string)comPortCb.SelectedValue;
                            entity.DistanceToIncident = (int)distanceNUD.Value;
                            entity.LaneData = (int)laneDataNUD.Value;
                            _uow.CapWINConfigurations.Set(entity.Id, entity);
                            _uow.Commit();
                            ((IncZoneMDIParent)this.MdiParent)._CapWinConfig = new CapWINConfig(entity);
                            LogEventsManager.LogEvent("CapWIN Configuration updated", LogEventTypes.CAPWIN_CONFIG, LogLevelTypes.INFO);
                            saveType = "updated";
                        }
                        else
                        {
                            Cursor.Current = Cursors.Default;
                            throw new Exception("The CapWIN configuration could not be found to update");
                        }
                        Cursor.Current = Cursors.Default;
                    }

                    MessageBox.Show("The configuration was " + saveType, SystemConstants.MessageBox_Caption_SaveConfirm, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    _InitializeForm();
                }
            }
            catch (Exception ex)
            {
                Cursor.Current = Cursors.Default;
                log.Error("saveConfigurationBt_Click Exception", ex);
                MessageBox.Show("The configuration was not saved, an error occured", SystemConstants.MessageBox_Caption_Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cancelConfigurationBt_Click(object sender, EventArgs e)
        {
            (new MainForm((IncZoneMDIParent)this.MdiParent)).Show();
            this.Close();
        }

        #region Private Form Methods
        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="Exception"></exception>
        private void _InitializeForm()
        {
 //           log.Debug("In _InitializeForm");

            //CapWINConfiguration entity = _uow.CapWINConfigurations.FindAll().FirstOrDefault();
            CapWINConfig CapWinConfig = ((IncZoneMDIParent)this.MdiParent)._CapWinConfig;

            try
            {
                capWinConnectStatusLb.Text = ((IncZoneMDIParent)this.MdiParent).GetConnectionStatus(ConnectionType.CAP_WIN);

                if (CapWinConfig == null)
                {
                    this.hostURLTb.Text = "";
                    this.usernameTb.Text = "";
                    this.passwordTb.Text = "";
                    string [] serialPorts = System.IO.Ports.SerialPort.GetPortNames();

                    if (serialPorts != null)
                    {
                        ComPort comPort = new ComPort();
                        comPort.Id = "";
                        comPort.Name = "Please select a COM Port...";
                        List<ComPort> portList = new List<ComPort>();
                        portList.Add(comPort);
                        foreach (string port in serialPorts)
                        {
                            comPort = new ComPort();
                            comPort.Id = port;
                            comPort.Name = port;
                            portList.Add(comPort);
                        }
                        this.comPortCb.DisplayMember = "Name";
                        this.comPortCb.ValueMember = "Id";
                        this.comPortCb.DataSource = portList;
                    }
                    this.comPortCb.SelectedIndex = 0;
                    this.baudRateTb.Text = "";
                    this.distanceNUD.Value = 3;
                    this.laneDataNUD.Value = 10;
                } 
                else 
                {
                    this.hostURLTb.Text = CapWinConfig.HostURL;
                    this.usernameTb.Text = CapWinConfig.Username;
                    this.passwordTb.Text = CapWinConfig.Password;
                    string[] serialPorts = System.IO.Ports.SerialPort.GetPortNames();

                    if (serialPorts != null)
                    {
                        ComPort comPort = new ComPort();
                        comPort.Id = "";
                        comPort.Name = "Please select a COM Port...";
                        List<ComPort> portList = new List<ComPort>();
                        portList.Add(comPort);
                        foreach (string port in serialPorts)
                        {
                            comPort = new ComPort();
                            comPort.Id = port;
                            comPort.Name = port;
                            portList.Add(comPort);
                        }
                        this.comPortCb.DisplayMember = "Name";
                        this.comPortCb.ValueMember = "Id";
                        this.comPortCb.DataSource = portList;
                    }
                    this.comPortCb.SelectedValue = CapWinConfig.ComPort;
                    this.baudRateTb.Text = CapWinConfig.BaudRate;
                    this.distanceNUD.Value = CapWinConfig.DistanceToIncident;
                    this.laneDataNUD.Value = CapWinConfig.LaneData;
                    IsFirstConfig = false;
                }
            }
            catch (Exception ex)
            {
                log.Error("Exception", ex);
                throw ex;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>bool</returns>
        /// <exception cref="Exception"></exception>
        private bool _ValidFormValues()
        {
//            log.Debug("In _ValidFormValues");

            try
            {
                capWINErrorProvider.Clear();

                if (string.IsNullOrEmpty(hostURLTb.Text))
                {
                    capWINErrorProvider.SetError(hostURLTb, "Host Url requires a value");
                    return false;
                }
                else
                {
                    capWINErrorProvider.SetError(hostURLTb, "");
                }
                if (!IsValidURL(hostURLTb.Text))
                {
                    capWINErrorProvider.SetError(hostURLTb, "Host Url requires a valdi Url");
                    return false;
                }
                else
                {
                    capWINErrorProvider.SetError(hostURLTb, "");
                }
                if (string.IsNullOrEmpty(usernameTb.Text))
                {
                    capWINErrorProvider.SetError(usernameTb, "Username requires a value");
                    return false;
                }
                else
                {
                    capWINErrorProvider.SetError(usernameTb, "");
                }
                if (string.IsNullOrEmpty(passwordTb.Text))
                {
                    capWINErrorProvider.SetError(passwordTb, "Password requires a value");
                    return false;
                }
                else
                {
                    capWINErrorProvider.SetError(passwordTb, "");
                }
                if (string.IsNullOrEmpty((string)comPortCb.SelectedValue))
                {
                    capWINErrorProvider.SetError(comPortCb, "Com Port requires a value");
                    return false;
                }
                else
                {
                    capWINErrorProvider.SetError(comPortCb, "");
                }
                if (string.IsNullOrEmpty(baudRateTb.Text))
                {
                    capWINErrorProvider.SetError(baudRateTb, "Baud Rate requires a value");
                    return false;
                }
                else
                {
                    capWINErrorProvider.SetError(baudRateTb, "");
                }
            }
            catch (Exception ex)
            {
                log.Error("Exception", ex);
                throw ex;
            }

            return true;
        }

        private bool _ValidTestFormValues()
        {
//            log.Debug("In _ValidTestFormValues");

            try
            {
                capWINErrorProvider.Clear();

                if (string.IsNullOrEmpty(hostURLTb.Text))
                {
                    capWINErrorProvider.SetError(hostURLTb, "Host Url requires a value");
                    return false;
                }
                else
                {
                    capWINErrorProvider.SetError(hostURLTb, "");
                }
                if (!IsValidURL(hostURLTb.Text))
                {
                    capWINErrorProvider.SetError(hostURLTb, "Host Url requires a valdi Url");
                    return false;
                }
                else
                {
                    capWINErrorProvider.SetError(hostURLTb, "");
                }
                if (string.IsNullOrEmpty(usernameTb.Text))
                {
                    capWINErrorProvider.SetError(usernameTb, "Username requires a value");
                    return false;
                }
                else
                {
                    capWINErrorProvider.SetError(usernameTb, "");
                }
                if (string.IsNullOrEmpty(passwordTb.Text))
                {
                    capWINErrorProvider.SetError(passwordTb, "Password requires a value");
                    return false;
                }
                else
                {
                    capWINErrorProvider.SetError(passwordTb, "");
                }

            }
            catch (Exception ex)
            {
                log.Error("Exception", ex);
                throw ex;
            }

            return true;
        }

        private bool IsValidURL(string uriName)
        {
            Uri uriResult;

            bool result = Uri.TryCreate(uriName, UriKind.Absolute, out uriResult) && uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps;

            return result;
        }

        #endregion

        private void capWinConnectStatusLb_TextChanged(object sender, EventArgs e)
        {

        }

        void CapWINRequestLabelTextChange(string newText)
        {
            capWinConnectStatusLb.Text = newText;
        }

        private void laneDataNUD_ValueChanged(object sender, EventArgs e)
        {

        }

        private void CapWINForm_Load(object sender, EventArgs e)
        {

        }
    }
}
