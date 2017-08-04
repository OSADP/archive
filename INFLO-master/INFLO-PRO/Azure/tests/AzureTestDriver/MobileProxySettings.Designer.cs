namespace AzureTestDriver
{
    partial class MobileProxySettings
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
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.txtBSMWebAPIUrl = new System.Windows.Forms.TextBox();
            this.txtProxyCount = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtGenerateInterval = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtMaxWorkerCount = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtBundlerMaxSize = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtBundlerTimeout = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.txtI2VPollPeriod = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.txtI2VWebAPIUrl = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.txtBSMBerFileLocation = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.btnSelectBsmBerFileLocation = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.Location = new System.Drawing.Point(418, 254);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 0;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.Location = new System.Drawing.Point(337, 254);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 1;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(38, 77);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(101, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "BSM WebAPI URL:";
            // 
            // txtBSMWebAPIUrl
            // 
            this.txtBSMWebAPIUrl.Location = new System.Drawing.Point(145, 74);
            this.txtBSMWebAPIUrl.Name = "txtBSMWebAPIUrl";
            this.txtBSMWebAPIUrl.Size = new System.Drawing.Size(350, 20);
            this.txtBSMWebAPIUrl.TabIndex = 3;
            // 
            // txtProxyCount
            // 
            this.txtProxyCount.Location = new System.Drawing.Point(145, 12);
            this.txtProxyCount.Name = "txtProxyCount";
            this.txtProxyCount.Size = new System.Drawing.Size(100, 20);
            this.txtProxyCount.TabIndex = 5;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(72, 15);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(67, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Proxy Count:";
            // 
            // txtGenerateInterval
            // 
            this.txtGenerateInterval.Location = new System.Drawing.Point(145, 100);
            this.txtGenerateInterval.Name = "txtGenerateInterval";
            this.txtGenerateInterval.Size = new System.Drawing.Size(100, 20);
            this.txtGenerateInterval.TabIndex = 7;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(21, 103);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(118, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "BSM Generate Interval:";
            // 
            // txtMaxWorkerCount
            // 
            this.txtMaxWorkerCount.Location = new System.Drawing.Point(145, 126);
            this.txtMaxWorkerCount.Name = "txtMaxWorkerCount";
            this.txtMaxWorkerCount.Size = new System.Drawing.Size(100, 20);
            this.txtMaxWorkerCount.TabIndex = 9;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 129);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(127, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "Max Send Worker Count:";
            // 
            // txtBundlerMaxSize
            // 
            this.txtBundlerMaxSize.Location = new System.Drawing.Point(145, 152);
            this.txtBundlerMaxSize.Name = "txtBundlerMaxSize";
            this.txtBundlerMaxSize.Size = new System.Drawing.Size(100, 20);
            this.txtBundlerMaxSize.TabIndex = 11;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(47, 155);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(92, 13);
            this.label5.TabIndex = 10;
            this.label5.Text = "Bundler Max Size:";
            // 
            // txtBundlerTimeout
            // 
            this.txtBundlerTimeout.Location = new System.Drawing.Point(145, 178);
            this.txtBundlerTimeout.Name = "txtBundlerTimeout";
            this.txtBundlerTimeout.Size = new System.Drawing.Size(100, 20);
            this.txtBundlerTimeout.TabIndex = 13;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(19, 181);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(120, 13);
            this.label6.TabIndex = 12;
            this.label6.Text = "Bundler Timeout Period:";
            // 
            // txtI2VPollPeriod
            // 
            this.txtI2VPollPeriod.Location = new System.Drawing.Point(145, 240);
            this.txtI2VPollPeriod.Name = "txtI2VPollPeriod";
            this.txtI2VPollPeriod.Size = new System.Drawing.Size(100, 20);
            this.txtI2VPollPeriod.TabIndex = 17;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(55, 243);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(84, 13);
            this.label7.TabIndex = 16;
            this.label7.Text = "I2V Poll Interval:";
            // 
            // txtI2VWebAPIUrl
            // 
            this.txtI2VWebAPIUrl.Location = new System.Drawing.Point(145, 214);
            this.txtI2VWebAPIUrl.Name = "txtI2VWebAPIUrl";
            this.txtI2VWebAPIUrl.Size = new System.Drawing.Size(350, 20);
            this.txtI2VWebAPIUrl.TabIndex = 15;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(45, 217);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(94, 13);
            this.label8.TabIndex = 14;
            this.label8.Text = "I2V WebAPI URL:";
            // 
            // txtBSMBerFileLocation
            // 
            this.txtBSMBerFileLocation.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtBSMBerFileLocation.Location = new System.Drawing.Point(145, 48);
            this.txtBSMBerFileLocation.Name = "txtBSMBerFileLocation";
            this.txtBSMBerFileLocation.ReadOnly = true;
            this.txtBSMBerFileLocation.Size = new System.Drawing.Size(308, 20);
            this.txtBSMBerFileLocation.TabIndex = 19;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(22, 51);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(117, 13);
            this.label9.TabIndex = 18;
            this.label9.Text = "BSM .ber File Location:";
            // 
            // btnSelectBsmBerFileLocation
            // 
            this.btnSelectBsmBerFileLocation.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelectBsmBerFileLocation.Location = new System.Drawing.Point(459, 45);
            this.btnSelectBsmBerFileLocation.Name = "btnSelectBsmBerFileLocation";
            this.btnSelectBsmBerFileLocation.Size = new System.Drawing.Size(34, 23);
            this.btnSelectBsmBerFileLocation.TabIndex = 20;
            this.btnSelectBsmBerFileLocation.Text = "...";
            this.btnSelectBsmBerFileLocation.UseVisualStyleBackColor = true;
            this.btnSelectBsmBerFileLocation.Click += new System.EventHandler(this.btnSelectBsmBerFileLocation_Click);
            // 
            // MobileProxySettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(505, 289);
            this.Controls.Add(this.btnSelectBsmBerFileLocation);
            this.Controls.Add(this.txtBSMBerFileLocation);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.txtI2VPollPeriod);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.txtI2VWebAPIUrl);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.txtBundlerTimeout);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.txtBundlerMaxSize);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.txtMaxWorkerCount);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txtGenerateInterval);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtProxyCount);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtBSMWebAPIUrl);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnCancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "MobileProxySettings";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Mobile Proxy Settings";
            this.Load += new System.EventHandler(this.MobileProxySettings_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtBSMWebAPIUrl;
        private System.Windows.Forms.TextBox txtProxyCount;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtGenerateInterval;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtMaxWorkerCount;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtBundlerMaxSize;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtBundlerTimeout;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtI2VPollPeriod;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txtI2VWebAPIUrl;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox txtBSMBerFileLocation;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Button btnSelectBsmBerFileLocation;
    }
}