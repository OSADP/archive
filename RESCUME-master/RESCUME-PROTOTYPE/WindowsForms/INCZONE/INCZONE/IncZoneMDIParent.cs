using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using INCZONE.Common;
using INCZONE.Exceptions;
using INCZONE.Forms;
using INCZONE.Forms.Configuration;
using INCZONE.Forms.Log;
using INCZONE.Managers;
using INCZONE.Model;
using INCZONE.NMEA;
using INCZONE.Repositories;
using INCZONE.VITAL;
using InTheHand.Net;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using InTheHand.Windows.Forms;
using Json;
using log4net;
using Newtonsoft.Json;
using Phidgets;
using Phidgets.Events;

namespace INCZONE
{
    public delegate void RequestLabelTextChangeDelegate(string newText);
    public delegate void RequestStatusChangeDelegate(string form,string status);
    public delegate void RequestAllStatusChangeDelegate(string status);
    public delegate void RequestButtonStatusChangeDelegate(bool status);
    public delegate void RequestIncidentUpdateDelegate(bool status);
    public delegate void RequestDIAUpdateDelegate(string newText);

    public partial class IncZoneMDIParent : BaseForm
    {
        readonly IUnitOfWork _uow;
        private int childFormNumber = 0;
        private static readonly ILog log = LogManager.GetLogger(SystemConstants.Logger_Ref);
        public ServiceConnectionState _CapWINState { get; set; }
        public ServiceConnectionState _CapWINMobileState { get; set; }
        public ServiceConnectionState _DGPSState { get; set; }
        public ServiceConnectionState _BlueToothState { get; set; }
        public ServiceConnectionState _DSRCState { get; set; }
        public ServiceConnectionState _RadioState { get; set; }
        public ServiceConnectionState _ResponderLocationState { get; set; }
        public ServiceConnectionState _VitalState { get; set; }
        public CapWINConfig _CapWinConfig { get; set; }
        public DGPSConfig _DGPSConfig { get; set; }
        public DSRCConfig _DSRCConfig { get; set; }
        public AlarmConfig _AlarmConfig { get; set; }
        public static Coordinate _ResponderLocation { get; set; }
        public static Coordinate _ResponderLocationOverRide { get; set; }
        private System.IO.Ports.SerialPort sport;
        public static Guid uid = new Guid("{6ba50000-7c6a-11e3-9b95-0002a5d5c51b}");
        private volatile string JsonStrings;
        public volatile bool _AradaSelected;
        public volatile bool _NoVitalNeeded;
        public volatile bool _VitalSelected;
        public BluetoothAddress _AradaAddress;
        public BluetoothClient AradaCient;
        public static Guid _SelectedMapSet = new Guid { };
        public static NetworkStream AradaPeer;
        public static NetworkStream VitalPeer;
        public static List<INCZONE.Common.MapNode> MapNodeList;
        public static List<INCZONE.Common.MapLink> MapLinkList;
        public static IncZoneMDIParent.AlarmMonitor AlarmMoni;
        private InterfaceKit ifKit;
        public static bool AppStarted = false;
        public volatile bool fiKitAtached = false;
        public volatile bool fiKitBypassed = false;
        public volatile bool CapWINBypassed = false;
        public string VitalComPort;
        public string IncidentName = "No Active Incident";
        public bool IncidentActive = false;
        public static VITALModule vitalModual;
        private Socket _NtripSocket;
        private IPEndPoint BroadCaster;
        private bool Init = false;
        //public static int _ReconnectCount = 0;

        public static String _LoadedMapName = "Not Selected";
        public static String _IncidentName = "Not Selected";
        public static String _dsrcName = "Not Selected";
        public static String _vitalName = "Not Selected";
        public static String _capwinPort = "Not Selected";

        public event RequestLabelTextChangeDelegate RequestLabelTextChange;
        public event RequestStatusChangeDelegate RequestStatusChange;
        public event RequestButtonStatusChangeDelegate RequestButtonStatusChange;
        public event RequestIncidentUpdateDelegate RequestIncidentChange;
        public event RequestDIAUpdateDelegate RequestDIAChange;

