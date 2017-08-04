using INCZONE.Common;
namespace INCZONE.Forms.Configuration
{
    partial class DGPSForm
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.dgpsConnectionStatusLb = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.hostIPTb = new System.Windows.Forms.TextBox();
            this.testConnectionBt = new System.Windows.Forms.Button();
            this.passwordTb = new System.Windows.Forms.TextBox();
            this.usernameTb = new System.Windows.Forms.TextBox();
            this.hostPortTb = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.Usernamelabel = new System.Windows.Forms.Label();
            this.Portlabel = new System.Windows.Forms.Label();
            this.HostIPlabel = new System.Windows.Forms.Label();
            this.isDefaultCheckB = new System.Windows.Forms.CheckBox();
            this.newConfigBt = new System.Windows.Forms.Button();
            this.deleteConfigBt = new System.Windows.Forms.Button();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.configNameTb = new System.Windows.Forms.TextBox();
            this.savedConfigsCb = new System.Windows.Forms.ComboBox();
            this.saveConfig = new System.Windows.Forms.Button();
            this.cancelConfigBt = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.label10 = new System.Windows.Forms.Label();
            this.locationRefreshNUD = new System.Windows.Forms.NumericUpDown();
            this.label11 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.dgpsRefreshNUD = new System.Windows.Forms.NumericUpDown();
            this.label6 = new System.Windows.Forms.Label();
            this.dgpsErrorProvider = new System.Windows.Forms.ErrorProvider(this.components);
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.locationRefreshNUD)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgpsRefreshNUD)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgpsErrorProvider)).BeginInit();
            this.groupBox4.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.dgpsConnectionStatusLb);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(29, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(892, 62);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Differential GPS Status";
            // 
            // dgpsConnectionStatusLb
            // 
            this.dgpsConnectionStatusLb.AutoSize = true;
            this.dgpsConnectionStatusLb.Location = new System.Drawing.Point(195, 29);
            this.dgpsConnectionStatusLb.Name = "dgpsConnectionStatusLb";
            this.dgpsConnectionStatusLb.Size = new System.Drawing.Size(53, 13);
            this.dgpsConnectionStatusLb.TabIndex = 1;
            this.dgpsConnectionStatusLb.Text = UIConstants.STATUS_UNKNOWN;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(75, 29);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(97, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Connection Status:";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.hostIPTb);
            this.groupBox2.Controls.Add(this.testConnectionBt);
            this.groupBox2.Controls.Add(this.passwordTb);
            this.groupBox2.Controls.Add(this.usernameTb);
            this.groupBox2.Controls.Add(this.hostPortTb);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.Usernamelabel);
            this.groupBox2.Controls.Add(this.Portlabel);
            this.groupBox2.Controls.Add(this.HostIPlabel);
            this.groupBox2.Location = new System.Drawing.Point(28, 282);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(891, 207);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "NTRIP Caster Account";
            // 
            // hostIPTb
            // 
            this.hostIPTb.Location = new System.Drawing.Point(195, 28);
            this.hostIPTb.Name = "hostIPTb";
            this.hostIPTb.Size = new System.Drawing.Size(100, 20);
            this.hostIPTb.TabIndex = 6;
            // 
            // testConnectionBt
            // 
            this.testConnectionBt.Location = new System.Drawing.Point(195, 163);
            this.testConnectionBt.Name = "testConnectionBt";
            this.testConnectionBt.Size = new System.Drawing.Size(100, 23);
            this.testConnectionBt.TabIndex = 10;
            this.testConnectionBt.Text = "Test Connection";
            this.testConnectionBt.UseVisualStyleBackColor = true;
            this.testConnectionBt.Click += new System.EventHandler(this.testConnectionBt_Click);
            // 
            // passwordTb
            // 
            this.passwordTb.Location = new System.Drawing.Point(195, 128);
            this.passwordTb.Name = "passwordTb";
            this.passwordTb.PasswordChar = '*';
            this.passwordTb.Size = new System.Drawing.Size(229, 20);
            this.passwordTb.TabIndex = 9;
            // 
            // usernameTb
            // 
            this.usernameTb.Location = new System.Drawing.Point(195, 97);
            this.usernameTb.Name = "usernameTb";
            this.usernameTb.Size = new System.Drawing.Size(229, 20);
            this.usernameTb.TabIndex = 8;
            // 
            // hostPortTb
            // 
            this.hostPortTb.Location = new System.Drawing.Point(195, 63);
            this.hostPortTb.MaxLength = 6;
            this.hostPortTb.Name = "hostPortTb";
            this.hostPortTb.Size = new System.Drawing.Size(69, 20);
            this.hostPortTb.TabIndex = 7;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(116, 135);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(56, 13);
            this.label5.TabIndex = 12;
            this.label5.Text = "Password:";
            // 
            // Usernamelabel
            // 
            this.Usernamelabel.AutoSize = true;
            this.Usernamelabel.Location = new System.Drawing.Point(114, 100);
            this.Usernamelabel.Name = "Usernamelabel";
            this.Usernamelabel.Size = new System.Drawing.Size(58, 13);
            this.Usernamelabel.TabIndex = 11;
            this.Usernamelabel.Text = "Username:";
            // 
            // Portlabel
            // 
            this.Portlabel.AutoSize = true;
            this.Portlabel.Location = new System.Drawing.Point(118, 70);
            this.Portlabel.Name = "Portlabel";
            this.Portlabel.Size = new System.Drawing.Size(54, 13);
            this.Portlabel.TabIndex = 10;
            this.Portlabel.Text = "Host Port:";
            // 
            // HostIPlabel
            // 
            this.HostIPlabel.AutoSize = true;
            this.HostIPlabel.Location = new System.Drawing.Point(68, 35);
            this.HostIPlabel.Name = "HostIPlabel";
            this.HostIPlabel.Size = new System.Drawing.Size(104, 13);
            this.HostIPlabel.TabIndex = 9;
            this.HostIPlabel.Text = "Host Address (IPv4):";
            // 
            // isDefaultCheckB
            // 
            this.isDefaultCheckB.AutoSize = true;
            this.isDefaultCheckB.Location = new System.Drawing.Point(195, 133);
            this.isDefaultCheckB.Name = "isDefaultCheckB";
            this.isDefaultCheckB.Size = new System.Drawing.Size(125, 17);
            this.isDefaultCheckB.TabIndex = 5;
            this.isDefaultCheckB.Text = "Default Configuration";
            this.isDefaultCheckB.UseVisualStyleBackColor = true;
            // 
            // newConfigBt
            // 
            this.newConfigBt.Location = new System.Drawing.Point(195, 60);
            this.newConfigBt.Name = "newConfigBt";
            this.newConfigBt.Size = new System.Drawing.Size(75, 23);
            this.newConfigBt.TabIndex = 3;
            this.newConfigBt.Text = "New";
            this.newConfigBt.UseVisualStyleBackColor = true;
            this.newConfigBt.Click += new System.EventHandler(this.newConfigBt_Click);
            // 
            // deleteConfigBt
            // 
            this.deleteConfigBt.Enabled = false;
            this.deleteConfigBt.Location = new System.Drawing.Point(523, 26);
            this.deleteConfigBt.Name = "deleteConfigBt";
            this.deleteConfigBt.Size = new System.Drawing.Size(75, 23);
            this.deleteConfigBt.TabIndex = 2;
            this.deleteConfigBt.Text = "Delete";
            this.deleteConfigBt.UseVisualStyleBackColor = true;
            this.deleteConfigBt.Click += new System.EventHandler(this.deleteConfigBt_Click);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(31, 36);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(139, 13);
            this.label9.TabIndex = 21;
            this.label9.Text = "Select Saved Configuration:";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(57, 105);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(115, 13);
            this.label8.TabIndex = 20;
            this.label8.Text = "Save Configuration As:";
            // 
            // configNameTb
            // 
            this.configNameTb.Location = new System.Drawing.Point(195, 98);
            this.configNameTb.Name = "configNameTb";
            this.configNameTb.Size = new System.Drawing.Size(293, 20);
            this.configNameTb.TabIndex = 4;
            // 
            // savedConfigsCb
            // 
            this.savedConfigsCb.FormattingEnabled = true;
            this.savedConfigsCb.Location = new System.Drawing.Point(195, 28);
            this.savedConfigsCb.Name = "savedConfigsCb";
            this.savedConfigsCb.Size = new System.Drawing.Size(313, 21);
            this.savedConfigsCb.TabIndex = 1;
            this.savedConfigsCb.SelectedIndexChanged += new System.EventHandler(this.savedConfigsCb_SelectedIndexChanged);
            // 
            // saveConfig
            // 
            this.saveConfig.Location = new System.Drawing.Point(28, 626);
            this.saveConfig.Name = "saveConfig";
            this.saveConfig.Size = new System.Drawing.Size(75, 23);
            this.saveConfig.TabIndex = 13;
            this.saveConfig.Text = "Save";
            this.saveConfig.UseVisualStyleBackColor = true;
            this.saveConfig.Click += new System.EventHandler(this.saveConfig_Click);
            // 
            // cancelConfigBt
            // 
            this.cancelConfigBt.Location = new System.Drawing.Point(121, 626);
            this.cancelConfigBt.Name = "cancelConfigBt";
            this.cancelConfigBt.Size = new System.Drawing.Size(75, 23);
            this.cancelConfigBt.TabIndex = 14;
            this.cancelConfigBt.Text = "Cancel";
            this.cancelConfigBt.UseVisualStyleBackColor = true;
            this.cancelConfigBt.Click += new System.EventHandler(this.cancelConfigBt_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.label10);
            this.groupBox3.Controls.Add(this.locationRefreshNUD);
            this.groupBox3.Controls.Add(this.label11);
            this.groupBox3.Controls.Add(this.label7);
            this.groupBox3.Controls.Add(this.dgpsRefreshNUD);
            this.groupBox3.Controls.Add(this.label6);
            this.groupBox3.Location = new System.Drawing.Point(28, 506);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(892, 105);
            this.groupBox3.TabIndex = 6;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Refresh Rates";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(321, 66);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(49, 13);
            this.label10.TabIndex = 5;
            this.label10.Text = "Seconds";
            // 
            // locationRefreshNUD
            // 
            this.locationRefreshNUD.Location = new System.Drawing.Point(195, 59);
            this.locationRefreshNUD.Minimum = new decimal(new int[] {
            3,
            0,
            0,
            0});
            this.locationRefreshNUD.Name = "locationRefreshNUD";
            this.locationRefreshNUD.Size = new System.Drawing.Size(120, 20);
            this.locationRefreshNUD.TabIndex = 12;
            this.locationRefreshNUD.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(94, 66);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(91, 13);
            this.label11.TabIndex = 3;
            this.label11.Text = "Location Refresh:";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(322, 31);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(49, 13);
            this.label7.TabIndex = 2;
            this.label7.Text = "Seconds";
            // 
            // dgpsRefreshNUD
            // 
            this.dgpsRefreshNUD.Location = new System.Drawing.Point(195, 24);
            this.dgpsRefreshNUD.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.dgpsRefreshNUD.Name = "dgpsRefreshNUD";
            this.dgpsRefreshNUD.Size = new System.Drawing.Size(120, 20);
            this.dgpsRefreshNUD.TabIndex = 11;
            this.dgpsRefreshNUD.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(105, 31);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(80, 13);
            this.label6.TabIndex = 0;
            this.label6.Text = "DGPS Refresh:";
            // 
            // dgpsErrorProvider
            // 
            this.dgpsErrorProvider.ContainerControl = this;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.savedConfigsCb);
            this.groupBox4.Controls.Add(this.isDefaultCheckB);
            this.groupBox4.Controls.Add(this.configNameTb);
            this.groupBox4.Controls.Add(this.newConfigBt);
            this.groupBox4.Controls.Add(this.label8);
            this.groupBox4.Controls.Add(this.deleteConfigBt);
            this.groupBox4.Controls.Add(this.label9);
            this.groupBox4.Location = new System.Drawing.Point(29, 89);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(890, 178);
            this.groupBox4.TabIndex = 15;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Maintain Configurations";
            // 
            // DGPSForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(992, 700);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.cancelConfigBt);
            this.Controls.Add(this.saveConfig);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "DGPSForm";
            this.Text = "Differential GPS Configuration";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.locationRefreshNUD)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgpsRefreshNUD)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgpsErrorProvider)).EndInit();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label dgpsConnectionStatusLb;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button saveConfig;
        private System.Windows.Forms.TextBox passwordTb;
        private System.Windows.Forms.TextBox usernameTb;
        private System.Windows.Forms.TextBox hostPortTb;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label Usernamelabel;
        private System.Windows.Forms.Label Portlabel;
        private System.Windows.Forms.Label HostIPlabel;
        private System.Windows.Forms.Button cancelConfigBt;
        private System.Windows.Forms.Button testConnectionBt;
        private System.Windows.Forms.Button deleteConfigBt;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox configNameTb;
        private System.Windows.Forms.ComboBox savedConfigsCb;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.NumericUpDown dgpsRefreshNUD;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button newConfigBt;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.NumericUpDown locationRefreshNUD;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.CheckBox isDefaultCheckB;
        private System.Windows.Forms.ErrorProvider dgpsErrorProvider;
        private System.Windows.Forms.TextBox hostIPTb;
        private System.Windows.Forms.GroupBox groupBox4;
    }
}