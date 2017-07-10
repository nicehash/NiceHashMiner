namespace MyDownloader.Extension.Notifications.UI
{
    partial class XPBalloonOptions
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
            this.chkBallon = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.numDuration = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.numDuration)).BeginInit();
            this.SuspendLayout();
            // 
            // chkBallon
            // 
            this.chkBallon.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.chkBallon.AutoEllipsis = true;
            this.chkBallon.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.chkBallon.Location = new System.Drawing.Point(0, 3);
            this.chkBallon.Name = "chkBallon";
            this.chkBallon.Size = new System.Drawing.Size(294, 19);
            this.chkBallon.TabIndex = 0;
            this.chkBallon.Text = "Show XP ballon when download finishes";
            this.chkBallon.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.chkBallon.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(-3, 30);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(50, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Duration:";
            // 
            // numDuration
            // 
            this.numDuration.Location = new System.Drawing.Point(0, 46);
            this.numDuration.Name = "numDuration";
            this.numDuration.Size = new System.Drawing.Size(120, 20);
            this.numDuration.TabIndex = 2;
            // 
            // XPBalloonOptions
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.numDuration);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.chkBallon);
            this.Name = "XPBalloonOptions";
            this.Size = new System.Drawing.Size(294, 150);
            ((System.ComponentModel.ISupportInitialize)(this.numDuration)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox chkBallon;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown numDuration;
    }
}
