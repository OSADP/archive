using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using INCZONE.Common;
using INCZONE.Managers;
using INCZONE.Model;
using INCZONE.Repositories;
using InTheHand.Net;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using InTheHand.Windows.Forms;
using log4net;

namespace INCZONE.Forms.Configuration
{
    public partial class BluetoothForm : Form
    {
        private bool _AradaSelected;
        private bool _VitalSelected;
        private bool _NoVitalNeeded;
        readonly IUnitOfWork _uow;
        private static readonly ILog log = LogManager.GetLogger(SystemConstants.Logger_Ref);

        public BluetoothForm(IncZoneMDIParent form)
        {
 //           log.Debug("In BluetoothForm Constructor");

            string connectionString = ConfigurationManager.ConnectionStrings["IncZoneEntities"].ConnectionString;
            this._uow = new UnitOfWork(connectionString);

            this.MdiParent = form;
            InitializeComponent();
            ((IncZoneMDIParent)form).RequestStatusChange += RequestStatusChange;
            _AradaSelected = form._AradaSelected;
            _VitalSelected = form._VitalSelected;
            _NoVitalNeeded = form._NoVitalNeeded;

            if (!_AradaSelected)
            {
                aradaStatusLb.Text = "Not Selected";
            }
            else
            {
                aradaStatusLb.Text = "Selected";
            }

            if (!_VitalSelected && !_NoVitalNeeded)
            {
                vitalStatusLb.Text = "Not Selected";
            }
            else if (!_VitalSelected && _NoVitalNeeded)
            {
                vitalStatusLb.Text = "Bypassed";
            }
            else
            {
                vitalStatusLb.Text = "Selected";
            }
        }

        private void BluetoothForm_Load(object sender, EventArgs e)
        {
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
            this.comPortCb.SelectedIndex = 0;
        }

        public BluetoothAddress BluetoothSelect()
        {
            var dlg = new SelectBluetoothDeviceDialog();
            var rslt = dlg.ShowDialog();
            if (rslt != DialogResult.OK)
            {
                return null;
            }
            var addr = dlg.SelectedDevice.DeviceAddress;
            return addr;
        }

        private void SelectAradaBt_Click(object sender, EventArgs e)
        {
            try
            {
                log.Debug("Selecting BT INterface");
                BluetoothAddress adr = BluetoothSelect();
                if (adr != null)
                {
                    log.Debug("Got address");
                    BluetoothConfig BluetoothConfig = _uow.BluetoothConfigs.FindAll().FirstOrDefault();
                    if (BluetoothConfig != null)
                    {
                        log.Debug("Committing");
                        BluetoothConfig.Arada = adr.ToByteArray();
                        _uow.BluetoothConfigs.Set(BluetoothConfig.Id, BluetoothConfig);
                        _uow.Commit();
                        log.Debug("Committed");
                    }
                    else
                    {
                        _uow.BluetoothConfigs.Add(new BluetoothConfig()
                        {
                            Arada = adr.ToByteArray(),
                            Vital = "NONE"
                        });
                        _uow.Commit();
                    }
                    log.Debug("Setting Parent values");
                    ((IncZoneMDIParent)this.MdiParent)._AradaAddress = adr;
                    ((IncZoneMDIParent)this.MdiParent)._AradaSelected = true;
                    ((IncZoneMDIParent)this.MdiParent).SetConnectionStatus(ConnectionType.BLUETOOTH, ServiceConnectionState.Unknown);
                    aradaStatusLb.Text = "Selected";
                    log.Debug("Setting Parent values complete");
                }
                else
                {
                    aradaStatusLb.Text = "Not Selected";
                    ((IncZoneMDIParent)this.MdiParent).SetConnectionStatus(ConnectionType.BLUETOOTH, ServiceConnectionState.Disconnected);
                }
            }
            catch (Exception ex)
            {
                log.Error("Could not connect to DSRC - " + ex.Message);
                DialogResult DialogResult = MessageBox.Show("Could not select Bluetooth Device. Please try again.", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SelectVital_Click(object sender, EventArgs e)
        {
            if (this.comPortCb.SelectedIndex != 0)
            {
                BluetoothConfig BluetoothConfig = _uow.BluetoothConfigs.FindAll().FirstOrDefault();
                if (BluetoothConfig != null)
                {
                    BluetoothConfig.Vital = (string)this.comPortCb.SelectedValue;
                    _uow.BluetoothConfigs.Set(BluetoothConfig.Id, BluetoothConfig);
                    _uow.Commit();
                }
                else
                {
                    _uow.BluetoothConfigs.Add(new BluetoothConfig()
                    {
                        Arada = { },
                        Vital = (string)this.comPortCb.SelectedValue
                    });
                    _uow.Commit();
                }

                ((IncZoneMDIParent)this.MdiParent)._VitalSelected = true;
                vitalStatusLb.Text = "Selected";
                ((IncZoneMDIParent)this.MdiParent)._NoVitalNeeded = false;
                ((IncZoneMDIParent)this.MdiParent).VitalComPort = (string)this.comPortCb.SelectedValue;
                /*
                try
                {
                    IncZoneMDIParent.vitalModual.Connect((string)this.comPortCb.SelectedValue);
                }
                catch(Exception ex)
                {
                    log.Error("Vital Connect Exception", ex);
                }
                 * */
 //               ((IncZoneMDIParent)this.MdiParent).SetConnectionStatus(ConnectionType.VITAL, ServiceConnectionState.Connected);
            }
            else
            {
                MessageBox.Show("Please select Vital's COM Port!", SystemConstants.MessageBox_Caption_Warn, MessageBoxButtons.OK, MessageBoxIcon.Warning);                
            }
        }

        private void BypassVital_Click(object sender, EventArgs e)
        {
            ((IncZoneMDIParent)this.MdiParent)._NoVitalNeeded = true;
            vitalStatusLb.Text = "Bypassed";
            ((IncZoneMDIParent)this.MdiParent).SetConnectionStatus(ConnectionType.VITAL, ServiceConnectionState.Bypassed);
        }

        void RequestStatusChange(string form, string status)
        {
            if (form == "Bluetooth")
            {
                if (status == UIConstants.STATUS_DISCONNECTED || status == UIConstants.STATUS_UNKNOWN)
                {
                    aradaStatusLb.Text = status;
                }
                else
                {
                    aradaStatusLb.Text = status;
                }
            }
        }
    }
}
