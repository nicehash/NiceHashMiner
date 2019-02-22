namespace NiceHashMiner.Forms
{
    partial class EnterBTCDialogSwitch
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
            this.buttonOK = new System.Windows.Forms.Button();
            this.linkToDriverDownloadPage = new System.Windows.Forms.LinkLabel();
            this.SuspendLayout();
            // 
            // buttonOK
            // 
            this.buttonOK.Location = new System.Drawing.Point(86, 12);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(117, 23);
            this.buttonOK.TabIndex = 0;
            this.buttonOK.Text = "&Login with NiceHash";
            this.buttonOK.UseCompatibleTextRendering = true;
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.ButtonOK_Click);
            // 
            // linkToDriverDownloadPage
            // 
            this.linkToDriverDownloadPage.AutoSize = true;
            this.linkToDriverDownloadPage.Location = new System.Drawing.Point(64, 52);
            this.linkToDriverDownloadPage.Name = "linkToDriverDownloadPage";
            this.linkToDriverDownloadPage.Size = new System.Drawing.Size(168, 13);
            this.linkToDriverDownloadPage.TabIndex = 1;
            this.linkToDriverDownloadPage.TabStop = true;
            this.linkToDriverDownloadPage.Text = "&(or enter Bitcoin address manually)";
            this.linkToDriverDownloadPage.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkToDriverDownloadPage_LinkClicked);
            // 
            // EnterBTCDialogSwitch
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(285, 81);
            this.Controls.Add(this.linkToDriverDownloadPage);
            this.Controls.Add(this.buttonOK);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.SystemColors.InfoText;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "EnterBTCDialogSwitch";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "NiceHash Miner Legacy";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.LinkLabel linkToDriverDownloadPage;
    }
}
