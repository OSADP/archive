namespace AzureTestDriver.Views
{
    partial class I2VDisplayControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.lblActiveWorkerCount = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.lblTotalErrorCount = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.lblTotalSuccessCount = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.gridI2VStatus = new System.Windows.Forms.DataGridView();
            this.successCountDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.errorCountDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.activeDataGridViewCheckBoxColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.ViewResponses = new System.Windows.Forms.DataGridViewButtonColumn();
            this.i2VNetworkControllerBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridI2VStatus)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.i2VNetworkControllerBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.lblActiveWorkerCount);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.lblTotalErrorCount);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.lblTotalSuccessCount);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.gridI2VStatus);
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(523, 224);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "I2V";
            // 
            // lblActiveWorkerCount
            // 
            this.lblActiveWorkerCount.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblActiveWorkerCount.AutoSize = true;
            this.lblActiveWorkerCount.Location = new System.Drawing.Point(143, 198);
            this.lblActiveWorkerCount.Name = "lblActiveWorkerCount";
            this.lblActiveWorkerCount.Size = new System.Drawing.Size(10, 13);
            this.lblActiveWorkerCount.TabIndex = 9;
            this.lblActiveWorkerCount.Text = "-";
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(6, 198);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(131, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "Total Active Workers:";
            // 
            // lblTotalErrorCount
            // 
            this.lblTotalErrorCount.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblTotalErrorCount.AutoSize = true;
            this.lblTotalErrorCount.Location = new System.Drawing.Point(143, 185);
            this.lblTotalErrorCount.Name = "lblTotalErrorCount";
            this.lblTotalErrorCount.Size = new System.Drawing.Size(10, 13);
            this.lblTotalErrorCount.TabIndex = 7;
            this.lblTotalErrorCount.Text = "-";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(29, 185);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(108, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Total Error Count:";
            // 
            // lblTotalSuccessCount
            // 
            this.lblTotalSuccessCount.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblTotalSuccessCount.AutoSize = true;
            this.lblTotalSuccessCount.Location = new System.Drawing.Point(143, 172);
            this.lblTotalSuccessCount.Name = "lblTotalSuccessCount";
            this.lblTotalSuccessCount.Size = new System.Drawing.Size(10, 13);
            this.lblTotalSuccessCount.TabIndex = 5;
            this.lblTotalSuccessCount.Text = "-";
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(8, 172);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(129, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Total Success Count:";
            // 
            // gridI2VStatus
            // 
            this.gridI2VStatus.AllowUserToAddRows = false;
            this.gridI2VStatus.AllowUserToDeleteRows = false;
            this.gridI2VStatus.AllowUserToResizeColumns = false;
            this.gridI2VStatus.AllowUserToResizeRows = false;
            this.gridI2VStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gridI2VStatus.AutoGenerateColumns = false;
            this.gridI2VStatus.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridI2VStatus.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.successCountDataGridViewTextBoxColumn,
            this.errorCountDataGridViewTextBoxColumn,
            this.activeDataGridViewCheckBoxColumn,
            this.ViewResponses});
            this.gridI2VStatus.DataSource = this.i2VNetworkControllerBindingSource;
            this.gridI2VStatus.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.gridI2VStatus.Location = new System.Drawing.Point(6, 19);
            this.gridI2VStatus.MultiSelect = false;
            this.gridI2VStatus.Name = "gridI2VStatus";
            this.gridI2VStatus.ReadOnly = true;
            this.gridI2VStatus.RowHeadersVisible = false;
            this.gridI2VStatus.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this.gridI2VStatus.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.gridI2VStatus.Size = new System.Drawing.Size(511, 150);
            this.gridI2VStatus.TabIndex = 3;
            this.gridI2VStatus.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.gridI2VStatus_CellContentClick);
            this.gridI2VStatus.Paint += new System.Windows.Forms.PaintEventHandler(this.gridI2VStatus_Paint);
            // 
            // successCountDataGridViewTextBoxColumn
            // 
            this.successCountDataGridViewTextBoxColumn.DataPropertyName = "SuccessCount";
            this.successCountDataGridViewTextBoxColumn.HeaderText = "SuccessCount";
            this.successCountDataGridViewTextBoxColumn.Name = "successCountDataGridViewTextBoxColumn";
            this.successCountDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // errorCountDataGridViewTextBoxColumn
            // 
            this.errorCountDataGridViewTextBoxColumn.DataPropertyName = "ErrorCount";
            this.errorCountDataGridViewTextBoxColumn.HeaderText = "ErrorCount";
            this.errorCountDataGridViewTextBoxColumn.Name = "errorCountDataGridViewTextBoxColumn";
            this.errorCountDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // activeDataGridViewCheckBoxColumn
            // 
            this.activeDataGridViewCheckBoxColumn.DataPropertyName = "Active";
            this.activeDataGridViewCheckBoxColumn.HeaderText = "Active";
            this.activeDataGridViewCheckBoxColumn.Name = "activeDataGridViewCheckBoxColumn";
            this.activeDataGridViewCheckBoxColumn.ReadOnly = true;
            // 
            // ViewResponses
            // 
            this.ViewResponses.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.ViewResponses.HeaderText = "View Responses";
            this.ViewResponses.Name = "ViewResponses";
            this.ViewResponses.ReadOnly = true;
            this.ViewResponses.Text = "\"View Responses\"";
            // 
            // i2VNetworkControllerBindingSource
            // 
            this.i2VNetworkControllerBindingSource.DataSource = typeof(AzureTestDriver.I2V.I2VNetworkController);
            // 
            // I2VDisplayControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox1);
            this.Name = "I2VDisplayControl";
            this.Size = new System.Drawing.Size(529, 230);
            this.Load += new System.EventHandler(this.I2VDisplayControl_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridI2VStatus)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.i2VNetworkControllerBindingSource)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.DataGridView gridI2VStatus;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblTotalSuccessCount;
        private System.Windows.Forms.Label lblTotalErrorCount;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblActiveWorkerCount;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.BindingSource i2VNetworkControllerBindingSource;
        private System.Windows.Forms.DataGridViewTextBoxColumn successCountDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn errorCountDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewCheckBoxColumn activeDataGridViewCheckBoxColumn;
        private System.Windows.Forms.DataGridViewButtonColumn ViewResponses;
    }
}
