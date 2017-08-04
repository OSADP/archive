using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using INCZONE.Common;
using INCZONE.Managers;
using INCZONE.NMEA;
using INCZONE.Repositories;
using log4net;

namespace INCZONE.Forms.Configuration
{
    public partial class GeneralForm : Form
    {
        readonly IUnitOfWork _uow;
        private static readonly ILog log = LogManager.GetLogger(SystemConstants.Logger_Ref);
        private List<CapWINLocation> _locationList = null;
        bool Init = false;
        private Socket _NtripSocket;
        private IPEndPoint BroadCaster;
        DGPSConfig _DGPSConfig = null;
        private int count = 0;

        public GeneralForm(Form form)
        {
//            log.Debug("In GeneralForm Constructor");
            string connectionString = ConfigurationManager.ConnectionStrings["IncZoneEntities"].ConnectionString;
            this._uow = new UnitOfWork(connectionString);
            this.MdiParent = form;
            InitializeComponent();
            

            if (((IncZoneMDIParent)this.MdiParent).fiKitBypassed)
            {
                bypassRadio.Enabled = false;
                resetRadio.Enabled = true;
            }
            else
            {
                bypassRadio.Enabled = true;
                resetRadio.Enabled = false;
            }

            if (((IncZoneMDIParent)this.MdiParent).CapWINBypassed)
            {
                byPassCapWINBt.Enabled = false;
                resetCapWINBt.Enabled = true;
            }
            else
            {
                byPassCapWINBt.Enabled = true;
                resetCapWINBt.Enabled = false;
            }

            _DGPSConfig = ((IncZoneMDIParent)this.MdiParent)._DGPSConfig;

            BroadCaster = new IPEndPoint(IPAddress.Parse(_DGPSConfig.HostIP), Convert.ToInt32(_DGPSConfig.HostPort));
            _NtripSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _NtripSocket.Blocking = true;
            _NtripSocket.Connect(BroadCaster);
        }

        private void refreshCapWinIncidentsBt_Click(object sender, EventArgs e)
        {
            GeneralForm_Load(sender, e);
        }

        private async void GeneralForm_Load(object sender, EventArgs e)
        {
            try
            {
                CapWINManager capWINManager = new CapWINManager(((IncZoneMDIParent)this.MdiParent)._CapWinConfig);
                CapWINIncidentListType1 list = await capWINManager.GetCapWINIncidentsList();

                _locationList = CapWINManager.GetCapWINIncidentsWithLocations(list);

                if (capWinIncidentsCB.Items != null)
                    capWinIncidentsCB.Items.Clear();

                this.capWinIncidentsCB.DisplayMember = "Text";
                this.capWinIncidentsCB.ValueMember = "Value";

                int index = 0;

                capWinIncidentsCB.Items.Insert(index, "Please Select and Incident");

                foreach (CapWINLocation locType in _locationList)
                {
                    index++;
                    decimal latitude = locType.Location.LocationTwoDimensionalGeographicCoordinate[0].GeographicCoordinateLatitude[0].LatitudeDegreeValue[0].Value;
                    decimal longitude = locType.Location.LocationTwoDimensionalGeographicCoordinate[0].GeographicCoordinateLongitude[0].LongitudeDegreeValue[0].Value;

                    string Text = locType.DisplayId + " (Lat) = " + latitude + " - (Long) = " + longitude;

                    capWinIncidentsCB.Items.Insert(index, Text);
                }

                capWinIncidentsCB.DropDownWidth = DropDownWidth(capWinIncidentsCB);
                capWinIncidentsCB.SelectedIndex = 0;
            }
            catch (Exception ex)
            {

                log.Error("Exception", ex);
                LogEventsManager.LogEvent("MDI Parent could not start", LogEventTypes.SYSTEM_ERROR, LogLevelTypes.ERROR);
                MessageBox.Show("The General COnfiguration page could not start, an error occured", SystemConstants.MessageBox_Caption_Error, MessageBoxButtons.OK, MessageBoxIcon.Error);                
            }
        }

