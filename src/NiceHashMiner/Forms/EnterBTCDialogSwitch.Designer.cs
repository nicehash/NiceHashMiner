namespace NiceHashMiner.Forms
{
    partial class EnterBTCDialogSwitch
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
            this.buttonOK = new System.Windows.Forms.Button();
            this.LinkButtonEnterBTCManually = new System.Windows.Forms.LinkLabel();
            this.LinkButtonRegister = new System.Windows.Forms.LinkLabel();
            this.SuspendLayout();
            // 
            // buttonOK
            // 
            this.buttonOK.Location = new System.Drawing.Point(110, 12);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(165, 35);
            this.buttonOK.TabIndex = 0;
            this.buttonOK.Text = "&Login with NiceHash";
            this.buttonOK.UseCompatibleTextRendering = true;
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.ButtonOK_Click);
            // 
            // LinkButtonEnterBTCManually
            // 
            this.LinkButtonEnterBTCManually.AutoSize = true;
            this.LinkButtonEnterBTCManually.Location = new System.Drawing.Point(112, 89);
            this.LinkButtonEnterBTCManually.Name = "LinkButtonEnterBTCManually";
            this.LinkButtonEnterBTCManually.Size = new System.Drawing.Size(168, 13);
            this.LinkButtonEnterBTCManually.TabIndex = 1;
            this.LinkButtonEnterBTCManually.TabStop = true;
            this.LinkButtonEnterBTCManually.Text = "&(or enter Bitcoin address manually)";
            this.LinkButtonEnterBTCManually.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkButtonEnterBTCManually_LinkClicked);
            // 
            // LinkButtonRegister
            // 
            this.LinkButtonRegister.AutoSize = true;
            this.LinkButtonRegister.Location = new System.Drawing.Point(97, 61);
            this.LinkButtonRegister.Name = "LinkButtonRegister";
            this.LinkButtonRegister.Size = new System.Drawing.Size(194, 13);
            this.LinkButtonRegister.TabIndex = 2;
            this.LinkButtonRegister.TabStop = true;
            this.LinkButtonRegister.Text = "&(Don\'t have an account? Register here)";
            this.LinkButtonRegister.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkButtonRegister_LinkClicked);
            // 
            // EnterBTCDialogSwitch
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(380, 130);
            this.Controls.Add(this.LinkButtonRegister);
            this.Controls.Add(this.LinkButtonEnterBTCManually);
            this.Controls.Add(this.buttonOK);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.SystemColors.InfoText;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "EnterBTCDialogSwitch";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = NHMProductInfo.Name;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.LinkLabel LinkButtonEnterBTCManually;
        private System.Windows.Forms.LinkLabel LinkButtonRegister;
    }
}
