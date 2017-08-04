namespace INCZONE.Forms.Configuration
{
    partial class MapDataForm
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
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.mapSetCB = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.refreshMapSetsBt = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // textBox1
            // 
            this.textBox1.Enabled = false;
            this.textBox1.Location = new System.Drawing.Point(488, 42);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(424, 199);
            this.textBox1.TabIndex = 0;
            // 
            // mapSetCB
            // 
            this.mapSetCB.FormattingEnabled = true;
            this.mapSetCB.Location = new System.Drawing.Point(110, 44);
            this.mapSetCB.Name = "mapSetCB";
            this.mapSetCB.Size = new System.Drawing.Size(326, 21);
            this.mapSetCB.TabIndex = 1;
            this.mapSetCB.SelectedIndexChanged += new System.EventHandler(this.mapSetCB_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(21, 52);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(83, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Select Map Set:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(485, 26);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(88, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Status Messages";
            // 
            // refreshMapSetsBt
            // 
            this.refreshMapSetsBt.Location = new System.Drawing.Point(110, 80);
            this.refreshMapSetsBt.Name = "refreshMapSetsBt";
            this.refreshMapSetsBt.Size = new System.Drawing.Size(99, 39);
            this.refreshMapSetsBt.TabIndex = 5;
            this.refreshMapSetsBt.Text = "Refresh Map Sets";
            this.refreshMapSetsBt.UseVisualStyleBackColor = true;
            this.refreshMapSetsBt.Click += new System.EventHandler(this.refreshMapSetsBt_Click);
            // 
            // MapDataForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(992, 700);
            this.Controls.Add(this.refreshMapSetsBt);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.mapSetCB);
            this.Controls.Add(this.textBox1);
            this.Name = "MapDataForm";
            this.Text = "Map Repository Configuration";
            this.Load += new System.EventHandler(this.MapDataForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.ComboBox mapSetCB;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button refreshMapSetsBt;

    }
}