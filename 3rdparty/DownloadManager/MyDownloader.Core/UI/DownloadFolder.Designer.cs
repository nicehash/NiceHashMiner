namespace MyDownloader.Core.UI
{
    partial class DownloadFolder
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
            this.btnSelAV = new System.Windows.Forms.Button();
            this.txtSaveTo = new System.Windows.Forms.TextBox();
            this.lblText = new System.Windows.Forms.Label();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.SuspendLayout();
            // 
            // btnSelAV
            // 
            this.btnSelAV.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelAV.Location = new System.Drawing.Point(321, 19);
            this.btnSelAV.Name = "btnSelAV";
            this.btnSelAV.Size = new System.Drawing.Size(26, 23);
            this.btnSelAV.TabIndex = 2;
            this.btnSelAV.Text = "...";
            this.btnSelAV.UseVisualStyleBackColor = true;
            this.btnSelAV.Click += new System.EventHandler(this.btnSelAV_Click);
            // 
            // txtSaveTo
            // 
            this.txtSaveTo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSaveTo.Location = new System.Drawing.Point(0, 19);
            this.txtSaveTo.Name = "txtSaveTo";
            this.txtSaveTo.Size = new System.Drawing.Size(319, 20);
            this.txtSaveTo.TabIndex = 1;
            // 
            // lblText
            // 
            this.lblText.AutoSize = true;
            this.lblText.Location = new System.Drawing.Point(0, 2);
            this.lblText.Name = "lblText";
            this.lblText.Size = new System.Drawing.Size(122, 13);
            this.lblText.TabIndex = 0;
            this.lblText.Text = "Default download folder:";
            // 
            // DownloadFolder
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnSelAV);
            this.Controls.Add(this.txtSaveTo);
            this.Controls.Add(this.lblText);
            this.Name = "DownloadFolder";
            this.Size = new System.Drawing.Size(347, 50);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnSelAV;
        private System.Windows.Forms.TextBox txtSaveTo;
        private System.Windows.Forms.Label lblText;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
    }
}
