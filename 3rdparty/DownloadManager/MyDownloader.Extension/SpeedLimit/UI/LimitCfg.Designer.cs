namespace MyDownloader.Extension.SpeedLimit.UI
{
    partial class LimitCfg
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
            this.chkEnableLimit = new System.Windows.Forms.CheckBox();
            this.numMaxRate = new System.Windows.Forms.NumericUpDown();
            this.lblMaxRate = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numMaxRate)).BeginInit();
            this.SuspendLayout();
            // 
            // chkEnableLimit
            // 
            this.chkEnableLimit.AutoSize = true;
            this.chkEnableLimit.Location = new System.Drawing.Point(0, 0);
            this.chkEnableLimit.Name = "chkEnableLimit";
            this.chkEnableLimit.Size = new System.Drawing.Size(111, 17);
            this.chkEnableLimit.TabIndex = 0;
            this.chkEnableLimit.Text = "Enable speed limit";
            this.chkEnableLimit.UseVisualStyleBackColor = true;
            this.chkEnableLimit.CheckedChanged += new System.EventHandler(this.chkEnableLimit_CheckedChanged);
            // 
            // numMaxRate
            // 
            this.numMaxRate.Location = new System.Drawing.Point(0, 48);
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
            this.numMaxRate.TabIndex = 2;
            this.numMaxRate.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // lblMaxRate
            // 
            this.lblMaxRate.AutoSize = true;
            this.lblMaxRate.Location = new System.Drawing.Point(-3, 32);
            this.lblMaxRate.Name = "lblMaxRate";
            this.lblMaxRate.Size = new System.Drawing.Size(88, 13);
            this.lblMaxRate.TabIndex = 1;
            this.lblMaxRate.Text = "Max Rate (kbps):";
            // 
            // LimitCfg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.numMaxRate);
            this.Controls.Add(this.lblMaxRate);
            this.Controls.Add(this.chkEnableLimit);
            this.Name = "LimitCfg";
            this.Size = new System.Drawing.Size(192, 71);
            ((System.ComponentModel.ISupportInitialize)(this.numMaxRate)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox chkEnableLimit;
        private System.Windows.Forms.NumericUpDown numMaxRate;
        private System.Windows.Forms.Label lblMaxRate;
    }
}