        /// <summary>
        /// 
        /// </summary>
        public IncZoneMDIParent()
        {
 //           log.Debug("In IncZoneMDIParent Constructor");
            string connectionString = ConfigurationManager.ConnectionStrings["IncZoneEntities"].ConnectionString;
            this._uow = new UnitOfWork(connectionString);

            _ResponderLocationOverRide = null;
            AlarmMoni = new AlarmMonitor();
            AlarmMoni.AlarmRaised += this.AlarmMon;
            AlarmMoni.VisualAlarmRaised += this.AlarmMon;
            vitalModual = new VITALModule();
            //_ReconnectCount = 0;

            InitializeComponent();

            try
            {

                CapWINConfiguration CapWinEntity = _uow.CapWINConfigurations.FindAll().FirstOrDefault();
                DGPSConfiguration DGPSEntity = _uow.DGPSConfigurations.FindWhere(m => m.IsDefault).FirstOrDefault();
                DSRCConfiguration DSRCEntity = _uow.DSRCConfigurations.FindAll().FirstOrDefault();
                AlarmConfiguration AlarmEntity = _uow.AlarmConfigurations.FindWhere(m => m.IsDefault).FirstOrDefault();

                SetConnectionStatus(ConnectionType.CAP_WIN, ServiceConnectionState.Disconnected);
                SetConnectionStatus(ConnectionType.DGPS, ServiceConnectionState.Bypassed);
                SetConnectionStatus(ConnectionType.CAP_WIN_MOBILE, ServiceConnectionState.Disconnected);
                SetConnectionStatus(ConnectionType.DSCR, ServiceConnectionState.Disconnected);
                SetConnectionStatus(ConnectionType.BLUETOOTH, ServiceConnectionState.Disconnected);
                SetConnectionStatus(ConnectionType.RADIO, ServiceConnectionState.Disconnected);
                SetConnectionStatus(ConnectionType.RESPONDER_LOC, ServiceConnectionState.Disconnected);
                SetConnectionStatus(ConnectionType.VITAL, ServiceConnectionState.Disconnected);

                try
                {

                    ifKit = new InterfaceKit();

                    ifKit.Attach += new AttachEventHandler(ifKit_Attach);
                    ifKit.Detach += new DetachEventHandler(ifKit_Detach);
                    ifKit.Error += new Phidgets.Events.ErrorEventHandler(ifKit_Error);
                    ifKit.OutputChange += new OutputChangeEventHandler(ifKit_OutputChange);
                    ifKit.open();
                    fiKitAtached = true;

                }
                catch (PhidgetException ex)
                {
                    log.Error("Could not start Phidget");
                }

                BluetoothConfig BluetoothConfig = _uow.BluetoothConfigs.FindAll().FirstOrDefault();
                if (_ConfiguredProperly(CapWinEntity, DGPSEntity, DSRCEntity, AlarmEntity, BluetoothConfig))
                {
                    _AradaAddress = new BluetoothAddress(BluetoothConfig.Arada);
                    /*
                    VitalComPort = BluetoothConfig.Vital;
                    if (VitalComPort != "NONE")
                    {
                        try
                        {
                            vitalModual.Connect(VitalComPort);
                        }
                        catch(Exception ex)
                        {
                            log.Error("Vital Connect Exception", ex);
                        }
                    }
                     * */
                    _AradaSelected = true;
                    _VitalSelected = true;
                    _NoVitalNeeded = false;
                    _SelectedMapSet = new Guid { };
                    _OpenForm(new MainForm(this));
                }
                else
                {
                    string ConfigMessage = string.Empty;
                    ConfigMessage = _GetConfigurationNeededMessage(CapWinEntity, DGPSEntity, DSRCEntity, AlarmEntity, BluetoothConfig);
                    _OpenForm(new ConnectionForm(this));
                    MessageBox.Show("The IncZone application is not properly configured" + ConfigMessage, SystemConstants.MessageBox_Caption_Warn, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                log.Error("IncZoneMDIParent Exception", ex);
                LogEventsManager.LogEvent("MDI Parent could not start", LogEventTypes.SYSTEM_ERROR, LogLevelTypes.ERROR,ex.Message);
                MessageBox.Show("The MDI Parent could not start, an error occured", SystemConstants.MessageBox_Caption_Error, MessageBoxButtons.OK, MessageBoxIcon.Error);                
            }
        }

        private void IncZoneMDIParent_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (ifKit != null)
            {
                ifKit.Attach -= new AttachEventHandler(ifKit_Attach);
                ifKit.Detach -= new DetachEventHandler(ifKit_Detach);
                ifKit.OutputChange -= new OutputChangeEventHandler(ifKit_OutputChange);
                ifKit.Error -= new Phidgets.Events.ErrorEventHandler(ifKit_Error);
                ifKit.close();
            }

            if (AppStarted)
            {
                _StopIncZone();
                Thread.Sleep(3000);
            }
        }

        #region Start/Stop and Handle Configuration
        internal void _StartIncZone()
        {
            bool CanStart = false;

            if (_ConfiguredProperly(_CapWinConfig, _DGPSConfig, _DSRCConfig, _AlarmConfig))
            {
                Form form = null;
                try
                {

                    if (_NoVitalNeeded)
                    {
                        if (_AradaSelected && _AradaAddress != null)
                        {
                            AradaPeer = BluetoothConnect(_AradaAddress);
                            CanStart = true;
                        }
                        else
                        {
                            IncZoneMDIParent.AppStarted = false;
                            form = this.ActiveMdiChild;
                            if (RequestButtonStatusChange != null)
                            {
                                RequestButtonStatusChange(false);
                            }
                            MessageBox.Show("The IncZone application is not ready to start, please select a DSRC device", SystemConstants.MessageBox_Caption_Warn, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            CanStart = false;
                        }
                    }
                    else
                    {
                        SetConnectionStatus(ConnectionType.DSCR, ServiceConnectionState.Connecting);
                        BluetoothConfig BluetoothConfig = _uow.BluetoothConfigs.FindAll().FirstOrDefault();
                        VitalComPort = BluetoothConfig.Vital;
                        if (VitalComPort != "NONE")
                        {
                            try
                            {
                                log.Debug("Connecting to VITAL port:" + VitalComPort);
                                vitalModual.Connect(VitalComPort);
                            }
                            catch (Exception ex)
                            {
                                log.Error("Vital Connect Exception", ex);
                            }
                        }
                        if (_AradaSelected && _AradaAddress != null)
                        {
                            AradaPeer = BluetoothConnect(_AradaAddress);
                            CanStart = true;
                        }
                        else
                        {
                            IncZoneMDIParent.AppStarted = false;
                            form = this.ActiveMdiChild;
                            if (RequestButtonStatusChange != null)
                            {
                                RequestButtonStatusChange(false);
                            }
                            MessageBox.Show("The IncZone application is not ready to start, please select a DSRC and Vital device", SystemConstants.MessageBox_Caption_Warn, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            CanStart = false;
                        }
                    }

                    if (CanStart)
                    {
                        Cursor.Current = Cursors.WaitCursor;
                        ResponderTimer.Enabled = true;
                        ResponderTimer.Interval = 10000;
                        ResponderTimer.Start();
                        StartCapWINMobile();
                        CapWINMobileTimer.Enabled = true;
                        CapWINMobileTimer.Interval = 1000;
                        CapWINMobileTimer.Start();
                        _StartLocationTimer();
                        bool TriggerVital = true;

                        // Remove NTRIP
//                        _NtripSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
//                        BroadCaster = new IPEndPoint(IPAddress.Parse(_DGPSConfig.HostIP), Convert.ToInt32(_DGPSConfig.HostPort));
//                        _NtripSocket.Connect(BroadCaster);
                        Init = false;
                        List<VehicleAlarm> VehicleAlarmList = _uow.VehicleAlarms.FindAll().ToList();
                        if (VehicleAlarmList != null && VehicleAlarmList.Count > 0)
                        {
                            TriggerVital = VehicleAlarmList[1].Active;
                        }

                        AlarmMoni.SetAlarm(new Alarm()
                        {
                            AlarmLevel = AlarmLevelTypes.Level_0,
                            TriggerVital = TriggerVital
                        });
                        AlarmMoni.SetAlarm(new VisualAlarm(0, 0, 0, 0));
                        SetConnectionStatus(ConnectionType.CAP_WIN, ServiceConnectionState.Connecting);
                        SetConnectionStatus(ConnectionType.CAP_WIN_MOBILE, ServiceConnectionState.Connecting);
                        SetConnectionStatus(ConnectionType.DGPS, ServiceConnectionState.Connecting);
                        IncZoneMDIParent.AppStarted = true;
                        form = this.ActiveMdiChild;
                        if (RequestButtonStatusChange != null)
                        {
                            RequestButtonStatusChange(true);
                        }
                        Cursor.Current = Cursors.Default;
                        ResponderTimer_Tick(this, new EventArgs());
                    }
                }
                catch (SocketException ex)
                {
                    Cursor.Current = Cursors.Default;
                    IncZoneMDIParent.AppStarted = false;
                    form = this.ActiveMdiChild;
                    if (RequestButtonStatusChange != null)
                    {
                        RequestButtonStatusChange(false);
                    }
                    _HandleSocketException(ex);
                }
                catch (Exception ex)
                {
                    Cursor.Current = Cursors.Default;
                    IncZoneMDIParent.AppStarted = false;
                    form = this.ActiveMdiChild;
                    if (RequestButtonStatusChange != null)
                    {
                        RequestButtonStatusChange(false);
                    }
                    var msg = "DSRC connection failed: " + MakeExceptionMessage(ex);
                    log.Error(msg, ex);
                    LogEventsManager.LogEvent("DSRC connection failed", LogEventTypes.BLUETOOTH_CONFIG, LogLevelTypes.ERROR);
                    MessageBox.Show(msg);
                }
            }
            else
            {
                IncZoneMDIParent.AppStarted = false;
                string msg = _GetConfigurationNeededMessage(_CapWinConfig, _DGPSConfig, _DSRCConfig, _AlarmConfig);
                MessageBox.Show("The IncZone application is not properly configured" + msg, SystemConstants.MessageBox_Caption_Warn, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void _HandleSocketException(SocketException ex)
        {
            string reason;
            switch (ex.ErrorCode)
            {
                case 10048: // SocketError.AddressAlreadyInUse
                    // RFCOMM only allow _one_ connection to a remote service from each device.
                    reason = "There is an existing connection to the remote DSRC Bluetooth Service";
                    break;
                case 10049: // SocketError.AddressNotAvailable
                    reason = "DSRC Bluetooth Service not running on remote device";
                    break;
                case 10064: // SocketError.HostDown
                    reason = "DSRC Bluetooth Service not using RFCOMM (huh!!!)";
                    break;
                case 10013: // SocketError.AccessDenied:
                    reason = "Authentication required";
                    break;
                case 10060: // SocketError.TimedOut:
                    reason = "Timed-out";
                    break;
                default:
                    reason = null;
                    break;
            }
            reason += " (" + ex.ErrorCode.ToString() + ") -- ";
            //
            var msg = "Bluetooth connection failed: " + MakeExceptionMessage(ex);
            msg = reason + msg;
            log.Error("DSRC Connection Exception", ex);
            LogEventsManager.LogEvent("DSRC connection failed", LogEventTypes.BLUETOOTH_CONFIG, LogLevelTypes.ERROR);
            MessageBox.Show(msg);
        }

        private void _StopSendingTIMAndEVA(NetworkStream AradaPeer)
        {
            MessageManager MessageManager = new MessageManager(this);

            if (_AradaConnected())
            {
                try
                {
                    log.Debug("Sending Stop EVA");
                    byte[] outgoing = MessageManager.CreatOutgoingMessage(TypeId.STOPEVA, null);
//                    await AradaPeer.WriteAsync(outgoing, 0, outgoing.Length);
                    AradaPeer.Write(outgoing, 0, outgoing.Length);
                    AradaPeer.Flush();
                    log.Debug("Sending Stop TIM");
                    outgoing = MessageManager.CreatOutgoingMessage(TypeId.STOPTIM, null);
//                    await AradaPeer.WriteAsync(outgoing, 0, outgoing.Length);
                    AradaPeer.Write(outgoing, 0, outgoing.Length);
                    AradaPeer.Flush();
                }
                catch(Exception ex)
                {
                    log.Error("Send Exception");
                    log.Error("_StopSendingTIMAndEVA", ex);
                }
            }
            else
            {
                log.Error("Arada not connected, could not send stop TIM and EVA");
            }
        }

        internal void _StopIncZone(bool StopDSRCMessages = true)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new MethodInvoker(() => { _StopIncZone(StopDSRCMessages); }));
                return;
            }
            try
            {
                IncidentName = "No Active Incident";
                ResponderTimer.Stop();
                CapWINMobileTimer.Stop();
                SystemActiveTimer.Stop();
                LocationTimer.Stop();
                if (StopDSRCMessages)
                {
                    _StopSendingTIMAndEVA(AradaPeer);
                }
                Thread.Sleep(1000);
                log.Debug("Continuing Stop IncZone");
                AppStarted = false;
                vitalModual.Disconnect();
                Cursor.Current = Cursors.WaitCursor;
                if (_NtripSocket != null)
                    _NtripSocket.Close();
                ShutDownCapWINMobilePort();
                _TriggerIncident(false);
                Init = false;
                if (AradaCient != null)
                    AradaCient.Close();
                if (AradaPeer != null)
                    AradaPeer.Close();

                Form form = this.ActiveMdiChild;
                if (RequestButtonStatusChange != null)
                {
                    log.Debug("_stopIncZone RequestButtonStatusChange");
                    RequestButtonStatusChange(false);
                }
                else
                {
                    log.Debug("_stopIncZone RequestButtonStatusChange == null");
                }
                bool TriggerVital = true;
                if (cts != null)
                    cts.Cancel();
                List<VehicleAlarm> VehicleAlarmList = _uow.VehicleAlarms.FindAll().ToList();
                if (VehicleAlarmList != null && VehicleAlarmList.Count > 0)
                {
                    TriggerVital = VehicleAlarmList[0].Active;
                }

                AlarmMoni.SetAlarm(new Alarm()
                {
                    AlarmLevel = AlarmLevelTypes.Level_0,
                    TriggerVital = TriggerVital
                }
                );

                AlarmMoni.SetAlarm(new VisualAlarm(0, 0, 0, 0));

                Cursor.Current = Cursors.Default;
                SetConnectionStatus(ConnectionType.CAP_WIN, ServiceConnectionState.Disconnected);
                SetConnectionStatus(ConnectionType.CAP_WIN_MOBILE, ServiceConnectionState.Disconnected);
                SetConnectionStatus(ConnectionType.DGPS, ServiceConnectionState.Disconnected);
                SetConnectionStatus(ConnectionType.DSCR, ServiceConnectionState.Disconnected);
                if (!_NoVitalNeeded)
                    SetConnectionStatus(ConnectionType.VITAL, ServiceConnectionState.Disconnected);
            }
            catch(Exception ex)
            {
                log.Error("_StopIncZone Exception", ex);
            }
        }

        private string _GetConfigurationNeededMessage(CapWINConfiguration CapWinEntity, DGPSConfiguration DGPSEntity, DSRCConfiguration DSRCEntity, AlarmConfiguration AlarmEntity, BluetoothConfig BluetoothConfig)
        {
            string msg = "";

            List<string> configList = new List<string>();

            if (CapWinEntity == null)
            {
                configList.Add("CapWIN");
            }
            if (DGPSEntity == null)
            {
                configList.Add("DGPS");
            }
            if (DSRCEntity == null)
            {
                configList.Add("DSRC");
            }
            if (AlarmEntity == null)
            {
                configList.Add("Alarm");
            }
            if (BluetoothConfig == null)
            {
                configList.Add("Bluetooth");
            }

            if (configList.Count > 0)
            {
                msg += ", please configure the following - ";
                foreach (string var in configList)
                {
                    msg += var + ", ";
                }
            }

            return msg;
        }

        private bool _ConfiguredProperly(CapWINConfiguration CapWinEntity, DGPSConfiguration DGPSEntity, DSRCConfiguration DSRCEntity, AlarmConfiguration AlarmEntity, BluetoothConfig BluetoothConfig)
        {
            bool returnVal = false;

            if (CapWinEntity != null && DGPSEntity != null && DSRCEntity != null && AlarmEntity != null && BluetoothConfig != null)
            {
                returnVal = true;
            }

            if (CapWinEntity != null)
            {
                _CapWinConfig = new CapWINConfig(CapWinEntity);
            }

            if (DGPSEntity != null)
            {
                _DGPSConfig = new DGPSConfig(DGPSEntity);
            }

            if (AlarmEntity != null)
            {
                _AlarmConfig = new AlarmConfig(AlarmEntity);
            }

            if (DSRCEntity != null)
            {
                _DSRCConfig = new DSRCConfig(DSRCEntity);
            }

            return returnVal;
        }

        private bool _ConfiguredProperly(CapWINConfig _CapWinConfig, DGPSConfig _DGPSConfig, DSRCConfig _DSRCConfig, AlarmConfig _AlarmConfig)
        {
            if (_CapWinConfig != null && _DGPSConfig != null && _DSRCConfig != null && _SelectedMapSet != Guid.Empty && fiKitAtached && _AlarmConfig != null)
            {
                return true;
            }

            return false;
        }

        private string _GetConfigurationNeededMessage(CapWINConfig _CapWinConfig, DGPSConfig _DGPSConfig, DSRCConfig _DSRCConfig, AlarmConfig _AlarmConfig)
        {
            string msg = "";

            List<string> configList = new List<string>();

            if (_CapWinConfig == null)
            {
                configList.Add("CapWIN");
            }
            if (_DGPSConfig == null)
            {
                configList.Add("DGPS");
            }
            if (_DSRCConfig == null)
            {
                configList.Add("DSRC");
            }
            if (_AlarmConfig == null)
            {
                configList.Add("Alarm");
            }
            if (_SelectedMapSet == Guid.Empty)
            {
                configList.Add("MapSet");
            }
            if (!fiKitAtached)
            {
                configList.Add("Radio Interface");
            }

            if (configList.Count > 0)
            {
                msg += ", please configure the following - ";
                foreach (string var in configList)
                {
                    msg += var + ", ";
                }
            }

            return msg;
        }
        #endregion

        #region TImers
        private void _StartLocationTimer()
        {
            try
            {
                LocationTimer.Interval = 100;//_DGPSConfig.LocationRefreshRate;
                LocationTimer.Start();
            }
            catch (Exception ex)
            {
                log.Error("_StartLocationTimer Exception", ex);
            }
        }

        public bool vitalStatus;
        private void _UpdateVitalConnected(bool state)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new MethodInvoker(() => { _UpdateVitalConnected(state); }));
                return;
            }
            vitalStatus = state;

            if (vitalStatus)
            {
                if (GetConnectionStatus(ConnectionType.VITAL) != UIConstants.STATUS_CONNECTED)
                    SetConnectionStatus(ConnectionType.VITAL, ServiceConnectionState.Connected);
            }
            else
            {
                if (GetConnectionStatus(ConnectionType.VITAL) != UIConstants.STATUS_DISCONNECTED)
                    SetConnectionStatus(ConnectionType.VITAL, ServiceConnectionState.Disconnected);
            }

            try
            {
                Form form = this.ActiveMdiChild;
                if (form is AlertsForm)
                {
                    AlertsForm AlertsForm = (AlertsForm)form;
                    AlertsForm.updateVitial(state);
                }
            }
            catch (Exception ex)
            {
                log.Error("_TriggerVisualAlarm Exception", ex);
            }

            
        }