        private void selectLocationBt_Click(object sender, EventArgs e)
        {
            try
            {
                if (capWinIncidentsCB.SelectedIndex != 0)
                {
                    CapWINLocation locType = _locationList.ElementAt((int)capWinIncidentsCB.SelectedIndex - 1);
                    decimal latitude = locType.Location.LocationTwoDimensionalGeographicCoordinate[0].GeographicCoordinateLatitude[0].LatitudeDegreeValue[0].Value;
                    decimal longitude = locType.Location.LocationTwoDimensionalGeographicCoordinate[0].GeographicCoordinateLongitude[0].LongitudeDegreeValue[0].Value;
                    IncZoneMDIParent._ResponderLocationOverRide = new Coordinate((double)longitude, (double)latitude);
                    ((IncZoneMDIParent)this.MdiParent).SetConnectionStatus(ConnectionType.RESPONDER_LOC, ServiceConnectionState.Connected);
                    MessageBox.Show("Responder location has be set to the selected value", "Responder Location", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else 
                {
                    MessageBox.Show("Please select an incident location", "Responder Location", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                log.Error("selectLocationBt_Click exception ",ex);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            IncZoneMDIParent._ResponderLocation = null;
            IncZoneMDIParent._ResponderLocationOverRide = null;
            capWinIncidentsCB.SelectedIndex = 0;
            MessageBox.Show("Responder location has been reset and will be set to the DSRC GPS location", "Responder Location", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private int DropDownWidth(ComboBox myCombo)
        {
            int maxWidth = 0;
            int temp = 0;
            Label label1 = new Label();

            foreach (var obj in myCombo.Items)
            {
                label1.Text = obj.ToString();
                temp = label1.PreferredWidth;
                if (temp > maxWidth)
                {
                    maxWidth = temp;
                }
            }
            label1.Dispose();
            return maxWidth;
        }

        private void bypassRadio_Click(object sender, EventArgs e)
        {
            ((IncZoneMDIParent)this.MdiParent).fiKitAtached = true;
            ((IncZoneMDIParent)this.MdiParent).fiKitBypassed = true;
            ((IncZoneMDIParent)this.MdiParent).SetConnectionStatus(ConnectionType.RADIO, ServiceConnectionState.Bypassed);
            bypassRadio.Enabled = false;
            resetRadio.Enabled = true;
        }

        private void resetRadio_Click(object sender, EventArgs e)
        {
            ((IncZoneMDIParent)this.MdiParent).fiKitAtached = true;
            ((IncZoneMDIParent)this.MdiParent).fiKitBypassed = false;
            bypassRadio.Enabled = true;
            resetRadio.Enabled = false;
        }

        private void byPassCapWIN_Click(object sender, EventArgs e)
        {
            ((IncZoneMDIParent)this.MdiParent).CapWINBypassed = true;
            ((IncZoneMDIParent)this.MdiParent).SetConnectionStatus(ConnectionType.CAP_WIN, ServiceConnectionState.Bypassed);
            byPassCapWINBt.Enabled = false;
            resetCapWINBt.Enabled = true;
        }

        private void resetCapWINBt_Click(object sender, EventArgs e)
        {
            ((IncZoneMDIParent)this.MdiParent).CapWINBypassed = false;
            byPassCapWINBt.Enabled = true;
            resetCapWINBt.Enabled = false;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            NTripManager nm = new NTripManager(_DGPSConfig);

            string responseData = string.Empty;

            if (!Init)
            {
                // NTRIPSocket.Send(Encoding.ASCII.GetBytes("GET /" + UIConstants.NTRIP_MOUNT_POINT + " HTTP/1.0\r\n"));
                _NtripSocket.Send(CreateRequest(""));
                Thread.Sleep(100);
                while (_NtripSocket.Available > 0)
                {
                    byte[] returndata = new byte[_NtripSocket.Available];
                    _NtripSocket.Receive(returndata); //Get response
                    responseData += Encoding.ASCII.GetString(returndata, 0, returndata.Length);
                    System.Threading.Thread.Sleep(100); //Wait for response
                }
                Init = true;
            }
            else
            {

                try
                {
                    while (_NtripSocket.Available > 0)
                    {
                        byte[] returndata = new byte[_NtripSocket.Available];
                        _NtripSocket.Receive(returndata);
                        responseData = Encoding.ASCII.GetString(returndata, 0, returndata.Length);
                        Thread.Sleep(100); //Wait for response
                    }

                    if (count == 3)
                    {
                        string gpgga = GPGGA.GenerateGPGGAcode(new Coordinate(-83,39));
                        _NtripSocket.Send(Encoding.ASCII.GetBytes(gpgga + "\r\n"));
                    }
                    count++;
                }
                catch (Exception ex)
                {
                    log.Error("Recieve Exception ", ex);
                }

                log.Debug("NTRIP " + responseData);
            }
        }

        private byte[] CreateRequest(string responderLocation)
        {
            //log.Debug("In CreateRequest");
            string msg = string.Empty;

            try
            {
                string auth = ToBase64(_DGPSConfig.Username + ":" + _DGPSConfig.Password);

                msg = "GET /" + UIConstants.NTRIP_MOUNT_POINT + " HTTP/1.1\r\n";

                msg += "Authorization: Basic " + auth + "\r\n"; //This line can be removed if no authorization is needed

                msg += "Accept: */*\r\nConnection: close\r\n";

                msg += "\r\n";
                //msg = "GET /" + UIConstants.NTRIP_MOUNT_POINT + " HTTP/1.1\r\n";
                //msg += "Authorization: Basic " + auth;
                //msg += "\r\n\r\n";
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
    }
}
