namespace NiceHashMiner.Forms.Components {
    partial class BenchmarkLimitControl {
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.textBoxQuick = new System.Windows.Forms.TextBox();
            this.labelQuick = new System.Windows.Forms.Label();
            this.panel3 = new System.Windows.Forms.Panel();
            this.textBoxStandard = new System.Windows.Forms.TextBox();
            this.labelStandard = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.textBoxPrecise = new System.Windows.Forms.TextBox();
            this.labelPrecise = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.flowLayoutPanel1);
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox1.Size = new System.Drawing.Size(231, 144);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "groupName";
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.panel1);
            this.flowLayoutPanel1.Controls.Add(this.panel3);
            this.flowLayoutPanel1.Controls.Add(this.panel2);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(4, 19);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(4);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(223, 121);
            this.flowLayoutPanel1.TabIndex = 0;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.textBoxQuick);
            this.panel1.Controls.Add(this.labelQuick);
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(267, 39);
            this.panel1.TabIndex = 1;
            // 
            // textBoxQuick
            // 
            this.textBoxQuick.Location = new System.Drawing.Point(83, 7);
            this.textBoxQuick.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxQuick.Name = "textBoxQuick";
            this.textBoxQuick.Size = new System.Drawing.Size(132, 22);
            this.textBoxQuick.TabIndex = 0;
            this.textBoxQuick.TextChanged += new System.EventHandler(this.textBoxQuick_TextChanged);
            // 
            // labelQuick
            // 
            this.labelQuick.AutoSize = true;
            this.labelQuick.Location = new System.Drawing.Point(4, 11);
            this.labelQuick.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelQuick.Name = "labelQuick";
            this.labelQuick.Size = new System.Drawing.Size(44, 17);
            this.labelQuick.TabIndex = 384;
            this.labelQuick.Text = "Quick";
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.textBoxStandard);
            this.panel3.Controls.Add(this.labelStandard);
            this.panel3.Location = new System.Drawing.Point(0, 39);
            this.panel3.Margin = new System.Windows.Forms.Padding(0);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(267, 39);
            this.panel3.TabIndex = 2;
            // 
            // textBoxStandard
            // 
            this.textBoxStandard.Location = new System.Drawing.Point(83, 7);
            this.textBoxStandard.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxStandard.Name = "textBoxStandard";
            this.textBoxStandard.Size = new System.Drawing.Size(132, 22);
            this.textBoxStandard.TabIndex = 1;
            this.textBoxStandard.TextChanged += new System.EventHandler(this.textBoxStandard_TextChanged);
            // 
            // labelStandard
            // 
            this.labelStandard.AutoSize = true;
            this.labelStandard.Location = new System.Drawing.Point(4, 11);
            this.labelStandard.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelStandard.Name = "labelStandard";
            this.labelStandard.Size = new System.Drawing.Size(66, 17);
            this.labelStandard.TabIndex = 384;
            this.labelStandard.Text = "Standard";
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.textBoxPrecise);
            this.panel2.Controls.Add(this.labelPrecise);
            this.panel2.Location = new System.Drawing.Point(0, 78);
            this.panel2.Margin = new System.Windows.Forms.Padding(0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(267, 39);
            this.panel2.TabIndex = 3;
            // 
            // textBoxPrecise
            // 
            this.textBoxPrecise.Location = new System.Drawing.Point(83, 7);
            this.textBoxPrecise.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxPrecise.Name = "textBoxPrecise";
            this.textBoxPrecise.Size = new System.Drawing.Size(132, 22);
            this.textBoxPrecise.TabIndex = 2;
            this.textBoxPrecise.TextChanged += new System.EventHandler(this.textBoxPrecise_TextChanged);
            // 
            // labelPrecise
            // 
            this.labelPrecise.AutoSize = true;
            this.labelPrecise.Location = new System.Drawing.Point(4, 11);
            this.labelPrecise.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelPrecise.Name = "labelPrecise";
            this.labelPrecise.Size = new System.Drawing.Size(55, 17);
            this.labelPrecise.TabIndex = 384;
            this.labelPrecise.Text = "Precise";
            // 
            // BenchmarkLimitControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox1);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "BenchmarkLimitControl";
            this.Size = new System.Drawing.Size(239, 144);
            this.groupBox1.ResumeLayout(false);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TextBox textBoxQuick;
        private System.Windows.Forms.Label labelQuick;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.TextBox textBoxPrecise;
        private System.Windows.Forms.Label labelPrecise;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.TextBox textBoxStandard;
        private System.Windows.Forms.Label labelStandard;
    }
}