        private async void LocationTimer_Tick(object sender, EventArgs e)
        {
            if (!_NoVitalNeeded)
                _UpdateVitalConnected(vitalModual != null && vitalModual.IsConnected());
            

            byte[] outbuf = { };
            byte[] temp = new byte[4000];
            //NTripManager NTripManager = new NTripManager(_DGPSConfig);

            if (_ResponderLocationOverRide != null)
            {
                _ResponderLocation = _ResponderLocationOverRide;
            }

            if (_AradaConnected())
            {
                if (AradaPeer.DataAvailable)
                {
                    int x = await AradaPeer.ReadAsync(temp, 0, temp.Length);
                    outbuf = new byte[x];
                    Array.Copy(temp, outbuf, x);

                    _UpdateJsonStrings(Encoding.ASCII.GetString(outbuf));
                    if (!JsonStrings.Contains('\0'))
                    {
                        _SetResponderCoordinate(JsonStrings);

                        List<JsonDIA> DIAList = _GetDeserializedDIAList(JsonStrings);

                        if (this.ActiveMdiChild is LogOutputForm)
                        {
                            foreach (JsonDIA var in DIAList)
                            {
                                if (RequestDIAChange != null)
                                {
                                    RequestDIAChange(var.ToString());
                                }

                            }
                        }

                        List<JsonTHREAT> THREATList = _GetDeserializedTHREATList(JsonStrings);
                        if (THREATList != null && THREATList.Count > 0)
                        {
                            List<Alarm> alarmList = new List<Alarm>();
                            bool TriggerAlarm1 = true;
                            bool TriggerAlarm2 = true;
                            bool TriggerAlarm3 = true;
                            bool TriggerAlarm4 = true;

                            List<VehicleAlarm> VehicleAlarms = _uow.VehicleAlarms.FindAll().ToList();
                            if (VehicleAlarms != null && VehicleAlarms.Count > 0)
                            {
                                TriggerAlarm1 = VehicleAlarms[1].Active;
                                TriggerAlarm2 = VehicleAlarms[2].Active;
                                TriggerAlarm3 = VehicleAlarms[3].Active;
                                TriggerAlarm4 = VehicleAlarms[4].Active;
                            }

                            foreach (JsonTHREAT var in THREATList)
                            {
                                if (var.tlevel0count > 0)
                                {
                                    Alarm alarm = new Alarm()
                                    {
                                        AlarmLevel = AlarmLevelTypes.Level_1,
                                        TriggerVital = TriggerAlarm1
                                    };

                                    alarmList.Add(alarm);
                                }

                                if (var.tlevel1count > 0)
                                {
                                    Alarm alarm = new Alarm()
                                    {
                                        AlarmLevel = AlarmLevelTypes.Level_2,
                                        TriggerVital = TriggerAlarm2
                                    };

                                    alarmList.Add(alarm);
                                }

                                if (var.tlevel2count > 0)
                                {
                                    Alarm alarm = new Alarm()
                                    {
                                        AlarmLevel = AlarmLevelTypes.Level_3,
                                        TriggerVital = TriggerAlarm3
                                    };

                                    alarmList.Add(alarm);
                                }

                                if (var.tlevel3count > 0)
                                {
                                    Alarm alarm = new Alarm()
                                    {
                                        AlarmLevel = AlarmLevelTypes.Level_4,
                                        TriggerVital = TriggerAlarm4
                                    };

                                    alarmList.Add(alarm);
                                }

                                Alarm tempAlarm = AlarmManager._GetHighestAlarm(alarmList);
                                if (tempAlarm != null)
                                {
                                    AlarmMoni.SetAlarm(tempAlarm);
                                }

                            }
                            VisualAlarm newAlarm = new VisualAlarm(THREATList[0].tlevel0count, THREATList[0].tlevel1count, THREATList[0].tlevel2count, THREATList[0].tlevel3count);
                            _TriggerVisualAlarm(newAlarm);
                            AlarmMoni.SetAlarm(newAlarm);
                        }
                    }
                }

            }                
        }

        private void SystemActiveTimer_Tick(object sender, EventArgs e)
        {
            bool TriggerAlarm = true;
            List<VehicleAlarm> VehicleAlarms = _uow.VehicleAlarms.FindAll().ToList();
            if (VehicleAlarms != null && VehicleAlarms.Count > 0)
            {
                TriggerAlarm = VehicleAlarms[0].Active;
            }

            AlarmMonitor AlarmMonitor = new AlarmMonitor();
            AlarmMonitor.SetAlarm(new Alarm()
            {
                AlarmLevel = AlarmLevelTypes.Level_1,
                TriggerVital = TriggerAlarm
            }
            );
            AlarmMonitor.SetAlarm(new VisualAlarm(1, 0, 0, 0));
        }

