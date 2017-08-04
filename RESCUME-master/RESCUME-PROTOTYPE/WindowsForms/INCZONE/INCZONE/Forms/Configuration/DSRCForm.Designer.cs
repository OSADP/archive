using INCZONE.Common;
namespace INCZONE.Forms.Configuration
{
    partial class DSRCForm
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.timNUD = new System.Windows.Forms.NumericUpDown();
            this.evaNUD = new System.Windows.Forms.NumericUpDown();
            this.bsmNUD = new System.Windows.Forms.NumericUpDown();
            this.acmNUD = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.saveConfigBt = new System.Windows.Forms.Button();
            this.cancelConfigBt = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.dsrcConnectStatusLb = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.timNUD)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.evaNUD)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.bsmNUD)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.acmNUD)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.timNUD);
            this.groupBox1.Controls.Add(this.evaNUD);
            this.groupBox1.Controls.Add(this.bsmNUD);
            this.groupBox1.Controls.Add(this.acmNUD);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(29, 99);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(892, 203);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Broadcast Frequencies";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(303, 151);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(20, 13);
            this.label8.TabIndex = 11;
            this.label8.Text = "Hz";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(303, 115);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(20, 13);
            this.label7.TabIndex = 10;
            this.label7.Text = "Hz";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(303, 80);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(20, 13);
            this.label6.TabIndex = 9;
            this.label6.Text = "Hz";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(303, 45);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(20, 13);
            this.label5.TabIndex = 8;
            this.label5.Text = "Hz";
            // 
            // timNUD
            // 
            this.timNUD.Location = new System.Drawing.Point(177, 144);
            this.timNUD.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.timNUD.Name = "timNUD";
            this.timNUD.Size = new System.Drawing.Size(120, 20);
            this.timNUD.TabIndex = 4;
            this.timNUD.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // evaNUD
            // 
            this.evaNUD.Location = new System.Drawing.Point(177, 108);
            this.evaNUD.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.evaNUD.Name = "evaNUD";
            this.evaNUD.Size = new System.Drawing.Size(120, 20);
            this.evaNUD.TabIndex = 3;
            this.evaNUD.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // bsmNUD
            // 
            this.bsmNUD.Location = new System.Drawing.Point(177, 73);
            this.bsmNUD.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.bsmNUD.Name = "bsmNUD";
            this.bsmNUD.Size = new System.Drawing.Size(120, 20);
            this.bsmNUD.TabIndex = 2;
            this.bsmNUD.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // acmNUD
            // 
            this.acmNUD.Location = new System.Drawing.Point(177, 38);
            this.acmNUD.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.acmNUD.Name = "acmNUD";
            this.acmNUD.Size = new System.Drawing.Size(120, 20);
            this.acmNUD.TabIndex = 1;
            this.acmNUD.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(119, 151);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(29, 13);
            this.label4.TabIndex = 3;
            this.label4.Text = "TIM:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(119, 115);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(31, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "EVA:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(119, 80);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(33, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "BSM:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(119, 45);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(33, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "ACM:";
            // 
            // saveConfigBt
            // 
            this.saveConfigBt.Location = new System.Drawing.Point(29, 318);
            this.saveConfigBt.Name = "saveConfigBt";
            this.saveConfigBt.Size = new System.Drawing.Size(75, 23);
            this.saveConfigBt.TabIndex = 5;
            this.saveConfigBt.Text = "Save";
            this.saveConfigBt.UseVisualStyleBackColor = true;
            this.saveConfigBt.Click += new System.EventHandler(this.saveConfigBt_Click);
            // 
            // cancelConfigBt
            // 
            this.cancelConfigBt.Location = new System.Drawing.Point(119, 318);
            this.cancelConfigBt.Name = "cancelConfigBt";
            this.cancelConfigBt.Size = new System.Drawing.Size(75, 23);
            this.cancelConfigBt.TabIndex = 6;
            this.cancelConfigBt.Text = "Camcel";
            this.cancelConfigBt.UseVisualStyleBackColor = true;
            this.cancelConfigBt.Click += new System.EventHandler(this.cancelConfigBt_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.dsrcConnectStatusLb);
            this.groupBox2.Controls.Add(this.label10);
            this.groupBox2.Location = new System.Drawing.Point(29, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(892, 71);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "DSRC Status";
            // 
            // dsrcConnectStatusLb
            // 
            this.dsrcConnectStatusLb.AutoSize = true;
            this.dsrcConnectStatusLb.Location = new System.Drawing.Point(174, 32);
            this.dsrcConnectStatusLb.Name = "dsrcConnectStatusLb";
            this.dsrcConnectStatusLb.Size = new System.Drawing.Size(53, 13);
            this.dsrcConnectStatusLb.TabIndex = 1;
            this.dsrcConnectStatusLb.Text = UIConstants.STATUS_UNKNOWN;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(55, 32);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(97, 13);
            this.label10.TabIndex = 0;
            this.label10.Text = "Connection Status:";
            // 
            // DSRCForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(992, 700);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.cancelConfigBt);
            this.Controls.Add(this.saveConfigBt);
            this.Controls.Add(this.groupBox1);
            this.Name = "DSRCForm";
            this.Text = "DSRC Configuration";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.timNUD)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.evaNUD)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.bsmNUD)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.acmNUD)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown timNUD;
        private System.Windows.Forms.NumericUpDown evaNUD;
        private System.Windows.Forms.NumericUpDown bsmNUD;
        private System.Windows.Forms.NumericUpDown acmNUD;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button saveConfigBt;
        private System.Windows.Forms.Button cancelConfigBt;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label dsrcConnectStatusLb;
        private System.Windows.Forms.Label label10;
    }
}