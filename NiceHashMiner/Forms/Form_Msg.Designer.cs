namespace NiceHashMiner.Forms
{
    partial class Form_Msg
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.label_msg = new System.Windows.Forms.Label();
            this.panelBut = new System.Windows.Forms.Panel();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer1.IsSplitterFixed = true;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.label_msg);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.panelBut);
            this.splitContainer1.Size = new System.Drawing.Size(350, 84);
            this.splitContainer1.SplitterDistance = 39;
            this.splitContainer1.SplitterWidth = 1;
            this.splitContainer1.TabIndex = 4;
            // 
            // label_msg
            // 
            this.label_msg.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label_msg.AutoSize = true;
            this.label_msg.Location = new System.Drawing.Point(0, 0);
            this.label_msg.Margin = new System.Windows.Forms.Padding(30);
            this.label_msg.MaximumSize = new System.Drawing.Size(480, 0);
            this.label_msg.MinimumSize = new System.Drawing.Size(350, 1);
            this.label_msg.Name = "label_msg";
            this.label_msg.Padding = new System.Windows.Forms.Padding(10, 10, 10, 60);
            this.label_msg.Size = new System.Drawing.Size(350, 83);
            this.label_msg.TabIndex = 0;
            this.label_msg.Text = "label";
            // 
            // panelBut
            // 
            this.panelBut.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelBut.Location = new System.Drawing.Point(0, 0);
            this.panelBut.Margin = new System.Windows.Forms.Padding(0);
            this.panelBut.Name = "panelBut";
            this.panelBut.Size = new System.Drawing.Size(350, 44);
            this.panelBut.TabIndex = 4;
            this.panelBut.ControlAdded += new System.Windows.Forms.ControlEventHandler(this.panelBut_ControlAdded);
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // Form_Msg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(350, 84);
            this.ControlBox = false;
            this.Controls.Add(this.splitContainer1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form_Msg";
            this.Text = "Form_Msg";
            this.Shown += new System.EventHandler(this.Form_Msg_Shown);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Label label_msg;
        private System.Windows.Forms.Panel panelBut;
        private System.Windows.Forms.Timer timer1;
    }
}