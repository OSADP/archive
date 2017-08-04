using INCZONE.Common;
namespace INCZONE.Forms.Configuration
{
    partial class BluetoothForm
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
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.BypassVital = new System.Windows.Forms.Button();
            this.SelectVital = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.vitalStatusLb = new System.Windows.Forms.Label();
            this.bluetoothGb = new System.Windows.Forms.GroupBox();
            this.SelectAradaBt = new System.Windows.Forms.Button();
            this.bluetoothLb = new System.Windows.Forms.Label();
            this.aradaStatusLb = new System.Windows.Forms.Label();
            this.comPortCb = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.groupBox2.SuspendLayout();
            this.bluetoothGb.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.comPortCb);
            this.groupBox2.Controls.Add(this.BypassVital);
            this.groupBox2.Controls.Add(this.SelectVital);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.vitalStatusLb);
            this.groupBox2.Location = new System.Drawing.Point(23, 129);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(892, 135);
            this.groupBox2.TabIndex = 12;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Vital Selected";
            // 
            // BypassVital
            // 
            this.BypassVital.Location = new System.Drawing.Point(335, 89);
            this.BypassVital.Name = "BypassVital";
            this.BypassVital.Size = new System.Drawing.Size(107, 23);
            this.BypassVital.TabIndex = 10;
            this.BypassVital.Text = "Bypass Vital";
            this.BypassVital.UseVisualStyleBackColor = true;
            this.BypassVital.Click += new System.EventHandler(this.BypassVital_Click);
            // 
            // SelectVital
            // 
            this.SelectVital.Location = new System.Drawing.Point(166, 89);
            this.SelectVital.Name = "SelectVital";
            this.SelectVital.Size = new System.Drawing.Size(98, 23);
            this.SelectVital.TabIndex = 9;
            this.SelectVital.Text = "Select";
            this.SelectVital.UseVisualStyleBackColor = true;
            this.SelectVital.Click += new System.EventHandler(this.SelectVital_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(44, 25);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(97, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Connection Status:";
            // 
            // vitalStatusLb
            // 
            this.vitalStatusLb.AutoSize = true;
            this.vitalStatusLb.Location = new System.Drawing.Point(163, 25);
            this.vitalStatusLb.Name = "vitalStatusLb";
            this.vitalStatusLb.Size = new System.Drawing.Size(53, 13);
            this.vitalStatusLb.TabIndex = 4;
            this.vitalStatusLb.Text = "Unknown";
            // 
            // bluetoothGb
            // 
            this.bluetoothGb.Controls.Add(this.label3);
            this.bluetoothGb.Controls.Add(this.SelectAradaBt);
            this.bluetoothGb.Controls.Add(this.bluetoothLb);
            this.bluetoothGb.Controls.Add(this.aradaStatusLb);
            this.bluetoothGb.Location = new System.Drawing.Point(23, 12);
            this.bluetoothGb.Name = "bluetoothGb";
            this.bluetoothGb.Size = new System.Drawing.Size(892, 101);
            this.bluetoothGb.TabIndex = 11;
            this.bluetoothGb.TabStop = false;
            this.bluetoothGb.Text = "DSRC Selected";
            // 
            // SelectAradaBt
            // 
            this.SelectAradaBt.Location = new System.Drawing.Point(166, 51);
            this.SelectAradaBt.Name = "SelectAradaBt";
            this.SelectAradaBt.Size = new System.Drawing.Size(98, 23);
            this.SelectAradaBt.TabIndex = 9;
            this.SelectAradaBt.Text = "Select";
            this.SelectAradaBt.UseVisualStyleBackColor = true;
            this.SelectAradaBt.Click += new System.EventHandler(this.SelectAradaBt_Click);
            // 
            // bluetoothLb
            // 
            this.bluetoothLb.AutoSize = true;
            this.bluetoothLb.Location = new System.Drawing.Point(44, 25);
            this.bluetoothLb.Name = "bluetoothLb";
            this.bluetoothLb.Size = new System.Drawing.Size(97, 13);
            this.bluetoothLb.TabIndex = 0;
            this.bluetoothLb.Text = "Connection Status:";
            // 
            // aradaStatusLb
            // 
            this.aradaStatusLb.AutoSize = true;
            this.aradaStatusLb.Location = new System.Drawing.Point(163, 25);
            this.aradaStatusLb.Name = "aradaStatusLb";
            this.aradaStatusLb.Size = new System.Drawing.Size(53, 13);
            this.aradaStatusLb.TabIndex = 4;
            this.aradaStatusLb.Text = "Unknown";
            // 
            // comPortCb
            // 
            this.comPortCb.FormattingEnabled = true;
            this.comPortCb.Location = new System.Drawing.Point(166, 53);
            this.comPortCb.Name = "comPortCb";
            this.comPortCb.Size = new System.Drawing.Size(98, 21);
            this.comPortCb.TabIndex = 11;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(65, 61);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(76, 13);
            this.label1.TabIndex = 12;
            this.label1.Text = "Vital Com Port:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(64, 61);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(77, 13);
            this.label3.TabIndex = 10;
            this.label3.Text = "DSRC Device:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(74, 99);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(67, 13);
            this.label4.TabIndex = 13;
            this.label4.Text = "Vital Device:";
            // 
            // BluetoothForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(992, 700);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.bluetoothGb);
            this.MinimizeBox = false;
            this.Name = "BluetoothForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Bluetooth Configuration";
            this.Load += new System.EventHandler(this.BluetoothForm_Load);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.bluetoothGb.ResumeLayout(false);
            this.bluetoothGb.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button BypassVital;
        private System.Windows.Forms.Button SelectVital;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label vitalStatusLb;
        private System.Windows.Forms.GroupBox bluetoothGb;
        private System.Windows.Forms.Button SelectAradaBt;
        private System.Windows.Forms.Label bluetoothLb;
        private System.Windows.Forms.Label aradaStatusLb;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comPortCb;
        private System.Windows.Forms.Label label3;

    }
}