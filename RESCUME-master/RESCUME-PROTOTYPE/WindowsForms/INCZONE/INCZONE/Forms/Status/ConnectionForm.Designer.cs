using INCZONE.Common;
namespace INCZONE.Forms
{
    partial class ConnectionForm
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
            this.bluetoothGb = new System.Windows.Forms.GroupBox();
            this.vitalStatusLb = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.bluetoothConfigureBt = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.aradaStatusLb = new System.Windows.Forms.Label();
            this.capWINMobileGb = new System.Windows.Forms.GroupBox();
            this.capWinMobileConfigureBt = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.capWINMobileStatusLb = new System.Windows.Forms.Label();
            this.capWinGb = new System.Windows.Forms.GroupBox();
            this.capWinConfigureBt = new System.Windows.Forms.Button();
            this.capWinLb = new System.Windows.Forms.Label();
            this.capWinStatusLb = new System.Windows.Forms.Label();
            this.dsrcGb = new System.Windows.Forms.GroupBox();
            this.dsrcConfigureBt = new System.Windows.Forms.Button();
            this.dsrcLb = new System.Windows.Forms.Label();
            this.dsrcStatusLb = new System.Windows.Forms.Label();
            this.dgpsGb = new System.Windows.Forms.GroupBox();
            this.dgpsConfigureBt = new System.Windows.Forms.Button();
            this.dgpsLb = new System.Windows.Forms.Label();
            this.dgpsStatusLb = new System.Windows.Forms.Label();
            this.radioInterfaceGb = new System.Windows.Forms.GroupBox();
            this.label4 = new System.Windows.Forms.Label();
            this.radioStatusLb = new System.Windows.Forms.Label();
            this.responderLocationGb = new System.Windows.Forms.GroupBox();
            this.label5 = new System.Windows.Forms.Label();
            this.responderLocationStatusLb = new System.Windows.Forms.Label();
            this.bluetoothGb.SuspendLayout();
            this.capWINMobileGb.SuspendLayout();
            this.capWinGb.SuspendLayout();
            this.dsrcGb.SuspendLayout();
            this.dgpsGb.SuspendLayout();
            this.radioInterfaceGb.SuspendLayout();
            this.responderLocationGb.SuspendLayout();
            this.SuspendLayout();
            // 
            // bluetoothGb
            // 
            this.bluetoothGb.Controls.Add(this.vitalStatusLb);
            this.bluetoothGb.Controls.Add(this.label3);
            this.bluetoothGb.Controls.Add(this.bluetoothConfigureBt);
            this.bluetoothGb.Controls.Add(this.label2);
            this.bluetoothGb.Controls.Add(this.aradaStatusLb);
            this.bluetoothGb.Location = new System.Drawing.Point(23, 12);
            this.bluetoothGb.Name = "bluetoothGb";
            this.bluetoothGb.Size = new System.Drawing.Size(892, 112);
            this.bluetoothGb.TabIndex = 14;
            this.bluetoothGb.TabStop = false;
            this.bluetoothGb.Text = "Bluetooth";
            // 
            // vitalStatusLb
            // 
            this.vitalStatusLb.AutoSize = true;
            this.vitalStatusLb.Location = new System.Drawing.Point(163, 47);
            this.vitalStatusLb.Name = "vitalStatusLb";
            this.vitalStatusLb.Size = new System.Drawing.Size(53, 13);
            this.vitalStatusLb.TabIndex = 12;
            this.vitalStatusLb.Text = "Unknown";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(20, 47);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(121, 13);
            this.label3.TabIndex = 11;
            this.label3.Text = "VItal Connection Status:";
            // 
            // bluetoothConfigureBt
            // 
            this.bluetoothConfigureBt.Location = new System.Drawing.Point(166, 72);
            this.bluetoothConfigureBt.Name = "bluetoothConfigureBt";
            this.bluetoothConfigureBt.Size = new System.Drawing.Size(75, 23);
            this.bluetoothConfigureBt.TabIndex = 10;
            this.bluetoothConfigureBt.Text = "Configure";
            this.bluetoothConfigureBt.UseVisualStyleBackColor = true;
            this.bluetoothConfigureBt.Click += new System.EventHandler(this.bluetoothConfigureBt_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 25);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(130, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "DSRC Connection Status:";
            // 
            // aradaStatusLb
            // 
            this.aradaStatusLb.AutoSize = true;
            this.aradaStatusLb.Location = new System.Drawing.Point(163, 25);
            this.aradaStatusLb.Name = "aradaStatusLb";
            this.aradaStatusLb.Size = new System.Drawing.Size(53, 13);
            this.aradaStatusLb.TabIndex = 6;
            this.aradaStatusLb.Text = "Unknown";
            // 
            // capWINMobileGb
            // 
            this.capWINMobileGb.Controls.Add(this.capWinMobileConfigureBt);
            this.capWINMobileGb.Controls.Add(this.label1);
            this.capWINMobileGb.Controls.Add(this.capWINMobileStatusLb);
            this.capWINMobileGb.Location = new System.Drawing.Point(23, 442);
            this.capWINMobileGb.Name = "capWINMobileGb";
            this.capWINMobileGb.Size = new System.Drawing.Size(892, 98);
            this.capWINMobileGb.TabIndex = 13;
            this.capWINMobileGb.TabStop = false;
            this.capWINMobileGb.Text = "Cap WIN Mobile";
            // 
            // capWinMobileConfigureBt
            // 
            this.capWinMobileConfigureBt.Location = new System.Drawing.Point(166, 55);
            this.capWinMobileConfigureBt.Name = "capWinMobileConfigureBt";
            this.capWinMobileConfigureBt.Size = new System.Drawing.Size(75, 23);
            this.capWinMobileConfigureBt.TabIndex = 15;
            this.capWinMobileConfigureBt.Text = "Configure";
            this.capWinMobileConfigureBt.UseVisualStyleBackColor = true;
            this.capWinMobileConfigureBt.Click += new System.EventHandler(this.capWinConfigureBt_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(44, 29);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(97, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Connection Status:";
            // 
            // capWINMobileStatusLb
            // 
            this.capWINMobileStatusLb.AutoSize = true;
            this.capWINMobileStatusLb.Location = new System.Drawing.Point(163, 29);
            this.capWINMobileStatusLb.Name = "capWINMobileStatusLb";
            this.capWINMobileStatusLb.Size = new System.Drawing.Size(53, 13);
            this.capWINMobileStatusLb.TabIndex = 8;
            this.capWINMobileStatusLb.Text = "Unknown";
            // 
            // capWinGb
            // 
            this.capWinGb.Controls.Add(this.capWinConfigureBt);
            this.capWinGb.Controls.Add(this.capWinLb);
            this.capWinGb.Controls.Add(this.capWinStatusLb);
            this.capWinGb.Location = new System.Drawing.Point(23, 338);
            this.capWinGb.Name = "capWinGb";
            this.capWinGb.Size = new System.Drawing.Size(892, 98);
            this.capWinGb.TabIndex = 12;
            this.capWinGb.TabStop = false;
            this.capWinGb.Text = "Cap WIN";
            // 
            // capWinConfigureBt
            // 
            this.capWinConfigureBt.Location = new System.Drawing.Point(166, 55);
            this.capWinConfigureBt.Name = "capWinConfigureBt";
            this.capWinConfigureBt.Size = new System.Drawing.Size(75, 23);
            this.capWinConfigureBt.TabIndex = 14;
            this.capWinConfigureBt.Text = "Configure";
            this.capWinConfigureBt.UseVisualStyleBackColor = true;
            this.capWinConfigureBt.Click += new System.EventHandler(this.capWinConfigureBt_Click);
            // 
            // capWinLb
            // 
            this.capWinLb.AutoSize = true;
            this.capWinLb.Location = new System.Drawing.Point(44, 29);
            this.capWinLb.Name = "capWinLb";
            this.capWinLb.Size = new System.Drawing.Size(97, 13);
            this.capWinLb.TabIndex = 3;
            this.capWinLb.Text = "Connection Status:";
            // 
            // capWinStatusLb
            // 
            this.capWinStatusLb.AutoSize = true;
            this.capWinStatusLb.Location = new System.Drawing.Point(163, 29);
            this.capWinStatusLb.Name = "capWinStatusLb";
            this.capWinStatusLb.Size = new System.Drawing.Size(53, 13);
            this.capWinStatusLb.TabIndex = 8;
            this.capWinStatusLb.Text = "Unknown";
            // 
            // dsrcGb
            // 
            this.dsrcGb.Controls.Add(this.dsrcConfigureBt);
            this.dsrcGb.Controls.Add(this.dsrcLb);
            this.dsrcGb.Controls.Add(this.dsrcStatusLb);
            this.dsrcGb.Location = new System.Drawing.Point(23, 234);
            this.dsrcGb.Name = "dsrcGb";
            this.dsrcGb.Size = new System.Drawing.Size(892, 98);
            this.dsrcGb.TabIndex = 11;
            this.dsrcGb.TabStop = false;
            this.dsrcGb.Text = "DSRC";
            // 
            // dsrcConfigureBt
            // 
            this.dsrcConfigureBt.Location = new System.Drawing.Point(166, 50);
            this.dsrcConfigureBt.Name = "dsrcConfigureBt";
            this.dsrcConfigureBt.Size = new System.Drawing.Size(75, 23);
            this.dsrcConfigureBt.TabIndex = 11;
            this.dsrcConfigureBt.Text = "Configure";
            this.dsrcConfigureBt.UseVisualStyleBackColor = true;
            this.dsrcConfigureBt.Click += new System.EventHandler(this.dsrcConfigureBt_Click);
            // 
            // dsrcLb
            // 
            this.dsrcLb.AutoSize = true;
            this.dsrcLb.Location = new System.Drawing.Point(44, 25);
            this.dsrcLb.Name = "dsrcLb";
            this.dsrcLb.Size = new System.Drawing.Size(97, 13);
            this.dsrcLb.TabIndex = 2;
            this.dsrcLb.Text = "Connection Status:";
            // 
            // dsrcStatusLb
            // 
            this.dsrcStatusLb.AutoSize = true;
            this.dsrcStatusLb.Location = new System.Drawing.Point(163, 25);
            this.dsrcStatusLb.Name = "dsrcStatusLb";
            this.dsrcStatusLb.Size = new System.Drawing.Size(53, 13);
            this.dsrcStatusLb.TabIndex = 7;
            this.dsrcStatusLb.Text = "Unknown";
            // 
            // dgpsGb
            // 
            this.dgpsGb.Controls.Add(this.dgpsConfigureBt);
            this.dgpsGb.Controls.Add(this.dgpsLb);
            this.dgpsGb.Controls.Add(this.dgpsStatusLb);
            this.dgpsGb.Location = new System.Drawing.Point(23, 130);
            this.dgpsGb.Name = "dgpsGb";
            this.dgpsGb.Size = new System.Drawing.Size(892, 98);
            this.dgpsGb.TabIndex = 10;
            this.dgpsGb.TabStop = false;
            this.dgpsGb.Text = "Differential GPS";
            // 
            // dgpsConfigureBt
            // 
            this.dgpsConfigureBt.Location = new System.Drawing.Point(166, 50);
            this.dgpsConfigureBt.Name = "dgpsConfigureBt";
            this.dgpsConfigureBt.Size = new System.Drawing.Size(75, 23);
            this.dgpsConfigureBt.TabIndex = 10;
            this.dgpsConfigureBt.Text = "Configure";
            this.dgpsConfigureBt.UseVisualStyleBackColor = true;
            this.dgpsConfigureBt.Click += new System.EventHandler(this.dgpsConfigureBt_Click);
            // 
            // dgpsLb
            // 
            this.dgpsLb.AutoSize = true;
            this.dgpsLb.Location = new System.Drawing.Point(44, 25);
            this.dgpsLb.Name = "dgpsLb";
            this.dgpsLb.Size = new System.Drawing.Size(97, 13);
            this.dgpsLb.TabIndex = 1;
            this.dgpsLb.Text = "Connection Status:";
            // 
            // dgpsStatusLb
            // 
            this.dgpsStatusLb.AutoSize = true;
            this.dgpsStatusLb.Location = new System.Drawing.Point(163, 25);
            this.dgpsStatusLb.Name = "dgpsStatusLb";
            this.dgpsStatusLb.Size = new System.Drawing.Size(53, 13);
            this.dgpsStatusLb.TabIndex = 6;
            this.dgpsStatusLb.Text = "Unknown";
            // 
            // radioInterfaceGb
            // 
            this.radioInterfaceGb.Controls.Add(this.label4);
            this.radioInterfaceGb.Controls.Add(this.radioStatusLb);
            this.radioInterfaceGb.Location = new System.Drawing.Point(23, 546);
            this.radioInterfaceGb.Name = "radioInterfaceGb";
            this.radioInterfaceGb.Size = new System.Drawing.Size(892, 61);
            this.radioInterfaceGb.TabIndex = 16;
            this.radioInterfaceGb.TabStop = false;
            this.radioInterfaceGb.Text = "Radio Interface";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(44, 29);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(97, 13);
            this.label4.TabIndex = 3;
            this.label4.Text = "Connection Status:";
            // 
            // radioStatusLb
            // 
            this.radioStatusLb.AutoSize = true;
            this.radioStatusLb.Location = new System.Drawing.Point(163, 29);
            this.radioStatusLb.Name = "radioStatusLb";
            this.radioStatusLb.Size = new System.Drawing.Size(53, 13);
            this.radioStatusLb.TabIndex = 8;
            this.radioStatusLb.Text = "Unknown";
            // 
            // responderLocationGb
            // 
            this.responderLocationGb.Controls.Add(this.label5);
            this.responderLocationGb.Controls.Add(this.responderLocationStatusLb);
            this.responderLocationGb.Location = new System.Drawing.Point(23, 613);
            this.responderLocationGb.Name = "responderLocationGb";
            this.responderLocationGb.Size = new System.Drawing.Size(892, 61);
            this.responderLocationGb.TabIndex = 17;
            this.responderLocationGb.TabStop = false;
            this.responderLocationGb.Text = "Responder Location";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(44, 29);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(97, 13);
            this.label5.TabIndex = 3;
            this.label5.Text = "Connection Status:";
            // 
            // responderLocationStatusLb
            // 
            this.responderLocationStatusLb.AutoSize = true;
            this.responderLocationStatusLb.Location = new System.Drawing.Point(163, 29);
            this.responderLocationStatusLb.Name = "responderLocationStatusLb";
            this.responderLocationStatusLb.Size = new System.Drawing.Size(53, 13);
            this.responderLocationStatusLb.TabIndex = 8;
            this.responderLocationStatusLb.Text = "Unknown";
            // 
            // ConnectionForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(992, 700);
            this.Controls.Add(this.responderLocationGb);
            this.Controls.Add(this.radioInterfaceGb);
            this.Controls.Add(this.bluetoothGb);
            this.Controls.Add(this.capWINMobileGb);
            this.Controls.Add(this.capWinGb);
            this.Controls.Add(this.dsrcGb);
            this.Controls.Add(this.dgpsGb);
            this.Name = "ConnectionForm";
            this.Text = "Connections Status";
            this.Load += new System.EventHandler(this.ConnectionForm_Load);
            this.bluetoothGb.ResumeLayout(false);
            this.bluetoothGb.PerformLayout();
            this.capWINMobileGb.ResumeLayout(false);
            this.capWINMobileGb.PerformLayout();
            this.capWinGb.ResumeLayout(false);
            this.capWinGb.PerformLayout();
            this.dsrcGb.ResumeLayout(false);
            this.dsrcGb.PerformLayout();
            this.dgpsGb.ResumeLayout(false);
            this.dgpsGb.PerformLayout();
            this.radioInterfaceGb.ResumeLayout(false);
            this.radioInterfaceGb.PerformLayout();
            this.responderLocationGb.ResumeLayout(false);
            this.responderLocationGb.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label dgpsLb;
        private System.Windows.Forms.Label dsrcLb;
        private System.Windows.Forms.Label capWinLb;
        private System.Windows.Forms.Label dgpsStatusLb;
        private System.Windows.Forms.Label dsrcStatusLb;
        private System.Windows.Forms.Label capWinStatusLb;
        private System.Windows.Forms.GroupBox dgpsGb;
        private System.Windows.Forms.GroupBox dsrcGb;
        private System.Windows.Forms.GroupBox capWinGb;
        private System.Windows.Forms.Button dgpsConfigureBt;
        private System.Windows.Forms.Button dsrcConfigureBt;
        private System.Windows.Forms.Button capWinConfigureBt;
        private System.Windows.Forms.GroupBox capWINMobileGb;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label capWINMobileStatusLb;
        private System.Windows.Forms.GroupBox bluetoothGb;
        private System.Windows.Forms.Label vitalStatusLb;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button bluetoothConfigureBt;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label aradaStatusLb;
        private System.Windows.Forms.Button capWinMobileConfigureBt;
        private System.Windows.Forms.GroupBox radioInterfaceGb;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label radioStatusLb;
        private System.Windows.Forms.GroupBox responderLocationGb;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label responderLocationStatusLb;
    }
}