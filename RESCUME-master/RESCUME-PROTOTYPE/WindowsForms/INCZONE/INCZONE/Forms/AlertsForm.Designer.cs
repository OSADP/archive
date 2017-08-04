namespace INCZONE.Forms
{
    partial class AlertsForm
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
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.AlarmLevel1Panel = new System.Windows.Forms.Panel();
            this.level1Text = new System.Windows.Forms.TextBox();
            this.AlarmLevel2Panel = new System.Windows.Forms.Panel();
            this.level2Text = new System.Windows.Forms.TextBox();
            this.AlarmLevel3Panel = new System.Windows.Forms.Panel();
            this.level3Text = new System.Windows.Forms.TextBox();
            this.AlarmLevel4Panel = new System.Windows.Forms.Panel();
            this.level4Text = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.testVisualAlarm = new System.Windows.Forms.Button();
            this.testAudibleAlarm = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.alarmLevel = new System.Windows.Forms.ComboBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.testBackToBackThreat = new System.Windows.Forms.Button();
            this.textBox4 = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.threatLevel = new System.Windows.Forms.ListBox();
            this.TestThreat = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.incidentNameLabel = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.StopApp = new System.Windows.Forms.Button();
            this.StartApp = new System.Windows.Forms.Button();
            this.label12 = new System.Windows.Forms.Label();
            this.loadedMapSet = new System.Windows.Forms.Label();
            this.dsrcUnit = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.vitalUnit = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.capwinPort = new System.Windows.Forms.Label();
            this.label20 = new System.Windows.Forms.Label();
            this.dgpsStatus = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.capwinStatus = new System.Windows.Forms.Label();
            this.label19 = new System.Windows.Forms.Label();
            this.capwinMobileStatus = new System.Windows.Forms.Label();
            this.label22 = new System.Windows.Forms.Label();
            this.radioStatus = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.vitalStatus = new System.Windows.Forms.Label();
            this.label25 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            this.label21 = new System.Windows.Forms.Label();
            this.label23 = new System.Windows.Forms.Label();
            this.vehicleSelect = new System.Windows.Forms.ComboBox();
            this.label24 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.ledDSRC = new INCZONE.Common.Led();
            this.ledRadio = new INCZONE.Common.Led();
            this.ledCapWinMobile = new INCZONE.Common.Led();
            this.ledCapWin = new INCZONE.Common.Led();
            this.ledDGPS = new INCZONE.Common.Led();
            this.vitalConnectedLED = new INCZONE.Common.Led();
            this.zoneActiveLED = new INCZONE.Common.Led();
            this.AlarmLevel1Panel.SuspendLayout();
            this.AlarmLevel2Panel.SuspendLayout();
            this.AlarmLevel3Panel.SuspendLayout();
            this.AlarmLevel4Panel.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(12, 26);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(88, 20);
            this.label1.TabIndex = 5;
            this.label1.Text = "No Threat";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(248, 26);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(168, 20);
            this.label2.TabIndex = 6;
            this.label2.Text = "Impending Violation";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(484, 26);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(79, 20);
            this.label3.TabIndex = 7;
            this.label3.Text = "Violation";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(720, 26);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(76, 20);
            this.label4.TabIndex = 8;
            this.label4.Text = "Collision";
            // 
            // AlarmLevel1Panel
            // 
            this.AlarmLevel1Panel.BackColor = System.Drawing.Color.Chartreuse;
            this.AlarmLevel1Panel.Controls.Add(this.level1Text);
            this.AlarmLevel1Panel.Location = new System.Drawing.Point(15, 49);
            this.AlarmLevel1Panel.Name = "AlarmLevel1Panel";
            this.AlarmLevel1Panel.Size = new System.Drawing.Size(230, 247);
            this.AlarmLevel1Panel.TabIndex = 9;
            // 
            // level1Text
            // 
            this.level1Text.BackColor = System.Drawing.SystemColors.Control;
            this.level1Text.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.level1Text.Font = new System.Drawing.Font("Microsoft Sans Serif", 36F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.level1Text.ForeColor = System.Drawing.Color.Chartreuse;
            this.level1Text.Location = new System.Drawing.Point(14, 14);
            this.level1Text.Multiline = true;
            this.level1Text.Name = "level1Text";
            this.level1Text.ReadOnly = true;
            this.level1Text.Size = new System.Drawing.Size(203, 216);
            this.level1Text.TabIndex = 0;
            this.level1Text.Text = "\r\nNo\r\nThreat";
            this.level1Text.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // AlarmLevel2Panel
            // 
            this.AlarmLevel2Panel.BackColor = System.Drawing.Color.Yellow;
            this.AlarmLevel2Panel.Controls.Add(this.level2Text);
            this.AlarmLevel2Panel.Location = new System.Drawing.Point(251, 49);
            this.AlarmLevel2Panel.Name = "AlarmLevel2Panel";
            this.AlarmLevel2Panel.Size = new System.Drawing.Size(230, 247);
            this.AlarmLevel2Panel.TabIndex = 10;
            // 
            // level2Text
            // 
            this.level2Text.BackColor = System.Drawing.SystemColors.Control;
            this.level2Text.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.level2Text.Font = new System.Drawing.Font("Microsoft Sans Serif", 32.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.level2Text.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.level2Text.Location = new System.Drawing.Point(14, 15);
            this.level2Text.Multiline = true;
            this.level2Text.Name = "level2Text";
            this.level2Text.ReadOnly = true;
            this.level2Text.Size = new System.Drawing.Size(203, 216);
            this.level2Text.TabIndex = 1;
            this.level2Text.TabStop = false;
            this.level2Text.Text = "\r\nImpending\r\nViolation";
            this.level2Text.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // AlarmLevel3Panel
            // 
            this.AlarmLevel3Panel.BackColor = System.Drawing.Color.Orange;
            this.AlarmLevel3Panel.Controls.Add(this.level3Text);
            this.AlarmLevel3Panel.Location = new System.Drawing.Point(487, 49);
            this.AlarmLevel3Panel.Name = "AlarmLevel3Panel";
            this.AlarmLevel3Panel.Size = new System.Drawing.Size(230, 247);
            this.AlarmLevel3Panel.TabIndex = 11;
            // 
            // level3Text
            // 
            this.level3Text.BackColor = System.Drawing.SystemColors.Control;
            this.level3Text.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.level3Text.Font = new System.Drawing.Font("Microsoft Sans Serif", 36F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.level3Text.ForeColor = System.Drawing.Color.Orange;
            this.level3Text.Location = new System.Drawing.Point(14, 15);
            this.level3Text.Multiline = true;
            this.level3Text.Name = "level3Text";
            this.level3Text.ReadOnly = true;
            this.level3Text.Size = new System.Drawing.Size(203, 216);
            this.level3Text.TabIndex = 1;
            this.level3Text.Text = "\r\nVehicle\r\nViolation!";
            this.level3Text.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // AlarmLevel4Panel
            // 
            this.AlarmLevel4Panel.BackColor = System.Drawing.Color.Red;
            this.AlarmLevel4Panel.Controls.Add(this.level4Text);
            this.AlarmLevel4Panel.Location = new System.Drawing.Point(723, 49);
            this.AlarmLevel4Panel.Name = "AlarmLevel4Panel";
            this.AlarmLevel4Panel.Size = new System.Drawing.Size(229, 247);
            this.AlarmLevel4Panel.TabIndex = 12;
            // 
            // level4Text
            // 
            this.level4Text.BackColor = System.Drawing.SystemColors.Control;
            this.level4Text.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.level4Text.Font = new System.Drawing.Font("Microsoft Sans Serif", 36F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.level4Text.ForeColor = System.Drawing.Color.Red;
            this.level4Text.Location = new System.Drawing.Point(13, 15);
            this.level4Text.Multiline = true;
            this.level4Text.Name = "level4Text";
            this.level4Text.ReadOnly = true;
            this.level4Text.Size = new System.Drawing.Size(203, 216);
            this.level4Text.TabIndex = 1;
            this.level4Text.Text = "\r\nImminent\r\nCollision!";
            this.level4Text.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.AlarmLevel4Panel);
            this.groupBox1.Controls.Add(this.AlarmLevel3Panel);
            this.groupBox1.Controls.Add(this.AlarmLevel2Panel);
            this.groupBox1.Controls.Add(this.AlarmLevel1Panel);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.Location = new System.Drawing.Point(12, 78);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(968, 319);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Alarm Level Indicators";
            // 
            // testVisualAlarm
            // 
            this.testVisualAlarm.Location = new System.Drawing.Point(309, 35);
            this.testVisualAlarm.Name = "testVisualAlarm";
            this.testVisualAlarm.Size = new System.Drawing.Size(75, 23);
            this.testVisualAlarm.TabIndex = 10;
            this.testVisualAlarm.Text = "Test Visual";
            this.testVisualAlarm.UseVisualStyleBackColor = true;
            this.testVisualAlarm.Click += new System.EventHandler(this.testVisualAlarm_Click);
            // 
            // testAudibleAlarm
            // 
            this.testAudibleAlarm.Location = new System.Drawing.Point(219, 35);
            this.testAudibleAlarm.Name = "testAudibleAlarm";
            this.testAudibleAlarm.Size = new System.Drawing.Size(75, 23);
            this.testAudibleAlarm.TabIndex = 9;
            this.testAudibleAlarm.Text = "Test Audible";
            this.testAudibleAlarm.UseVisualStyleBackColor = true;
            this.testAudibleAlarm.Click += new System.EventHandler(this.testAudibleAlarm_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(24, 45);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(62, 13);
            this.label5.TabIndex = 8;
            this.label5.Text = "Alarm Level";
            // 
            // alarmLevel
            // 
            this.alarmLevel.FormattingEnabled = true;
            this.alarmLevel.Location = new System.Drawing.Point(92, 37);
            this.alarmLevel.Name = "alarmLevel";
            this.alarmLevel.Size = new System.Drawing.Size(121, 21);
            this.alarmLevel.TabIndex = 7;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.testBackToBackThreat);
            this.groupBox3.Controls.Add(this.textBox4);
            this.groupBox3.Controls.Add(this.label9);
            this.groupBox3.Controls.Add(this.label8);
            this.groupBox3.Controls.Add(this.label7);
            this.groupBox3.Controls.Add(this.textBox3);
            this.groupBox3.Controls.Add(this.textBox2);
            this.groupBox3.Controls.Add(this.textBox1);
            this.groupBox3.Controls.Add(this.threatLevel);
            this.groupBox3.Controls.Add(this.TestThreat);
            this.groupBox3.Controls.Add(this.label6);
            this.groupBox3.Controls.Add(this.testAudibleAlarm);
            this.groupBox3.Controls.Add(this.testVisualAlarm);
            this.groupBox3.Controls.Add(this.alarmLevel);
            this.groupBox3.Controls.Add(this.label5);
            this.groupBox3.Location = new System.Drawing.Point(998, 65);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(420, 214);
            this.groupBox3.TabIndex = 11;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Alert Testing";
            this.groupBox3.Visible = false;
            // 
            // testBackToBackThreat
            // 
            this.testBackToBackThreat.Location = new System.Drawing.Point(309, 118);
            this.testBackToBackThreat.Name = "testBackToBackThreat";
            this.testBackToBackThreat.Size = new System.Drawing.Size(109, 23);
            this.testBackToBackThreat.TabIndex = 22;
            this.testBackToBackThreat.Text = "Alarm Priority Test";
            this.testBackToBackThreat.UseVisualStyleBackColor = true;
            this.testBackToBackThreat.Click += new System.EventHandler(this.testBackToBackThreat_Click);
            // 
            // textBox4
            // 
            this.textBox4.Location = new System.Drawing.Point(258, 167);
            this.textBox4.Name = "textBox4";
            this.textBox4.Size = new System.Drawing.Size(36, 20);
            this.textBox4.TabIndex = 21;
            this.textBox4.Text = "0";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(216, 148);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(28, 13);
            this.label9.TabIndex = 20;
            this.label9.Text = "Cars";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(215, 122);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(28, 13);
            this.label8.TabIndex = 19;
            this.label8.Text = "Cars";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(216, 99);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(28, 13);
            this.label7.TabIndex = 18;
            this.label7.Text = "Cars";
            // 
            // textBox3
            // 
            this.textBox3.Location = new System.Drawing.Point(258, 141);
            this.textBox3.Name = "textBox3";
            this.textBox3.Size = new System.Drawing.Size(36, 20);
            this.textBox3.TabIndex = 17;
            this.textBox3.Text = "0";
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(257, 115);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(36, 20);
            this.textBox2.TabIndex = 16;
            this.textBox2.Text = "0";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(258, 89);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(35, 20);
            this.textBox1.TabIndex = 15;
            this.textBox1.Text = "0";
            // 
            // threatLevel
            // 
            this.threatLevel.FormattingEnabled = true;
            this.threatLevel.Location = new System.Drawing.Point(90, 89);
            this.threatLevel.Name = "threatLevel";
            this.threatLevel.SelectionMode = System.Windows.Forms.SelectionMode.MultiSimple;
            this.threatLevel.Size = new System.Drawing.Size(120, 108);
            this.threatLevel.TabIndex = 14;
            // 
            // TestThreat
            // 
            this.TestThreat.Location = new System.Drawing.Point(309, 89);
            this.TestThreat.Name = "TestThreat";
            this.TestThreat.Size = new System.Drawing.Size(75, 23);
            this.TestThreat.TabIndex = 13;
            this.TestThreat.Text = "Test Threat";
            this.TestThreat.UseVisualStyleBackColor = true;
            this.TestThreat.Click += new System.EventHandler(this.TestThreat_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(17, 99);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(67, 13);
            this.label6.TabIndex = 12;
            this.label6.Text = "Threat Level";
            // 
            // incidentNameLabel
            // 
            this.incidentNameLabel.AutoSize = true;
            this.incidentNameLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.incidentNameLabel.Location = new System.Drawing.Point(57, 22);
            this.incidentNameLabel.Name = "incidentNameLabel";
            this.incidentNameLabel.Size = new System.Drawing.Size(185, 25);
            this.incidentNameLabel.TabIndex = 13;
            this.incidentNameLabel.Text = "No Active Incident";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label11.Location = new System.Drawing.Point(54, 513);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(54, 20);
            this.label11.TabIndex = 15;
            this.label11.Text = "VITAL";
            // 
            // StopApp
            // 
            this.StopApp.Font = new System.Drawing.Font("Microsoft Sans Serif", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.StopApp.Location = new System.Drawing.Point(796, 9);
            this.StopApp.Name = "StopApp";
            this.StopApp.Size = new System.Drawing.Size(184, 70);
            this.StopApp.TabIndex = 17;
            this.StopApp.Text = "Stop";
            this.StopApp.UseVisualStyleBackColor = true;
            this.StopApp.Click += new System.EventHandler(this.StopApp_Click);
            // 
            // StartApp
            // 
            this.StartApp.Font = new System.Drawing.Font("Microsoft Sans Serif", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.StartApp.Location = new System.Drawing.Point(602, 9);
            this.StartApp.Name = "StartApp";
            this.StartApp.Size = new System.Drawing.Size(184, 70);
            this.StartApp.TabIndex = 16;
            this.StartApp.Text = "Start";
            this.StartApp.UseVisualStyleBackColor = true;
            this.StartApp.Click += new System.EventHandler(this.StartApp_Click);
            // 
            // label12
            // 
            this.label12.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label12.Location = new System.Drawing.Point(389, 431);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(167, 21);
            this.label12.TabIndex = 18;
            this.label12.Text = "DSRC Status:";
            this.label12.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // loadedMapSet
            // 
            this.loadedMapSet.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.loadedMapSet.Location = new System.Drawing.Point(170, 400);
            this.loadedMapSet.Name = "loadedMapSet";
            this.loadedMapSet.Size = new System.Drawing.Size(392, 21);
            this.loadedMapSet.TabIndex = 19;
            this.loadedMapSet.Text = "---";
            this.loadedMapSet.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // dsrcUnit
            // 
            this.dsrcUnit.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dsrcUnit.Location = new System.Drawing.Point(578, 431);
            this.dsrcUnit.Name = "dsrcUnit";
            this.dsrcUnit.Size = new System.Drawing.Size(111, 21);
            this.dsrcUnit.TabIndex = 21;
            this.dsrcUnit.Text = "---";
            this.dsrcUnit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label14
            // 
            this.label14.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label14.Location = new System.Drawing.Point(12, 400);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(156, 21);
            this.label14.TabIndex = 20;
            this.label14.Text = "Loaded Map Set:";
            this.label14.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // vitalUnit
            // 
            this.vitalUnit.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.vitalUnit.Location = new System.Drawing.Point(588, 593);
            this.vitalUnit.Name = "vitalUnit";
            this.vitalUnit.Size = new System.Drawing.Size(392, 21);
            this.vitalUnit.TabIndex = 23;
            this.vitalUnit.Text = "---";
            this.vitalUnit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.vitalUnit.Visible = false;
            // 
            // label16
            // 
            this.label16.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label16.Location = new System.Drawing.Point(379, 594);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(187, 21);
            this.label16.TabIndex = 22;
            this.label16.Text = "Vital Unit:";
            this.label16.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.label16.Visible = false;
            // 
            // capwinPort
            // 
            this.capwinPort.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.capwinPort.Location = new System.Drawing.Point(588, 635);
            this.capwinPort.Name = "capwinPort";
            this.capwinPort.Size = new System.Drawing.Size(392, 21);
            this.capwinPort.TabIndex = 27;
            this.capwinPort.Text = "---";
            this.capwinPort.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.capwinPort.Visible = false;
            // 
            // label20
            // 
            this.label20.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label20.Location = new System.Drawing.Point(379, 636);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(187, 21);
            this.label20.TabIndex = 26;
            this.label20.Text = "CapWIN Port:";
            this.label20.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.label20.Visible = false;
            // 
            // dgpsStatus
            // 
            this.dgpsStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dgpsStatus.Location = new System.Drawing.Point(202, 652);
            this.dgpsStatus.Name = "dgpsStatus";
            this.dgpsStatus.Size = new System.Drawing.Size(111, 21);
            this.dgpsStatus.TabIndex = 29;
            this.dgpsStatus.Text = "---";
            this.dgpsStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.dgpsStatus.Visible = false;
            // 
            // label15
            // 
            this.label15.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label15.Location = new System.Drawing.Point(13, 652);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(167, 21);
            this.label15.TabIndex = 28;
            this.label15.Text = "DGPS Status:";
            this.label15.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.label15.Visible = false;
            // 
            // capwinStatus
            // 
            this.capwinStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.capwinStatus.Location = new System.Drawing.Point(873, 452);
            this.capwinStatus.Name = "capwinStatus";
            this.capwinStatus.Size = new System.Drawing.Size(111, 21);
            this.capwinStatus.TabIndex = 31;
            this.capwinStatus.Text = "---";
            this.capwinStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label19
            // 
            this.label19.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label19.Location = new System.Drawing.Point(684, 452);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(167, 21);
            this.label19.TabIndex = 30;
            this.label19.Text = "CapWin Status:";
            this.label19.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // capwinMobileStatus
            // 
            this.capwinMobileStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.capwinMobileStatus.Location = new System.Drawing.Point(873, 431);
            this.capwinMobileStatus.Name = "capwinMobileStatus";
            this.capwinMobileStatus.Size = new System.Drawing.Size(111, 21);
            this.capwinMobileStatus.TabIndex = 33;
            this.capwinMobileStatus.Text = "---";
            this.capwinMobileStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label22
            // 
            this.label22.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label22.Location = new System.Drawing.Point(684, 431);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(167, 21);
            this.label22.TabIndex = 32;
            this.label22.Text = "CapWin Mobile Status:";
            this.label22.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // radioStatus
            // 
            this.radioStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.radioStatus.Location = new System.Drawing.Point(578, 452);
            this.radioStatus.Name = "radioStatus";
            this.radioStatus.Size = new System.Drawing.Size(111, 21);
            this.radioStatus.TabIndex = 35;
            this.radioStatus.Text = "---";
            this.radioStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label17
            // 
            this.label17.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label17.Location = new System.Drawing.Point(389, 452);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(167, 21);
            this.label17.TabIndex = 34;
            this.label17.Text = "Radio Status:";
            this.label17.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // vitalStatus
            // 
            this.vitalStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.vitalStatus.Location = new System.Drawing.Point(578, 473);
            this.vitalStatus.Name = "vitalStatus";
            this.vitalStatus.Size = new System.Drawing.Size(111, 21);
            this.vitalStatus.TabIndex = 39;
            this.vitalStatus.Text = "---";
            this.vitalStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label25
            // 
            this.label25.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label25.Location = new System.Drawing.Point(389, 473);
            this.label25.Name = "label25";
            this.label25.Size = new System.Drawing.Size(167, 21);
            this.label25.TabIndex = 38;
            this.label25.Text = "VITAL Status:";
            this.label25.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label10.Location = new System.Drawing.Point(48, 609);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(55, 20);
            this.label10.TabIndex = 41;
            this.label10.Text = "DGPS";
            this.label10.Visible = false;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label13.Location = new System.Drawing.Point(187, 472);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(65, 20);
            this.label13.TabIndex = 43;
            this.label13.Text = "CapWin";
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label18.Location = new System.Drawing.Point(187, 431);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(115, 20);
            this.label18.TabIndex = 45;
            this.label18.Text = "CapWin Mobile";
            // 
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label21.Location = new System.Drawing.Point(54, 472);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(51, 20);
            this.label21.TabIndex = 47;
            this.label21.Text = "Radio";
            // 
            // label23
            // 
            this.label23.AutoSize = true;
            this.label23.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label23.Location = new System.Drawing.Point(54, 431);
            this.label23.Name = "label23";
            this.label23.Size = new System.Drawing.Size(55, 20);
            this.label23.TabIndex = 49;
            this.label23.Text = "DSRC";
            // 
            // vehicleSelect
            // 
            this.vehicleSelect.FormattingEnabled = true;
            this.vehicleSelect.Items.AddRange(new object[] {
            "Caprice",
            "Cruze"});
            this.vehicleSelect.Location = new System.Drawing.Point(619, 512);
            this.vehicleSelect.Name = "vehicleSelect";
            this.vehicleSelect.Size = new System.Drawing.Size(217, 21);
            this.vehicleSelect.TabIndex = 50;
            this.vehicleSelect.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // label24
            // 
            this.label24.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label24.Location = new System.Drawing.Point(407, 513);
            this.label24.Name = "label24";
            this.label24.Size = new System.Drawing.Size(187, 21);
            this.label24.TabIndex = 51;
            this.label24.Text = "Vehicle:";
            this.label24.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // button1
            // 
            this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button1.Location = new System.Drawing.Point(619, 555);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(98, 35);
            this.button1.TabIndex = 52;
            this.button1.Text = "Alarm On";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button2.Location = new System.Drawing.Point(738, 555);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(98, 35);
            this.button2.TabIndex = 53;
            this.button2.Text = "Alarm Off";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // ledDSRC
            // 
            this.ledDSRC.Active = false;
            this.ledDSRC.BackColor = System.Drawing.Color.Transparent;
            this.ledDSRC.ColorOff = System.Drawing.Color.Red;
            this.ledDSRC.ColorOn = System.Drawing.Color.Chartreuse;
            this.ledDSRC.Location = new System.Drawing.Point(12, 424);
            this.ledDSRC.Name = "ledDSRC";
            this.ledDSRC.Size = new System.Drawing.Size(35, 35);
            this.ledDSRC.TabIndex = 48;
            this.ledDSRC.Text = "led1";
            // 
            // ledRadio
            // 
            this.ledRadio.Active = false;
            this.ledRadio.BackColor = System.Drawing.Color.Transparent;
            this.ledRadio.ColorOff = System.Drawing.Color.Red;
            this.ledRadio.ColorOn = System.Drawing.Color.Chartreuse;
            this.ledRadio.Location = new System.Drawing.Point(12, 465);
            this.ledRadio.Name = "ledRadio";
            this.ledRadio.Size = new System.Drawing.Size(35, 35);
            this.ledRadio.TabIndex = 46;
            this.ledRadio.Text = "led1";
            // 
            // ledCapWinMobile
            // 
            this.ledCapWinMobile.Active = false;
            this.ledCapWinMobile.BackColor = System.Drawing.Color.Transparent;
            this.ledCapWinMobile.ColorOff = System.Drawing.Color.Red;
            this.ledCapWinMobile.ColorOn = System.Drawing.Color.Chartreuse;
            this.ledCapWinMobile.Location = new System.Drawing.Point(145, 424);
            this.ledCapWinMobile.Name = "ledCapWinMobile";
            this.ledCapWinMobile.Size = new System.Drawing.Size(35, 35);
            this.ledCapWinMobile.TabIndex = 44;
            this.ledCapWinMobile.Text = "led1";
            // 
            // ledCapWin
            // 
            this.ledCapWin.Active = false;
            this.ledCapWin.BackColor = System.Drawing.Color.Transparent;
            this.ledCapWin.ColorOff = System.Drawing.Color.Red;
            this.ledCapWin.ColorOn = System.Drawing.Color.Chartreuse;
            this.ledCapWin.Location = new System.Drawing.Point(145, 465);
            this.ledCapWin.Name = "ledCapWin";
            this.ledCapWin.Size = new System.Drawing.Size(35, 35);
            this.ledCapWin.TabIndex = 42;
            this.ledCapWin.Text = "led1";
            // 
            // ledDGPS
            // 
            this.ledDGPS.Active = false;
            this.ledDGPS.BackColor = System.Drawing.Color.Transparent;
            this.ledDGPS.ColorOff = System.Drawing.Color.Red;
            this.ledDGPS.ColorOn = System.Drawing.Color.Chartreuse;
            this.ledDGPS.Location = new System.Drawing.Point(6, 602);
            this.ledDGPS.Name = "ledDGPS";
            this.ledDGPS.Size = new System.Drawing.Size(35, 35);
            this.ledDGPS.TabIndex = 40;
            this.ledDGPS.Text = "led1";
            this.ledDGPS.Visible = false;
            // 
            // vitalConnectedLED
            // 
            this.vitalConnectedLED.Active = false;
            this.vitalConnectedLED.BackColor = System.Drawing.Color.Transparent;
            this.vitalConnectedLED.ColorOff = System.Drawing.Color.Red;
            this.vitalConnectedLED.ColorOn = System.Drawing.Color.Chartreuse;
            this.vitalConnectedLED.Location = new System.Drawing.Point(12, 506);
            this.vitalConnectedLED.Name = "vitalConnectedLED";
            this.vitalConnectedLED.Size = new System.Drawing.Size(35, 35);
            this.vitalConnectedLED.TabIndex = 14;
            this.vitalConnectedLED.Text = "led1";
            // 
            // zoneActiveLED
            // 
            this.zoneActiveLED.Active = false;
            this.zoneActiveLED.BackColor = System.Drawing.Color.Transparent;
            this.zoneActiveLED.ColorOff = System.Drawing.Color.Black;
            this.zoneActiveLED.ColorOn = System.Drawing.Color.Chartreuse;
            this.zoneActiveLED.Location = new System.Drawing.Point(12, 14);
            this.zoneActiveLED.Name = "zoneActiveLED";
            this.zoneActiveLED.Size = new System.Drawing.Size(39, 39);
            this.zoneActiveLED.TabIndex = 12;
            // 
            // AlertsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1370, 700);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label24);
            this.Controls.Add(this.vehicleSelect);
            this.Controls.Add(this.label23);
            this.Controls.Add(this.ledDSRC);
            this.Controls.Add(this.label21);
            this.Controls.Add(this.ledRadio);
            this.Controls.Add(this.label18);
            this.Controls.Add(this.ledCapWinMobile);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.ledCapWin);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.ledDGPS);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.vitalStatus);
            this.Controls.Add(this.label25);
            this.Controls.Add(this.radioStatus);
            this.Controls.Add(this.label17);
            this.Controls.Add(this.capwinMobileStatus);
            this.Controls.Add(this.label22);
            this.Controls.Add(this.capwinStatus);
            this.Controls.Add(this.label19);
            this.Controls.Add(this.dgpsStatus);
            this.Controls.Add(this.label15);
            this.Controls.Add(this.capwinPort);
            this.Controls.Add(this.label20);
            this.Controls.Add(this.vitalUnit);
            this.Controls.Add(this.label16);
            this.Controls.Add(this.dsrcUnit);
            this.Controls.Add(this.label14);
            this.Controls.Add(this.loadedMapSet);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.StopApp);
            this.Controls.Add(this.StartApp);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.vitalConnectedLED);
            this.Controls.Add(this.incidentNameLabel);
            this.Controls.Add(this.zoneActiveLED);
            this.Controls.Add(this.groupBox1);
            this.Name = "AlertsForm";
            this.Text = "Alerts";
            this.Activated += new System.EventHandler(this.AlertsForm_Activated);
            this.Load += new System.EventHandler(this.AlertsForm_Load);
            this.AlarmLevel1Panel.ResumeLayout(false);
            this.AlarmLevel1Panel.PerformLayout();
            this.AlarmLevel2Panel.ResumeLayout(false);
            this.AlarmLevel2Panel.PerformLayout();
            this.AlarmLevel3Panel.ResumeLayout(false);
            this.AlarmLevel3Panel.PerformLayout();
            this.AlarmLevel4Panel.ResumeLayout(false);
            this.AlarmLevel4Panel.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Panel AlarmLevel1Panel;
        private System.Windows.Forms.Panel AlarmLevel2Panel;
        private System.Windows.Forms.Panel AlarmLevel3Panel;
        private System.Windows.Forms.Panel AlarmLevel4Panel;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button testVisualAlarm;
        private System.Windows.Forms.Button testAudibleAlarm;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox alarmLevel;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Button TestThreat;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.ListBox threatLevel;
        private System.Windows.Forms.TextBox textBox4;
        private System.Windows.Forms.Button testBackToBackThreat;
        private System.Windows.Forms.TextBox level1Text;
        private Common.Led zoneActiveLED;
        private System.Windows.Forms.Label incidentNameLabel;
        private System.Windows.Forms.TextBox level2Text;
        private System.Windows.Forms.TextBox level3Text;
        private System.Windows.Forms.TextBox level4Text;
        private Common.Led vitalConnectedLED;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Button StopApp;
        private System.Windows.Forms.Button StartApp;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label loadedMapSet;
        private System.Windows.Forms.Label dsrcUnit;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label vitalUnit;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Label capwinPort;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.Label dgpsStatus;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label capwinStatus;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.Label capwinMobileStatus;
        private System.Windows.Forms.Label label22;
        private System.Windows.Forms.Label radioStatus;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Label vitalStatus;
        private System.Windows.Forms.Label label25;
        private System.Windows.Forms.Label label10;
        private Common.Led ledDGPS;
        private System.Windows.Forms.Label label13;
        private Common.Led ledCapWin;
        private System.Windows.Forms.Label label18;
        private Common.Led ledCapWinMobile;
        private System.Windows.Forms.Label label21;
        private Common.Led ledRadio;
        private System.Windows.Forms.Label label23;
        private Common.Led ledDSRC;
        private System.Windows.Forms.ComboBox vehicleSelect;
        private System.Windows.Forms.Label label24;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;

    }
}