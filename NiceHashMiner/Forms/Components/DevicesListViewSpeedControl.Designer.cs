namespace NiceHashMiner.Forms.Components
{
    partial class DevicesListViewSpeedControl
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
            this.speedHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.secondarySpeedHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.profitHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SuspendLayout();
            // 
            // listViewDevices
            // 
            this.listViewDevices.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.speedHeader,
            this.secondarySpeedHeader,
            this.profitHeader});
            this.listViewDevices.Size = new System.Drawing.Size(1294, 582);
            // 
            // devicesHeader
            // 
            this.devicesHeader.Width = 300;
            // 
            // speedHeader
            // 
            this.speedHeader.DisplayIndex = 1;
            this.speedHeader.Text = "Speed";
            this.speedHeader.Width = 100;
            // 
            // secondarySpeedHeader
            // 
            this.secondarySpeedHeader.DisplayIndex = 2;
            this.secondarySpeedHeader.Text = "Secondary Speed";
            this.secondarySpeedHeader.Width = 100;
            // 
            // profitHeader
            // 
            this.profitHeader.DisplayIndex = 3;
            this.profitHeader.Text = "Profit";
            this.profitHeader.Width = 100;
            // 
            // DevicesListViewSpeedControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Name = "DevicesListViewSpeedControl";
            this.Size = new System.Drawing.Size(1294, 582);
            this.Controls.SetChildIndex(this.listViewDevices, 0);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ColumnHeader speedHeader;
        private System.Windows.Forms.ColumnHeader secondarySpeedHeader;
        private System.Windows.Forms.ColumnHeader profitHeader;
    }
}
