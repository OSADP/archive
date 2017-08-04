namespace INCZONE.Forms.Log
{
    partial class EventLogForm
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
            this.beginDate = new System.Windows.Forms.DateTimePicker();
            this.endDate = new System.Windows.Forms.DateTimePicker();
            this.beginDateLb = new System.Windows.Forms.Label();
            this.emdDateLb = new System.Windows.Forms.Label();
            this.filterGb = new System.Windows.Forms.GroupBox();
            this.logLevelCb = new System.Windows.Forms.ComboBox();
            this.logLevelLb = new System.Windows.Forms.Label();
            this.eventTypeLb = new System.Windows.Forms.Label();
            this.eventTypeCb = new System.Windows.Forms.ComboBox();
            this.logEventsGrid = new System.Windows.Forms.DataGridView();
            this.SystemId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.MaessageDate = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.EventMessage = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.EventType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.showAllEventsBt = new System.Windows.Forms.Button();
            this.filterEventsBt = new System.Windows.Forms.Button();
            this.filterGb.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.logEventsGrid)).BeginInit();
            this.SuspendLayout();
            // 
            // beginDate
            // 
            this.beginDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.beginDate.Location = new System.Drawing.Point(85, 18);
            this.beginDate.Name = "beginDate";
            this.beginDate.Size = new System.Drawing.Size(99, 20);
            this.beginDate.TabIndex = 0;
            this.beginDate.Value = new System.DateTime(2014, 3, 23, 0, 0, 0, 0);
            this.beginDate.ValueChanged += new System.EventHandler(this.beginDate_ValueChanged);
            // 
            // endDate
            // 
            this.endDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.endDate.Location = new System.Drawing.Point(300, 18);
            this.endDate.Name = "endDate";
            this.endDate.Size = new System.Drawing.Size(91, 20);
            this.endDate.TabIndex = 1;
            this.endDate.ValueChanged += new System.EventHandler(this.endDate_ValueChanged);
            // 
            // beginDateLb
            // 
            this.beginDateLb.AutoSize = true;
            this.beginDateLb.Location = new System.Drawing.Point(17, 24);
            this.beginDateLb.Name = "beginDateLb";
            this.beginDateLb.Size = new System.Drawing.Size(63, 13);
            this.beginDateLb.TabIndex = 2;
            this.beginDateLb.Text = "Begin Date:";
            // 
            // emdDateLb
            // 
            this.emdDateLb.AutoSize = true;
            this.emdDateLb.Location = new System.Drawing.Point(242, 24);
            this.emdDateLb.Name = "emdDateLb";
            this.emdDateLb.Size = new System.Drawing.Size(55, 13);
            this.emdDateLb.TabIndex = 3;
            this.emdDateLb.Text = "End Date:";
            // 
            // filterGb
            // 
            this.filterGb.Controls.Add(this.logLevelCb);
            this.filterGb.Controls.Add(this.logLevelLb);
            this.filterGb.Controls.Add(this.eventTypeLb);
            this.filterGb.Controls.Add(this.eventTypeCb);
            this.filterGb.Controls.Add(this.beginDate);
            this.filterGb.Controls.Add(this.emdDateLb);
            this.filterGb.Controls.Add(this.endDate);
            this.filterGb.Controls.Add(this.beginDateLb);
            this.filterGb.Location = new System.Drawing.Point(22, 12);
            this.filterGb.Name = "filterGb";
            this.filterGb.Size = new System.Drawing.Size(892, 138);
            this.filterGb.TabIndex = 4;
            this.filterGb.TabStop = false;
            this.filterGb.Text = "Filter";
            // 
            // logLevelCb
            // 
            this.logLevelCb.FormattingEnabled = true;
            this.logLevelCb.Location = new System.Drawing.Point(85, 92);
            this.logLevelCb.Name = "logLevelCb";
            this.logLevelCb.Size = new System.Drawing.Size(171, 21);
            this.logLevelCb.TabIndex = 5;
            this.logLevelCb.SelectedIndexChanged += new System.EventHandler(this.logLevelCb_SelectedIndexChanged);
            // 
            // logLevelLb
            // 
            this.logLevelLb.AutoSize = true;
            this.logLevelLb.Location = new System.Drawing.Point(23, 100);
            this.logLevelLb.Name = "logLevelLb";
            this.logLevelLb.Size = new System.Drawing.Size(57, 13);
            this.logLevelLb.TabIndex = 4;
            this.logLevelLb.Text = "Log Level:";
            // 
            // eventTypeLb
            // 
            this.eventTypeLb.AutoSize = true;
            this.eventTypeLb.Location = new System.Drawing.Point(15, 64);
            this.eventTypeLb.Name = "eventTypeLb";
            this.eventTypeLb.Size = new System.Drawing.Size(65, 13);
            this.eventTypeLb.TabIndex = 1;
            this.eventTypeLb.Text = "Event Type:";
            // 
            // eventTypeCb
            // 
            this.eventTypeCb.FormattingEnabled = true;
            this.eventTypeCb.Location = new System.Drawing.Point(85, 56);
            this.eventTypeCb.Name = "eventTypeCb";
            this.eventTypeCb.Size = new System.Drawing.Size(306, 21);
            this.eventTypeCb.TabIndex = 0;
            this.eventTypeCb.SelectedIndexChanged += new System.EventHandler(this.eventTypeCb_SelectedIndexChanged);
            // 
            // logEventsGrid
            // 
            this.logEventsGrid.AllowUserToAddRows = false;
            this.logEventsGrid.AllowUserToDeleteRows = false;
            this.logEventsGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.logEventsGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.SystemId,
            this.MaessageDate,
            this.EventMessage,
            this.EventType});
            this.logEventsGrid.Location = new System.Drawing.Point(22, 203);
            this.logEventsGrid.MultiSelect = false;
            this.logEventsGrid.Name = "logEventsGrid";
            this.logEventsGrid.ReadOnly = true;
            this.logEventsGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.logEventsGrid.Size = new System.Drawing.Size(892, 431);
            this.logEventsGrid.TabIndex = 5;
            this.logEventsGrid.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.logEventsGrid_CellDoubleClick);
            // 
            // SystemId
            // 
            this.SystemId.HeaderText = "System Id";
            this.SystemId.Name = "SystemId";
            this.SystemId.ReadOnly = true;
            // 
            // MaessageDate
            // 
            this.MaessageDate.HeaderText = "Date";
            this.MaessageDate.Name = "MaessageDate";
            this.MaessageDate.ReadOnly = true;
            // 
            // EventMessage
            // 
            this.EventMessage.HeaderText = "Event Message";
            this.EventMessage.MinimumWidth = 450;
            this.EventMessage.Name = "EventMessage";
            this.EventMessage.ReadOnly = true;
            this.EventMessage.Width = 450;
            // 
            // EventType
            // 
            this.EventType.HeaderText = "Event Type";
            this.EventType.MinimumWidth = 200;
            this.EventType.Name = "EventType";
            this.EventType.ReadOnly = true;
            this.EventType.Width = 200;
            // 
            // showAllEventsBt
            // 
            this.showAllEventsBt.Location = new System.Drawing.Point(22, 174);
            this.showAllEventsBt.Name = "showAllEventsBt";
            this.showAllEventsBt.Size = new System.Drawing.Size(75, 23);
            this.showAllEventsBt.TabIndex = 4;
            this.showAllEventsBt.Text = "Show All";
            this.showAllEventsBt.UseVisualStyleBackColor = true;
            this.showAllEventsBt.Click += new System.EventHandler(this.showAllEventsBt_Click);
            // 
            // filterEventsBt
            // 
            this.filterEventsBt.Location = new System.Drawing.Point(109, 174);
            this.filterEventsBt.Name = "filterEventsBt";
            this.filterEventsBt.Size = new System.Drawing.Size(95, 23);
            this.filterEventsBt.TabIndex = 8;
            this.filterEventsBt.Text = "Show By Filter";
            this.filterEventsBt.UseVisualStyleBackColor = true;
            this.filterEventsBt.Click += new System.EventHandler(this.filterEventsBt_Click);
            // 
            // EventLogForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(972, 702);
            this.Controls.Add(this.filterEventsBt);
            this.Controls.Add(this.showAllEventsBt);
            this.Controls.Add(this.logEventsGrid);
            this.Controls.Add(this.filterGb);
            this.Name = "EventLogForm";
            this.Text = "Event Log";
            this.filterGb.ResumeLayout(false);
            this.filterGb.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.logEventsGrid)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DateTimePicker beginDate;
        private System.Windows.Forms.DateTimePicker endDate;
        private System.Windows.Forms.Label beginDateLb;
        private System.Windows.Forms.Label emdDateLb;
        private System.Windows.Forms.GroupBox filterGb;
        private System.Windows.Forms.Label eventTypeLb;
        private System.Windows.Forms.ComboBox eventTypeCb;
        private System.Windows.Forms.DataGridView logEventsGrid;
        private System.Windows.Forms.Button showAllEventsBt;
        private System.Windows.Forms.Button filterEventsBt;
        private System.Windows.Forms.DataGridViewTextBoxColumn SystemId;
        private System.Windows.Forms.DataGridViewTextBoxColumn MaessageDate;
        private System.Windows.Forms.DataGridViewTextBoxColumn EventMessage;
        private System.Windows.Forms.DataGridViewTextBoxColumn EventType;
        private System.Windows.Forms.Label logLevelLb;
        private System.Windows.Forms.ComboBox logLevelCb;
    }
}