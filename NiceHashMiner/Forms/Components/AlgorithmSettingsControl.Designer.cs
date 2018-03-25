namespace NiceHashMiner.Forms.Components {
    partial class AlgorithmSettingsControl {
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
            this.groupBoxSelectedAlgorithmSettings = new System.Windows.Forms.GroupBox();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.field_PowerUsage = new NiceHashMiner.Forms.Components.Field();
            this.fieldBoxBenchmarkSpeed = new NiceHashMiner.Forms.Components.Field();
            this.secondaryFieldBoxBenchmarkSpeed = new NiceHashMiner.Forms.Components.Field();
            this.groupBoxExtraLaunchParameters = new System.Windows.Forms.GroupBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.richTextBoxExtraLaunchParameters = new System.Windows.Forms.RichTextBox();
            this.groupBoxSelectedAlgorithmSettings.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.groupBoxExtraLaunchParameters.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBoxSelectedAlgorithmSettings
            // 
            this.groupBoxSelectedAlgorithmSettings.Controls.Add(this.flowLayoutPanel1);
            this.groupBoxSelectedAlgorithmSettings.Location = new System.Drawing.Point(6, 6);
            this.groupBoxSelectedAlgorithmSettings.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.groupBoxSelectedAlgorithmSettings.Name = "groupBoxSelectedAlgorithmSettings";
            this.groupBoxSelectedAlgorithmSettings.Padding = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.groupBoxSelectedAlgorithmSettings.Size = new System.Drawing.Size(458, 548);
            this.groupBoxSelectedAlgorithmSettings.TabIndex = 11;
            this.groupBoxSelectedAlgorithmSettings.TabStop = false;
            this.groupBoxSelectedAlgorithmSettings.Text = "Selected Algorithm Settings:";
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.field_PowerUsage);
            this.flowLayoutPanel1.Controls.Add(this.fieldBoxBenchmarkSpeed);
            this.flowLayoutPanel1.Controls.Add(this.secondaryFieldBoxBenchmarkSpeed);
            this.flowLayoutPanel1.Controls.Add(this.groupBoxExtraLaunchParameters);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(6, 30);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(446, 512);
            this.flowLayoutPanel1.TabIndex = 12;
            // 
            // field_PowerUsage
            // 
            this.field_PowerUsage.AutoSize = true;
            this.field_PowerUsage.BackColor = System.Drawing.Color.Transparent;
            this.field_PowerUsage.EntryText = "";
            this.field_PowerUsage.LabelText = "Power Usage (W):";
            this.field_PowerUsage.Location = new System.Drawing.Point(12, 12);
            this.field_PowerUsage.Margin = new System.Windows.Forms.Padding(12, 12, 12, 12);
            this.field_PowerUsage.Name = "field_PowerUsage";
            this.field_PowerUsage.Size = new System.Drawing.Size(440, 83);
            this.field_PowerUsage.TabIndex = 15;
            // 
            // fieldBoxBenchmarkSpeed
            // 
            this.fieldBoxBenchmarkSpeed.AutoSize = true;
            this.fieldBoxBenchmarkSpeed.BackColor = System.Drawing.Color.Transparent;
            this.fieldBoxBenchmarkSpeed.EntryText = "";
            this.fieldBoxBenchmarkSpeed.LabelText = "Benchmark Speed (H/s):";
            this.fieldBoxBenchmarkSpeed.Location = new System.Drawing.Point(12, 119);
            this.fieldBoxBenchmarkSpeed.Margin = new System.Windows.Forms.Padding(12, 12, 12, 12);
            this.fieldBoxBenchmarkSpeed.Name = "fieldBoxBenchmarkSpeed";
            this.fieldBoxBenchmarkSpeed.Size = new System.Drawing.Size(440, 83);
            this.fieldBoxBenchmarkSpeed.TabIndex = 1;
            // 
            // secondaryFieldBoxBenchmarkSpeed
            // 
            this.secondaryFieldBoxBenchmarkSpeed.AutoSize = true;
            this.secondaryFieldBoxBenchmarkSpeed.BackColor = System.Drawing.Color.Transparent;
            this.secondaryFieldBoxBenchmarkSpeed.EntryText = "";
            this.secondaryFieldBoxBenchmarkSpeed.LabelText = "Secondary Benchmark Speed (H/s):";
            this.secondaryFieldBoxBenchmarkSpeed.Location = new System.Drawing.Point(12, 226);
            this.secondaryFieldBoxBenchmarkSpeed.Margin = new System.Windows.Forms.Padding(12, 12, 12, 12);
            this.secondaryFieldBoxBenchmarkSpeed.Name = "secondaryFieldBoxBenchmarkSpeed";
            this.secondaryFieldBoxBenchmarkSpeed.Size = new System.Drawing.Size(440, 83);
            this.secondaryFieldBoxBenchmarkSpeed.TabIndex = 16;
            // 
            // groupBoxExtraLaunchParameters
            // 
            this.groupBoxExtraLaunchParameters.Controls.Add(this.pictureBox1);
            this.groupBoxExtraLaunchParameters.Controls.Add(this.richTextBoxExtraLaunchParameters);
            this.groupBoxExtraLaunchParameters.Location = new System.Drawing.Point(470, 6);
            this.groupBoxExtraLaunchParameters.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.groupBoxExtraLaunchParameters.Name = "groupBoxExtraLaunchParameters";
            this.groupBoxExtraLaunchParameters.Padding = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.groupBoxExtraLaunchParameters.Size = new System.Drawing.Size(434, 183);
            this.groupBoxExtraLaunchParameters.TabIndex = 14;
            this.groupBoxExtraLaunchParameters.TabStop = false;
            this.groupBoxExtraLaunchParameters.Text = "Extra Launch Parameters:";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::NiceHashMiner.Properties.Resources.info_black_18;
            this.pictureBox1.Location = new System.Drawing.Point(386, 0);
            this.pictureBox1.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(18, 18);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox1.TabIndex = 1;
            this.pictureBox1.TabStop = false;
            // 
            // richTextBoxExtraLaunchParameters
            // 
            this.richTextBoxExtraLaunchParameters.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBoxExtraLaunchParameters.Location = new System.Drawing.Point(6, 30);
            this.richTextBoxExtraLaunchParameters.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.richTextBoxExtraLaunchParameters.Name = "richTextBoxExtraLaunchParameters";
            this.richTextBoxExtraLaunchParameters.Size = new System.Drawing.Size(422, 147);
            this.richTextBoxExtraLaunchParameters.TabIndex = 0;
            this.richTextBoxExtraLaunchParameters.Text = "";
            // 
            // AlgorithmSettingsControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBoxSelectedAlgorithmSettings);
            this.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.Name = "AlgorithmSettingsControl";
            this.Size = new System.Drawing.Size(470, 560);
            this.groupBoxSelectedAlgorithmSettings.ResumeLayout(false);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.groupBoxExtraLaunchParameters.ResumeLayout(false);
            this.groupBoxExtraLaunchParameters.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxSelectedAlgorithmSettings;
        private System.Windows.Forms.GroupBox groupBoxExtraLaunchParameters;
        private System.Windows.Forms.RichTextBox richTextBoxExtraLaunchParameters;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private Field fieldBoxBenchmarkSpeed;
        private Field secondaryFieldBoxBenchmarkSpeed;
        private Field field_PowerUsage;
        private System.Windows.Forms.PictureBox pictureBox1;
    }
}
