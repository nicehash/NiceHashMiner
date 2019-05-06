namespace NiceHashMiner.Forms.Components
{
    partial class DevicesMainBoardDevicesListViewSpeedControl
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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.devicesMainBoard1 = new NiceHashMiner.Forms.Components.DevicesMainBoard();
            this.devicesListViewSpeedControl1 = new NiceHashMiner.Forms.Components.DevicesListViewSpeedControl();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.devicesMainBoard1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.devicesListViewSpeedControl1);
            this.splitContainer1.Size = new System.Drawing.Size(960, 423);
            this.splitContainer1.SplitterDistance = 228;
            this.splitContainer1.TabIndex = 0;
            // 
            // devicesMainBoard1
            // 
            this.devicesMainBoard1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.devicesMainBoard1.Location = new System.Drawing.Point(0, 0);
            this.devicesMainBoard1.Name = "devicesMainBoard1";
            this.devicesMainBoard1.Size = new System.Drawing.Size(960, 228);
            this.devicesMainBoard1.TabIndex = 0;
            // 
            // devicesListViewSpeedControl1
            // 
            this.devicesListViewSpeedControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.devicesListViewSpeedControl1.FirstColumnText = "Enabled";
            this.devicesListViewSpeedControl1.Location = new System.Drawing.Point(0, 0);
            this.devicesListViewSpeedControl1.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.devicesListViewSpeedControl1.Name = "devicesListViewSpeedControl1";
            this.devicesListViewSpeedControl1.SaveToGeneralConfig = false;
            this.devicesListViewSpeedControl1.Size = new System.Drawing.Size(960, 191);
            this.devicesListViewSpeedControl1.TabIndex = 0;
            // 
            // DevicesMainBoardDevicesListViewSpeedControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Name = "DevicesMainBoardDevicesListViewSpeedControl";
            this.Size = new System.Drawing.Size(960, 423);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

#endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private DevicesMainBoard devicesMainBoard1;
        private DevicesListViewSpeedControl devicesListViewSpeedControl1;
    }
}
