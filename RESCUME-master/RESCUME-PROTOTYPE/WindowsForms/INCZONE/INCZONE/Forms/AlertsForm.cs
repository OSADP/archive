using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using INCZONE.Common;
using INCZONE.Managers;
using log4net;

namespace INCZONE.Forms
{
    public partial class AlertsForm : Form
    {

        private static readonly ILog log = LogManager.GetLogger(SystemConstants.Logger_Ref);
        AlarmConfig AlarmConfig;

        IncZoneMDIParent parentForm;


        public AlertsForm(IncZoneMDIParent form)
        {
            this.parentForm = form;

            this.MdiParent = form;
            InitializeComponent();

            AlarmLevel1Panel.Visible = false;
            AlarmLevel2Panel.Visible = false;
            AlarmLevel3Panel.Visible = false;
            AlarmLevel4Panel.Visible = false;
            zoneActiveLED.Active = false;

            AlarmConfig = ((IncZoneMDIParent)form)._AlarmConfig;

            ((IncZoneMDIParent)form).RequestIncidentChange += RequestIncidentChange;
            ((IncZoneMDIParent)form).RequestStatusChange += RequestStatusChange;
            ((IncZoneMDIParent)form).RequestButtonStatusChange += RequestButtonStatusChange;

            loadedMapSet.Text = IncZoneMDIParent._LoadedMapName;

            if (IncZoneMDIParent.AppStarted)
            {
                StartApp.Enabled = false;
                StopApp.Enabled = true;
//                vehicleSelect.Enabled = false;
//                if (vitalConnectedLED.Active)
//                {
//                    button1.Enabled = true;
//                    button2.Enabled = true;
 //               }
            }
            else
            {
                StartApp.Enabled = true;
                StopApp.Enabled = false;
//                vehicleSelect.Enabled = true;
 //               button1.Enabled = false;
   //             button2.Enabled = false;
            }
            setStatus();

        }

        private void RequestIncidentChange(bool status)
        {
           if (InvokeRequired)
                {
                Func<int> del = delegate()
                {
                    zoneActiveLED.Active = status;
                    incidentNameLabel.Text = parentForm.IncidentName;
                    if (!status)
                    {
                        ClearVisualAlarm();
                    }
                    return 0;
                };

                Invoke(del);
            }
            else
            {
                zoneActiveLED.Active = status;
                incidentNameLabel.Text = parentForm.IncidentName;
                if (!status)
                {
                    ClearVisualAlarm();
                }

            }
        }

        public void ShowIncident(bool visible)
        {
            Func<int> del = delegate()
            {
                if (InvokeRequired)
                {

                }
                zoneActiveLED.Active = visible;
                return 0;
            };

            Invoke(del);

        }

        private void AlertsForm_Load(object sender, EventArgs e)
        {
            incidentNameLabel.Text = parentForm.IncidentName;
            zoneActiveLED.Active = parentForm.IncidentActive;

            alarmLevel.DisplayMember = "Name";
            alarmLevel.ValueMember = "Id";
            List<ComboBoxItem> list = new List<ComboBoxItem>();

            list.Add(new ComboBoxItem("0", "Select..."));
            list.Add(new ComboBoxItem("1", "Alarm Level 0"));
            list.Add(new ComboBoxItem("2", "Alarm Level 1"));
            list.Add(new ComboBoxItem("3", "Alarm Level 2"));
            list.Add(new ComboBoxItem("4", "Alarm Level 3"));
            list.Add(new ComboBoxItem("5", "Alarm Level 4"));

            alarmLevel.DataSource = list;

            threatLevel.DisplayMember = "Name";
            threatLevel.ValueMember = "Id";
            list = new List<ComboBoxItem>();
            list.Add(new ComboBoxItem("2", "Alarm Level 1"));
            list.Add(new ComboBoxItem("3", "Alarm Level 2"));
            list.Add(new ComboBoxItem("4", "Alarm Level 3"));
            list.Add(new ComboBoxItem("5", "Alarm Level 4"));

            threatLevel.DataSource = list;
        }