        private async void ResponderTimer_Tick(object sender, EventArgs e)
        {
            if (_AradaConnected())
            {
                if (_ResponderLocationValid())
                {

                    if (GetConnectionStatus(ConnectionType.CAP_WIN) != UIConstants.STATUS_CONNECTED)
                        _UpdateCapWINStatus(ServiceConnectionState.Connected);
//                    if (GetConnectionStatus(ConnectionType.DGPS) != UIConstants.STATUS_CONNECTED)
//                        _UpdateDGPSStatus(ServiceConnectionState.Connected);
                   

                    if (GetConnectionStatus(ConnectionType.CAP_WIN) != UIConstants.STATUS_CONNECTED)
                        _UpdateCapWINStatus(ServiceConnectionState.Connected);
                    try
                    {

                        MessageManager MessageManager = new MessageManager(this);
                        string TIMBits = string.Empty;
                        byte[] bytes = { };

                        if (!CapWINBypassed)
                        {
                            CapWINManager CapWINManager = new CapWINManager(_CapWinConfig);
                            CapWINIncidentListType1 CapWINIncidentListType = await CapWINManager.GetCapWINIncidentsList();
                            TIMBits = await MessageManager.CreateTIMMessage(_ResponderLocation, CapWINIncidentListType, _CapWinConfig.DistanceToIncident, _CapWinConfig.LaneData, IncZoneMDIParent.MapNodeList, IncZoneMDIParent.MapLinkList);
                            //LogEventsManager.LogEvent("Incident Received", LogEventTypes.INCIDENT, LogLevelTypes.INFO);
                        }
                        _UpdateCapWINDisconnectedStatus();
                        try
                        {
                            if (!CapWINBypassed)
                            {
                                if (!string.IsNullOrEmpty(TIMBits))
                                {
                                    _TriggerIncident(true);
                                    byte[] outgoingEVA = MessageManager.CreatOutgoingMessage(TypeId.EVA, "300");
                                    LogEventsManager.LogEvent("TIM Sent", LogEventTypes.TIM, LogLevelTypes.INFO, DateTime.Now.ToString());
                                    byte[] outgoingTIM = MessageManager.CreatOutgoingMessage(TypeId.TIM, TIMBits);

                                    await AradaPeer.WriteAsync(outgoingEVA, 0, outgoingEVA.Length);
                                    AradaPeer.Flush();
                                    await AradaPeer.WriteAsync(outgoingTIM, 0, outgoingTIM.Length);
                                    AradaPeer.Flush();
                                }
                                else
                                {
                                    _TriggerIncident(false);
                                }
                            }
                        }
                        catch (TIMException timex)
                        {
                            log.Error("TIM message create exception: " + timex.Message);
                        }
                        catch (SocketException se)
                        {
                            log.Error("CapWIN SocketException ", se);
                            throw se;
                        }
                        catch (IOException ioe)
                        {
                            log.Error("CapWIN IOException ", ioe);
                            throw ioe;
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    }
                    catch (SocketException se)
                    {
                        log.Error("DSRC or Capwin SocketException ", se);
                        _StopIncZone();
                        DialogResult DialogResult = MessageBox.Show("Lost connection to the DSRC radio. The incident was stopped.", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    catch (IOException ex)
                    {
                        log.Error("DSRC or Capwin IOException ", ex);
                        _StopIncZone();
                        DialogResult DialogResult = MessageBox.Show("Lost connection to the DSRC radio. The incident was stopped.", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    catch (Exception capwinex)
                    {
                        _UpdateCapWINStatus(ServiceConnectionState.Disconnected);
                        log.Error("CapWIN Exception ", capwinex);
                    }
                }
            }
        }

        private string _GetNMEAGPGGAString(Coordinate _ResponderLocationOverRide)
        {

            return GPGGA.GenerateGPGGAcode(_ResponderLocationOverRide);
        }

        private byte[] CreateRequest(string responderLocation)
        {
            //log.Debug("In CreateRequest");
            string msg = string.Empty;

            try
            {
                string auth = ToBase64(_DGPSConfig.Username + ":" + _DGPSConfig.Password);

                msg = "GET /" + UIConstants.NTRIP_MOUNT_POINT + " HTTP/1.0\r\n";
                msg += "Authorization: Basic " + auth;
                msg += "\r\n\r\n";
            }
            catch (Exception ex)
            {
                log.Error("CreateRequest Exception", ex);
            }

            return Encoding.ASCII.GetBytes(msg);
        }

        private static string ToBase64(string str)
        {
            //log.Debug("In ToBase64");

            byte[] byteArray = null;

            try
            {
                Encoding asciiEncoding = Encoding.ASCII;
                byteArray = asciiEncoding.GetBytes(str);

            }
            catch (Exception ex)
            {
                log.Error("ToBase64 Exception", ex);
            }

            return Convert.ToBase64String(byteArray, 0, byteArray.Length);
        }

        private void CapWINMobileTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                if (GetConnectionStatus(ConnectionType.CAP_WIN_MOBILE) != UIConstants.STATUS_CONNECTED)
                    _UpdateCapWINMobileStatus(ServiceConnectionState.Connected);

                if (!string.IsNullOrEmpty(JsonStrings))
                {
                    List<JsonGPGGA> JsonNMEAList = _GetDeserializedNMEAList(JsonStrings);
                    foreach (JsonGPGGA var in JsonNMEAList)
                    {
                        Send(var.gga);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Serial DoWork Exception : ", ex);
                _UpdateCapWINMobileStatus(ServiceConnectionState.Disconnected);
                LogEventsManager.LogEvent("CapWIN Mobile Exception", LogEventTypes.SYSTEM_ERROR, LogLevelTypes.ERROR);
                throw ex;
            }
        }
        #endregion

        #region JSON/NMEA string parsing
        private void _SetResponderCoordinate(string JsonStrings)
        {
            Func<int> del = delegate()
            {
                try
                {
                    if (_ResponderLocationOverRide == null)
                    {
                        string CleanNMEA = _GetStringsByTypeId(JsonStrings, TypeId.NMEA);

                        if (!string.IsNullOrEmpty(CleanNMEA))
                        {
                            log.Debug(CleanNMEA);
                            List<JsonGPGGA> NMEAList = JsonConvert.DeserializeObject<List<JsonGPGGA>>(CleanNMEA);
                            _ResponderLocation = _GetResponderCoordinate(NMEAList);
                        }
                    } else 
                    {
                        _ResponderLocation = _ResponderLocationOverRide;
                    }
                    Form form = this.ActiveMdiChild;
                }
                catch (Exception ex)
                {
                    log.Error("JSon DeserializeObject Exception : " + ex.Message);
                }
                return 0;
            };

            Invoke(del);
        }

        private static List<JsonGPGGA> _GetDeserializedNMEAList(string JsonStrings)
        {
            List<JsonGPGGA> NMEAList = new List<JsonGPGGA>();

            try
            {
                string CleanNMEA = _GetStringsByTypeId(JsonStrings, TypeId.NMEA);

                if (!string.IsNullOrEmpty(CleanNMEA))
                {
                    //log.Debug(CleanNMEA);
                    NMEAList = JsonConvert.DeserializeObject<List<JsonGPGGA>>(CleanNMEA);
                }
            }
            catch (Exception ex)
            {
                log.Error("JSon DeserializeObject NMEA Exception : " + ex.Message);
            }

            return NMEAList;
        }

        private static List<JsonDIA> _GetDeserializedDIAList(string JsonStrings)
        {
            List<JsonDIA> DIAList = new List<JsonDIA>();

            try
            {
                string CleanDIA = _GetStringsByTypeId(JsonStrings, TypeId.DIA);

                if (!string.IsNullOrEmpty(CleanDIA))
                {
                    //log.Debug(CleanDIA);
                    DIAList = JsonConvert.DeserializeObject<List<JsonDIA>>(CleanDIA);
                }
            }
            catch (Exception ex)
            {
                log.Error("JSon DeserializeObject DIA Exception : " + ex.Message);
            }

            return DIAList;
        }

        private static List<JsonTHREAT> _GetDeserializedTHREATList(string JsonStrings)
        {
            List<JsonTHREAT> THREATList = new List<JsonTHREAT>();

            try
            {
                string CleanTHREAT = _GetStringsByTypeId(JsonStrings, TypeId.THREAT);

                if (!string.IsNullOrEmpty(CleanTHREAT))
                {
                    log.Debug(CleanTHREAT);
                    THREATList = JsonConvert.DeserializeObject<List<JsonTHREAT>>(CleanTHREAT);
//                    log.Debug("THREATList Count : " + THREATList.Count);
                }
            }
            catch (Exception ex)
            {
                log.Error("JSon DeserializeObject THREAT Exception : " + ex.Message);
            }

            return THREATList;
        }

        private static Coordinate _GetResponderCoordinate(List<JsonGPGGA> NMEAList)
        {
            if (NMEAList != null && NMEAList.Count > 0)
            {
                GPGGA GPGGA = new GPGGA(NMEAList[0].gga);
                
                return GPGGA.Position;
            }

            return null;
        }

        private string _GetResponderDGPSNMEA(List<JsonGPGGA> NMEAList)
        {
            if (NMEAList != null && NMEAList.Count > 0)
            {
                return NMEAList[0].gga;
            }

            return null;
        }

        private static string _GetStringsByTypeId(string JsonStrings,TypeId TypeId)
        {
            char BeginChar = Convert.ToChar(0x02);
            char EndChar = Convert.ToChar(0x03);
            string ReturnString = string.Empty;
            string ConcatString = string.Empty;
        
            try
            {
                if (!string.IsNullOrEmpty(JsonStrings))
                {
                    string[] SplitString = JsonStrings.Split(BeginChar);
                    if (SplitString != null)
                    {
                        foreach (string var in SplitString)
                        {
                            if (!string.IsNullOrEmpty(var))
                            {
                                int count = var.Count(x => x == EndChar);
                                if (count == 1)
                                {
                                    if (TypeId.DIA == TypeId)
                                    {
                                        if (var.StartsWith("{\"typeid\":\"DIA\",\"gpsfix\":") && var[var.Length - 1] == EndChar)
                                        {
                                            ConcatString += var.Substring(0, var.Length - 1) + ",";
                                        }
                                    }
                                    else if (TypeId.NMEA == TypeId)
                                    {
                                        if (var.StartsWith("{\"typeid\":\"NMEA\",\"gga\":\"") && var[var.Length - 1] == EndChar)
                                        {
                                            ConcatString += var.Substring(0, var.Length - 1) + ",";
                                        }
                                    }
                                    else if (TypeId.THREAT == TypeId)
                                    {
                                        if (var.StartsWith("{\"typeid\":\"THREAT\",\"tlevel0count\":") && var[var.Length - 1] == EndChar)
                                        {
                                            ConcatString += var.Substring(0, var.Length - 1) + ",";
                                        }
                                    }
                                }
                            }
                        }

                        if (!string.IsNullOrEmpty(ConcatString))
                        {
                            ReturnString = "[" + ConcatString.Remove(ConcatString.Length - 1, 1) + "]";
                            //log.Debug(ReturnString);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("_GetStringsByTypeId ", ex);
            }

            return ReturnString;
        }

        private void _UpdateJsonStrings(string JsonStrings)
        {
            Func<int> del = delegate()
            {
                this.JsonStrings = JsonStrings;
                return 0;
            };

            Invoke(del);
        }
        #endregion

        #region Serial Port

        public void StartCapWINMobile()
        {
            try
            {
                if (!string.IsNullOrEmpty(_CapWinConfig.BaudRate) && !string.IsNullOrEmpty(_CapWinConfig.ComPort))
                {
                    _UpdateCapWINMobileStatus(ServiceConnectionState.Connecting);
                    try
                    {
                        serialport_connect(_CapWinConfig.ComPort, Convert.ToInt32(_CapWinConfig.BaudRate), System.IO.Ports.Parity.None, 8, System.IO.Ports.StopBits.One);
                    }
                    catch (IOException ioe)
                    {
                        log.Error("CapWIN Mobile serial port not configured properly", ioe);
                        LogEventsManager.LogEvent("CapWIN Mobile serial not configured properly", LogEventTypes.SYSTEM_ERROR, LogLevelTypes.ERROR);
                        _UpdateCapWINMobileStatus(ServiceConnectionState.Disconnected);
                        throw ioe;
                    }
                    catch (Exception e)
                    {
                        log.Error("CapWIN Mobile serial comunication could not be started", e);
                        LogEventsManager.LogEvent("CapWIN Mobile serial comunication could not be started", LogEventTypes.SYSTEM_ERROR, LogLevelTypes.ERROR);
                        _UpdateCapWINMobileStatus(ServiceConnectionState.Disconnected);
                        throw e;
                    }
                }
                else
                {
                    LogEventsManager.LogEvent("CapWIN Mobile serial port not configured properly", LogEventTypes.CAPWIN_MOBILE_DISCONNECTED, LogLevelTypes.INFO);
                    _UpdateCapWINMobileStatus(ServiceConnectionState.Disconnected);
                }
            } 
            catch(Exception ex)
            {
                log.Error("Start CapWIN Mobile exception", ex);
                _UpdateCapWINMobileStatus(ServiceConnectionState.Disconnected);
                LogEventsManager.LogEvent("CapWIN Mobile stopped", LogEventTypes.CAPWIN_MOBILE_DISCONNECTED, LogLevelTypes.INFO);
                ShutDownCapWINMobilePort();
                MessageBox.Show("The CapWIN Mobile service could not be started, an error occured", SystemConstants.MessageBox_Caption_Error, MessageBoxButtons.OK, MessageBoxIcon.Error);                
            }
        }

        public void ShutDownCapWINMobilePort()
        {
            if (sport != null)
            {
                try
                {
                    if (sport.IsOpen)
                        sport.Close();
                }
                catch (Exception)
                {
                    log.Error("CapWIN Mobile stopped");
                }

                SetConnectionStatus(ConnectionType.CAP_WIN_MOBILE, ServiceConnectionState.Disconnected);
                LogEventsManager.LogEvent("CapWIN Mobile stopped", LogEventTypes.CAPWIN_DISCONNECTED, LogLevelTypes.INFO);
            }
            else
            {
                SetConnectionStatus(ConnectionType.CAP_WIN_MOBILE, ServiceConnectionState.Disconnected);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="port"></param>
        /// <param name="baudrate"></param>
        /// <param name="parity"></param>
        /// <param name="databits"></param>
        /// <param name="stopbits"></param>
        /// <exception cref="Exception"></exception>
        /// <exception cref="IOException"></exception>
        public void serialport_connect(String port, int baudrate, Parity parity, int databits, StopBits stopbits)
        {
//            log.Debug("In serialport_connect");
            try
            {
                sport = new System.IO.Ports.SerialPort(port, baudrate, parity, databits, stopbits);
                sport.RtsEnable = false;
                sport.DtrEnable = false;

                if (!sport.IsOpen)
                {
                    sport.Open();
                }

                sport.Handshake = Handshake.None;
                sport.RtsEnable = true;
                sport.DtrEnable = true;
            }
            catch (IOException ioe)
            {
                log.Error("Serial Port not found", ioe);
                LogEventsManager.LogEvent("CapWIN Mobile Serial Port not found Exception", LogEventTypes.SYSTEM_ERROR, LogLevelTypes.ERROR);
                ShutDownCapWINMobilePort();
                throw ioe;
            }
            catch (Exception ex) 
            {
                log.Error("Serial Port could not connect", ex);
                LogEventsManager.LogEvent("CapWIN Mobile Serial Port could not connect Exception", LogEventTypes.SYSTEM_ERROR, LogLevelTypes.ERROR);
                throw ex;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <exception cref="Exception"></exception>
        public void Send(string data)
        {
            try
            {
                if (sport.IsOpen)
                {
                    byte[] byteArray = Encoding.ASCII.GetBytes(data + "\r\n");
                    sport.Write(byteArray, 0, byteArray.Length);
                }
            }
            catch (Exception ex)
            {
                log.Error("Serial Port write exception : ", ex);
                LogEventsManager.LogEvent("CapWIN Mobile Exception", LogEventTypes.CAPWIN_UNKNOWN, LogLevelTypes.ERROR);
                throw ex;
            }
        }

        private void _UpdateCapWINMobileStatus(ServiceConnectionState status)
        {
            Func<int> del = delegate()
            {
                Form form = this.ActiveMdiChild;

                if (status == ServiceConnectionState.Connected)
                {
                    SetConnectionStatus(ConnectionType.CAP_WIN_MOBILE, status);
                    LogEventsManager.LogEvent("CapWIN Mobile Connected", LogEventTypes.CAPWIN_MOBILE_CONNECTED, LogLevelTypes.INFO);
                }
                if (status == ServiceConnectionState.Disconnected)
                {
                    SetConnectionStatus(ConnectionType.CAP_WIN_MOBILE, status);
                    LogEventsManager.LogEvent("CapWIN Mobile Disconnected", LogEventTypes.CAPWIN_MOBILE_DISCONNECTED, LogLevelTypes.INFO);
                }
                return 0;
            };

            Invoke(del);
        }
        #endregion

        #region Menu Strip
       private void alertsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _CloseAllMDIForms(this);
            _OpenForm(new AlertsForm(this));
        }


        private void ShowNewForm(object sender, EventArgs e)
        {
            Form childForm = new Form();
            childForm.MdiParent = this;
            childForm.Text = "Window " + childFormNumber++;
            childForm.Show();
        }

        private void ExitToolsStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void CloseAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Form childForm in MdiChildren)
            {
                childForm.Close();
            }
        }

        private void eventLogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _CloseAllMDIForms(this);
            _OpenForm(new EventLogForm(this));
        }

        private void incZoneMainToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _CloseAllMDIForms(this);
            _OpenForm(new MainForm(this));
        }

        private void connectionStatusToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _CloseAllMDIForms(this);
            _OpenForm(new ConnectionForm(this));
        }

        private void alarmToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _CloseAllMDIForms(this);
            _OpenForm(new AlarmForm(this, ifKit));
        }

        private void capWINToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _CloseAllMDIForms(this);
            _OpenForm(new CapWINForm(this));
        }

        private void dGPSToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _CloseAllMDIForms(this);
           _OpenForm(new DGPSForm(this));
        }

        private void dSRCToolStripMenuItem_Click(object sender, EventArgs e)
        {
           _CloseAllMDIForms(this);
           _OpenForm(new DSRCForm(this));
        }

        private void generalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _CloseAllMDIForms(this);
            _OpenForm(new GeneralForm(this));
        }

        private void bluetoothClientMenuItem_Click(object sender, EventArgs e)
        {
            _CloseAllMDIForms(this);
            _OpenForm(new BluetoothForm(this));
          
        }

        private void mapRepositoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _CloseAllMDIForms(this);
            _OpenForm(new MapDataForm(this));
        }

        private void logOutputToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _CloseAllMDIForms(this);
            _OpenForm(new LogOutputForm(this));
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            (new AboutBox()).ShowDialog();
        }
        #endregion
 
