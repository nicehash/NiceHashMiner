namespace NiceHashMiner.Forms
{
    partial class Form_AutoStartMining
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
            this.button_Cancel = new System.Windows.Forms.Button();
            this.label_msg = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(136, 126);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 23);
            this.button_Cancel.TabIndex = 0;
            this.button_Cancel.Text = "Cancel";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // label_msg
            // 
            this.label_msg.AutoSize = true;
            this.label_msg.Location = new System.Drawing.Point(12, 9);
            this.label_msg.MaximumSize = new System.Drawing.Size(320, 0);
            this.label_msg.Name = "label_msg";
            this.label_msg.Size = new System.Drawing.Size(54, 13);
            this.label_msg.TabIndex = 1;
            this.label_msg.Text = "label_msg";
            this.label_msg.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // timer1
            // 
            this.timer1.Interval = 1000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // Form_AutoStartMining
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(344, 161);
            this.Controls.Add(this.label_msg);
            this.Controls.Add(this.button_Cancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(360, 200);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(360, 200);
            this.Name = "Form_AutoStartMining";
            this.Text = "Form_AutoStartMining";
            this.TopMost = true;
            this.Shown += new System.EventHandler(this.Form_AutoStartMining_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Label label_msg;
        private System.Windows.Forms.Timer timer1;
    }
}