        private void testAudibleAlarm_Click(object sender, EventArgs e)
        {
            List<Alarm> list = new List<Alarm>();

            switch ((string)alarmLevel.SelectedValue)
            {
                case "1":
                    Alarm alarm = new Alarm()
                    {
                        AlarmLevel = AlarmLevelTypes.Level_0,
                    };
                    IncZoneMDIParent.AlarmMoni.SetAlarm(alarm);
                    break;
                case "2":
                    alarm = new Alarm()
                    {
                        AlarmLevel = AlarmLevelTypes.Level_1,
                    };
                    IncZoneMDIParent.AlarmMoni.SetAlarm(alarm);
                    break;
                case "3":
                    alarm = new Alarm()
                    {
                        AlarmLevel = AlarmLevelTypes.Level_2,
                    };
                    IncZoneMDIParent.AlarmMoni.SetAlarm(alarm);
                    break;
                case "4":
                    alarm = new Alarm()
                    {
                        AlarmLevel = AlarmLevelTypes.Level_3,
                    };
                    IncZoneMDIParent.AlarmMoni.SetAlarm(alarm);
                    break;
                case "5":
                    alarm = new Alarm()
                    {
                        AlarmLevel = AlarmLevelTypes.Level_4,
                    };
                    IncZoneMDIParent.AlarmMoni.SetAlarm(alarm);
                    break;
                default:
                    break;
            };
        }

        private void testVisualAlarm_Click(object sender, EventArgs e)
        {

            List<Alarm> list = new List<Alarm>();

                switch ((string)alarmLevel.SelectedValue)
                {
                    case "1":
                        IncZoneMDIParent.AlarmMoni.SetAlarm(new VisualAlarm(1, 0, 0, 0));
                        break;
                    case "2":
                        IncZoneMDIParent.AlarmMoni.SetAlarm(new VisualAlarm(1, 0, 0, 0));
                        break;
                    case "3":
                        IncZoneMDIParent.AlarmMoni.SetAlarm(new VisualAlarm(0, 1, 0, 0));
                        break;
                    case "4":
                        IncZoneMDIParent.AlarmMoni.SetAlarm(new VisualAlarm(0, 0, 1, 0));
                        break;
                    case "5":
                        IncZoneMDIParent.AlarmMoni.SetAlarm(new VisualAlarm(0, 0, 0, 1));
                        break;
                    default:
                        break;
                };
        }

