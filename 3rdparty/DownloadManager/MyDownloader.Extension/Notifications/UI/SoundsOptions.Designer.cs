namespace MyDownloader.Extension.Notifications.UI
{
    partial class SoundsOptions
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
            this.openFDlg = new System.Windows.Forms.OpenFileDialog();
            this.soundChooserEnded = new MyDownloader.Extension.Notifications.UI.SoundChooser();
            this.soundChooserRemoved = new MyDownloader.Extension.Notifications.UI.SoundChooser();
            this.soundChooserAdded = new MyDownloader.Extension.Notifications.UI.SoundChooser();
            this.SuspendLayout();
            // 
            // openFDlg
            // 
            this.openFDlg.Filter = "Wav Files (*.wav)|*.wav";
            // 
            // soundChooserEnded
            // 
            this.soundChooserEnded.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.soundChooserEnded.FileName = "";
            this.soundChooserEnded.Location = new System.Drawing.Point(3, 94);
            this.soundChooserEnded.Name = "soundChooserEnded";
            this.soundChooserEnded.Size = new System.Drawing.Size(365, 41);
            this.soundChooserEnded.TabIndex = 2;
            this.soundChooserEnded.Text = "Download Ended";
            // 
            // soundChooserRemoved
            // 
            this.soundChooserRemoved.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.soundChooserRemoved.FileName = "";
            this.soundChooserRemoved.Location = new System.Drawing.Point(3, 47);
            this.soundChooserRemoved.Name = "soundChooserRemoved";
            this.soundChooserRemoved.Size = new System.Drawing.Size(365, 41);
            this.soundChooserRemoved.TabIndex = 1;
            this.soundChooserRemoved.Text = "Download Removed";
            // 
            // soundChooserAdded
            // 
            this.soundChooserAdded.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.soundChooserAdded.FileName = "";
            this.soundChooserAdded.Location = new System.Drawing.Point(3, 0);
            this.soundChooserAdded.Name = "soundChooserAdded";
            this.soundChooserAdded.Size = new System.Drawing.Size(365, 41);
            this.soundChooserAdded.TabIndex = 0;
            this.soundChooserAdded.Text = "Download Added";
            // 
            // SoundsOptions
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.soundChooserEnded);
            this.Controls.Add(this.soundChooserRemoved);
            this.Controls.Add(this.soundChooserAdded);
            this.Name = "SoundsOptions";
            this.Size = new System.Drawing.Size(369, 226);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.OpenFileDialog openFDlg;
        private SoundChooser soundChooserAdded;
        private SoundChooser soundChooserRemoved;
        private SoundChooser soundChooserEnded;

    }
}
