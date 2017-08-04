namespace INCZONE
{
    partial class IncZoneMDIParent
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(IncZoneMDIParent));
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.LocationTimer = new System.Windows.Forms.Timer(this.components);
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.fileMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.incZoneMainToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.alertsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.configurationMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.alarmToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bluetoothClientMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.capWINToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dGPSToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dSRCToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.generalToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mapRepositoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.connectionStatusToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.eventLogToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.contentsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.indexToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.searchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.logOutputToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.SplashWorker = new System.ComponentModel.BackgroundWorker();
            this.ResponderTimer = new System.Windows.Forms.Timer(this.components);
            this.CapWINMobileTimer = new System.Windows.Forms.Timer(this.components);
            this.AlarmWorker = new System.ComponentModel.BackgroundWorker();
            this.SystemActiveTimer = new System.Windows.Forms.Timer(this.components);
            this.menuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // LocationTimer
            // 
            this.LocationTimer.Tick += new System.EventHandler(this.LocationTimer_Tick);
            // 
            // menuStrip
            // 
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileMenu,
            this.incZoneMainToolStripMenuItem,
            this.alertsToolStripMenuItem,
            this.configurationMenu,
            this.connectionStatusToolStripMenuItem,
            this.eventLogToolStripMenuItem,
            this.helpMenu,
            this.logOutputToolStripMenuItem});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Size = new System.Drawing.Size(1016, 24);
            this.menuStrip.TabIndex = 0;
            this.menuStrip.Text = "MenuStrip";
            // 
            // fileMenu
            // 
            this.fileMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exitToolStripMenuItem});
            this.fileMenu.ImageTransparentColor = System.Drawing.SystemColors.ActiveBorder;
            this.fileMenu.Name = "fileMenu";
            this.fileMenu.Size = new System.Drawing.Size(35, 20);
            this.fileMenu.Text = "&File";
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(92, 22);
            this.exitToolStripMenuItem.Text = "E&xit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.ExitToolsStripMenuItem_Click);
            // 
            // incZoneMainToolStripMenuItem
            // 
            this.incZoneMainToolStripMenuItem.Name = "incZoneMainToolStripMenuItem";
            this.incZoneMainToolStripMenuItem.Size = new System.Drawing.Size(87, 20);
            this.incZoneMainToolStripMenuItem.Text = "Inc-Zone Main";
            this.incZoneMainToolStripMenuItem.Click += new System.EventHandler(this.incZoneMainToolStripMenuItem_Click);
            // 
            // alertsToolStripMenuItem
            // 
            this.alertsToolStripMenuItem.Name = "alertsToolStripMenuItem";
            this.alertsToolStripMenuItem.Size = new System.Drawing.Size(47, 20);
            this.alertsToolStripMenuItem.Text = "Alerts";
            this.alertsToolStripMenuItem.Click += new System.EventHandler(this.alertsToolStripMenuItem_Click);
            // 
            // configurationMenu
            // 
            this.configurationMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.alarmToolStripMenuItem,
            this.bluetoothClientMenuItem,
            this.capWINToolStripMenuItem,
            this.dGPSToolStripMenuItem,
            this.dSRCToolStripMenuItem,
            this.generalToolStripMenuItem,
            this.mapRepositoryToolStripMenuItem});
            this.configurationMenu.Name = "configurationMenu";
            this.configurationMenu.Size = new System.Drawing.Size(84, 20);
            this.configurationMenu.Text = "Configuration";
            // 
            // alarmToolStripMenuItem
            // 
            this.alarmToolStripMenuItem.Name = "alarmToolStripMenuItem";
            this.alarmToolStripMenuItem.Size = new System.Drawing.Size(149, 22);
            this.alarmToolStripMenuItem.Text = "Alarm";
            this.alarmToolStripMenuItem.Click += new System.EventHandler(this.alarmToolStripMenuItem_Click);
            // 
            // bluetoothClientMenuItem
            // 
            this.bluetoothClientMenuItem.Name = "bluetoothClientMenuItem";
            this.bluetoothClientMenuItem.Size = new System.Drawing.Size(149, 22);
            this.bluetoothClientMenuItem.Text = "Bluetooth";
            this.bluetoothClientMenuItem.Click += new System.EventHandler(this.bluetoothClientMenuItem_Click);
            // 
            // capWINToolStripMenuItem
            // 
            this.capWINToolStripMenuItem.Name = "capWINToolStripMenuItem";
            this.capWINToolStripMenuItem.Size = new System.Drawing.Size(149, 22);
            this.capWINToolStripMenuItem.Text = "CapWIN";
            this.capWINToolStripMenuItem.Click += new System.EventHandler(this.capWINToolStripMenuItem_Click);
            // 
            // dGPSToolStripMenuItem
            // 
            this.dGPSToolStripMenuItem.Name = "dGPSToolStripMenuItem";
            this.dGPSToolStripMenuItem.Size = new System.Drawing.Size(149, 22);
            this.dGPSToolStripMenuItem.Text = "DGPS";
            this.dGPSToolStripMenuItem.Click += new System.EventHandler(this.dGPSToolStripMenuItem_Click);
            // 
            // dSRCToolStripMenuItem
            // 
            this.dSRCToolStripMenuItem.Name = "dSRCToolStripMenuItem";
            this.dSRCToolStripMenuItem.Size = new System.Drawing.Size(149, 22);
            this.dSRCToolStripMenuItem.Text = "DSRC";
            this.dSRCToolStripMenuItem.Click += new System.EventHandler(this.dSRCToolStripMenuItem_Click);
            // 
            // generalToolStripMenuItem
            // 
            this.generalToolStripMenuItem.Name = "generalToolStripMenuItem";
            this.generalToolStripMenuItem.Size = new System.Drawing.Size(149, 22);
            this.generalToolStripMenuItem.Text = "General";
            this.generalToolStripMenuItem.Click += new System.EventHandler(this.generalToolStripMenuItem_Click);
            // 
            // mapRepositoryToolStripMenuItem
            // 
            this.mapRepositoryToolStripMenuItem.Name = "mapRepositoryToolStripMenuItem";
            this.mapRepositoryToolStripMenuItem.Size = new System.Drawing.Size(149, 22);
            this.mapRepositoryToolStripMenuItem.Text = "Map Repository";
            this.mapRepositoryToolStripMenuItem.Click += new System.EventHandler(this.mapRepositoryToolStripMenuItem_Click);
            // 
            // connectionStatusToolStripMenuItem
            // 
            this.connectionStatusToolStripMenuItem.Name = "connectionStatusToolStripMenuItem";
            this.connectionStatusToolStripMenuItem.Size = new System.Drawing.Size(107, 20);
            this.connectionStatusToolStripMenuItem.Text = "Connection Status";
            this.connectionStatusToolStripMenuItem.Click += new System.EventHandler(this.connectionStatusToolStripMenuItem_Click);
            // 
            // eventLogToolStripMenuItem
            // 
            this.eventLogToolStripMenuItem.Name = "eventLogToolStripMenuItem";
            this.eventLogToolStripMenuItem.Size = new System.Drawing.Size(67, 20);
            this.eventLogToolStripMenuItem.Text = "Event Log";
            this.eventLogToolStripMenuItem.Click += new System.EventHandler(this.eventLogToolStripMenuItem_Click);
            // 
            // helpMenu
            // 
            this.helpMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.contentsToolStripMenuItem,
            this.indexToolStripMenuItem,
            this.searchToolStripMenuItem,
            this.toolStripSeparator8,
            this.aboutToolStripMenuItem});
            this.helpMenu.Name = "helpMenu";
            this.helpMenu.Size = new System.Drawing.Size(40, 20);
            this.helpMenu.Text = "&Help";
            // 
            // contentsToolStripMenuItem
            // 
            this.contentsToolStripMenuItem.Name = "contentsToolStripMenuItem";
            this.contentsToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F1)));
            this.contentsToolStripMenuItem.Size = new System.Drawing.Size(162, 22);
            this.contentsToolStripMenuItem.Text = "&Contents";
            // 
            // indexToolStripMenuItem
            // 
            this.indexToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("indexToolStripMenuItem.Image")));
            this.indexToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Black;
            this.indexToolStripMenuItem.Name = "indexToolStripMenuItem";
            this.indexToolStripMenuItem.Size = new System.Drawing.Size(162, 22);
            this.indexToolStripMenuItem.Text = "&Index";
            // 
            // searchToolStripMenuItem
            // 
            this.searchToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("searchToolStripMenuItem.Image")));
            this.searchToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Black;
            this.searchToolStripMenuItem.Name = "searchToolStripMenuItem";
            this.searchToolStripMenuItem.Size = new System.Drawing.Size(162, 22);
            this.searchToolStripMenuItem.Text = "&Search";
            // 
            // toolStripSeparator8
            // 
            this.toolStripSeparator8.Name = "toolStripSeparator8";
            this.toolStripSeparator8.Size = new System.Drawing.Size(159, 6);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(162, 22);
            this.aboutToolStripMenuItem.Text = "&About ... ...";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // logOutputToolStripMenuItem
            // 
            this.logOutputToolStripMenuItem.Name = "logOutputToolStripMenuItem";
            this.logOutputToolStripMenuItem.Size = new System.Drawing.Size(73, 20);
            this.logOutputToolStripMenuItem.Text = "Log Output";
            this.logOutputToolStripMenuItem.Click += new System.EventHandler(this.logOutputToolStripMenuItem_Click);
            // 
            // ResponderTimer
            // 
            this.ResponderTimer.Interval = 10000;
            this.ResponderTimer.Tick += new System.EventHandler(this.ResponderTimer_Tick);
            // 
            // CapWINMobileTimer
            // 
            this.CapWINMobileTimer.Interval = 1000;
            this.CapWINMobileTimer.Tick += new System.EventHandler(this.CapWINMobileTimer_Tick);
            // 
            // AlarmWorker
            // 
            this.AlarmWorker.WorkerReportsProgress = true;
            this.AlarmWorker.WorkerSupportsCancellation = true;
            // 
            // SystemActiveTimer
            // 
            this.SystemActiveTimer.Interval = 600000;
            this.SystemActiveTimer.Tick += new System.EventHandler(this.SystemActiveTimer_Tick);
            // 
            // IncZoneMDIParent
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1016, 740);
            this.Controls.Add(this.menuStrip);
            this.IsMdiContainer = true;
            this.MainMenuStrip = this.menuStrip;
            this.Name = "IncZoneMDIParent";
            this.Text = "R.E.S.C.U.M.E Incident Zone (IncZone)";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.IncZoneMDIParent_FormClosing);
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion


        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator8;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fileMenu;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpMenu;
        private System.Windows.Forms.ToolStripMenuItem contentsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem indexToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem searchToolStripMenuItem;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.ToolStripMenuItem configurationMenu;
        private System.Windows.Forms.ToolStripMenuItem bluetoothClientMenuItem;
        private System.Windows.Forms.ToolStripMenuItem dGPSToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem capWINToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem dSRCToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem generalToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem eventLogToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem connectionStatusToolStripMenuItem;
        private System.Windows.Forms.Timer LocationTimer;
        private System.Windows.Forms.ToolStripMenuItem alarmToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mapRepositoryToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem alertsToolStripMenuItem;
        private System.ComponentModel.BackgroundWorker SplashWorker;
        private System.Windows.Forms.ToolStripMenuItem incZoneMainToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem logOutputToolStripMenuItem;
        private System.Windows.Forms.Timer ResponderTimer;
        private System.Windows.Forms.Timer CapWINMobileTimer;
        private System.ComponentModel.BackgroundWorker AlarmWorker;
        private System.Windows.Forms.Timer SystemActiveTimer;
    }
}