        private void TestThreat_Click(object sender, EventArgs e)
        {
            List<Alarm> list = new List<Alarm>();
            List<Alarm> list2 = new List<Alarm>();
            VisualAlarm VisualAlarm1 = new VisualAlarm();
            VisualAlarm VisualAlarm2 = new VisualAlarm();

            foreach (ComboBoxItem str in threatLevel.SelectedItems)
            {
                switch (str.Id)
                {
                    case "2":
                        for (int i = 0; i < int.Parse(textBox1.Text); i++)
                        {
                            Alarm alarm = new Alarm()
                            {
                                AlarmLevel = AlarmLevelTypes.Level_1,
                            };
                            list.Add(alarm);
                            VisualAlarm1.tlevel0count++;
                        }
                        break;
                    case "3":
                        for (int i = 0; i < int.Parse(textBox2.Text); i++)
                        {
                            Alarm alarm = new Alarm()
                            {
                                AlarmLevel = AlarmLevelTypes.Level_2,
                            };
                            list.Add(alarm);
                            VisualAlarm1.tlevel1count++;
                        }
                        break;
                    case "4":
                        for (int i = 0; i < int.Parse(textBox3.Text); i++)
                        {
                            Alarm alarm = new Alarm()
                            {
                                AlarmLevel = AlarmLevelTypes.Level_3,
                            };
                            list.Add(alarm);
                            VisualAlarm1.tlevel2count++;
                        }
                        break;
                    case "5":
                        for (int i = 0; i < int.Parse(textBox4.Text); i++)
                        {
                            Alarm alarm = new Alarm()
                            {
                                AlarmLevel = AlarmLevelTypes.Level_4,
                            };
                            list.Add(alarm);
                            VisualAlarm1.tlevel3count++;
                        }
                        break;
                    default:
                        break;
                };
            }

            foreach (ComboBoxItem str in threatLevel.SelectedItems)
            {
                switch (str.Id)
                {
                    case "2":
                        for (int i = 0; i < int.Parse(textBox1.Text); i++)
                        {
                            Alarm alarm = new Alarm()
                            {
                                AlarmLevel = AlarmLevelTypes.Level_1,
                            };
                            list2.Add(alarm);
                            VisualAlarm2.tlevel0count++;
                        }
                        break;
                    case "3":
                        for (int i = 0; i < int.Parse(textBox2.Text); i++)
                        {
                            Alarm alarm = new Alarm()
                            {
                                AlarmLevel = AlarmLevelTypes.Level_2,
                            };
                            list2.Add(alarm);
                            VisualAlarm2.tlevel1count++;
                        }
                        break;
                    case "4":
                        for (int i = 0; i < int.Parse(textBox3.Text); i++)
                        {
                            Alarm alarm = new Alarm()
                            {
                                AlarmLevel = AlarmLevelTypes.Level_3,
                            };
                            list2.Add(alarm);
                            VisualAlarm2.tlevel2count++;
                        }
                        break;
                    case "5":
                        for (int i = 0; i < int.Parse(textBox4.Text); i++)
                        {
                            Alarm alarm = new Alarm()
                            {
                                AlarmLevel = AlarmLevelTypes.Level_3,
                            };
                            list2.Add(alarm);
                            VisualAlarm2.tlevel3count++;
                        }
                        break;
                    default:
                        break;
                };
            }

            IncZoneMDIParent.AlarmMoni.SetAlarm(AlarmManager._GetHighestAlarm(list2));
            IncZoneMDIParent.AlarmMoni.SetAlarm(VisualAlarm1);
            IncZoneMDIParent.AlarmMoni.SetAlarm(new VisualAlarm(0,0,0,0));
            Thread.Sleep(2000);
            IncZoneMDIParent.AlarmMoni.SetAlarm(AlarmManager._GetHighestAlarm(list));
            IncZoneMDIParent.AlarmMoni.SetAlarm(VisualAlarm2);
            IncZoneMDIParent.AlarmMoni.SetAlarm(new VisualAlarm(0, 0, 0, 0));
        }

        bool vitalIsActive = false;

        public void ShowAlarm(VisualAlarm VisualAlarm)
        {
            AlarmLevel1Panel.Visible = VisualAlarm.tlevel0count > 0;
            AlarmLevel2Panel.Visible = VisualAlarm.tlevel1count > 0;
            AlarmLevel3Panel.Visible = VisualAlarm.tlevel2count > 0;
            AlarmLevel4Panel.Visible = VisualAlarm.tlevel3count > 0;
        }

        public void ClearVisualAlarm()
        {
            AlarmLevel1Panel.Visible = false;
            AlarmLevel2Panel.Visible = false;
            AlarmLevel3Panel.Visible = false;
            AlarmLevel4Panel.Visible = false;
        }


        private void _TimeOutVisualAlarm()
        { 
        
        }

        private void testBackToBackThreat_Click(object sender, EventArgs e)
        {
            IncZoneMDIParent.AlarmMoni.SetAlarm(new Alarm()
            {
                AlarmLevel = AlarmLevelTypes.Level_1,
                TriggerVital = false
            });
            Thread.Sleep(500);
            IncZoneMDIParent.AlarmMoni.SetAlarm(new Alarm(){
                    AlarmLevel = AlarmLevelTypes.Level_2,
                    TriggerVital = false
            });
            Thread.Sleep(500);
            IncZoneMDIParent.AlarmMoni.SetAlarm(new Alarm()
            {
                AlarmLevel = AlarmLevelTypes.Level_3,
                TriggerVital = false
            });
            Thread.Sleep(500);
            IncZoneMDIParent.AlarmMoni.SetAlarm(new Alarm()
            {
                AlarmLevel = AlarmLevelTypes.Level_4,
                TriggerVital = false
            });
            Thread.Sleep(6000);
            IncZoneMDIParent.AlarmMoni.SetAlarm(new Alarm()
            {
                AlarmLevel = AlarmLevelTypes.Level_1,
                TriggerVital = false
            });
            Thread.Sleep(2000);
            IncZoneMDIParent.AlarmMoni.SetAlarm(new Alarm()
            {
                AlarmLevel = AlarmLevelTypes.Level_4,
                TriggerVital = false
            });
        }

