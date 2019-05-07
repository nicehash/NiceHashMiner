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
            this.profitHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.fiatHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SuspendLayout();
            // 
            // listViewDevices
            // 
            this.listViewDevices.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.speedHeader,
            this.profitHeader,
            this.fiatHeader});
            this.listViewDevices.Margin = new System.Windows.Forms.Padding(2);
            // 
            // devicesHeader
            // 
            this.devicesHeader.Width = 300;
            // 
            // speedHeader
            // 
            this.speedHeader.Text = "Speeds";
            this.speedHeader.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.speedHeader.Width = 100;
            // 
            // profitHeader
            // 
            this.profitHeader.Text = "mBTC/Day";
            this.profitHeader.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.profitHeader.Width = 100;
            // 
            // fiatHeader
            // 
            this.fiatHeader.Text = "USD/Day";
            this.fiatHeader.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.fiatHeader.Width = 100;
            // 
            // DevicesListViewSpeedControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.Name = "DevicesListViewSpeedControl";
            this.Controls.SetChildIndex(this.listViewDevices, 0);
            this.ResumeLayout(false);

        }

#endregion

        private System.Windows.Forms.ColumnHeader speedHeader;
        private System.Windows.Forms.ColumnHeader profitHeader;
        private System.Windows.Forms.ColumnHeader fiatHeader;
    }
}
