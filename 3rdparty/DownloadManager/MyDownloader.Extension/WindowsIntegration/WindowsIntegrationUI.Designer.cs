namespace MyDownloader.Extension.WindowsIntegration
{
    partial class WindowsIntegrationUI
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
            this.chkStartWithWindows = new System.Windows.Forms.CheckBox();
            this.chkMonitorWindowsClipboard = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // chkStartWithWindows
            // 
            this.chkStartWithWindows.AutoSize = true;
            this.chkStartWithWindows.Location = new System.Drawing.Point(0, 0);
            this.chkStartWithWindows.Name = "chkStartWithWindows";
            this.chkStartWithWindows.Size = new System.Drawing.Size(191, 17);
            this.chkStartWithWindows.TabIndex = 0;
            this.chkStartWithWindows.Text = "Start MyDownloader with Windows";
            this.chkStartWithWindows.UseVisualStyleBackColor = true;
            // 
            // chkMonitorWindowsClipboard
            // 
            this.chkMonitorWindowsClipboard.AutoSize = true;
            this.chkMonitorWindowsClipboard.Location = new System.Drawing.Point(0, 16);
            this.chkMonitorWindowsClipboard.Name = "chkMonitorWindowsClipboard";
            this.chkMonitorWindowsClipboard.Size = new System.Drawing.Size(155, 17);
            this.chkMonitorWindowsClipboard.TabIndex = 1;
            this.chkMonitorWindowsClipboard.Text = "Monitor Windows Clipboard";
            this.chkMonitorWindowsClipboard.UseVisualStyleBackColor = true;
            // 
            // WindowsIntegrationUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.chkMonitorWindowsClipboard);
            this.Controls.Add(this.chkStartWithWindows);
            this.Name = "WindowsIntegrationUI";
            this.Size = new System.Drawing.Size(429, 111);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox chkStartWithWindows;
        private System.Windows.Forms.CheckBox chkMonitorWindowsClipboard;
    }
}
