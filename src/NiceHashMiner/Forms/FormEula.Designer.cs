namespace NiceHashMiner.Forms
{
    partial class FormEula
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
            this.richTextBoxToS = new System.Windows.Forms.RichTextBox();
            this.buttonAcceptToS = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // richTextBoxToS
            // 
            this.richTextBoxToS.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.richTextBoxToS.Location = new System.Drawing.Point(0, 0);
            this.richTextBoxToS.Name = "richTextBoxToS";
            this.richTextBoxToS.Size = new System.Drawing.Size(582, 309);
            this.richTextBoxToS.TabIndex = 0;
            this.richTextBoxToS.Text = "";
            // 
            // buttonAcceptToS
            // 
            this.buttonAcceptToS.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.buttonAcceptToS.FlatAppearance.BorderSize = 2;
            this.buttonAcceptToS.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonAcceptToS.Location = new System.Drawing.Point(0, 315);
            this.buttonAcceptToS.Name = "buttonAcceptToS";
            this.buttonAcceptToS.Size = new System.Drawing.Size(582, 38);
            this.buttonAcceptToS.TabIndex = 2;
            this.buttonAcceptToS.Text = "I accept the Terms Of Use";
            this.buttonAcceptToS.UseVisualStyleBackColor = true;
            this.buttonAcceptToS.Click += new System.EventHandler(this.buttonAcceptToS_Click);
            // 
            // FormEula
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(582, 353);
            this.Controls.Add(this.buttonAcceptToS);
            this.Controls.Add(this.richTextBoxToS);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(600, 400);
            this.Name = "FormEula";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = NHMProductInfo.TermsOfUse;
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox richTextBoxToS;
        private System.Windows.Forms.Button buttonAcceptToS;
    }
}
