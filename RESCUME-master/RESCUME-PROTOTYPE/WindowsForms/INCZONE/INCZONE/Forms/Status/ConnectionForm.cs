using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using INCZONE.Common;
using INCZONE.Forms.Configuration;
using INCZONE.Managers;
using InTheHand.Net;
using InTheHand.Windows.Forms;

namespace INCZONE.Forms
{
    public partial class ConnectionForm : BaseForm
    {
        private ServiceConnectionState _CapWINState;
        private ServiceConnectionState _DGPSState;
        private ServiceConnectionState _CapWINMobileState;
        private ServiceConnectionState _DSRCState;
        private ServiceConnectionState _RadioState;
        private ServiceConnectionState _ResponderLocationState;
        private ServiceConnectionState _VitalState;
        private ServiceConnectionState _BlueToothState;

        public ConnectionForm(IncZoneMDIParent form)
        {
            this.MdiParent = form;
            InitializeComponent();
            _CapWINState = form._CapWINState;
            _DGPSState = form._DGPSState;
            _CapWINMobileState = form._CapWINMobileState;
            _DSRCState = form._DSRCState;
            _RadioState = form._RadioState;
            _ResponderLocationState = form._ResponderLocationState;
            _VitalState = form._VitalState;
            _BlueToothState = form._BlueToothState;

            form.RequestStatusChange += RequestStatusChange;

            if (_CapWINState == ServiceConnectionState.Unknown || _CapWINState == ServiceConnectionState.Disconnected)
            {
                capWinStatusLb.Text = UIConstants.STATUS_DISCONNECTED;
                
            }
            else
            {
                capWinStatusLb.Text = UIConstants.STATUS_CONNECTED;
            }
            
            if (_DGPSState == ServiceConnectionState.Unknown || _DGPSState == ServiceConnectionState.Disconnected)
            {
                dgpsStatusLb.Text = UIConstants.STATUS_DISCONNECTED;
            }
            else
            {
                dgpsStatusLb.Text = UIConstants.STATUS_CONNECTED;
            }

            if (_DSRCState == ServiceConnectionState.Unknown || _DSRCState == ServiceConnectionState.Disconnected)
            {
                dsrcStatusLb.Text = UIConstants.STATUS_DISCONNECTED;
            }
            else
            {
                dsrcStatusLb.Text = UIConstants.STATUS_CONNECTED;
            }

            if (_CapWINMobileState == ServiceConnectionState.Unknown || _CapWINMobileState == ServiceConnectionState.Disconnected)
            {
                capWINMobileStatusLb.Text = UIConstants.STATUS_DISCONNECTED;
            }
            else
            {
                capWINMobileStatusLb.Text = UIConstants.STATUS_CONNECTED;
            }

            if (_BlueToothState == ServiceConnectionState.Unknown || _BlueToothState == ServiceConnectionState.Disconnected)
            {
                aradaStatusLb.Text = UIConstants.STATUS_DISCONNECTED;
            }
            else
            {
                aradaStatusLb.Text = UIConstants.STATUS_CONNECTED;
            }
            if (_VitalState == ServiceConnectionState.Unknown || _VitalState == ServiceConnectionState.Disconnected)
            {
                vitalStatusLb.Text = UIConstants.STATUS_DISCONNECTED;
            }
            else
            {
                vitalStatusLb.Text = UIConstants.STATUS_CONNECTED;
            }
            if (_RadioState == ServiceConnectionState.Unknown || _RadioState == ServiceConnectionState.Disconnected)
            {
                radioStatusLb.Text = UIConstants.STATUS_DISCONNECTED;
            }
            else
            {
                radioStatusLb.Text = UIConstants.STATUS_CONNECTED;
            }
            if (_ResponderLocationState == ServiceConnectionState.Unknown || _ResponderLocationState == ServiceConnectionState.Disconnected)
            {
                responderLocationStatusLb.Text = UIConstants.STATUS_DISCONNECTED;
            }
            else
            {
                responderLocationStatusLb.Text = UIConstants.STATUS_CONNECTED;
            }
        }

        private void dgpsConfigureBt_Click(object sender, EventArgs e)
        {
            _OpenForm(new DGPSForm(this.MdiParent));
        }

        private void dsrcConfigureBt_Click(object sender, EventArgs e)
        {
            _OpenForm(new DSRCForm(this.MdiParent));
        }

        private void capWinConfigureBt_Click(object sender, EventArgs e)
        {
            _OpenForm(new CapWINForm(this.MdiParent));
        }

        void RequestStatusChange(string form, string status)
        {
            if (form == "CapWIN")
            {
                capWinStatusLb.Text = status;
            } 
            else if (form == "DGPS")
            {
                dgpsStatusLb.Text = status;
            }
            else if (form == "CapWINMobile")
            {
                capWINMobileStatusLb.Text = status;
            }
            else if (form == "BluetoothForm")
            {
                aradaStatusLb.Text = status;
            }
            else if (form == "DSRC")
            {
                dgpsStatusLb.Text = status;
            }
            else if (form == "Radio")
            {
                radioStatusLb.Text = status;
            }
            else if (form == "ResponderLocation")
            {
                responderLocationStatusLb.Text = status;
            }
        }

        private void bluetoothConfigureBt_Click(object sender, EventArgs e)
        {
            _OpenForm(new BluetoothForm((IncZoneMDIParent)this.MdiParent));
        }

        private void ConnectionForm_Load(object sender, EventArgs e)
        {

        }
    }
}
