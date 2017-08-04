namespace AzureTestDriver.Views
{
    partial class BsmDisplayControl
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
            this.lblEstimatedSuccessRate = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.lblEstimatedCompletionTime = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.lblActiveWorkerCount = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.lblTotalErrorCount = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.lblTotalSuccessCount = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.gridBsmStatus = new System.Windows.Forms.DataGridView();
            this.bsmNetworkControllerBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.successCountDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.errorCountDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.activeWorkerCountDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.EstimatedCompletionTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ViewResponses = new System.Windows.Forms.DataGridViewButtonColumn();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridBsmStatus)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.bsmNetworkControllerBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.lblEstimatedSuccessRate);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.lblEstimatedCompletionTime);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.lblActiveWorkerCount);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.lblTotalErrorCount);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.lblTotalSuccessCount);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.gridBsmStatus);
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(523, 224);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "BSM";
            // 
            // lblEstimatedSuccessRate
            // 
            this.lblEstimatedSuccessRate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblEstimatedSuccessRate.AutoSize = true;
            this.lblEstimatedSuccessRate.Location = new System.Drawing.Point(380, 185);
            this.lblEstimatedSuccessRate.Name = "lblEstimatedSuccessRate";
            this.lblEstimatedSuccessRate.Size = new System.Drawing.Size(10, 13);
            this.lblEstimatedSuccessRate.TabIndex = 13;
            this.lblEstimatedSuccessRate.Text = "-";
            // 
            // label6
            // 
            this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(225, 185);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(149, 13);
            this.label6.TabIndex = 12;
            this.label6.Text = "Estimated Success Rate:";
            // 
            // lblEstimatedCompletionTime
            // 
            this.lblEstimatedCompletionTime.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblEstimatedCompletionTime.AutoSize = true;
            this.lblEstimatedCompletionTime.Location = new System.Drawing.Point(380, 172);
            this.lblEstimatedCompletionTime.Name = "lblEstimatedCompletionTime";
            this.lblEstimatedCompletionTime.Size = new System.Drawing.Size(10, 13);
            this.lblEstimatedCompletionTime.TabIndex = 11;
            this.lblEstimatedCompletionTime.Text = "-";
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(211, 172);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(163, 13);
            this.label5.TabIndex = 10;
            this.label5.Text = "Estimated Completion Time:";
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
            // gridBsmStatus
            // 
            this.gridBsmStatus.AllowUserToAddRows = false;
            this.gridBsmStatus.AllowUserToDeleteRows = false;
            this.gridBsmStatus.AllowUserToResizeColumns = false;
            this.gridBsmStatus.AllowUserToResizeRows = false;
            this.gridBsmStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gridBsmStatus.AutoGenerateColumns = false;
            this.gridBsmStatus.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridBsmStatus.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.successCountDataGridViewTextBoxColumn,
            this.errorCountDataGridViewTextBoxColumn,
            this.activeWorkerCountDataGridViewTextBoxColumn,
            this.EstimatedCompletionTime,
            this.ViewResponses});
            this.gridBsmStatus.DataSource = this.bsmNetworkControllerBindingSource;
            this.gridBsmStatus.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.gridBsmStatus.Location = new System.Drawing.Point(6, 19);
            this.gridBsmStatus.MultiSelect = false;
            this.gridBsmStatus.Name = "gridBsmStatus";
            this.gridBsmStatus.ReadOnly = true;
            this.gridBsmStatus.RowHeadersVisible = false;
            this.gridBsmStatus.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this.gridBsmStatus.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.gridBsmStatus.Size = new System.Drawing.Size(511, 150);
            this.gridBsmStatus.TabIndex = 3;
            this.gridBsmStatus.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.gridBsmStatus_CellContentClick);
            this.gridBsmStatus.Paint += new System.Windows.Forms.PaintEventHandler(this.gridBsmStatus_Paint);
            // 
            // bsmNetworkControllerBindingSource
            // 
            this.bsmNetworkControllerBindingSource.DataSource = typeof(AzureTestDriver.BSM.BsmNetworkController);
            // 
            // successCountDataGridViewTextBoxColumn
            // 
            this.successCountDataGridViewTextBoxColumn.DataPropertyName = "SuccessCount";
            this.successCountDataGridViewTextBoxColumn.HeaderText = "Success";
            this.successCountDataGridViewTextBoxColumn.Name = "successCountDataGridViewTextBoxColumn";
            this.successCountDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // errorCountDataGridViewTextBoxColumn
            // 
            this.errorCountDataGridViewTextBoxColumn.DataPropertyName = "ErrorCount";
            this.errorCountDataGridViewTextBoxColumn.HeaderText = "Error";
            this.errorCountDataGridViewTextBoxColumn.Name = "errorCountDataGridViewTextBoxColumn";
            this.errorCountDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // activeWorkerCountDataGridViewTextBoxColumn
            // 
            this.activeWorkerCountDataGridViewTextBoxColumn.DataPropertyName = "ActiveWorkerCount";
            this.activeWorkerCountDataGridViewTextBoxColumn.HeaderText = "Active";
            this.activeWorkerCountDataGridViewTextBoxColumn.Name = "activeWorkerCountDataGridViewTextBoxColumn";
            this.activeWorkerCountDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // EstimatedCompletionTime
            // 
            this.EstimatedCompletionTime.DataPropertyName = "EstimatedCompletionTime";
            this.EstimatedCompletionTime.HeaderText = "Completion Time";
            this.EstimatedCompletionTime.Name = "EstimatedCompletionTime";
            this.EstimatedCompletionTime.ReadOnly = true;
            // 
            // ViewResponses
            // 
            this.ViewResponses.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.ViewResponses.HeaderText = "View Responses";
            this.ViewResponses.Name = "ViewResponses";
            this.ViewResponses.ReadOnly = true;
            // 
            // BsmDisplayControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox1);
            this.Name = "BsmDisplayControl";
            this.Size = new System.Drawing.Size(529, 230);
            this.Load += new System.EventHandler(this.BsmDisplayControl_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridBsmStatus)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.bsmNetworkControllerBindingSource)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.DataGridView gridBsmStatus;
        private System.Windows.Forms.BindingSource bsmNetworkControllerBindingSource;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblTotalSuccessCount;
        private System.Windows.Forms.Label lblTotalErrorCount;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblActiveWorkerCount;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label lblEstimatedCompletionTime;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label lblEstimatedSuccessRate;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.DataGridViewTextBoxColumn successCountDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn errorCountDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn activeWorkerCountDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn EstimatedCompletionTime;
        private System.Windows.Forms.DataGridViewButtonColumn ViewResponses;
    }
}
