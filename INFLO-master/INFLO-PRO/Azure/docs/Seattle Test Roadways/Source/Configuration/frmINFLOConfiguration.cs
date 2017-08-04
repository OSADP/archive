using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace Configuration
{
    using INFLOClassLib;
    public partial class frmINFLOConfiguration : Form
    {
        //clsDatabase related variables
        private clsDatabase DB;

        //private string DBInterfaceType = string.Empty;
        //private string StrConnection = string.Empty;
        private string AccessDBFileName = string.Empty;
        private string DSNName = string.Empty;
        private string SqlStrConnection = string.Empty;

        //Roadway entity lists
        private clsRoadway Roadway = new clsRoadway();
        private List<clsRoadwayLink> RLList = new List<clsRoadwayLink>();
        private List<clsRoadwaySubLink> RSLList = new List<clsRoadwaySubLink>();
        private List<clsDetectorStation> DSList = new List<clsDetectorStation>();
        private List<clsDetectionZone> DZList = new List<clsDetectionZone>();
        private List<clsMileMarker> MMList = new List<clsMileMarker>();
        private string INFLOConfigurationFile = string.Empty;

        public frmINFLOConfiguration()
        {
            InitializeComponent();
        }

        private void LogTxtMsg(TextBox txtControl, string Text)
        {
            Text = "\r\n" + Text;
            //Text = Environment.NewLine + "\t" + Text;
            if (txtControl.Text.Length > 30000)
                txtControl.Text = "";

            txtControl.SelectionStart = txtControl.Text.Length;
            txtControl.SelectedText = Text;
            txtControl.SelectionStart = txtControl.Text.Length;
        }

        private void frmINFLOConfiguration_Load(object sender, EventArgs e)
        {
            this.Show();

            string retValue = string.Empty;

            retValue = ReadINFLOConfigurationFiles();
            if (retValue.Length > 0)
            {
                LogTxtMsg(txtLog, "\r\n\t" + retValue);
                LogTxtMsg(txtLog, "\r\nThe INFLO Configuration application is terminating.");
                return;
            }
        }

        private string ReadINFLOConfigurationFiles()
        {
            //Configuration files variables
            string RoadwayLinkConfigFile = string.Empty;
            string DetectorStationConfigFile = string.Empty;
            string DetectionZoneConfigFile = string.Empty;
            string MileMarkerConfigFile = string.Empty;

            string retValue = string.Empty;

            //txtINFLOConfigFile.Text = clsGlobalVars.INFLOConfigFile;
            //txtINFLOConfigFile.Refresh();

            #region "Read the INFLO configuration file"
            INFLOConfigurationFile = Application.StartupPath + "\\Config\\INFLOConfig.xml";
            //txtINFLOConfigFile.Text = Application.StartupPath + "\\Config\\INFLOConfig.xml";
            txtINFLOConfigFile.Text = INFLOConfigurationFile;
            txtINFLOConfigFile.Refresh();
            LogTxtMsg(txtLog, "\r\nReading the contents of the INFLO configuration file: " + clsGlobalVars.INFLOConfigFile);
            if (clsGlobalVars.INFLOConfigFile.Length > 0)
            {
                //retValue = clsMiscFunctions.ReadINFLOConfigFile(clsGlobalVars.INFLOConfigFile, ref Roadway);
                retValue = clsMiscFunctions.ReadINFLOConfigFile(INFLOConfigurationFile, ref Roadway);
                if (retValue.Length > 0)
                {
                    return retValue;
                }
                RoadwayLinkConfigFile = clsGlobalVars.RoadwayLinkConfigFile;
                txtRoadwayLinkConfigFile.Text = RoadwayLinkConfigFile;
                txtRoadwayLinkConfigFile.Refresh();
                DetectorStationConfigFile = clsGlobalVars.DetectorStationConfigFile;
                txtDetectionStationConfigFile.Text = DetectorStationConfigFile;
                txtDetectionStationConfigFile.Refresh();
                DetectionZoneConfigFile = clsGlobalVars.DetectionZoneConfigFile;
                txtDetectionZoneConfigFile.Text = DetectionZoneConfigFile;
                txtDetectionZoneConfigFile.Refresh();
                MileMarkerConfigFile = clsGlobalVars.MileMarkerConfigFile;
            }
            else
            {
                retValue = "\tThe INFLO configuration file name was not specified in the Global variables class.";
                return retValue;
            }
            #endregion

            #region "Establish connection to INFLO database"
            LogTxtMsg(txtLog, "\r\n\tEstablish connection to the INFLO database: " +
                              "\r\n\t\tDBInterfaceType: " + clsGlobalVars.DBInterfaceType +
                              "\r\n\t\tAccessDBFileName: " + clsGlobalVars.AccessDBFileName +
                              "\r\n\t\tAccessDBFileName: " + clsGlobalVars.DSNName +
                              "\r\n\t\tAccessDBFileName: " + clsGlobalVars.SqlServer +
                              "\r\n\t\tAccessDBFileName: " + clsGlobalVars.SqlServerDatabase +
                              "\r\n\t\tAccessDBFileName: " + clsGlobalVars.SqlServerUserId +
                              "\r\n\t\tAccessDBFileName: " + clsGlobalVars.SqlStrConnection);
            if (clsGlobalVars.DBInterfaceType.Length > 0)
            {
                DB = new clsDatabase(clsGlobalVars.DBInterfaceType);
                if (DB.ConnectionStr.Length > 0)
                {
                    LogTxtMsg(txtLog, "\r\n\tDatabase Connection string: " + DB.ConnectionStr);
                    retValue = DB.OpenDBConnection();
                    if (retValue.Length > 0)
                    {
                        return retValue;
                    }
                }
                else
                {
                    retValue = "\r\n\tError in generating connection string to INFLO database.";
                    return retValue;
                }
            }
            else
            {
                retValue = "\tThe INFLO Application can not connect to the INFLO database. " +
                           "\r\n\tThe DBInterfaceType= " + clsGlobalVars.DBInterfaceType + "  is not specified.";
                return retValue;
            }
            #endregion

            #region "Read the Roadway Link configuration file"
            LogTxtMsg(txtLog, "\r\n\tRead the contents of the Roadway Link configuration file.");
            if (RoadwayLinkConfigFile.Length > 0)
            {
                retValue = clsMiscFunctions.ReadRoadwayLinkConfigFile(clsGlobalVars.RoadwayLinkConfigFile, ref RLList);
                if (retValue.Length > 0)
                {
                    return retValue;
                }
            }
            else
            {
                retValue =  "\tThe INFLO Roadway Link configuration file name was not specified in the INFLO configuration file.";
                return retValue;
            }
            #endregion

            #region "Read Detector Station configuration file"
            LogTxtMsg(txtLog, "\r\n\tRead the contents of the Detector Station configuration file.");
            if (clsGlobalVars.DetectorStationConfigFile.Length > 0)
            {
                retValue = clsMiscFunctions.ReadDetectorStationConfigFile(clsGlobalVars.DetectorStationConfigFile, ref DSList);
                if (retValue.Length > 0)
                {
                    return retValue;
                }
            }
            else
            {
                retValue = "\tThe INFLO Detector Station configuration file name was not specified in the INFLO configuration file.";
                return retValue;
            }
            #endregion

            #region "Read Detection Zones configuration file"
            LogTxtMsg(txtLog, "\r\n\tRead the contents of the Detection Zones configuration file.");
            if (DetectionZoneConfigFile.Length > 0)
            {
                retValue = clsMiscFunctions.ReadDetectionZonesConfigFile(clsGlobalVars.DetectionZoneConfigFile, ref DZList);
                if (retValue.Length > 0)
                {
                    return retValue;
                }
            }
            else
            {
                retValue = "\tThe INFLO Detection Zones configuration file name was not specified in the INFLO configuration file.";
                return retValue;
            }
            #endregion

            #region "Read MileMarker configuration file"
            LogTxtMsg(txtLog, "\r\n\tRead the contents of the Mile Marker configuration file.");
            if (MileMarkerConfigFile.Length > 0)
            {
                retValue = clsMiscFunctions.ReadMileMarkerConfigFile(clsGlobalVars.MileMarkerConfigFile, ref MMList);
                if (retValue.Length > 0)
                {
                    return retValue;
                }
            }
            else
            {
                retValue = "\tThe INFLO MileMarker configuration file name was not specified in the INFLO configuration file.";
                return retValue;
            }
            #endregion

            LogTxtMsg(txtLog, "\r\nFinished processing the INFLO Application configuration files");

            #region "Load Roadway info into INFLO database"
            LogTxtMsg(txtLog, "\r\n\tLoad the Roadway info into the INFLO database.");
            retValue = clsMiscFunctions.LoadRoadwayInfoIntoINFLODatabase(Roadway, ref DB);
            if (retValue.Length > 0)
            {
                return retValue;
            }
            #endregion

            #region "Load Roadway Sub Link info into INFLO database"
            LogTxtMsg(txtLog, "\r\n\tLoad the Roadway Sub Link info into the INFLO database.");
            retValue = clsMiscFunctions.LoadRoadwaySubLinkInfoIntoINFLODatabase(Roadway, ref DB);
            if (retValue.Length > 0)
            {
                return retValue;
            }
            #endregion

            #region "Load Roadway Link info into INFLO database"
            LogTxtMsg(txtLog, "\r\n\tLoad the Roadway Link info into the INFLO database.");
            retValue = clsMiscFunctions.LoadRoadwayLinkInfoIntoINFLODatabase(RLList, ref DB);
            if (retValue.Length > 0)
            {
                return retValue;
            }
            #endregion

            #region "Load Detector Station info into INFLO database"
            /*LogTxtMsg(txtLog, "\r\n\tLoad the Detector Station info into the INFLO database.");
            retValue = clsMiscFunctions.LoadDetectorStationInfoIntoINFLODatabase(DSList, ref DB);
            if (retValue.Length > 0)
            {
                return retValue;
            }*/
            #endregion

            #region "Load Detection Zone info into INFLO database"
            LogTxtMsg(txtLog, "\r\n\tLoad the Detection Zone info into the INFLO database.");
            retValue = clsMiscFunctions.LoadDetectionZoneInfoIntoINFLODatabase(DZList, ref DB);
            if (retValue.Length > 0)
            {
                return retValue;
            }
            #endregion

            #region "Load MileMarker info into INFLO database"
            LogTxtMsg(txtLog, "\r\n\tLoad the Mile Marker info into the INFLO database.");
            retValue = clsMiscFunctions.LoadMileMarkerInfoIntoINFLODatabase(MMList, ref DB);
            if (retValue.Length > 0)
            {
                return retValue;
            }
            #endregion
            #region "Load INFLO Thresholds into INFLO database"
            LogTxtMsg(txtLog, "\r\n\tLoad the INFLO thresholds and parameters into the INFLO database.");
            retValue = clsMiscFunctions.LoadThresholdsIntoINFLODatabase(ref DB);
            if (retValue.Length > 0)
            {
                return retValue;
            }
            #endregion

            LogTxtMsg(txtLog, "\r\nFinished loading the INFLO configuration parameters into the INFLO database");
            return retValue;
        }


        private void btnReadINFLOConfigurationFiles_Click(object sender, EventArgs e)
        {
            string retValue = string.Empty;

            retValue = ReadINFLOConfigurationFiles();
            if (retValue.Length > 0)
            {
                LogTxtMsg(txtLog, retValue);
                LogTxtMsg(txtLog, "\r\nThe INFLO Configuration application is terminating.");
                return;
            }
        }
        private void btnSelectINFLOConfigFile_Click(object sender, EventArgs e)
        {
            openFileDialog1.InitialDirectory = System.Environment.SpecialFolder.MyDocuments.ToString();
            openFileDialog1.Filter = "XML Files (.xml)|*.xml|All Files (*.*)|*.*";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.Multiselect = false;

            DialogResult DialogResult = openFileDialog1.ShowDialog();
            if (DialogResult == System.Windows.Forms.DialogResult.OK)
            {
                clsGlobalVars.INFLOConfigFile = openFileDialog1.FileName;
                txtINFLOConfigFile.Text = clsGlobalVars.INFLOConfigFile;
                LogTxtMsg(txtLog, "INFLO config file: " + clsGlobalVars.INFLOConfigFile);
            }
        }

        private string GetDatabaseTables()
        {
            string retValue = string.Empty;

            //DataRow CurrRows = new DataRow();
            string strSQLStatement = string.Empty;
            //DataRow MyRow = new DataRow();

            LogTxtMsg(txtLog, "Get database tables:");
            if (DB.DBInterfaceType == clsDatabase.enDBInterfaceType.enOLEDB)
            {
                string[] restrictions = new string[4];
                restrictions[3] = "Table";
                try
                {
                    DataTable table = DB.OLEDBConnection.GetSchema("Tables", restrictions);
                    foreach (DataRow row in table.Rows)
                    {
                        foreach (DataColumn col in table.Columns)
                        {
                            LogTxtMsg(txtLog, "\t" + col.ColumnName + row[2] + "(" + col + ")");
                        }
                    }
                    for (int i = 0; i < table.Rows.Count; i++)
                    {
                        LogTxtMsg(txtLog, "\t" + table.Rows[i][2].ToString());
                    }
                }
                catch (Exception errorEvent)
                {
                    retValue = "\tError in opening MS Access file: " + DB.OLEMSAccessFileName + "\r\n\t\t" + errorEvent.Message;
                    //if(!(CurrRows == null))
                    //    CurrRows = null;
                    //if(!(MyRow == null))
                    //    MyRow = null;
                }
                finally
                {
                }
            }
            return retValue;
        }

    }
}
