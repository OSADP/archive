using INCZONE.Common;
namespace INCZONE.Forms.Configuration
{
    partial class CapWINForm
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
            this.capWinConnectStatusLb = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.testConnectionBt = new System.Windows.Forms.Button();
            this.hostURLTb = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.passwordTb = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.usernameTb = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.baudRateTb = new System.Windows.Forms.TextBox();
            this.comPortCb = new System.Windows.Forms.ComboBox();
            this.label9 = new System.Windows.Forms.Label();
            this.saveConfigurationBt = new System.Windows.Forms.Button();
            this.cancelConfigurationBt = new System.Windows.Forms.Button();
            this.capWINErrorProvider = new System.Windows.Forms.ErrorProvider(this.components);
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.label8 = new System.Windows.Forms.Label();
            this.laneDataNUD = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.distanceNUD = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.capWINErrorProvider)).BeginInit();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.laneDataNUD)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.distanceNUD)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.capWinConnectStatusLb);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(29, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(892, 71);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Cap WIN Status";
            // 
            // capWinConnectStatusLb
            // 
            this.capWinConnectStatusLb.AutoSize = true;
            this.capWinConnectStatusLb.Location = new System.Drawing.Point(174, 32);
            this.capWinConnectStatusLb.Name = "capWinConnectStatusLb";
            this.capWinConnectStatusLb.Size = new System.Drawing.Size(53, 13);
            this.capWinConnectStatusLb.TabIndex = 1;
            this.capWinConnectStatusLb.Text = "Unknown";
            this.capWinConnectStatusLb.TextChanged += new System.EventHandler(this.capWinConnectStatusLb_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(55, 32);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(97, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Connection Status:";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.testConnectionBt);
            this.groupBox2.Controls.Add(this.hostURLTb);
            this.groupBox2.Controls.Add(this.label7);
            this.groupBox2.Controls.Add(this.passwordTb);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.usernameTb);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Location = new System.Drawing.Point(29, 98);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(892, 183);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Cap WIN Account";
            // 
            // testConnectionBt
            // 
            this.testConnectionBt.Location = new System.Drawing.Point(177, 134);
            this.testConnectionBt.Name = "testConnectionBt";
            this.testConnectionBt.Size = new System.Drawing.Size(100, 23);
            this.testConnectionBt.TabIndex = 4;
            this.testConnectionBt.Text = "Test Connection";
            this.testConnectionBt.UseVisualStyleBackColor = true;
            this.testConnectionBt.Click += new System.EventHandler(this.testConnectionBt_Click);
            // 
            // hostURLTb
            // 
            this.hostURLTb.Location = new System.Drawing.Point(177, 29);
            this.hostURLTb.Name = "hostURLTb";
            this.hostURLTb.Size = new System.Drawing.Size(449, 20);
            this.hostURLTb.TabIndex = 1;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(95, 36);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(57, 13);
            this.label7.TabIndex = 4;
            this.label7.Text = "Host URL:";
            // 
            // passwordTb
            // 
            this.passwordTb.Location = new System.Drawing.Point(177, 99);
            this.passwordTb.Name = "passwordTb";
            this.passwordTb.Size = new System.Drawing.Size(218, 20);
            this.passwordTb.TabIndex = 3;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(99, 106);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(53, 13);
            this.label6.TabIndex = 2;
            this.label6.Text = "Password";
            // 
            // usernameTb
            // 
            this.usernameTb.Location = new System.Drawing.Point(177, 64);
            this.usernameTb.Name = "usernameTb";
            this.usernameTb.Size = new System.Drawing.Size(218, 20);
            this.usernameTb.TabIndex = 2;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(94, 71);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(58, 13);
            this.label5.TabIndex = 0;
            this.label5.Text = "Username:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(91, 71);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(61, 13);
            this.label2.TabIndex = 11;
            this.label2.Text = "Baud Rate:";
            // 
            // baudRateTb
            // 
            this.baudRateTb.Location = new System.Drawing.Point(177, 64);
            this.baudRateTb.Name = "baudRateTb";
            this.baudRateTb.Size = new System.Drawing.Size(100, 20);
            this.baudRateTb.TabIndex = 6;
            // 
            // comPortCb
            // 
            this.comPortCb.FormattingEnabled = true;
            this.comPortCb.Location = new System.Drawing.Point(177, 28);
            this.comPortCb.Name = "comPortCb";
            this.comPortCb.Size = new System.Drawing.Size(218, 21);
            this.comPortCb.TabIndex = 5;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(96, 36);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(56, 13);
            this.label9.TabIndex = 9;
            this.label9.Text = "COM Port:";
            // 
            // saveConfigurationBt
            // 
            this.saveConfigurationBt.Location = new System.Drawing.Point(29, 548);
            this.saveConfigurationBt.Name = "saveConfigurationBt";
            this.saveConfigurationBt.Size = new System.Drawing.Size(75, 23);
            this.saveConfigurationBt.TabIndex = 9;
            this.saveConfigurationBt.Text = "Save";
            this.saveConfigurationBt.UseVisualStyleBackColor = true;
            this.saveConfigurationBt.Click += new System.EventHandler(this.saveConfigurationBt_Click);
            // 
            // cancelConfigurationBt
            // 
            this.cancelConfigurationBt.Location = new System.Drawing.Point(122, 548);
            this.cancelConfigurationBt.Name = "cancelConfigurationBt";
            this.cancelConfigurationBt.Size = new System.Drawing.Size(75, 23);
            this.cancelConfigurationBt.TabIndex = 7;
            this.cancelConfigurationBt.Text = "Cancel";
            this.cancelConfigurationBt.UseVisualStyleBackColor = true;
            this.cancelConfigurationBt.Click += new System.EventHandler(this.cancelConfigurationBt_Click);
            // 
            // capWINErrorProvider
            // 
            this.capWINErrorProvider.ContainerControl = this;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.label2);
            this.groupBox3.Controls.Add(this.comPortCb);
            this.groupBox3.Controls.Add(this.baudRateTb);
            this.groupBox3.Controls.Add(this.label9);
            this.groupBox3.Location = new System.Drawing.Point(29, 297);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(892, 109);
            this.groupBox3.TabIndex = 8;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Inc Zone to CapWIN mobile";
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.label8);
            this.groupBox4.Controls.Add(this.laneDataNUD);
            this.groupBox4.Controls.Add(this.label3);
            this.groupBox4.Controls.Add(this.label10);
            this.groupBox4.Controls.Add(this.distanceNUD);
            this.groupBox4.Controls.Add(this.label4);
            this.groupBox4.Location = new System.Drawing.Point(29, 421);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(892, 112);
            this.groupBox4.TabIndex = 9;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Distance to Incident";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(283, 70);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(39, 13);
            this.label8.TabIndex = 6;
            this.label8.Text = "Meters";
            // 
            // laneDataNUD
            // 
            this.laneDataNUD.Location = new System.Drawing.Point(177, 68);
            this.laneDataNUD.Maximum = new decimal(new int[] {
            150,
            0,
            0,
            0});
            this.laneDataNUD.Minimum = new decimal(new int[] {
            30,
            0,
            0,
            0});
            this.laneDataNUD.Name = "laneDataNUD";
            this.laneDataNUD.Size = new System.Drawing.Size(100, 20);
            this.laneDataNUD.TabIndex = 8;
            this.laneDataNUD.Value = new decimal(new int[] {
            30,
            0,
            0,
            0});
            this.laneDataNUD.ValueChanged += new System.EventHandler(this.laneDataNUD_ValueChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(47, 70);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(121, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Length of incident zone:";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(283, 32);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(39, 13);
            this.label10.TabIndex = 3;
            this.label10.Text = "Meters";
            // 
            // distanceNUD
            // 
            this.distanceNUD.Location = new System.Drawing.Point(177, 30);
            this.distanceNUD.Minimum = new decimal(new int[] {
            3,
            0,
            0,
            0});
            this.distanceNUD.Name = "distanceNUD";
            this.distanceNUD.Size = new System.Drawing.Size(100, 20);
            this.distanceNUD.TabIndex = 7;
            this.distanceNUD.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(11, 32);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(160, 13);
            this.label4.TabIndex = 0;
            this.label4.Text = "Maximum distance from incident:";
            // 
            // CapWINForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(992, 700);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.cancelConfigurationBt);
            this.Controls.Add(this.saveConfigurationBt);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "CapWINForm";
            this.Text = "Cap WIN Configuration";
            this.Load += new System.EventHandler(this.CapWINForm_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.capWINErrorProvider)).EndInit();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.laneDataNUD)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.distanceNUD)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label capWinConnectStatusLb;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button testConnectionBt;
        private System.Windows.Forms.TextBox hostURLTb;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox passwordTb;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox usernameTb;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button saveConfigurationBt;
        private System.Windows.Forms.ComboBox comPortCb;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Button cancelConfigurationBt;
        private System.Windows.Forms.ErrorProvider capWINErrorProvider;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox baudRateTb;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.NumericUpDown distanceNUD;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.NumericUpDown laneDataNUD;
        private System.Windows.Forms.Label label3;
    }
}