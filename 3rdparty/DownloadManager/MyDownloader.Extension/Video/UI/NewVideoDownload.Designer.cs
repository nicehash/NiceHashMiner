namespace MyDownloader.Extension.Video.UI
{
    partial class NewVideoDownload
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
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.txtFilename = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.chkStartNow = new System.Windows.Forms.CheckBox();
            this.txtURL = new System.Windows.Forms.TextBox();
            this.numSegments = new System.Windows.Forms.NumericUpDown();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.lblSites = new System.Windows.Forms.LinkLabel();
            this.videoFormatCtrl1 = new MyDownloader.Extension.Video.UI.VideoFormatCtrl();
            this.downloadFolder1 = new MyDownloader.Core.UI.DownloadFolder();
            this.waitControl1 = new MyDownloader.Core.UI.WaitControl();
            ((System.ComponentModel.ISupportInitialize)(this.numSegments)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(392, 246);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 12;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Enabled = false;
            this.btnOK.Location = new System.Drawing.Point(312, 246);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 11;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // txtFilename
            // 
            this.txtFilename.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtFilename.Location = new System.Drawing.Point(8, 171);
            this.txtFilename.Name = "txtFilename";
            this.txtFilename.Size = new System.Drawing.Size(456, 20);
            this.txtFilename.TabIndex = 8;
            this.txtFilename.Leave += new System.EventHandler(this.txtFilename_Leave);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(8, 155);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(55, 13);
            this.label4.TabIndex = 7;
            this.label4.Text = "File name:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(5, 56);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(54, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Segments";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(8, 8);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(311, 13);
            this.label9.TabIndex = 0;
            this.label9.Text = "URL (e.g: http://www.youtube.com/watch?v=AdPWWDkKS8s)";
            // 
            // chkStartNow
            // 
            this.chkStartNow.AutoSize = true;
            this.chkStartNow.Location = new System.Drawing.Point(116, 74);
            this.chkStartNow.Name = "chkStartNow";
            this.chkStartNow.Size = new System.Drawing.Size(73, 17);
            this.chkStartNow.TabIndex = 5;
            this.chkStartNow.Text = "Start Now";
            this.chkStartNow.UseVisualStyleBackColor = true;
            // 
            // txtURL
            // 
            this.txtURL.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtURL.Location = new System.Drawing.Point(8, 24);
            this.txtURL.Name = "txtURL";
            this.txtURL.Size = new System.Drawing.Size(456, 20);
            this.txtURL.TabIndex = 1;
            this.txtURL.TextChanged += new System.EventHandler(this.txtURL_TextChanged);
            // 
            // numSegments
            // 
            this.numSegments.Location = new System.Drawing.Point(8, 72);
            this.numSegments.Name = "numSegments";
            this.numSegments.Size = new System.Drawing.Size(75, 20);
            this.numSegments.TabIndex = 4;
            this.numSegments.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            // 
            // pictureBox1
            // 
            this.pictureBox1.InitialImage = null;
            this.pictureBox1.Location = new System.Drawing.Point(192, 46);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(272, 72);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pictureBox1.TabIndex = 36;
            this.pictureBox1.TabStop = false;
            // 
            // lblSites
            // 
            this.lblSites.AutoSize = true;
            this.lblSites.Location = new System.Drawing.Point(384, 8);
            this.lblSites.Name = "lblSites";
            this.lblSites.Size = new System.Drawing.Size(80, 13);
            this.lblSites.TabIndex = 2;
            this.lblSites.TabStop = true;
            this.lblSites.Text = "Supported sites";
            this.lblSites.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lblSites_LinkClicked);
            // 
            // videoFormatCtrl1
            // 
            this.videoFormatCtrl1.Location = new System.Drawing.Point(8, 112);
            this.videoFormatCtrl1.Name = "videoFormatCtrl1";
            this.videoFormatCtrl1.Size = new System.Drawing.Size(208, 40);
            this.videoFormatCtrl1.TabIndex = 6;
            this.videoFormatCtrl1.VideoFormat = MyDownloader.Extension.Video.VideoFormat.None;
            // 
            // downloadFolder1
            // 
            this.downloadFolder1.LabelText = "Save to:";
            this.downloadFolder1.Location = new System.Drawing.Point(8, 197);
            this.downloadFolder1.Name = "downloadFolder1";
            this.downloadFolder1.Size = new System.Drawing.Size(456, 43);
            this.downloadFolder1.TabIndex = 9;
            // 
            // waitControl1
            // 
            this.waitControl1.Location = new System.Drawing.Point(8, 248);
            this.waitControl1.Name = "waitControl1";
            this.waitControl1.Size = new System.Drawing.Size(288, 16);
            this.waitControl1.TabIndex = 37;
            this.waitControl1.Text = "Getting video title...";
            this.waitControl1.Visible = false;
            // 
            // NewVideoDownload
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(471, 273);
            this.Controls.Add(this.waitControl1);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.videoFormatCtrl1);
            this.Controls.Add(this.lblSites);
            this.Controls.Add(this.txtFilename);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.chkStartNow);
            this.Controls.Add(this.txtURL);
            this.Controls.Add(this.numSegments);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.downloadFolder1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "NewVideoDownload";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "New Video Download";
            this.Load += new System.EventHandler(this.NewVideoDownload_Load);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.NewVideoDownload_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.numSegments)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.TextBox txtFilename;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.CheckBox chkStartNow;
        private System.Windows.Forms.TextBox txtURL;
        private System.Windows.Forms.NumericUpDown numSegments;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.LinkLabel lblSites;
        private VideoFormatCtrl videoFormatCtrl1;
        private MyDownloader.Core.UI.DownloadFolder downloadFolder1;
        private MyDownloader.Core.UI.WaitControl waitControl1;
    }
}