namespace MyDownloader.Extension.AutoDownloads.UI
{
    partial class ScheduledDownloadEnabler
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ScheduledDownloadEnabler));
            this.chkDisableWorkOnlyAt = new System.Windows.Forms.CheckBox();
            this.numMaxJobs = new System.Windows.Forms.NumericUpDown();
            this.chkOverrideMaxActive = new System.Windows.Forms.CheckBox();
            this.chkStartScheduler = new System.Windows.Forms.CheckBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numMaxJobs)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // chkDisableWorkOnlyAt
            // 
            this.chkDisableWorkOnlyAt.AutoSize = true;
            this.chkDisableWorkOnlyAt.Location = new System.Drawing.Point(0, 72);
            this.chkDisableWorkOnlyAt.Name = "chkDisableWorkOnlyAt";
            this.chkDisableWorkOnlyAt.Size = new System.Drawing.Size(299, 17);
            this.chkDisableWorkOnlyAt.TabIndex = 3;
            this.chkDisableWorkOnlyAt.Text = "Disable \"Work only at\" configuration and work at any time";
            this.chkDisableWorkOnlyAt.UseVisualStyleBackColor = true;
            // 
            // numMaxJobs
            // 
            this.numMaxJobs.Location = new System.Drawing.Point(0, 46);
            this.numMaxJobs.Name = "numMaxJobs";
            this.numMaxJobs.Size = new System.Drawing.Size(96, 20);
            this.numMaxJobs.TabIndex = 2;
            // 
            // chkOverrideMaxActive
            // 
            this.chkOverrideMaxActive.AutoSize = true;
            this.chkOverrideMaxActive.Location = new System.Drawing.Point(0, 23);
            this.chkOverrideMaxActive.Name = "chkOverrideMaxActive";
            this.chkOverrideMaxActive.Size = new System.Drawing.Size(189, 17);
            this.chkOverrideMaxActive.TabIndex = 1;
            this.chkOverrideMaxActive.Text = "Override max active downloads to:";
            this.chkOverrideMaxActive.UseVisualStyleBackColor = true;
            this.chkOverrideMaxActive.CheckedChanged += new System.EventHandler(this.chkOverrideMaxActive_CheckedChanged);
            // 
            // chkStartScheduler
            // 
            this.chkStartScheduler.AutoSize = true;
            this.chkStartScheduler.Location = new System.Drawing.Point(0, 0);
            this.chkStartScheduler.Name = "chkStartScheduler";
            this.chkStartScheduler.Size = new System.Drawing.Size(126, 17);
            this.chkStartScheduler.TabIndex = 0;
            this.chkStartScheduler.Text = "Start auto-downloads";
            this.chkStartScheduler.UseVisualStyleBackColor = true;
            this.chkStartScheduler.CheckedChanged += new System.EventHandler(this.chkStartScheduler_CheckedChanged);
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.BackColor = System.Drawing.SystemColors.Info;
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.label2);
            this.panel1.Location = new System.Drawing.Point(0, 95);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(393, 66);
            this.panel1.TabIndex = 34;
            // 
            // label2
            // 
            this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label2.Location = new System.Drawing.Point(0, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(391, 64);
            this.label2.TabIndex = 0;
            this.label2.Text = resources.GetString("label2.Text");
            // 
            // ScheduledDownloadEnabler
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.chkDisableWorkOnlyAt);
            this.Controls.Add(this.numMaxJobs);
            this.Controls.Add(this.chkOverrideMaxActive);
            this.Controls.Add(this.chkStartScheduler);
            this.Controls.Add(this.panel1);
            this.Name = "ScheduledDownloadEnabler";
            this.Size = new System.Drawing.Size(393, 163);
            this.Load += new System.EventHandler(this.ScheduledDownloadEnabler_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numMaxJobs)).EndInit();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox chkDisableWorkOnlyAt;
        private System.Windows.Forms.NumericUpDown numMaxJobs;
        private System.Windows.Forms.CheckBox chkOverrideMaxActive;
        private System.Windows.Forms.CheckBox chkStartScheduler;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label2;
    }
}