        #region DSRC Bluetooth 
        private bool _ResponderLocationValid()
        {
            if (_ResponderLocationOverRide != null)
                return true;

            if (_ResponderLocation != null)
            {
                if (_ResponderLocation.Latitude != 0 && _ResponderLocation.Longitude != 0)
                {
                    return true;
                }
            }

            return false;
        }

        public NetworkStream BluetoothConnect(BluetoothAddress addr)
        {

            AradaCient = new BluetoothClient();

            AradaCient.Authenticate = true;
            NetworkStream peer = null;

            try
            {
                log.Debug("Bluetooth Connecting to Peer");
                AradaCient.Connect(addr, uid);
                peer = AradaCient.GetStream();
                SetConnectionStatus(ConnectionType.DSCR, ServiceConnectionState.Connected);
                log.Debug("Bluetooth Connected to Peer");
            }
            catch (SocketException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return peer;
        }

        private void ConnectionCleanup(Stream peer)
        {
            if (AradaPeer != null)
                AradaPeer.Close();
        }

        private void ConnectionCleanup()
        {
            if (AradaPeer != null)
                AradaPeer.Close();
        }

        public void BluetoothDisconnect()
        {
            if (AradaPeer != null)
                AradaPeer.Close();
        }

        private static string MakeExceptionMessage(Exception ex)
        {
#if !NETCF
            return ex.Message;
#else
            // Probably no messages in NETCF.
            return ex.GetType().Name;
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>bool</returns>
        private bool _AradaConnected()
        {
            try
            {
                if (AradaCient.Connected)
                {
                    if (GetConnectionStatus(ConnectionType.BLUETOOTH) != ServiceConnectionState.Connected.ToString())
                        SetConnectionStatus(ConnectionType.BLUETOOTH, ServiceConnectionState.Connected);
                    return true;
                }
                if (GetConnectionStatus(ConnectionType.BLUETOOTH) != ServiceConnectionState.Disconnected.ToString())
                    SetConnectionStatus(ConnectionType.BLUETOOTH, ServiceConnectionState.Disconnected);
            }
            catch (Exception ex)
            {
                log.Error("_AradaConnected", ex);
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        private void _ShutdownReconnect()
        {
            log.Debug("Trying to reconnect DSRC");
            try
            {
                if (AradaCient != null)
                    AradaCient.Close();
                if (AradaPeer != null)
                    AradaPeer.Close();
                AradaPeer = BluetoothConnect(_AradaAddress);

            }
            catch (Exception ex)
            {
                log.Error("_ShutdownReconnect exception", ex);
            }
        }
        #endregion

        #region Status Updates
        public string GetConnectionStatus(ConnectionType connType)
        {
            switch (connType)
            {
                case ConnectionType.CAP_WIN:
                    if (this._CapWINState.Equals(ServiceConnectionState.Connected))
                        return UIConstants.STATUS_CONNECTED;
                    else if (this._CapWINState.Equals(ServiceConnectionState.Connecting))
                        return UIConstants.STATUS_CONNECTING;
                    else if (this._CapWINState.Equals(ServiceConnectionState.Disconnected))
                        return UIConstants.STATUS_DISCONNECTED;
                    else if (this._CapWINState.Equals(ServiceConnectionState.Unknown))
                        return UIConstants.STATUS_UNKNOWN;
                    else
                        return UIConstants.STATUS_UNKNOWN;
                case ConnectionType.DGPS:
                    if (this._DGPSState.Equals(ServiceConnectionState.Connected))
                        return UIConstants.STATUS_CONNECTED;
                    else if (this._DGPSState.Equals(ServiceConnectionState.Connecting))
                        return UIConstants.STATUS_CONNECTING;
                    else if (this._DGPSState.Equals(ServiceConnectionState.Disconnected))
                        return UIConstants.STATUS_DISCONNECTED;
                    else if (this._DGPSState.Equals(ServiceConnectionState.Unknown))
                        return UIConstants.STATUS_UNKNOWN;
                    else
                        return UIConstants.STATUS_UNKNOWN;
                case ConnectionType.DSCR:
                    if (this._DSRCState.Equals(ServiceConnectionState.Connected))
                        return UIConstants.STATUS_CONNECTED;
                    else if (this._DSRCState.Equals(ServiceConnectionState.Connecting))
                        return UIConstants.STATUS_CONNECTING;
                    else if (this._DSRCState.Equals(ServiceConnectionState.Disconnected))
                        return UIConstants.STATUS_DISCONNECTED;
                    else if (this._DSRCState.Equals(ServiceConnectionState.Unknown))
                        return UIConstants.STATUS_UNKNOWN;
                    else
                        return UIConstants.STATUS_UNKNOWN;
                case ConnectionType.CAP_WIN_MOBILE:
                    if (this._CapWINMobileState.Equals(ServiceConnectionState.Connected))
                        return UIConstants.STATUS_CONNECTED;
                    else if (this._CapWINMobileState.Equals(ServiceConnectionState.Connecting))
                        return UIConstants.STATUS_CONNECTING;
                    else if (this._CapWINMobileState.Equals(ServiceConnectionState.Disconnected))
                        return UIConstants.STATUS_DISCONNECTED;
                    else if (this._CapWINMobileState.Equals(ServiceConnectionState.Unknown))
                        return UIConstants.STATUS_UNKNOWN;
                    else
                        return UIConstants.STATUS_UNKNOWN;
                case ConnectionType.BLUETOOTH:
                    if (this._BlueToothState.Equals(ServiceConnectionState.Connected))
                        return UIConstants.STATUS_CONNECTED;
                    else if (this._BlueToothState.Equals(ServiceConnectionState.Connecting))
                        return UIConstants.STATUS_CONNECTING;
                    else if (this._BlueToothState.Equals(ServiceConnectionState.Disconnected))
                        return UIConstants.STATUS_DISCONNECTED;
                    else if (this._BlueToothState.Equals(ServiceConnectionState.Unknown))
                        return UIConstants.STATUS_UNKNOWN;
                    else
                        return UIConstants.STATUS_UNKNOWN;
                case ConnectionType.RADIO:
                    if (this._RadioState.Equals(ServiceConnectionState.Connected))
                        return UIConstants.STATUS_CONNECTED;
                    else if (this._RadioState.Equals(ServiceConnectionState.Connecting))
                        return UIConstants.STATUS_CONNECTING;
                    else if (this._RadioState.Equals(ServiceConnectionState.Disconnected))
                        return UIConstants.STATUS_DISCONNECTED;
                    else if (this._RadioState.Equals(ServiceConnectionState.Unknown))
                        return UIConstants.STATUS_UNKNOWN;
                    else
                        return UIConstants.STATUS_UNKNOWN;
                case ConnectionType.RESPONDER_LOC:
                    if (this._ResponderLocationState.Equals(ServiceConnectionState.Connected))
                        return UIConstants.STATUS_CONNECTED;
                    else if (this._ResponderLocationState.Equals(ServiceConnectionState.Connecting))
                        return UIConstants.STATUS_CONNECTING;
                    else if (this._ResponderLocationState.Equals(ServiceConnectionState.Disconnected))
                        return UIConstants.STATUS_DISCONNECTED;
                    else if (this._ResponderLocationState.Equals(ServiceConnectionState.Unknown))
                        return UIConstants.STATUS_UNKNOWN;
                    else
                        return UIConstants.STATUS_UNKNOWN;
                case ConnectionType.VITAL:
                    if (this._VitalState.Equals(ServiceConnectionState.Connected))
                        return UIConstants.STATUS_CONNECTED;
                    else if (this._VitalState.Equals(ServiceConnectionState.Connecting))
                        return UIConstants.STATUS_CONNECTING;
                    else if (this._VitalState.Equals(ServiceConnectionState.Disconnected))
                        return UIConstants.STATUS_DISCONNECTED;
                    else if (this._VitalState.Equals(ServiceConnectionState.Unknown))
                        return UIConstants.STATUS_UNKNOWN;
                    else
                        return UIConstants.STATUS_UNKNOWN;
            }

            return UIConstants.STATUS_UNKNOWN;
        }

        public void SetConnectionStatus(ConnectionType connType, object state)
        {
            switch (connType)
            {
                case ConnectionType.CAP_WIN:
                    this._CapWINState = (ServiceConnectionState)state;
                    break;
                case ConnectionType.DGPS:
                    this._DGPSState = (ServiceConnectionState)state;
                    break;
                case ConnectionType.DSCR:
                    this._DSRCState = (ServiceConnectionState)state;
                    break;
                case ConnectionType.BLUETOOTH:
                    this._BlueToothState = (ServiceConnectionState)state;
                    break;
                case ConnectionType.CAP_WIN_MOBILE:
                    this._CapWINMobileState = (ServiceConnectionState)state;
                    break;
                case ConnectionType.RADIO:
                    this._RadioState = (ServiceConnectionState)state;
                    break;
                case ConnectionType.RESPONDER_LOC:
                    this._ResponderLocationState = (ServiceConnectionState)state;
                    break;
                case ConnectionType.VITAL:
                    this._VitalState = (ServiceConnectionState)state;
                    break;
                default:
                    throw new Exception("Connection Type not found");
            }
            if (RequestStatusChange != null)
            {
                log.Debug("Request Status Change " + connType.ToString() + " " + state.ToString());
                RequestStatusChange(connType.ToString(), state.ToString());
            }
        }

        private void _UpdateCapWINDisconnectedStatus()
        {
            Func<int> del = delegate()
            {
                if (GetConnectionStatus(ConnectionType.CAP_WIN) == UIConstants.STATUS_DISCONNECTED || GetConnectionStatus(ConnectionType.CAP_WIN) == UIConstants.STATUS_CONNECTING)
                {
                    Form form = this.ActiveMdiChild;

                    SetConnectionStatus(ConnectionType.CAP_WIN, ServiceConnectionState.Connected);
                    LogEventsManager.LogEvent("CapWIN Reconnected", LogEventTypes.CAPWIN_CONNECTED, LogLevelTypes.INFO);
                    if (form is CapWINForm)
                    {
                        if (RequestLabelTextChange != null)
                        {
                            RequestLabelTextChange(UIConstants.STATUS_CONNECTED);
                        }
                    }
                }
                return 0;
            };

            Invoke(del);
        }

        private void _UpdateDGPSDisconnectedStatus()
        {
            Func<int> del = delegate()
            {
                if (GetConnectionStatus(ConnectionType.DGPS) == UIConstants.STATUS_DISCONNECTED || GetConnectionStatus(ConnectionType.DGPS) == UIConstants.STATUS_CONNECTING)
                {
                    Form form = this.ActiveMdiChild;
                    Type type = form.GetType();

                    SetConnectionStatus(ConnectionType.DGPS, ServiceConnectionState.Connected);
                    LogEventsManager.LogEvent("DGPS NTrip Reconnected", LogEventTypes.DGPS_CONNECTED, LogLevelTypes.INFO);
                    if (form is DGPSForm)
                    {
                        if (RequestLabelTextChange != null)
                        {
                            RequestLabelTextChange(UIConstants.STATUS_CONNECTED);
                        }
                    }
                }
                return 0;
            };

            Invoke(del);
        }

        private void _UpdateDGPSStatus(ServiceConnectionState status)
        {
            Func<int> del = delegate()
            {
                Form form = this.ActiveMdiChild;

                if (status == ServiceConnectionState.Connected)
                {
                    SetConnectionStatus(ConnectionType.DGPS, status);
                    LogEventsManager.LogEvent("DGPS Ntrip Connected", LogEventTypes.DGPS_CONNECTED, LogLevelTypes.INFO);
                    if (form is DGPSForm)
                    {
                        if (RequestLabelTextChange != null)
                        {
                            RequestLabelTextChange(UIConstants.STATUS_CONNECTED);
                        }
                    }
                }
                else if (status == ServiceConnectionState.Disconnected)
                {
                    SetConnectionStatus(ConnectionType.DGPS, status);
                    LogEventsManager.LogEvent("DGPS NTrip Disconnected", LogEventTypes.DGPS_DISCONNECTED, LogLevelTypes.INFO);
                    if (form is DGPSForm)
                    {
                        if (RequestLabelTextChange != null)
                        {
                            RequestLabelTextChange(UIConstants.STATUS_DISCONNECTED);
                        }
                    }
                }
                else if (status == ServiceConnectionState.Connecting)
                {
                    SetConnectionStatus(ConnectionType.DGPS, status);
                    if (form is DGPSForm)
                    {
                        if (RequestLabelTextChange != null)
                        {
                            RequestLabelTextChange(UIConstants.STATUS_CONNECTING);
                        }
                    }
                }
                return 0;
            };

            Invoke(del);
        }

        private void _UpdateCapWINStatus(ServiceConnectionState status)
        {
            Func<int> del = delegate()
            {
                Form form = this.ActiveMdiChild;

                if (status == ServiceConnectionState.Connected)
                {
                    SetConnectionStatus(ConnectionType.CAP_WIN, status);
                    LogEventsManager.LogEvent("CapWIN Connected", LogEventTypes.CAPWIN_CONNECTED, LogLevelTypes.INFO);
                    if (form is CapWINForm)
                    {
                        if (RequestLabelTextChange != null)
                        {
                            RequestLabelTextChange(UIConstants.STATUS_CONNECTED);
                        }
                    }
                }
                else if (status == ServiceConnectionState.Disconnected)
                {
                    SetConnectionStatus(ConnectionType.CAP_WIN, status);
                    LogEventsManager.LogEvent("CapWIN Disconnected", LogEventTypes.CAPWIN_DISCONNECTED, LogLevelTypes.INFO);
                    if (form is CapWINForm)
                    {
                        if (RequestLabelTextChange != null)
                        {
                            RequestLabelTextChange(UIConstants.STATUS_DISCONNECTED);
                        }
                    }
                }
                else if (status == ServiceConnectionState.Connecting)
                {
                    SetConnectionStatus(ConnectionType.CAP_WIN, status);
                    if (form is CapWINForm)
                    {
                        if (RequestLabelTextChange != null)
                        {
                            RequestLabelTextChange(UIConstants.STATUS_CONNECTING);
                        }
                    }
                }
                return 0;
            };

            Invoke(del);
        }


        #endregion

        #region Alarms
        public class AlarmEventArgs : EventArgs
        {
            private Alarm newAlarm;

            public Alarm NewAlarm
            {
                get { return this.newAlarm; }
            }

            public AlarmEventArgs(Alarm newAlarm)
            {
                this.newAlarm = newAlarm;
            }
        }

        public class VisualAlarmEventArgs : EventArgs
        {
            private VisualAlarm newVisualAlarm;

            public VisualAlarm NewVisualAlarm
            {
                get { return this.newVisualAlarm; }
            }

            public VisualAlarmEventArgs(VisualAlarm newVisualAlarm)
            {
                this.newVisualAlarm = newVisualAlarm;
            }
        }

        public delegate void AlarmEventHandler(object sender, AlarmEventArgs ev);
        public delegate void VisualAlarmEventHandler(object sender, VisualAlarmEventArgs ev);

        public class AlarmMonitor
        {

            private Alarm alarm;
            private VisualAlarm visualAlarm;

            public event AlarmEventHandler AlarmRaised;
            public event VisualAlarmEventHandler VisualAlarmRaised;

            public AlarmMonitor(Alarm alarm)
            {
                this.alarm = alarm;
            }

            public AlarmMonitor(VisualAlarm visualAlarm)
            {
                this.visualAlarm = visualAlarm;
            }

            public AlarmMonitor()
            {
            }

            public void SetAlarm(Alarm newAlarm)
            {
                OnRaiseAlarmEvent(newAlarm);

                this.alarm = newAlarm;
            }

            public void SetAlarm(VisualAlarm visualAlarm)
            {
                OnRaiseAlarmEvent(visualAlarm);

                this.visualAlarm = visualAlarm;
            }

            public void CancelAlarm(VisualAlarm visualAlarm)
            {
                OnRaiseAlarmEvent(visualAlarm);

                this.visualAlarm = visualAlarm;
            }

           public Alarm GetAlarm()
           {
              return this.alarm;
           }

           public VisualAlarm GetVisualAlarm()
           {
               return this.visualAlarm;
           }

           protected virtual void OnRaiseAlarmEvent(Alarm newAlarm)
           {
               if (AlarmRaised != null)
               {
                   AlarmRaised(this, new AlarmEventArgs(newAlarm));
               }
           }

           protected virtual void OnRaiseAlarmEvent(VisualAlarm newVisualAlarm)
           {
               if (VisualAlarmRaised != null)
                   VisualAlarmRaised(this, new VisualAlarmEventArgs(newVisualAlarm));
           }
        }

        private void AlarmMon(object sender, AlarmEventArgs e)
        {
 //           log.Debug("In AlarmMon");
            _RunAlarmsWorkerThread(e.NewAlarm);
        }

        private void AlarmMon(object sender, VisualAlarmEventArgs e)
        {
//            log.Debug("In AlarmMon");

            //_TriggerVisualAlarm(e.NewVisualAlarm);
        }

        CancellationTokenSource cts = new CancellationTokenSource();

        public class ThreadObject
        {
            public AlarmThreadObj AlarmThreadObj { get; set; }
            public CancellationTokenSource cts { get; set; }
            public AlarmConfig AlarmConfig { get; set; }
        }

        private Thread t2 = null;
        private Thread t3 = null;
        private Thread t4 = null;

        private void _RunAlarmsWorkerThread(Alarm newAlarm)
        {
 //           log.Debug("In _RunAlarmsWorkerThread");

            try
            {
                AlarmThreadObj AlarmThreadObj = new AlarmThreadObj()
                {
                    alarm = newAlarm,
                    ifkit = ifKit,
                    vitalModule = vitalModual,
                    fiKitByPassed = fiKitBypassed,
                    NoVitalNeeded = _NoVitalNeeded,
                };            

                if (_OldAlarm == null)
                {
                    _OldAlarm = AlarmThreadObj.alarm;
                    t2 = new Thread(new ParameterizedThreadStart(_TriggerAudioAlarm));
                    t3 = new Thread(new ParameterizedThreadStart(_TriggerRadioAlarm));
                    t4 = new Thread(new ParameterizedThreadStart(_TriggerVitalAlarm));
                    ThreadObject threadObj = new ThreadObject()
                    {
                        AlarmThreadObj = AlarmThreadObj,
                        cts = cts,
                        AlarmConfig = _AlarmConfig
                    };

                    t2.Start(threadObj);
                    t3.Start(threadObj);
                    t4.Start(threadObj);
                } 
                else if ((int)newAlarm.AlarmLevel > (int)_OldAlarm.AlarmLevel)
                {
                    cts.Cancel();
                    cts = new CancellationTokenSource();
                    ThreadObject threadObj = new ThreadObject()
                    {
                        AlarmThreadObj = AlarmThreadObj,
                        cts = cts,
                        AlarmConfig = _AlarmConfig
                    };
                    t2 = new Thread(new ParameterizedThreadStart(_TriggerAudioAlarm));
                    t3 = new Thread(new ParameterizedThreadStart(_TriggerRadioAlarm));
                    t4 = new Thread(new ParameterizedThreadStart(_TriggerVitalAlarm));
                    t2.Start(threadObj);
                    t3.Start(threadObj);
                    t4.Start(threadObj);
                }
                else if (!t2.IsAlive && !t3.IsAlive && !t4.IsAlive)
                {
                    cts = new CancellationTokenSource();
                    ThreadObject threadObj = new ThreadObject()
                    {
                        AlarmThreadObj = AlarmThreadObj,
                        cts = cts,
                        AlarmConfig = _AlarmConfig
                    };
                    _OldAlarm = AlarmThreadObj.alarm;

                    if (t2 != null && !t2.IsAlive)
                    {
                        t2 = new Thread(new ParameterizedThreadStart(_TriggerAudioAlarm));
                        t2.Start(threadObj);
                    }
                    if ((t3 != null && !t3.IsAlive))
                    {
                        t3 = new Thread(new ParameterizedThreadStart(_TriggerRadioAlarm));
                        t3.Start(threadObj);
                    }
                    if ((t4 != null && !t4.IsAlive))
                    {
                        t4 = new Thread(new ParameterizedThreadStart(_TriggerVitalAlarm));
                        t4.Start(threadObj);
                    }                    
                }

            }
            catch (Exception e)
            {
                log.Error("_RunWorkerThread Exception", e);
            }
        }

        private Alarm _OldAlarm = null;

        private static void _TriggerVitalAlarm(object obj)
        {
            ThreadObject threadObj = (ThreadObject)obj;
            CancellationToken ct = threadObj.cts.Token;
            AlarmLevelTypes alarmLevelTypes = threadObj.AlarmThreadObj.alarm.AlarmLevel;
            int Persistance = threadObj.AlarmConfig.VitalConfigs[((int)alarmLevelTypes) - 1].Persistance;
            bool TriggerVital = threadObj.AlarmConfig.VitalConfigs[((int)alarmLevelTypes) - 1].Active;

            try
            {
                if (!threadObj.AlarmThreadObj.NoVitalNeeded && TriggerVital)
                {
                    Stopwatch sw = new Stopwatch();
                    bool loop = true;
                    

                    sw.Start();
                    //threadObj.AlarmThreadObj.vitalModule.ActivateAlarms();
                    while (!ct.IsCancellationRequested && loop)
                    {
                        if (sw.ElapsedMilliseconds >= Persistance)
                        {
                            //threadObj.AlarmThreadObj.vitalModule.DeactivateAlarms();
                            loop = false;
                        }
                    }
                    //threadObj.AlarmThreadObj.vitalModule.DeactivateAlarms();
                    sw.Stop();
                }
            }
            catch (Exception ex)
            {
                log.Error("_TriggerVitalAlarm Exception", ex);
            }
        }

        public static void _TriggerAudioAlarm(object obj)
        {
            ThreadObject threadObj = (ThreadObject)obj;
            CancellationToken ct = threadObj.cts.Token;
            AlarmLevelTypes alarmLevelTypes = threadObj.AlarmThreadObj.alarm.AlarmLevel;

            try
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                bool loop = true;
                int Duration = Convert.ToInt16(threadObj.AlarmConfig.AudibleVisualConfigs[((int)alarmLevelTypes) - 1].Duration);
                int Frequency = Convert.ToInt16(threadObj.AlarmConfig.AudibleVisualConfigs[((int)alarmLevelTypes) - 1].Frequency);
                int Persistance = threadObj.AlarmConfig.AudibleVisualConfigs[((int)alarmLevelTypes) - 1].Persistance;

                log.Debug(DateTime.Now + " - " + !ct.IsCancellationRequested);
                while (!ct.IsCancellationRequested && loop)
                {
                    log.Debug("Generating alarm Frequency:" + Frequency + " Duration:" + Duration);

                    Console.Beep(Frequency, Duration);
                    if (sw.ElapsedMilliseconds >= Persistance)
                    {
                        loop = false;
                    }
                    Thread.Sleep(150);
                }
                log.Debug("ct.IsCancellationRequested:" + ct.IsCancellationRequested + " loop:" + loop);
                sw.Stop();
            }
            catch (Exception ex)
            {
                log.Error("audibleAlarmBt_Click Exception", ex);
            }
        }

        private static void _TriggerRadioAlarm(object obj)
        {
            ThreadObject threadObj = (ThreadObject)obj;
            CancellationToken ct = threadObj.cts.Token;
            AlarmLevelTypes alarmLevelTypes = threadObj.AlarmThreadObj.alarm.AlarmLevel;
            int Persistance = threadObj.AlarmConfig.AudibleVisualConfigs[((int)alarmLevelTypes) - 1].Persistance;
            bool RadioActive = threadObj.AlarmConfig.AudibleVisualConfigs[((int)alarmLevelTypes) - 1].RadioActive;

            if (!threadObj.AlarmThreadObj.fiKitByPassed && RadioActive)
            {
                try
                {
                    bool loop = true;
                    Stopwatch sw = new Stopwatch();
                    threadObj.AlarmThreadObj.ifkit.outputs[0] = true;
                    threadObj.AlarmThreadObj.ifkit.outputs[1] = true;
                    

                    sw.Start();
                    while (!ct.IsCancellationRequested && loop)
                    {
                        if (sw.ElapsedMilliseconds >= Persistance)
                        {
                            loop = false;
                        }
                    }

                    threadObj.AlarmThreadObj.ifkit.outputs[1] = false;
                    threadObj.AlarmThreadObj.ifkit.outputs[0] = false;
                    sw.Stop();
                }
                catch (Exception ex)
                {
                    log.Error("_TriggerRadioAlarm Exception", ex);
                    RadioActive = false;
                }
            }
        }

        public VisualAlarm lastAlarm = new VisualAlarm(0, 0, 0, 0);
        bool vitalIsActive = false;

        public void setVehicle(String vehicle)
        {
            log.Debug("Parent Set Vehicle:" + vehicle);
            vitalModual.setVehicle(vehicle);
        }

        private void _TriggerVisualAlarm(VisualAlarm VisualAlarm)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new MethodInvoker(() => { _TriggerVisualAlarm(VisualAlarm); }));
                return;
            }

            bool vitalShouldBeActive = false;

            vitalShouldBeActive |= (VisualAlarm.tlevel0count > 0 && _AlarmConfig.VitalConfigs.Count() > 1 ? _AlarmConfig.VitalConfigs[1].Active : false);
            vitalShouldBeActive |= (VisualAlarm.tlevel1count > 0 && _AlarmConfig.VitalConfigs.Count() > 2 ? _AlarmConfig.VitalConfigs[2].Active : false);
            vitalShouldBeActive |= (VisualAlarm.tlevel2count > 0 && _AlarmConfig.VitalConfigs.Count() > 3 ? _AlarmConfig.VitalConfigs[3].Active : false);
            vitalShouldBeActive |= (VisualAlarm.tlevel3count > 0 && _AlarmConfig.VitalConfigs.Count() > 4 ? _AlarmConfig.VitalConfigs[4].Active : false);

            if (vitalShouldBeActive && !vitalIsActive)
            {
                //TURN VITAL ON
                log.Debug("Activating Vital");
                IncZoneMDIParent.vitalModual.ActivateAlarms();
            }
            else if (vitalIsActive && !vitalShouldBeActive)
            {
                log.Debug("Deactivating Vital");
                IncZoneMDIParent.vitalModual.DeactivateAlarms();
                //TURN VITAL OFF
            }

            vitalIsActive = vitalShouldBeActive;
            lastAlarm = VisualAlarm;

            try
            {
                Form form = this.ActiveMdiChild;
                if (form is AlertsForm)
                {
                    AlertsForm AlertsForm = (AlertsForm)form;
                    AlertsForm.ShowAlarm(VisualAlarm);
                }
            }
            catch (Exception ex)
            {
                log.Error("_TriggerVisualAlarm Exception", ex);
            }
        }

