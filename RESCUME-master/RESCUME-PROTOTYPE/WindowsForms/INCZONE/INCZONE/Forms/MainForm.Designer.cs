namespace INCZONE.Forms
{
    partial class MainForm
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
            this.StartApp = new System.Windows.Forms.Button();
            this.StopApp = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // StartApp
            // 
            this.StartApp.Font = new System.Drawing.Font("Microsoft Sans Serif", 48F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.StartApp.Location = new System.Drawing.Point(40, 111);
            this.StartApp.Name = "StartApp";
            this.StartApp.Size = new System.Drawing.Size(435, 250);
            this.StartApp.TabIndex = 0;
            this.StartApp.Text = "Start Inc-Zone";
            this.StartApp.UseVisualStyleBackColor = true;
            this.StartApp.Click += new System.EventHandler(this.StartApp_Click);
            // 
            // StopApp
            // 
            this.StopApp.Font = new System.Drawing.Font("Microsoft Sans Serif", 48F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.StopApp.Location = new System.Drawing.Point(526, 111);
            this.StopApp.Name = "StopApp";
            this.StopApp.Size = new System.Drawing.Size(435, 250);
            this.StopApp.TabIndex = 1;
            this.StopApp.Text = "Stop  Inc-Zone";
            this.StopApp.UseVisualStyleBackColor = true;
            this.StopApp.Click += new System.EventHandler(this.StopApp_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(992, 700);
            this.Controls.Add(this.StopApp);
            this.Controls.Add(this.StartApp);
            this.Name = "MainForm";
            this.Text = "IncZone Main";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button StartApp;
        private System.Windows.Forms.Button StopApp;
    }
}