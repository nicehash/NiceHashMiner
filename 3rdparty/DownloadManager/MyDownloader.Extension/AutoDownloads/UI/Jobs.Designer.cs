namespace MyDownloader.Extension.AutoDownloads.UI
{
    partial class Jobs
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Jobs));
            MyDownloader.Extension.AutoDownloads.DayHourMatrix dayHourMatrix1 = new MyDownloader.Extension.AutoDownloads.DayHourMatrix();
            this.numMaxJobs = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.chkUseTime = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.pnlTime = new System.Windows.Forms.Panel();
            this.label5 = new System.Windows.Forms.Label();
            this.panel3 = new System.Windows.Forms.Panel();
            this.numMaxRate = new System.Windows.Forms.NumericUpDown();
            this.lblMaxRate = new System.Windows.Forms.Label();
            this.chkAutoStart = new System.Windows.Forms.CheckBox();
            this.timeGrid1 = new MyDownloader.Extension.AutoDownloads.UI.TimeGrid();
            ((System.ComponentModel.ISupportInitialize)(this.numMaxJobs)).BeginInit();
            this.pnlTime.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numMaxRate)).BeginInit();
            this.SuspendLayout();
            // 
            // numMaxJobs
            // 
            this.numMaxJobs.Location = new System.Drawing.Point(0, 41);
            this.numMaxJobs.Name = "numMaxJobs";
            this.numMaxJobs.Size = new System.Drawing.Size(96, 20);
            this.numMaxJobs.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(-3, 25);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(55, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Max Jobs:";
            // 
            // chkUseTime
            // 
            this.chkUseTime.AutoSize = true;
            this.chkUseTime.Location = new System.Drawing.Point(0, 77);
            this.chkUseTime.Name = "chkUseTime";
            this.chkUseTime.Size = new System.Drawing.Size(89, 17);
            this.chkUseTime.TabIndex = 3;
            this.chkUseTime.Text = "Work only at:";
            this.chkUseTime.UseVisualStyleBackColor = true;
            this.chkUseTime.CheckedChanged += new System.EventHandler(this.chkUseTime_CheckedChanged);
            // 
            // label1
            // 
            this.label1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.label1.Location = new System.Drawing.Point(0, 152);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(400, 75);
            this.label1.TabIndex = 20;
            this.label1.Text = resources.GetString("label1.Text");
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.Green;
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Location = new System.Drawing.Point(8, 8);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(16, 16);
            this.panel1.TabIndex = 21;
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.Gray;
            this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel2.Location = new System.Drawing.Point(230, 8);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(16, 16);
            this.panel2.TabIndex = 22;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(24, 8);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(55, 13);
            this.label3.TabIndex = 23;
            this.label3.Text = "Full speed";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(248, 8);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(44, 13);
            this.label4.TabIndex = 24;
            this.label4.Text = "Turn off";
            // 
            // pnlTime
            // 
            this.pnlTime.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlTime.Controls.Add(this.label5);
            this.pnlTime.Controls.Add(this.panel3);
            this.pnlTime.Controls.Add(this.panel1);
            this.pnlTime.Controls.Add(this.label4);
            this.pnlTime.Controls.Add(this.timeGrid1);
            this.pnlTime.Controls.Add(this.label3);
            this.pnlTime.Controls.Add(this.label1);
            this.pnlTime.Controls.Add(this.panel2);
            this.pnlTime.Location = new System.Drawing.Point(0, 101);
            this.pnlTime.Name = "pnlTime";
            this.pnlTime.Size = new System.Drawing.Size(400, 227);
            this.pnlTime.TabIndex = 4;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(136, 8);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(40, 13);
            this.label5.TabIndex = 26;
            this.label5.Text = "Limited";
            // 
            // panel3
            // 
            this.panel3.BackColor = System.Drawing.Color.PaleGreen;
            this.panel3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel3.Location = new System.Drawing.Point(117, 8);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(16, 16);
            this.panel3.TabIndex = 25;
            // 
            // numMaxRate
            // 
            this.numMaxRate.Location = new System.Drawing.Point(3, 344);
            this.numMaxRate.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numMaxRate.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numMaxRate.Name = "numMaxRate";
            this.numMaxRate.Size = new System.Drawing.Size(96, 20);
            this.numMaxRate.TabIndex = 6;
            this.numMaxRate.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // lblMaxRate
            // 
            this.lblMaxRate.AutoSize = true;
            this.lblMaxRate.Location = new System.Drawing.Point(0, 328);
            this.lblMaxRate.Name = "lblMaxRate";
            this.lblMaxRate.Size = new System.Drawing.Size(152, 13);
            this.lblMaxRate.TabIndex = 5;
            this.lblMaxRate.Text = "Limited Download Rate (kbps):";
            // 
            // chkAutoStart
            // 
            this.chkAutoStart.AutoSize = true;
            this.chkAutoStart.Location = new System.Drawing.Point(0, 0);
            this.chkAutoStart.Name = "chkAutoStart";
            this.chkAutoStart.Size = new System.Drawing.Size(196, 17);
            this.chkAutoStart.TabIndex = 0;
            this.chkAutoStart.Text = "Auto-start auto-downloads at startup";
            this.chkAutoStart.UseVisualStyleBackColor = true;
            // 
            // timeGrid1
            // 
            this.timeGrid1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.timeGrid1.Location = new System.Drawing.Point(0, 32);
            this.timeGrid1.Name = "timeGrid1";
            this.timeGrid1.SelectedTimes = dayHourMatrix1;
            this.timeGrid1.Size = new System.Drawing.Size(395, 120);
            this.timeGrid1.StartPosition = new System.Drawing.Point(32, 16);
            this.timeGrid1.TabIndex = 0;
            // 
            // Jobs
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.chkAutoStart);
            this.Controls.Add(this.numMaxRate);
            this.Controls.Add(this.lblMaxRate);
            this.Controls.Add(this.pnlTime);
            this.Controls.Add(this.chkUseTime);
            this.Controls.Add(this.numMaxJobs);
            this.Controls.Add(this.label2);
            this.Name = "Jobs";
            this.Size = new System.Drawing.Size(400, 403);
            ((System.ComponentModel.ISupportInitialize)(this.numMaxJobs)).EndInit();
            this.pnlTime.ResumeLayout(false);
            this.pnlTime.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numMaxRate)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.NumericUpDown numMaxJobs;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox chkUseTime;
        private TimeGrid timeGrid1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Panel pnlTime;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.NumericUpDown numMaxRate;
        private System.Windows.Forms.Label lblMaxRate;
        private System.Windows.Forms.CheckBox chkAutoStart;
    }
}
