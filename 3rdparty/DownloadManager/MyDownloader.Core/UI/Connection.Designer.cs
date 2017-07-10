namespace MyDownloader.Core.UI
{
    partial class Connection
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
            this.label1 = new System.Windows.Forms.Label();
            this.numRetryDelay = new System.Windows.Forms.NumericUpDown();
            this.label6 = new System.Windows.Forms.Label();
            this.numMinSegSize = new System.Windows.Forms.NumericUpDown();
            this.numMaxRetries = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.lblMinSize = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.numMaxSegments = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.numRetryDelay)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMinSegSize)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMaxRetries)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMaxSegments)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(-3, 95);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Retry Delay:";
            // 
            // numRetryDelay
            // 
            this.numRetryDelay.Location = new System.Drawing.Point(0, 112);
            this.numRetryDelay.Name = "numRetryDelay";
            this.numRetryDelay.Size = new System.Drawing.Size(96, 20);
            this.numRetryDelay.TabIndex = 6;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(-3, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(95, 13);
            this.label6.TabIndex = 0;
            this.label6.Text = "Min Segment Size:";
            // 
            // numMinSegSize
            // 
            this.numMinSegSize.Location = new System.Drawing.Point(0, 17);
            this.numMinSegSize.Maximum = new decimal(new int[] {
            2147483647,
            0,
            0,
            0});
            this.numMinSegSize.Name = "numMinSegSize";
            this.numMinSegSize.Size = new System.Drawing.Size(116, 20);
            this.numMinSegSize.TabIndex = 1;
            this.numMinSegSize.ValueChanged += new System.EventHandler(this.numMinSegSize_ValueChanged);
            // 
            // numMaxRetries
            // 
            this.numMaxRetries.Location = new System.Drawing.Point(0, 160);
            this.numMaxRetries.Name = "numMaxRetries";
            this.numMaxRetries.Size = new System.Drawing.Size(96, 20);
            this.numMaxRetries.TabIndex = 8;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(-3, 143);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(129, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "Max Retries (0 for infinite):";
            // 
            // lblMinSize
            // 
            this.lblMinSize.AutoSize = true;
            this.lblMinSize.Location = new System.Drawing.Point(122, 19);
            this.lblMinSize.Name = "lblMinSize";
            this.lblMinSize.Size = new System.Drawing.Size(54, 13);
            this.lblMinSize.TabIndex = 2;
            this.lblMinSize.Text = "lblMinSize";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(-3, 48);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(80, 13);
            this.label5.TabIndex = 3;
            this.label5.Text = "Max Segments:";
            // 
            // numMaxSegments
            // 
            this.numMaxSegments.Location = new System.Drawing.Point(0, 65);
            this.numMaxSegments.Maximum = new decimal(new int[] {
            2147483647,
            0,
            0,
            0});
            this.numMaxSegments.Name = "numMaxSegments";
            this.numMaxSegments.Size = new System.Drawing.Size(116, 20);
            this.numMaxSegments.TabIndex = 4;
            // 
            // Connection
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.label5);
            this.Controls.Add(this.numMaxSegments);
            this.Controls.Add(this.numMaxRetries);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lblMinSize);
            this.Controls.Add(this.numRetryDelay);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.numMinSegSize);
            this.Name = "Connection";
            this.Size = new System.Drawing.Size(363, 282);
            ((System.ComponentModel.ISupportInitialize)(this.numRetryDelay)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMinSegSize)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMaxRetries)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMaxSegments)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown numRetryDelay;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.NumericUpDown numMinSegSize;
        private System.Windows.Forms.NumericUpDown numMaxRetries;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label lblMinSize;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown numMaxSegments;
    }
}
