using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using INCZONE.Common;
using INCZONE.Managers;
using INCZONE.Model;
using INCZONE.Repositories;
using log4net;
using Phidgets;
using Phidgets.Events;

namespace INCZONE.Forms.Configuration
{
    public partial class AlarmForm : Form
    {
        readonly IUnitOfWork _uow;
        private static readonly ILog log = LogManager.GetLogger(SystemConstants.Logger_Ref);
        private bool IsFirstConfig = true;
        private InterfaceKit ifKit;
        private ErrorEventBox errorBox;

        public AlarmForm(Form parentForm, InterfaceKit ifKit)
        {
//            log.Debug("In AlarmForm Constructor");
            this.MdiParent = parentForm;

            string connectionString = ConfigurationManager.ConnectionStrings["IncZoneEntities"].ConnectionString;
            this._uow = new UnitOfWork(connectionString);

            InitializeComponent();
            errorBox = new ErrorEventBox();
            this.ifKit = ifKit;
            try
            {
                _InitializeForm();
            }
            catch (Exception ex)
            {
                log.Error("AlarmForm Exception", ex);
                MessageBox.Show("The Alarm form could not be initialized, an error occured", SystemConstants.MessageBox_Caption_Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private void _PopulateFields(int Id)
        {
//            log.Debug("In _PopulateFields");

            try
            {
                AlarmConfiguration entity = _uow.AlarmConfigurations.FindById(Id);
                this.configNameTb.Text = entity.Name;
                this.isDefaultCB.Checked = entity.IsDefault;
                _SetAudibleVisualAlarms(entity);
                _SetVehicleAlarms(entity);
            }
            catch (Exception ex)
            {
                log.Error("Exception", ex);
                throw ex;
            }
        }

        private void _SetVehicleAlarms(AlarmConfiguration entity)
        {
            List<VehicleAlarm> list = entity.VehicleAlarms.OrderBy(m => m.AlarmLevel.Id).ToList();
            VehicleAlarm VehicleAlarm = null;

            VehicleAlarm = list[0];
            vehicleAlarmPersistence0Cb.SelectedValue = VehicleAlarm.Persistance.ToString();
            alarm0Cb.Checked = VehicleAlarm.Active;

            VehicleAlarm = list[1];
            vehicleAlarmPersistence1Cb.SelectedValue = VehicleAlarm.Persistance.ToString();
            alarm1Cb.Checked = VehicleAlarm.Active;

            VehicleAlarm = list[2];
            vehicleAlarmPersistence2Cb.SelectedValue = VehicleAlarm.Persistance.ToString();
            alarm2Cb.Checked = VehicleAlarm.Active;

            VehicleAlarm = list[3];
            vehicleAlarmPersistence3Cb.SelectedValue = VehicleAlarm.Persistance.ToString();
            alarm3Cb.Checked = VehicleAlarm.Active;

            VehicleAlarm = list[4];
            vehicleAlarmPersistence4Cb.SelectedValue = VehicleAlarm.Persistance.ToString();
            alarm4Cb.Checked = VehicleAlarm.Active;

        }

        private void _InitializeForm()
        {
            try
            {
                //Init the DGPS Configurations
                this.savedConfigsCb.DisplayMember = "Name";
                this.savedConfigsCb.ValueMember = "Id";
                List<AlarmConfiguration> list = _uow.AlarmConfigurations.FindAll().ToList<AlarmConfiguration>();

                if (list != null && list.Count > 0)
                {
                    IsFirstConfig = false;
                }

                AlarmConfiguration entity = new AlarmConfiguration();
                entity.Id = 0;
                entity.Name = "Please select an Alarm Configuration...";
                list.Add(entity);
                this.savedConfigsCb.DataSource = list.OrderBy(m => m.Id).ToList<AlarmConfiguration>();

                _SetDuration(audibleAlarmDuration0Cb);
                _SetDuration(audibleAlarmDuration1Cb);
                _SetDuration(audibleAlarmDuration2Cb);
                _SetDuration(audibleAlarmDuration3Cb);
                _SetDuration(audibleAlarmDuration4Cb);
                _SetAudibleFrequency(audibleAlarmFrequency0Cb);
                _SetAudibleFrequency(audibleAlarmFrequency1Cb);
                _SetAudibleFrequency(audibleAlarmFrequency2Cb);
                _SetAudibleFrequency(audibleAlarmFrequency3Cb);
                _SetAudibleFrequency(audibleAlarmFrequency4Cb);
                _SetPersistance(audibleAlarmPersistance0CB);
                _SetPersistance(audibleAlarmPersistance1CB);
                _SetPersistance(audibleAlarmPersistance2CB);
                _SetPersistance(audibleAlarmPersistance3CB);
                _SetPersistance(audibleAlarmPersistance4CB);
                _SetPersistance(vehicleAlarmPersistence0Cb);
                _SetPersistance(vehicleAlarmPersistence1Cb);
                _SetPersistance(vehicleAlarmPersistence2Cb);
                _SetPersistance(vehicleAlarmPersistence3Cb);
                _SetPersistance(vehicleAlarmPersistence4Cb);

                _initFieldsAndButtons();
            }
            catch (Exception ex)
            {
                log.Error("Init Form Exception", ex);
            }
        }


        private void _initFieldsAndButtons()
        {
//            log.Debug("In _initFieldsAndButtons");

            try
            {
                this.deleteConfigBt.Enabled = false;
                this.isDefaultCB.Checked = false;
                this.configNameTb.Text = "";

                this.alarm0Cb.Checked = false;
                this.alarm1Cb.Checked = false;
                this.alarm2Cb.Checked = false;
                this.alarm3Cb.Checked = false;
                this.alarm4Cb.Checked = false;

                this.radio0Cb.Checked = false;
                this.radio1Cb.Checked = false;
                this.radio2Cb.Checked = false;
                this.radio3Cb.Checked = false;
                this.radio4Cb.Checked = false;

                if (this.savedConfigsCb.Items != null && this.savedConfigsCb.Items.Count > 0)
                    this.savedConfigsCb.SelectedIndex = 0;
                if (this.audibleAlarmPersistance3CB.Items != null && this.audibleAlarmPersistance3CB.Items.Count > 0)
                    this.audibleAlarmPersistance3CB.SelectedIndex = 0;
                if (this.audibleAlarmFrequency3Cb.Items != null && this.audibleAlarmFrequency3Cb.Items.Count > 0)
                    this.audibleAlarmFrequency3Cb.SelectedIndex = 0;
                if (this.audibleAlarmDuration3Cb.Items != null && this.audibleAlarmDuration3Cb.Items.Count > 0)
                    this.audibleAlarmDuration3Cb.SelectedIndex = 0;
                if (this.audibleAlarmPersistance4CB.Items != null && this.audibleAlarmPersistance4CB.Items.Count > 0)
                    this.audibleAlarmPersistance4CB.SelectedIndex = 0;
                if (this.audibleAlarmFrequency4Cb.Items != null && this.audibleAlarmFrequency4Cb.Items.Count > 0)
                    this.audibleAlarmFrequency4Cb.SelectedIndex = 0;
                if (this.audibleAlarmDuration4Cb.Items != null && this.audibleAlarmDuration4Cb.Items.Count > 0)
                    this.audibleAlarmDuration4Cb.SelectedIndex = 0;
                if (this.audibleAlarmPersistance2CB.Items != null && this.audibleAlarmPersistance2CB.Items.Count > 0)
                    this.audibleAlarmPersistance2CB.SelectedIndex = 0;
                if (this.audibleAlarmFrequency2Cb.Items != null && this.audibleAlarmFrequency2Cb.Items.Count > 0)
                    this.audibleAlarmFrequency2Cb.SelectedIndex = 0;
                if (this.audibleAlarmDuration2Cb.Items != null && this.audibleAlarmDuration2Cb.Items.Count > 0)
                    this.audibleAlarmDuration2Cb.SelectedIndex = 0;
                if (this.audibleAlarmPersistance1CB.Items != null && this.audibleAlarmPersistance1CB.Items.Count > 0)
                    this.audibleAlarmPersistance1CB.SelectedIndex = 0;
                if (this.audibleAlarmFrequency1Cb.Items != null && this.audibleAlarmFrequency1Cb.Items.Count > 0)
                    this.audibleAlarmFrequency1Cb.SelectedIndex = 0;
                if (this.audibleAlarmDuration1Cb.Items != null && this.audibleAlarmDuration1Cb.Items.Count > 0)
                    this.audibleAlarmDuration1Cb.SelectedIndex = 0;
                if (this.audibleAlarmPersistance0CB.Items != null && this.audibleAlarmPersistance0CB.Items.Count > 0)
                    this.audibleAlarmPersistance0CB.SelectedIndex = 0;
                if (this.audibleAlarmFrequency0Cb.Items != null && this.audibleAlarmFrequency0Cb.Items.Count > 0)
                    this.audibleAlarmFrequency0Cb.SelectedIndex = 0;
                if (this.audibleAlarmDuration0Cb.Items != null && this.audibleAlarmDuration0Cb.Items.Count > 0)
                    this.audibleAlarmDuration0Cb.SelectedIndex = 0;

                if (this.vehicleAlarmPersistence0Cb.Items != null && this.vehicleAlarmPersistence0Cb.Items.Count > 0)
                    this.vehicleAlarmPersistence0Cb.SelectedIndex = 0;
                if (this.vehicleAlarmPersistence1Cb.Items != null && this.vehicleAlarmPersistence1Cb.Items.Count > 0)
                    this.vehicleAlarmPersistence1Cb.SelectedIndex = 0;
                if (this.vehicleAlarmPersistence2Cb.Items != null && this.vehicleAlarmPersistence2Cb.Items.Count > 0)
                    this.vehicleAlarmPersistence2Cb.SelectedIndex = 0;
                if (this.vehicleAlarmPersistence3Cb.Items != null && this.vehicleAlarmPersistence3Cb.Items.Count > 0)
                    this.vehicleAlarmPersistence3Cb.SelectedIndex = 0;
                if (this.vehicleAlarmPersistence4Cb.Items != null && this.vehicleAlarmPersistence4Cb.Items.Count > 0)
                    this.vehicleAlarmPersistence4Cb.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                log.Error("Exception", ex);
                throw ex;
            }
        }

        private void _SetPersistance(ComboBox comboBox)
        {
            comboBox.DisplayMember = "Name";
            comboBox.ValueMember = "Id";
            comboBox.DataSource = _GetPersistance();
        }

        private object _GetPersistance()
        {
            List<ComboBoxItem> list = new List<ComboBoxItem>();

            list.Add(new ComboBoxItem("0", "Select..."));
            list.Add(new ComboBoxItem("1000", "1"));
            list.Add(new ComboBoxItem("2000", "2"));
            list.Add(new ComboBoxItem("3000", "3"));
            list.Add(new ComboBoxItem("4000", "4"));
            list.Add(new ComboBoxItem("5000", "5"));
            list.Add(new ComboBoxItem("6000", "6"));
            list.Add(new ComboBoxItem("7000", "7"));
            list.Add(new ComboBoxItem("8000", "8"));
            list.Add(new ComboBoxItem("9000", "9"));
            list.Add(new ComboBoxItem("10000", "10"));
            return list;
        }

        private void _SetAudibleFrequency(ComboBox comboBox)
        {
            //220 to 660 hz
            comboBox.DisplayMember = "Name";
            comboBox.ValueMember = "Id";
            comboBox.DataSource = _GetFrequency();
        }

        private object _GetFrequency()
        {
            List<ComboBoxItem> list = new List<ComboBoxItem>();

            list.Add(new ComboBoxItem("0", "Select..."));
            list.Add(new ComboBoxItem("220", "220"));
            list.Add(new ComboBoxItem("330", "330"));
            list.Add(new ComboBoxItem("440", "440"));
            list.Add(new ComboBoxItem("560", "560"));
            list.Add(new ComboBoxItem("670", "670"));
            list.Add(new ComboBoxItem("1000", "1000"));
            list.Add(new ComboBoxItem("1500", "1500"));
            list.Add(new ComboBoxItem("2000", "2000"));
            list.Add(new ComboBoxItem("2500", "2500"));
            list.Add(new ComboBoxItem("3000", "3000"));

            return list;
        }

        private void _SetDuration(ComboBox comboBox)
        {
            comboBox.DisplayMember = "Name";
            comboBox.ValueMember = "Id";
            comboBox.DataSource = _GetDuration();
            //100 To 1000 ms
        }

        private object _GetDuration()
        {
            List<ComboBoxItem> list = new List<ComboBoxItem>();
            /*
            list.Add(new ComboBoxItem("0", "Select..."));

            for (int i = 1; i <= 10;i++)
            {
                ComboBoxItem comboBoxItem = new ComboBoxItem()
                {
                    Id = (i * 100).ToString(),
                    Name = (i * 100).ToString()
                };
                list.Add(comboBoxItem);
            }
            */
            list.Add(new ComboBoxItem("0", "Select..."));
            list.Add(new ComboBoxItem("100", "100"));
            list.Add(new ComboBoxItem("150", "150"));
            list.Add(new ComboBoxItem("200", "200"));
            list.Add(new ComboBoxItem("250", "250"));
            list.Add(new ComboBoxItem("300", "300"));
            list.Add(new ComboBoxItem("400", "400"));
            list.Add(new ComboBoxItem("500", "500"));
            list.Add(new ComboBoxItem("600", "600"));
            list.Add(new ComboBoxItem("700", "700"));
            list.Add(new ComboBoxItem("800", "800"));
            list.Add(new ComboBoxItem("900", "900"));
            list.Add(new ComboBoxItem("1000", "1000"));

            return list;
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
                    IQueryable<AlarmConfiguration> configList = _uow.AlarmConfigurations.FindAll();
                    AlarmConfiguration entity = _uow.AlarmConfigurations.FindById((int)this.savedConfigsCb.SelectedValue);
                    IQueryable<AudibleVisualAlarm> AudibleVisualRemoveList = _uow.AudibleVisualAlarms.FindWhere(m => m.AlarmConfiguration_Id == entity.Id);
                    IQueryable<VehicleAlarm> VehicleAlarmRemoveList = _uow.VehicleAlarms.FindWhere(m => m.AlarmConfiguration.Id == entity.Id);

                    if (configList.Count() < 2)
                    {
                        if (MessageBox.Show("The selected configuration is the last configuration.  If you continue the system will not send/receive Alarm data", SystemConstants.MessageBox_Caption_DeleteConfirg, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
                        {
                            foreach (AudibleVisualAlarm var in AudibleVisualRemoveList)
                            {
                                _uow.AudibleVisualAlarms.Remove(var);
                                _uow.Commit();
                            }
                            foreach (VehicleAlarm var in VehicleAlarmRemoveList)
                            {
                                _uow.VehicleAlarms.Remove(var);
                                _uow.Commit();
                            }
                            _uow.AlarmConfigurations.Remove(entity);
                            _uow.Commit();
                            LogEventsManager.LogEvent("The Last Alarm Configuration Removed", LogEventTypes.ALARM_CONFIG, LogLevelTypes.INFO);
                            _InitializeForm();
                            MessageBox.Show("The selected configuration was deleted", SystemConstants.MessageBox_Caption_DeleteConfirg, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    else
                    {
                        AlarmConfiguration isDefault = _uow.AlarmConfigurations.FindWhere(m => m.IsDefault == true).FirstOrDefault();

                        if (!entity.Equals(isDefault))
                        {
                            foreach (AudibleVisualAlarm var in AudibleVisualRemoveList)
                            {
                                _uow.AudibleVisualAlarms.Remove(var);
                                _uow.Commit();
                            }
                            _uow.AlarmConfigurations.Remove(entity);
                            _uow.Commit();
                            LogEventsManager.LogEvent("Alarm Configuration Removed", LogEventTypes.ALARM_CONFIG, LogLevelTypes.INFO);
                            _InitializeForm();
                            MessageBox.Show("The selected configuration was deleted", SystemConstants.MessageBox_Caption_DeleteConfirg, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            if (MessageBox.Show("The selected configuration is the default configuration.  If you continue the system will not send/receive Alarm data until a default configuration is selected", SystemConstants.MessageBox_Caption_DeleteConfirg, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
                            {
                                foreach (AudibleVisualAlarm var in AudibleVisualRemoveList)
                                {
                                    _uow.AudibleVisualAlarms.Remove(var);
                                    _uow.Commit();
                                }
                                _uow.AlarmConfigurations.Remove(entity);
                                _uow.Commit();
                                LogEventsManager.LogEvent("Default Alarm Configuration Removed", LogEventTypes.ALARM_CONFIG, LogLevelTypes.INFO);
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
 //           log.Debug("In newConfigBt_Click");

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
        private void saveConfigBt_Click(object sender, EventArgs e)
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
                        AlarmConfiguration entity = new AlarmConfiguration()
                        {
                            IsDefault = this.isDefaultCB.Checked,
                            Name = this.configNameTb.Text,
                            AudibleVisualAlarms = _SetAudibleVisualAlarms(),
                            VehicleAlarms = _SetVehicleAlarms()
                        };

                        if (this.isDefaultCB.Checked)
                        {
                            AlarmConfiguration defaultCheck = _uow.AlarmConfigurations.FindWhere(m => m.IsDefault == true).FirstOrDefault();
                            if (defaultCheck != null)
                            {
                                defaultCheck.IsDefault = false;
                                _uow.AlarmConfigurations.Set(defaultCheck.Id, defaultCheck);
                            }
                        }
                        _uow.AlarmConfigurations.Add(entity);
                        _uow.Commit();
                        
                        ((IncZoneMDIParent)this.MdiParent)._AlarmConfig = new AlarmConfig(entity);
                        
                        if (IsFirstConfig)
                        {
                            LogEventsManager.LogEvent("Alarm Configuration Added", LogEventTypes.ALARM_CONFIG_INITIAL, LogLevelTypes.INFO);
                        }
                        else
                        {
                            LogEventsManager.LogEvent("Alarm Configuration Added", LogEventTypes.ALARM_CONFIG, LogLevelTypes.INFO);
                        }
                        saveType = "added";
                    }
                    else
                    {
                        AlarmConfiguration entity = _uow.AlarmConfigurations.FindById((int)this.savedConfigsCb.SelectedValue);


                        entity.IsDefault = this.isDefaultCB.Checked;
                        entity.Name = this.configNameTb.Text;
                        IQueryable<AudibleVisualAlarm> AudibleVisualRemoveList = _uow.AudibleVisualAlarms.FindWhere(m => m.AlarmConfiguration_Id == entity.Id);
                        foreach (AudibleVisualAlarm var in AudibleVisualRemoveList)
                        {
                            _uow.AudibleVisualAlarms.Remove(var);
                            _uow.Commit();
                        }

                        IQueryable<VehicleAlarm> VehicleAlarmRemoveList = _uow.VehicleAlarms.FindWhere(m => m.AlarmConfiguration.Id == entity.Id);
                        foreach (VehicleAlarm var in VehicleAlarmRemoveList)
                        {
                            _uow.VehicleAlarms.Remove(var);
                            _uow.Commit();
                        }
                        entity.AudibleVisualAlarms = _SetAudibleVisualAlarms(entity.Id);
                        entity.VehicleAlarms = _SetVehicleAlarms(entity.Id);

                        if (this.isDefaultCB.Checked)
                        {
                            AlarmConfiguration defaultCheck = _uow.AlarmConfigurations.FindWhere(m => m.IsDefault == true).FirstOrDefault();
                            if (defaultCheck != null && !entity.Equals(defaultCheck))
                            {
                                defaultCheck.IsDefault = false;
                                _uow.AlarmConfigurations.Set(defaultCheck.Id, defaultCheck);
                            }
                        }

                        _uow.AlarmConfigurations.Set(Id, entity);
                        _uow.Commit();
                        ((IncZoneMDIParent)this.MdiParent)._AlarmConfig = new AlarmConfig(entity);

                        LogEventsManager.LogEvent("Alarm Configuration Updated", LogEventTypes.ALARM_CONFIG, LogLevelTypes.INFO);
                        saveType = "updated";
                    }
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

        private List<VehicleAlarm> _SetVehicleAlarms(int p)
        {
            List<VehicleAlarm> list = new List<VehicleAlarm>();

            VehicleAlarm entity = new VehicleAlarm()
            {
                AlarmConfiguration = _uow.AlarmConfigurations.FindById(p),
                AlarmLevel = _uow.AlarmLevels.FindById(1),
                Persistance = int.Parse((string)vehicleAlarmPersistence0Cb.SelectedValue),
                Active = alarm0Cb.Checked
            };

            list.Add(entity);

            entity = new VehicleAlarm()
            {
                AlarmConfiguration = _uow.AlarmConfigurations.FindById(p),
                AlarmLevel = _uow.AlarmLevels.FindById(2),
                Persistance = int.Parse((string)vehicleAlarmPersistence1Cb.SelectedValue),
                Active = alarm1Cb.Checked
            };

            list.Add(entity);

            entity = new VehicleAlarm()
            {
                AlarmConfiguration = _uow.AlarmConfigurations.FindById(p),
                AlarmLevel = _uow.AlarmLevels.FindById(3),
                Persistance = int.Parse((string)vehicleAlarmPersistence2Cb.SelectedValue),
                Active = alarm2Cb.Checked
            };

            list.Add(entity);

            entity = new VehicleAlarm()
            {
                AlarmConfiguration = _uow.AlarmConfigurations.FindById(p),
                AlarmLevel = _uow.AlarmLevels.FindById(4),
                Persistance = int.Parse((string)vehicleAlarmPersistence3Cb.SelectedValue),
                Active = alarm3Cb.Checked
            };

            list.Add(entity);

            entity = new VehicleAlarm()
            {
                AlarmConfiguration = _uow.AlarmConfigurations.FindById(p),
                AlarmLevel = _uow.AlarmLevels.FindById(5),
                Persistance = int.Parse((string)vehicleAlarmPersistence4Cb.SelectedValue),
                Active = alarm4Cb.Checked
            };
            list.Add(entity);

            return list;
        }

        private List<VehicleAlarm> _SetVehicleAlarms()
        {
            List<VehicleAlarm> list = new List<VehicleAlarm>();

            VehicleAlarm entity = new VehicleAlarm()
            {
                AlarmLevel = _uow.AlarmLevels.FindById(1),
                Persistance = int.Parse((string)vehicleAlarmPersistence0Cb.SelectedValue),
                Active = alarm0Cb.Checked
            };

            list.Add(entity);

            entity = new VehicleAlarm()
            {
                AlarmLevel = _uow.AlarmLevels.FindById(2),
                Persistance = int.Parse((string)vehicleAlarmPersistence1Cb.SelectedValue),
                Active = alarm1Cb.Checked
            };

            list.Add(entity);

            entity = new VehicleAlarm()
            {
                AlarmLevel = _uow.AlarmLevels.FindById(3),
                Persistance = int.Parse((string)vehicleAlarmPersistence2Cb.SelectedValue),
                Active = alarm2Cb.Checked
            };

            list.Add(entity);

            entity = new VehicleAlarm()
            {
                AlarmLevel = _uow.AlarmLevels.FindById(4),
                Persistance = int.Parse((string)vehicleAlarmPersistence3Cb.SelectedValue),
                Active = alarm3Cb.Checked
            };

            list.Add(entity);

            entity = new VehicleAlarm()
            {
                AlarmLevel = _uow.AlarmLevels.FindById(5),
                Persistance = int.Parse((string)vehicleAlarmPersistence4Cb.SelectedValue),
                Active = alarm4Cb.Checked
            };
            list.Add(entity);

            return list;     
        }

        private List<AudibleVisualAlarm> _SetAudibleVisualAlarms()
        {
            List<AudibleVisualAlarm> list = new List<AudibleVisualAlarm>();

            AudibleVisualAlarm entity = new AudibleVisualAlarm()
            {
                AlarmLevel_Id = 1,
                Duration = (string)audibleAlarmDuration0Cb.SelectedValue,
                Frequency = (string)audibleAlarmFrequency0Cb.SelectedValue,
                Persistance = int.Parse((string)audibleAlarmPersistance0CB.SelectedValue),
                RadioActive = radio0Cb.Checked
            };

            list.Add(entity);

            entity = new AudibleVisualAlarm()
            {
                AlarmLevel_Id = 2,
                Duration = (string)audibleAlarmDuration1Cb.SelectedValue,
                Frequency = (string)audibleAlarmFrequency1Cb.SelectedValue,
                Persistance = int.Parse((string)audibleAlarmPersistance1CB.SelectedValue),
                RadioActive = radio1Cb.Checked
            };

            list.Add(entity);

            entity = new AudibleVisualAlarm()
            {
                AlarmLevel_Id = 3,
                Duration = (string)audibleAlarmDuration2Cb.SelectedValue,
                Frequency = (string)audibleAlarmFrequency2Cb.SelectedValue,
                Persistance = int.Parse((string)audibleAlarmPersistance2CB.SelectedValue),
                RadioActive = radio2Cb.Checked
            };

            list.Add(entity);

            entity = new AudibleVisualAlarm()
            {
                AlarmLevel_Id = 4,
                Duration = (string)audibleAlarmDuration3Cb.SelectedValue,
                Frequency = (string)audibleAlarmFrequency3Cb.SelectedValue,
                Persistance = int.Parse((string)audibleAlarmPersistance3CB.SelectedValue),
                RadioActive = radio3Cb.Checked
            };

            list.Add(entity);

            entity = new AudibleVisualAlarm()
            {
                AlarmLevel_Id = 5,
                Duration = (string)audibleAlarmDuration4Cb.SelectedValue,
                Frequency = (string)audibleAlarmFrequency4Cb.SelectedValue,
                Persistance = int.Parse((string)audibleAlarmPersistance4CB.SelectedValue),
                RadioActive = radio4Cb.Checked
            };
            list.Add(entity);

            return list;            
        }

        private List<AudibleVisualAlarm> _SetAudibleVisualAlarms(int Id)
        {
            List<AudibleVisualAlarm> list = new List<AudibleVisualAlarm>();

            AudibleVisualAlarm entity = new AudibleVisualAlarm()
            {
                AlarmConfiguration_Id = Id,
                AlarmLevel_Id = 1,
                Duration = (string)audibleAlarmDuration0Cb.SelectedValue,
                Frequency = (string)audibleAlarmFrequency0Cb.SelectedValue,
                Persistance = int.Parse((string)audibleAlarmPersistance0CB.SelectedValue),
                RadioActive = radio0Cb.Checked
            };

            list.Add(entity);

            entity = new AudibleVisualAlarm()
            {
                AlarmConfiguration_Id = Id,
                AlarmLevel_Id = 2,
                Duration = (string)audibleAlarmDuration1Cb.SelectedValue,
                Frequency = (string)audibleAlarmFrequency1Cb.SelectedValue,
                Persistance = int.Parse((string)audibleAlarmPersistance1CB.SelectedValue),
                RadioActive = radio1Cb.Checked
            };

            list.Add(entity);

            entity = new AudibleVisualAlarm()
            {
                AlarmConfiguration_Id = Id,
                AlarmLevel_Id = 3,
                Duration = (string)audibleAlarmDuration2Cb.SelectedValue,
                Frequency = (string)audibleAlarmFrequency2Cb.SelectedValue,
                Persistance = int.Parse((string)audibleAlarmPersistance2CB.SelectedValue),
                RadioActive = radio2Cb.Checked
            };

            list.Add(entity);

            entity = new AudibleVisualAlarm()
            {
                AlarmConfiguration_Id = Id,
                AlarmLevel_Id = 4,
                Duration = (string)audibleAlarmDuration3Cb.SelectedValue,
                Frequency = (string)audibleAlarmFrequency3Cb.SelectedValue,
                Persistance = int.Parse((string)audibleAlarmPersistance3CB.SelectedValue),
                RadioActive = radio3Cb.Checked
            };

            list.Add(entity);

            entity = new AudibleVisualAlarm()
            {
                AlarmConfiguration_Id = Id,
                AlarmLevel_Id = 5,
                Duration = (string)audibleAlarmDuration4Cb.SelectedValue,
                Frequency = (string)audibleAlarmFrequency4Cb.SelectedValue,
                Persistance = int.Parse((string)audibleAlarmPersistance4CB.SelectedValue),
                RadioActive = radio4Cb.Checked
            };
            list.Add(entity);

            return list;
        }

        private void _SetAudibleVisualAlarms(AlarmConfiguration entity)
        {

            List<AudibleVisualAlarm> list = entity.AudibleVisualAlarms.OrderBy(m => m.AlarmLevel_Id).ToList();
            AudibleVisualAlarm AudibleVisualAlarm = null;

            AudibleVisualAlarm = list[0];
            audibleAlarmDuration0Cb.SelectedValue = AudibleVisualAlarm.Duration;
            audibleAlarmFrequency0Cb.SelectedValue = AudibleVisualAlarm.Frequency;
            audibleAlarmPersistance0CB.SelectedValue = AudibleVisualAlarm.Persistance.ToString();
            radio0Cb.Checked = AudibleVisualAlarm.RadioActive; 

            AudibleVisualAlarm = list[1];
            audibleAlarmDuration1Cb.SelectedValue = AudibleVisualAlarm.Duration;
            audibleAlarmFrequency1Cb.SelectedValue = AudibleVisualAlarm.Frequency;
            audibleAlarmPersistance1CB.SelectedValue = AudibleVisualAlarm.Persistance.ToString();
            radio1Cb.Checked = AudibleVisualAlarm.RadioActive;

            AudibleVisualAlarm = list[2];
            audibleAlarmDuration2Cb.SelectedValue = AudibleVisualAlarm.Duration;
            audibleAlarmFrequency2Cb.SelectedValue = AudibleVisualAlarm.Frequency;
            audibleAlarmPersistance2CB.SelectedValue = AudibleVisualAlarm.Persistance.ToString();
            radio2Cb.Checked = AudibleVisualAlarm.RadioActive;

            AudibleVisualAlarm = list[3];
            audibleAlarmDuration3Cb.SelectedValue = AudibleVisualAlarm.Duration;
            audibleAlarmFrequency3Cb.SelectedValue = AudibleVisualAlarm.Frequency;
            audibleAlarmPersistance3CB.SelectedValue = AudibleVisualAlarm.Persistance.ToString();
            radio3Cb.Checked = AudibleVisualAlarm.RadioActive;

            AudibleVisualAlarm = list[4];
            audibleAlarmDuration4Cb.SelectedValue = AudibleVisualAlarm.Duration;
            audibleAlarmFrequency4Cb.SelectedValue = AudibleVisualAlarm.Frequency;
            audibleAlarmPersistance4CB.SelectedValue = AudibleVisualAlarm.Persistance.ToString();
            radio4Cb.Checked = AudibleVisualAlarm.RadioActive;
        }

        private bool _ValidFormValues()
        {
 //           log.Debug("In _ValidFormValues");

            try
            {
                alarmErrorProvider.Clear();

                if (string.IsNullOrEmpty(configNameTb.Text))
                {
                    alarmErrorProvider.SetError(configNameTb, "Configuration Name requires a value");
                    return false;
                }
                else
                {
                    alarmErrorProvider.SetError(configNameTb, "");
                }
                if (!_ComboBoxHasValue(audibleAlarmDuration0Cb, "Alarm Duration"))
                {
                    return false;
                }
                if (!_ComboBoxHasValue(audibleAlarmDuration1Cb, "Alarm Duration"))
                {
                    return false;
                }
                if (!_ComboBoxHasValue(audibleAlarmDuration2Cb, "Alarm Duration"))
                {
                    return false;
                }
                if (!_ComboBoxHasValue(audibleAlarmDuration3Cb, "Alarm Duration"))
                {
                    return false;
                }
                if (!_ComboBoxHasValue(audibleAlarmDuration4Cb, "Alarm Duration"))
                {
                    return false;
                }
                if (!_ComboBoxHasValue(audibleAlarmFrequency0Cb, "Alarm Frequency"))
                {
                    return false;
                }
                if (!_ComboBoxHasValue(audibleAlarmFrequency1Cb, "Alarm Frequency"))
                {
                    return false;
                }
                if (!_ComboBoxHasValue(audibleAlarmFrequency2Cb, "Alarm Frequency"))
                {
                    return false;
                }
                if (!_ComboBoxHasValue(audibleAlarmFrequency3Cb, "Alarm Frequency"))
                {
                    return false;
                }
                if (!_ComboBoxHasValue(audibleAlarmFrequency4Cb, "Alarm Frequency"))
                {
                    return false;
                }
                if (!_ComboBoxHasValue(audibleAlarmPersistance0CB, "Alarm Persistance"))
                {
                    return false;
                }
                if (!_ComboBoxHasValue(audibleAlarmPersistance1CB, "Alarm Persistance"))
                {
                    return false;
                }
                if (!_ComboBoxHasValue(audibleAlarmPersistance2CB, "Alarm Persistance"))
                {
                    return false;
                }
                if (!_ComboBoxHasValue(audibleAlarmPersistance3CB, "Alarm Persistance"))
                {
                    return false;
                }
                if (!_ComboBoxHasValue(audibleAlarmPersistance4CB, "Alarm Persistance"))
                {
                    return false;
                }
                if (!_ComboBoxHasValue(vehicleAlarmPersistence0Cb, "Vehicle Persistance"))
                {
                    return false;
                }
                if (!_ComboBoxHasValue(vehicleAlarmPersistence1Cb, "Vehicle Persistance"))
                {
                    return false;
                }
                if (!_ComboBoxHasValue(vehicleAlarmPersistence2Cb, "Vehicle Persistance"))
                {
                    return false;
                }
                if (!_ComboBoxHasValue(vehicleAlarmPersistence3Cb, "Vehicle Persistance"))
                {
                    return false;
                }
                if (!_ComboBoxHasValue(vehicleAlarmPersistence4Cb, "Vehicle Persistance"))
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                log.Error("Exception", ex);
                throw ex;
            }

            return true;
        }

        private bool _ComboBoxHasValue(NumericUpDown numericUpDown, string controlName)
        {
            if (numericUpDown.Value < 0)
            {
                alarmErrorProvider.SetError(numericUpDown, controlName + " requires a value");
                return false;
            }
            else
            {
                alarmErrorProvider.SetError(numericUpDown, "");
            }

            return true;
        }

        private bool _ComboBoxHasValue(ComboBox comboBox, string controlName)
        {
            if (comboBox.SelectedIndex == 0)
            {
                alarmErrorProvider.SetError(comboBox, controlName + " requires a value");
                return false;
            }
            else
            {
                alarmErrorProvider.SetError(comboBox, "");
            }

            return true;
        }

        private void audibleAlarm0Bt_Click(object sender, EventArgs e)
        {
            AlarmManager _AlarmManager = new AlarmManager();
            try
            {
                if (audibleAlarmDuration0Cb.SelectedIndex != 0 &&
                audibleAlarmFrequency0Cb.SelectedIndex != 0 &&
                audibleAlarmPersistance0CB.SelectedIndex != 0)
                {
                    _AlarmManager.GenerateAudioAlarm(Convert.ToInt32(audibleAlarmDuration0Cb.SelectedValue), Convert.ToInt32(audibleAlarmPersistance0CB.SelectedValue), Convert.ToInt32(audibleAlarmFrequency0Cb.SelectedValue));
                }
                else
                {
                    MessageBox.Show("You must select all of the Audible Alarm 0 settings before you can test", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
            catch (Exception ex)
            {
                log.Error("audibleAlarm4Bt_Click Exception", ex);
                if (!errorBox.Visible)
                    errorBox.Show();
                errorBox.addMessage(DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + ": " + ex.Message);
            }
        }

        private void audibleAlarm1Bt_Click(object sender, EventArgs e)
        {
            AlarmManager _AlarmManager = new AlarmManager();

            try
            {
                if (audibleAlarmDuration1Cb.SelectedIndex != 0 &&
                audibleAlarmFrequency1Cb.SelectedIndex != 0 && 
                audibleAlarmPersistance1CB.SelectedIndex != 0)
                {
                    _AlarmManager.GenerateAudioAlarm(Convert.ToInt32(audibleAlarmDuration1Cb.SelectedValue), Convert.ToInt32(audibleAlarmPersistance1CB.SelectedValue), Convert.ToInt32(audibleAlarmFrequency1Cb.SelectedValue));
                }
                else
                {
                    MessageBox.Show("You must select all of the Audible Alarm 1 settings before you can test", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
            catch (Exception ex)
            {
                log.Error("audibleAlarm4Bt_Click Exception", ex);
                if (!errorBox.Visible)
                    errorBox.Show();
                errorBox.addMessage(DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + ": " + ex.Message);
            }
        }

        private void audibleAlarm2Bt_Click(object sender, EventArgs e)
        {
            AlarmManager _AlarmManager = new AlarmManager();

            try
            {
                if (audibleAlarmDuration2Cb.SelectedIndex != 0 &&
                audibleAlarmFrequency2Cb.SelectedIndex != 0 &&
                audibleAlarmPersistance2CB.SelectedIndex != 0)
                {
                    _AlarmManager.GenerateAudioAlarm(Convert.ToInt32(audibleAlarmDuration2Cb.SelectedValue), Convert.ToInt32(audibleAlarmPersistance2CB.SelectedValue), Convert.ToInt32(audibleAlarmFrequency2Cb.SelectedValue));
                }
                else
                {
                    MessageBox.Show("You must select all of the Audible Alarm 2 settings before you can test", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
            catch (Exception ex)
            {
                log.Error("audibleAlarm4Bt_Click Exception", ex);
                if (!errorBox.Visible)
                    errorBox.Show();
                errorBox.addMessage(DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + ": " + ex.Message);
            }
        }

        private void audibleAlarm3Bt_Click(object sender, EventArgs e)
        {
            AlarmManager _AlarmManager = new AlarmManager();

            try
            {
                if (audibleAlarmDuration3Cb.SelectedIndex != 0 &&
                audibleAlarmFrequency3Cb.SelectedIndex != 0 &&
                audibleAlarmPersistance3CB.SelectedIndex != 0)
                {
                    _AlarmManager.GenerateAudioAlarm(Convert.ToInt32(audibleAlarmDuration3Cb.SelectedValue), Convert.ToInt32(audibleAlarmPersistance3CB.SelectedValue), Convert.ToInt32(audibleAlarmFrequency3Cb.SelectedValue));
                }
                else
                {
                    MessageBox.Show("You must select all of the Audible Alarm 3 settings before you can test", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
            catch (Exception ex)
            {
                log.Error("audibleAlarm4Bt_Click Exception", ex);
                if (!errorBox.Visible)
                    errorBox.Show();
                errorBox.addMessage(DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + ": " + ex.Message);
            }
        }

        private void audibleAlarm4Bt_Click(object sender, EventArgs e)
        {
            AlarmManager _AlarmManager = new AlarmManager();

            try
            {
                if (audibleAlarmDuration4Cb.SelectedIndex != 0 &&
                audibleAlarmFrequency4Cb.SelectedIndex != 0 &&
                audibleAlarmPersistance4CB.SelectedIndex != 0)
                {
                    _AlarmManager.GenerateAudioAlarm(Convert.ToInt32(audibleAlarmDuration4Cb.SelectedValue), Convert.ToInt32(audibleAlarmPersistance4CB.SelectedValue), Convert.ToInt32(audibleAlarmFrequency4Cb.SelectedValue));
                }
                else
                {
                    MessageBox.Show("You must select all of the Audible Alarm 4 settings before you can test", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
            catch (Exception ex)
            {
                log.Error("audibleAlarm4Bt_Click Exception", ex);
                if (!errorBox.Visible)
                        errorBox.Show();
                errorBox.addMessage(DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + ": " + ex.Message);
            }
        }

        private void radioAlarm0Bt_Click(object sender, EventArgs e)
        {
            AlarmManager _AlarmManager = new AlarmManager();
            try
            {
                if (audibleAlarmDuration0Cb.SelectedIndex != 0 &&
                audibleAlarmFrequency0Cb.SelectedIndex != 0 &&
                audibleAlarmPersistance0CB.SelectedIndex != 0 && radio0Cb.Checked)
                {
                    Task t1 = _AlarmManager.GenerateRadioAlarm(Convert.ToInt32(audibleAlarmDuration0Cb.SelectedValue), Convert.ToInt32(audibleAlarmPersistance0CB.SelectedValue), ifKit, ((IncZoneMDIParent)this.MdiParent).fiKitBypassed);
                    Task t2 = _AlarmManager.GenerateAudioAlarm(Convert.ToInt32(audibleAlarmDuration0Cb.SelectedValue), Convert.ToInt32(audibleAlarmPersistance0CB.SelectedValue), Convert.ToInt32(audibleAlarmFrequency0Cb.SelectedValue));

                    Task.WaitAll(t1, t2);
                }
                else
                {
                    MessageBox.Show("You must select all of the Audible Alarm 0 settings before you can test", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
            catch (Exception ex)
            {
                log.Error("radioAlarm0Bt_Click Exception", ex);
                if (!errorBox.Visible)
                    errorBox.Show();
                errorBox.addMessage(DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + ": " + ex.Message);
            }
        }
        private void radioAlarm1Bt_Click(object sender, EventArgs e)
        {
            AlarmManager _AlarmManager = new AlarmManager();
            try
            {
                if (audibleAlarmDuration1Cb.SelectedIndex != 0 &&
                audibleAlarmFrequency1Cb.SelectedIndex != 0 &&
                audibleAlarmPersistance1CB.SelectedIndex != 0 && radio1Cb.Checked)
                {
                    Task t1 = _AlarmManager.GenerateRadioAlarm(Convert.ToInt32(audibleAlarmDuration1Cb.SelectedValue), Convert.ToInt32(audibleAlarmPersistance1CB.SelectedValue), ifKit, ((IncZoneMDIParent)this.MdiParent).fiKitBypassed);
                    Task t2 = _AlarmManager.GenerateAudioAlarm(Convert.ToInt32(audibleAlarmDuration1Cb.SelectedValue), Convert.ToInt32(audibleAlarmPersistance1CB.SelectedValue), Convert.ToInt32(audibleAlarmFrequency1Cb.SelectedValue));

                    Task.WaitAll(t1, t2);
                }
                else
                {
                    MessageBox.Show("You must select all of the Audible Alarm 1 settings before you can test", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
            catch (Exception ex)
            {
                log.Error("radioAlarm1Bt_Click Exception", ex);
                if (!errorBox.Visible)
                    errorBox.Show();
                errorBox.addMessage(DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + ": " + ex.Message);
            }
        }
        private void radioAlarm2Bt_Click(object sender, EventArgs e)
        {
            AlarmManager _AlarmManager = new AlarmManager();
            try
            {
                if (audibleAlarmDuration2Cb.SelectedIndex != 0 &&
                audibleAlarmFrequency2Cb.SelectedIndex != 0 &&
                audibleAlarmPersistance2CB.SelectedIndex != 0 && radio2Cb.Checked)
                {
                    Task t1 = _AlarmManager.GenerateRadioAlarm(Convert.ToInt32(audibleAlarmDuration2Cb.SelectedValue), Convert.ToInt32(audibleAlarmPersistance2CB.SelectedValue), ifKit, ((IncZoneMDIParent)this.MdiParent).fiKitBypassed);
                    Task t2 = _AlarmManager.GenerateAudioAlarm(Convert.ToInt32(audibleAlarmDuration2Cb.SelectedValue), Convert.ToInt32(audibleAlarmPersistance2CB.SelectedValue), Convert.ToInt32(audibleAlarmFrequency2Cb.SelectedValue));

                    Task.WaitAll(t1, t2);
                }
                else
                {
                    MessageBox.Show("You must select all of the Audible Alarm 2 settings before you can test", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
            catch (Exception ex)
            {
                log.Error("radioAlarm2Bt_Click Exception", ex);
                if (!errorBox.Visible)
                    errorBox.Show();
                errorBox.addMessage(DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + ": " + ex.Message);
            }
        }
        private void radioAlarm3Bt_Click(object sender, EventArgs e)
        {
            AlarmManager _AlarmManager = new AlarmManager();
            try
            {
                if (audibleAlarmDuration3Cb.SelectedIndex != 0 &&
                audibleAlarmFrequency3Cb.SelectedIndex != 0 &&
                audibleAlarmPersistance3CB.SelectedIndex != 0 && radio3Cb.Checked)
                {
                    Task t1 = _AlarmManager.GenerateRadioAlarm(Convert.ToInt32(audibleAlarmDuration3Cb.SelectedValue), Convert.ToInt32(audibleAlarmPersistance3CB.SelectedValue), ifKit, ((IncZoneMDIParent)this.MdiParent).fiKitBypassed);
                    Task t2 = _AlarmManager.GenerateAudioAlarm(Convert.ToInt32(audibleAlarmDuration3Cb.SelectedValue), Convert.ToInt32(audibleAlarmPersistance3CB.SelectedValue), Convert.ToInt32(audibleAlarmFrequency3Cb.SelectedValue));

                    Task.WaitAll(t1, t2);
                }
                else
                {
                    MessageBox.Show("You must select all of the Audible Alarm 3 settings before you can test", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
            catch (Exception ex)
            {
                log.Error("radioAlarm3Bt_Click Exception", ex);
                if (!errorBox.Visible)
                    errorBox.Show();
                errorBox.addMessage(DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + ": " + ex.Message);
            }
        }
        private void radioAlarm4Bt_Click(object sender, EventArgs e)
        {
            AlarmManager _AlarmManager = new AlarmManager();
            try
            {
                if (audibleAlarmDuration4Cb.SelectedIndex != 0 &&
                audibleAlarmFrequency4Cb.SelectedIndex != 0 &&
                audibleAlarmPersistance4CB.SelectedIndex != 0 && radio4Cb.Checked)
                {
                    Task t1 = _AlarmManager.GenerateRadioAlarm(Convert.ToInt32(audibleAlarmDuration4Cb.SelectedValue), Convert.ToInt32(audibleAlarmPersistance4CB.SelectedValue), ifKit, ((IncZoneMDIParent)this.MdiParent).fiKitBypassed);
                    Task t2 = _AlarmManager.GenerateAudioAlarm(Convert.ToInt32(audibleAlarmDuration4Cb.SelectedValue), Convert.ToInt32(audibleAlarmPersistance4CB.SelectedValue), Convert.ToInt32(audibleAlarmFrequency4Cb.SelectedValue));

                    Task.WaitAll(t1, t2);
                }
                else
                {
                    MessageBox.Show("You must select all of the Audible Alarm 4 settings before you can test", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
            catch (Exception ex)
            {
                log.Error("radioAlarm4Bt_Click Exception", ex);
                if (!errorBox.Visible)
                    errorBox.Show();
                errorBox.addMessage(DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + ": " + ex.Message);
            }
        }

        private void vehicleAlarm0Bt_Click(object sender, EventArgs e)
        {
            AlarmManager _AlarmManager = new AlarmManager();
            try
            {
                if (vehicleAlarmPersistence0Cb.SelectedIndex != 0)
                {
                    _AlarmManager.GenerateVitalAlarm(Convert.ToInt32(vehicleAlarmPersistence0Cb.SelectedValue), IncZoneMDIParent.vitalModual);
                }
                else
                {
                    MessageBox.Show("You must select a Vehicle Alarm 0 settings before you can test", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
            catch (Exception ex)
            {
                log.Error("vehicleAlarm0Bt_Click Exception", ex);
                if (!errorBox.Visible)
                    errorBox.Show();
                errorBox.addMessage(DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + ": " + ex.Message);
            }
        }

        private void vehicleAlarm1Bt_Click(object sender, EventArgs e)
        {
            AlarmManager _AlarmManager = new AlarmManager();
            try
            {
                if (vehicleAlarmPersistence1Cb.SelectedIndex != 0)
                {
                    _AlarmManager.GenerateVitalAlarm(Convert.ToInt32(vehicleAlarmPersistence1Cb.SelectedValue), IncZoneMDIParent.vitalModual);
                }
                else
                {
                    MessageBox.Show("You must select a Vehicle Alarm 1 settings before you can test", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
            catch (Exception ex)
            {
                log.Error("vehicleAlarm1Bt_Click Exception", ex);
                if (!errorBox.Visible)
                    errorBox.Show();
                errorBox.addMessage(DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + ": " + ex.Message);
            }
        }

        private void vehicleAlarm2Bt_Click(object sender, EventArgs e)
        {
            AlarmManager _AlarmManager = new AlarmManager();
            try
            {
                if (vehicleAlarmPersistence2Cb.SelectedIndex != 0)
                {
                    _AlarmManager.GenerateVitalAlarm(Convert.ToInt32(vehicleAlarmPersistence2Cb.SelectedValue), IncZoneMDIParent.vitalModual);
                }
                else
                {
                    MessageBox.Show("You must select a Vehicle Alarm 2 settings before you can test", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
            catch (Exception ex)
            {
                log.Error("vehicleAlarm2Bt_Click Exception", ex);
                if (!errorBox.Visible)
                    errorBox.Show();
                errorBox.addMessage(DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + ": " + ex.Message);
            }
        }

        private void vehicleAlarm3Bt_Click(object sender, EventArgs e)
        {
            AlarmManager _AlarmManager = new AlarmManager();
            try
            {
                if (vehicleAlarmPersistence3Cb.SelectedIndex != 0)
                {
                    _AlarmManager.GenerateVitalAlarm(Convert.ToInt32(vehicleAlarmPersistence3Cb.SelectedValue), IncZoneMDIParent.vitalModual);
                }
                else
                {
                    MessageBox.Show("You must select a Vehicle Alarm 3 settings before you can test", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
            catch (Exception ex)
            {
                log.Error("vehicleAlarm3Bt_Click Exception", ex);
                if (!errorBox.Visible)
                    errorBox.Show();
                errorBox.addMessage(DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + ": " + ex.Message);
            }
        }

        private void vehicleAlarm4Bt_Click(object sender, EventArgs e)
        {
            AlarmManager _AlarmManager = new AlarmManager();
            try
            {
                if (vehicleAlarmPersistence4Cb.SelectedIndex != 0)
                {
                    _AlarmManager.GenerateVitalAlarm(Convert.ToInt32(vehicleAlarmPersistence4Cb.SelectedValue), IncZoneMDIParent.vitalModual);
                }
                else
                {
                    MessageBox.Show("You must select a Vehicle Alarm 4 settings before you can test", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
            catch (Exception ex)
            {
                log.Error("vehicleAlarm4Bt_Click Exception", ex);
                if (!errorBox.Visible)
                    errorBox.Show();
                errorBox.addMessage(DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + ": " + ex.Message);
            }
        }
    }
}
