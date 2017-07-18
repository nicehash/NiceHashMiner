namespace NiceHashMiner.Forms.Components {
    partial class GroupProfitControl {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.groupBoxMinerGroup = new System.Windows.Forms.GroupBox();
            this.labelSpeedIndicator = new System.Windows.Forms.Label();
            this.labelCurentcyPerDayVaue = new System.Windows.Forms.Label();
            this.labelBTCRateValue = new System.Windows.Forms.Label();
            this.labelBTCRateIndicator = new System.Windows.Forms.Label();
            this.labelSpeedValue = new System.Windows.Forms.Label();
            this.groupBoxMinerGroup.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxMinerGroup
            // 
            this.groupBoxMinerGroup.Controls.Add(this.labelSpeedIndicator);
            this.groupBoxMinerGroup.Controls.Add(this.labelCurentcyPerDayVaue);
            this.groupBoxMinerGroup.Controls.Add(this.labelBTCRateValue);
            this.groupBoxMinerGroup.Controls.Add(this.labelBTCRateIndicator);
            this.groupBoxMinerGroup.Controls.Add(this.labelSpeedValue);
            this.groupBoxMinerGroup.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.groupBoxMinerGroup.Location = new System.Drawing.Point(0, 0);
            this.groupBoxMinerGroup.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBoxMinerGroup.Name = "groupBoxMinerGroup";
            this.groupBoxMinerGroup.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBoxMinerGroup.Size = new System.Drawing.Size(804, 49);
            this.groupBoxMinerGroup.TabIndex = 108;
            this.groupBoxMinerGroup.TabStop = false;
            this.groupBoxMinerGroup.Text = "Mining Devices { N/A } ";
            // 
            // labelSpeedIndicator
            // 
            this.labelSpeedIndicator.AutoSize = true;
            this.labelSpeedIndicator.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.labelSpeedIndicator.Location = new System.Drawing.Point(9, 25);
            this.labelSpeedIndicator.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelSpeedIndicator.Name = "labelSpeedIndicator";
            this.labelSpeedIndicator.Size = new System.Drawing.Size(67, 20);
            this.labelSpeedIndicator.TabIndex = 108;
            this.labelSpeedIndicator.Text = "Speed:";
            // 
            // labelCurentcyPerDayVaue
            // 
            this.labelCurentcyPerDayVaue.AutoSize = true;
            this.labelCurentcyPerDayVaue.Location = new System.Drawing.Point(651, 25);
            this.labelCurentcyPerDayVaue.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelCurentcyPerDayVaue.Name = "labelCurentcyPerDayVaue";
            this.labelCurentcyPerDayVaue.Size = new System.Drawing.Size(85, 20);
            this.labelCurentcyPerDayVaue.TabIndex = 104;
            this.labelCurentcyPerDayVaue.Text = "0.00 $/Day";
            // 
            // labelBTCRateValue
            // 
            this.labelBTCRateValue.AutoSize = true;
            this.labelBTCRateValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.labelBTCRateValue.Location = new System.Drawing.Point(475, 25);
            this.labelBTCRateValue.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelBTCRateValue.Name = "labelBTCRateValue";
            this.labelBTCRateValue.Size = new System.Drawing.Size(168, 20);
            this.labelBTCRateValue.TabIndex = 105;
            this.labelBTCRateValue.Text = "0.00000000 BTC/Day";
            // 
            // labelBTCRateIndicator
            // 
            this.labelBTCRateIndicator.AutoSize = true;
            this.labelBTCRateIndicator.Location = new System.Drawing.Point(419, 25);
            this.labelBTCRateIndicator.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelBTCRateIndicator.Name = "labelBTCRateIndicator";
            this.labelBTCRateIndicator.Size = new System.Drawing.Size(48, 20);
            this.labelBTCRateIndicator.TabIndex = 106;
            this.labelBTCRateIndicator.Text = "Rate:";
            // 
            // labelSpeedValue
            // 
            this.labelSpeedValue.AutoSize = true;
            this.labelSpeedValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.labelSpeedValue.Location = new System.Drawing.Point(84, 25);
            this.labelSpeedValue.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelSpeedValue.Name = "labelSpeedValue";
            this.labelSpeedValue.Size = new System.Drawing.Size(40, 20);
            this.labelSpeedValue.TabIndex = 107;
            this.labelSpeedValue.Text = "N/A";
            // 
            // GroupProfitControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.Controls.Add(this.groupBoxMinerGroup);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "GroupProfitControl";
            this.Size = new System.Drawing.Size(806, 55);
            this.groupBoxMinerGroup.ResumeLayout(false);
            this.groupBoxMinerGroup.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxMinerGroup;
        private System.Windows.Forms.Label labelSpeedIndicator;
        private System.Windows.Forms.Label labelCurentcyPerDayVaue;
        private System.Windows.Forms.Label labelBTCRateValue;
        private System.Windows.Forms.Label labelBTCRateIndicator;
        private System.Windows.Forms.Label labelSpeedValue;


    }
}
