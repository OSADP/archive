namespace INCZONE.Forms.Configuration
{
    partial class GeneralForm
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
            this.label1 = new System.Windows.Forms.Label();
            this.capWinIncidentsCB = new System.Windows.Forms.ComboBox();
            this.refreshCapWinIncidentsBt = new System.Windows.Forms.Button();
            this.selectLocationBt = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.bypassRadio = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.resetRadio = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.byPassCapWINBt = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.resetCapWINBt = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(33, 37);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(128, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Set Responder Location::";
            // 
            // capWinIncidentsCB
            // 
            this.capWinIncidentsCB.FormattingEnabled = true;
            this.capWinIncidentsCB.Location = new System.Drawing.Point(179, 29);
            this.capWinIncidentsCB.Name = "capWinIncidentsCB";
            this.capWinIncidentsCB.Size = new System.Drawing.Size(350, 21);
            this.capWinIncidentsCB.TabIndex = 1;
            // 
            // refreshCapWinIncidentsBt
            // 
            this.refreshCapWinIncidentsBt.Location = new System.Drawing.Point(548, 27);
            this.refreshCapWinIncidentsBt.Name = "refreshCapWinIncidentsBt";
            this.refreshCapWinIncidentsBt.Size = new System.Drawing.Size(106, 23);
            this.refreshCapWinIncidentsBt.TabIndex = 2;
            this.refreshCapWinIncidentsBt.Text = "Refresh Incidents";
            this.refreshCapWinIncidentsBt.UseVisualStyleBackColor = true;
            this.refreshCapWinIncidentsBt.Click += new System.EventHandler(this.refreshCapWinIncidentsBt_Click);
            // 
            // selectLocationBt
            // 
            this.selectLocationBt.Location = new System.Drawing.Point(179, 56);
            this.selectLocationBt.Name = "selectLocationBt";
            this.selectLocationBt.Size = new System.Drawing.Size(95, 23);
            this.selectLocationBt.TabIndex = 3;
            this.selectLocationBt.Text = "Select Location";
            this.selectLocationBt.UseVisualStyleBackColor = true;
            this.selectLocationBt.Click += new System.EventHandler(this.selectLocationBt_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(280, 56);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 4;
            this.button1.Text = "Reset";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // bypassRadio
            // 
            this.bypassRadio.Location = new System.Drawing.Point(179, 38);
            this.bypassRadio.Name = "bypassRadio";
            this.bypassRadio.Size = new System.Drawing.Size(95, 23);
            this.bypassRadio.TabIndex = 5;
            this.bypassRadio.Text = "Bypass";
            this.bypassRadio.UseVisualStyleBackColor = true;
            this.bypassRadio.Click += new System.EventHandler(this.bypassRadio_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(123, 48);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(38, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Radio:";
            // 
            // resetRadio
            // 
            this.resetRadio.Location = new System.Drawing.Point(280, 38);
            this.resetRadio.Name = "resetRadio";
            this.resetRadio.Size = new System.Drawing.Size(75, 23);
            this.resetRadio.TabIndex = 7;
            this.resetRadio.Text = "Reset";
            this.resetRadio.UseVisualStyleBackColor = true;
            this.resetRadio.Click += new System.EventHandler(this.resetRadio_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.selectLocationBt);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.capWinIncidentsCB);
            this.groupBox1.Controls.Add(this.refreshCapWinIncidentsBt);
            this.groupBox1.Controls.Add(this.button1);
            this.groupBox1.Location = new System.Drawing.Point(23, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(957, 100);
            this.groupBox1.TabIndex = 8;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Bypass DSRC Location";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.bypassRadio);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.resetRadio);
            this.groupBox2.Location = new System.Drawing.Point(23, 129);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(957, 100);
            this.groupBox2.TabIndex = 9;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "ByPass Radio Interface";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.byPassCapWINBt);
            this.groupBox3.Controls.Add(this.label3);
            this.groupBox3.Controls.Add(this.resetCapWINBt);
            this.groupBox3.Location = new System.Drawing.Point(23, 244);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(957, 100);
            this.groupBox3.TabIndex = 10;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "ByPass CapWIN";
            // 
            // byPassCapWINBt
            // 
            this.byPassCapWINBt.Location = new System.Drawing.Point(179, 37);
            this.byPassCapWINBt.Name = "byPassCapWINBt";
            this.byPassCapWINBt.Size = new System.Drawing.Size(95, 23);
            this.byPassCapWINBt.TabIndex = 8;
            this.byPassCapWINBt.Text = "Bypass";
            this.byPassCapWINBt.UseVisualStyleBackColor = true;
            this.byPassCapWINBt.Click += new System.EventHandler(this.byPassCapWIN_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(110, 47);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(51, 13);
            this.label3.TabIndex = 9;
            this.label3.Text = "CapWIN:";
            // 
            // resetCapWINBt
            // 
            this.resetCapWINBt.Location = new System.Drawing.Point(280, 37);
            this.resetCapWINBt.Name = "resetCapWINBt";
            this.resetCapWINBt.Size = new System.Drawing.Size(75, 23);
            this.resetCapWINBt.TabIndex = 10;
            this.resetCapWINBt.Text = "Reset";
            this.resetCapWINBt.UseVisualStyleBackColor = true;
            this.resetCapWINBt.Click += new System.EventHandler(this.resetCapWINBt_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(108, 549);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 11;
            this.button2.Text = "button2";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Visible = false;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // GeneralForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(992, 700);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "GeneralForm";
            this.Text = "General Configuration";
            this.Load += new System.EventHandler(this.GeneralForm_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox capWinIncidentsCB;
        private System.Windows.Forms.Button refreshCapWinIncidentsBt;
        private System.Windows.Forms.Button selectLocationBt;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button bypassRadio;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button resetRadio;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Button byPassCapWINBt;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button resetCapWINBt;
        private System.Windows.Forms.Button button2;
    }
}