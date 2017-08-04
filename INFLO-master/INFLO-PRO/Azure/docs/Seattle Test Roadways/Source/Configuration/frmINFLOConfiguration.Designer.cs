namespace Configuration
{
    partial class frmINFLOConfiguration
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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.btnReadINFLOConfigurationFiles = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.txtDetectionZoneConfigFile = new System.Windows.Forms.TextBox();
            this.btnSelectINFLOConfigFile = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.txtDetectionStationConfigFile = new System.Windows.Forms.TextBox();
            this.txtRoadwayLinkConfigFile = new System.Windows.Forms.TextBox();
            this.txtINFLOConfigFile = new System.Windows.Forms.TextBox();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.tmrCVData = new System.Windows.Forms.Timer(this.components);
            this.fbd = new System.Windows.Forms.FolderBrowserDialog();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.btnReadINFLOConfigurationFiles);
            this.splitContainer1.Panel1.Controls.Add(this.label2);
            this.splitContainer1.Panel1.Controls.Add(this.txtDetectionZoneConfigFile);
            this.splitContainer1.Panel1.Controls.Add(this.btnSelectINFLOConfigFile);
            this.splitContainer1.Panel1.Controls.Add(this.label4);
            this.splitContainer1.Panel1.Controls.Add(this.label3);
            this.splitContainer1.Panel1.Controls.Add(this.label1);
            this.splitContainer1.Panel1.Controls.Add(this.txtDetectionStationConfigFile);
            this.splitContainer1.Panel1.Controls.Add(this.txtRoadwayLinkConfigFile);
            this.splitContainer1.Panel1.Controls.Add(this.txtINFLOConfigFile);
            this.splitContainer1.Panel1MinSize = 150;
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.txtLog);
            this.splitContainer1.Size = new System.Drawing.Size(1254, 588);
            this.splitContainer1.SplitterDistance = 150;
            this.splitContainer1.SplitterWidth = 10;
            this.splitContainer1.TabIndex = 0;
            // 
            // btnReadINFLOConfigurationFiles
            // 
            this.btnReadINFLOConfigurationFiles.BackColor = System.Drawing.SystemColors.Control;
            this.btnReadINFLOConfigurationFiles.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnReadINFLOConfigurationFiles.Location = new System.Drawing.Point(977, 39);
            this.btnReadINFLOConfigurationFiles.Name = "btnReadINFLOConfigurationFiles";
            this.btnReadINFLOConfigurationFiles.Size = new System.Drawing.Size(246, 26);
            this.btnReadINFLOConfigurationFiles.TabIndex = 13;
            this.btnReadINFLOConfigurationFiles.Text = "Read INFLO Configuration Files";
            this.btnReadINFLOConfigurationFiles.UseVisualStyleBackColor = false;
            this.btnReadINFLOConfigurationFiles.Click += new System.EventHandler(this.btnReadINFLOConfigurationFiles_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(14, 113);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(205, 16);
            this.label2.TabIndex = 12;
            this.label2.Text = "Detection Zone Configuration File";
            // 
            // txtDetectionZoneConfigFile
            // 
            this.txtDetectionZoneConfigFile.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtDetectionZoneConfigFile.Location = new System.Drawing.Point(235, 107);
            this.txtDetectionZoneConfigFile.Name = "txtDetectionZoneConfigFile";
            this.txtDetectionZoneConfigFile.Size = new System.Drawing.Size(736, 22);
            this.txtDetectionZoneConfigFile.TabIndex = 11;
            // 
            // btnSelectINFLOConfigFile
            // 
            this.btnSelectINFLOConfigFile.BackColor = System.Drawing.SystemColors.Control;
            this.btnSelectINFLOConfigFile.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSelectINFLOConfigFile.Location = new System.Drawing.Point(977, 8);
            this.btnSelectINFLOConfigFile.Name = "btnSelectINFLOConfigFile";
            this.btnSelectINFLOConfigFile.Size = new System.Drawing.Size(246, 26);
            this.btnSelectINFLOConfigFile.TabIndex = 8;
            this.btnSelectINFLOConfigFile.Text = "Select INFLO Config File";
            this.btnSelectINFLOConfigFile.UseVisualStyleBackColor = false;
            this.btnSelectINFLOConfigFile.Click += new System.EventHandler(this.btnSelectINFLOConfigFile_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(14, 80);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(215, 16);
            this.label4.TabIndex = 7;
            this.label4.Text = "Detection Station Configuration File";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(30, 48);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(199, 16);
            this.label3.TabIndex = 6;
            this.label3.Text = "Roadway Link Configuration File";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(77, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(152, 16);
            this.label1.TabIndex = 4;
            this.label1.Text = "INFLO Configuration File";
            // 
            // txtDetectionStationConfigFile
            // 
            this.txtDetectionStationConfigFile.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtDetectionStationConfigFile.Location = new System.Drawing.Point(235, 74);
            this.txtDetectionStationConfigFile.Name = "txtDetectionStationConfigFile";
            this.txtDetectionStationConfigFile.Size = new System.Drawing.Size(736, 22);
            this.txtDetectionStationConfigFile.TabIndex = 3;
            // 
            // txtRoadwayLinkConfigFile
            // 
            this.txtRoadwayLinkConfigFile.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtRoadwayLinkConfigFile.Location = new System.Drawing.Point(235, 42);
            this.txtRoadwayLinkConfigFile.Name = "txtRoadwayLinkConfigFile";
            this.txtRoadwayLinkConfigFile.Size = new System.Drawing.Size(736, 22);
            this.txtRoadwayLinkConfigFile.TabIndex = 2;
            // 
            // txtINFLOConfigFile
            // 
            this.txtINFLOConfigFile.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtINFLOConfigFile.Location = new System.Drawing.Point(235, 10);
            this.txtINFLOConfigFile.Name = "txtINFLOConfigFile";
            this.txtINFLOConfigFile.Size = new System.Drawing.Size(736, 22);
            this.txtINFLOConfigFile.TabIndex = 1;
            // 
            // txtLog
            // 
            this.txtLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtLog.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtLog.Location = new System.Drawing.Point(0, 0);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtLog.Size = new System.Drawing.Size(1250, 424);
            this.txtLog.TabIndex = 0;
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // frmINFLOConfiguration
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1254, 588);
            this.Controls.Add(this.splitContainer1);
            this.Name = "frmINFLOConfiguration";
            this.Text = "INFLO Configuration";
            this.Load += new System.EventHandler(this.frmINFLOConfiguration_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtDetectionStationConfigFile;
        private System.Windows.Forms.TextBox txtRoadwayLinkConfigFile;
        private System.Windows.Forms.TextBox txtINFLOConfigFile;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Timer tmrCVData;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtDetectionZoneConfigFile;
        private System.Windows.Forms.Button btnSelectINFLOConfigFile;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.FolderBrowserDialog fbd;
        private System.Windows.Forms.Button btnReadINFLOConfigurationFiles;
    }
}