        private void _TriggerIncident(bool status)
        {
            IncidentActive = status;
            Form form = this.ActiveMdiChild;
            if (form is AlertsForm)
            {
                if (RequestIncidentChange != null)
                    RequestIncidentChange(status);
            }                           
        }
        #endregion

        #region Phidget
        //IfKit attach event handler
        //Here we'll display the interface kit details as well as determine how many output and input
        //fields to display as well as determine the range of values for input sensitivity slider
        void ifKit_Attach(object sender, AttachEventArgs e)
        {
            fiKitAtached = true;
            SetConnectionStatus(ConnectionType.RADIO, ServiceConnectionState.Connected);
            Form form = this.ActiveMdiChild;

        }

        //Ifkit detach event handler
        //Here we display the attached status, which will be false as the device is disconnected. 
        //We will also clear the display fields and hide the inputs and outputs.
        void ifKit_Detach(object sender, DetachEventArgs e)
        {
            fiKitAtached = false;
            SetConnectionStatus(ConnectionType.RADIO, ServiceConnectionState.Disconnected);
            Form form = this.ActiveMdiChild;
        }

        //Error event handler
        void ifKit_Error(object sender, Phidgets.Events.ErrorEventArgs e)
        {
            Phidget phid = (Phidget)sender;

            switch (e.Type)
            {
                case PhidgetException.ErrorType.PHIDGET_ERREVENT_BADPASSWORD:
                    phid.close();
                    Environment.Exit(0);
                    break;
                case PhidgetException.ErrorType.PHIDGET_ERREVENT_PACKETLOST:
                    //Ignore this error - it's not useful in this context.
                    return;
                case PhidgetException.ErrorType.PHIDGET_ERREVENT_OVERRUN:
                    //Ignore this error - it's not useful in this context.
                    return;
                default:
                    throw new Exception(DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + ": " + e.Description);
            }
        }

        void ifKit_OutputChange(object sender, OutputChangeEventArgs e)
        {
        }

        public void activateVital()
        {
            IncZoneMDIParent.vitalModual.ActivateAlarms();
        }

        public void deactivateVital()
        {
            IncZoneMDIParent.vitalModual.DeactivateAlarms();
        }

        #endregion
    }
}