        private void AlertsForm_Activated(object sender, EventArgs e)
        {
            ShowAlarm(this.parentForm.lastAlarm);
            updateVitial(this.parentForm.vitalStatus);
        }

        public void updateVitial(bool status)
        {
//            vitalConnectedLED.Active = status;

        }

        private void RequestButtonStatusChange(bool status)
        {
            log.Debug("AlertsForm Status Changed");
            if (status)
            {
                StartApp.Enabled = false;
                StopApp.Enabled = true;
            }
            else
            {
                StartApp.Enabled = true;
                StopApp.Enabled = false;
            }            
        }

        private void StartApp_Click(object sender, EventArgs e)
        {
            ((IncZoneMDIParent)this.MdiParent)._StartIncZone();
        }

        private void StopApp_Click(object sender, EventArgs e)
        {
            ((IncZoneMDIParent)this.MdiParent)._StopIncZone(true);
        }

        void RequestStatusChange(string form, string status)
        {
            log.Debug("AlertsForm RequestStatusChange - " + status);
            setStatus();
        }

        private void setStatus()
        {
            try
            {
                if (dsrcUnit != null)
                {
                    dsrcUnit.Text = ((IncZoneMDIParent)this.MdiParent)._DSRCState.ToString();
                    if (((IncZoneMDIParent)this.MdiParent)._DSRCState == ServiceConnectionState.Connected)
                    {
                        ledDSRC.Active = true;
                    }
                    else
                    {
                        ledDSRC.Active = false;
                    }

                }
                if (dgpsStatus != null)
                {
                    dgpsStatus.Text = ((IncZoneMDIParent)this.MdiParent)._DGPSState.ToString();
                    if (((IncZoneMDIParent)this.MdiParent)._DGPSState == ServiceConnectionState.Connected)
                    {
                        ledDGPS.Active = true;
                    }
                    else
                    {
                        ledDGPS.Active = false;
                    }
                }
                if (capwinStatus != null)
                {
                    capwinStatus.Text = ((IncZoneMDIParent)this.MdiParent)._CapWINState.ToString();
                    if (((IncZoneMDIParent)this.MdiParent)._CapWINState == ServiceConnectionState.Connected)
                    {
                        ledCapWin.Active = true;
                    }
                    else
                    {
                        ledCapWin.Active = false;
                    }
                }
                if (capwinMobileStatus != null)
                {
                    capwinMobileStatus.Text = ((IncZoneMDIParent)this.MdiParent)._CapWINMobileState.ToString();
                    if (((IncZoneMDIParent)this.MdiParent)._CapWINMobileState == ServiceConnectionState.Connected)
                    {
                        ledCapWinMobile.Active = true;
                    }
                    else
                    {
                        ledCapWinMobile.Active = false;
                    }
                }
                if (radioStatus != null)
                {
                    radioStatus.Text = ((IncZoneMDIParent)this.MdiParent)._RadioState.ToString();
                    if (((IncZoneMDIParent)this.MdiParent)._RadioState == ServiceConnectionState.Connected)
                    {
                        ledRadio.Active = true;
                    }
                    else
                    {
                        ledRadio.Active = false;
                    }
                }
                if (vitalStatus != null)
                {
                    vitalStatus.Text = ((IncZoneMDIParent)this.MdiParent)._VitalState.ToString();
                    if (((IncZoneMDIParent)this.MdiParent)._VitalState == ServiceConnectionState.Connected)
                    {
                        vitalConnectedLED.Active = true;
                    }
                    else
                    {
                        vitalConnectedLED.Active = false;
                    }
                }
            }
            catch (Exception)
            {

                log.Error("Updating Status Indicators");
            }

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            log.Debug("Set Vehicle:" + vehicleSelect.SelectedItem.ToString());
            parentForm.setVehicle(vehicleSelect.SelectedItem.ToString());
        }

        private void button1_Click(object sender, EventArgs e)
        {
            parentForm.activateVital();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            parentForm.deactivateVital();
        }
    }
}
