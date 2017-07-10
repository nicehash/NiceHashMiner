namespace MyDownloader.Extension.AntiVirus.UI
{
    partial class AVOptions
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
            this.chkAllowAV = new System.Windows.Forms.CheckBox();
            this.txtAVFileName = new System.Windows.Forms.TextBox();
            this.btnSelAV = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.txtFileTypes = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtParameter = new System.Windows.Forms.TextBox();
            this.openFileDlg = new System.Windows.Forms.OpenFileDialog();
            this.SuspendLayout();
            // 
            // chkAllowAV
            // 
            this.chkAllowAV.AutoSize = true;
            this.chkAllowAV.Location = new System.Drawing.Point(3, 3);
            this.chkAllowAV.Name = "chkAllowAV";
            this.chkAllowAV.Size = new System.Drawing.Size(233, 17);
            this.chkAllowAV.TabIndex = 0;
            this.chkAllowAV.Text = "Scan files with AV after finishing downloads:";
            this.chkAllowAV.UseVisualStyleBackColor = true;
            this.chkAllowAV.CheckedChanged += new System.EventHandler(this.chkAllowAV_CheckedChanged);
            // 
            // txtAVFileName
            // 
            this.txtAVFileName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtAVFileName.Location = new System.Drawing.Point(4, 21);
            this.txtAVFileName.Name = "txtAVFileName";
            this.txtAVFileName.Size = new System.Drawing.Size(284, 20);
            this.txtAVFileName.TabIndex = 1;
            // 
            // btnSelAV
            // 
            this.btnSelAV.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelAV.Location = new System.Drawing.Point(291, 20);
            this.btnSelAV.Name = "btnSelAV";
            this.btnSelAV.Size = new System.Drawing.Size(26, 23);
            this.btnSelAV.TabIndex = 2;
            this.btnSelAV.Text = "...";
            this.btnSelAV.UseVisualStyleBackColor = true;
            this.btnSelAV.Click += new System.EventHandler(this.btnSelAV_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(0, 51);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(93, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Scaning file types:";
            // 
            // txtFileTypes
            // 
            this.txtFileTypes.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtFileTypes.Location = new System.Drawing.Point(3, 67);
            this.txtFileTypes.Name = "txtFileTypes";
            this.txtFileTypes.Size = new System.Drawing.Size(314, 20);
            this.txtFileTypes.TabIndex = 4;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(0, 95);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(58, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Parameter:";
            // 
            // txtParameter
            // 
            this.txtParameter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtParameter.Location = new System.Drawing.Point(3, 112);
            this.txtParameter.Name = "txtParameter";
            this.txtParameter.Size = new System.Drawing.Size(314, 20);
            this.txtParameter.TabIndex = 6;
            // 
            // openFileDlg
            // 
            this.openFileDlg.Filter = "Executable|*.exe";
            // 
            // AVOptions
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.txtParameter);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtFileTypes);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnSelAV);
            this.Controls.Add(this.txtAVFileName);
            this.Controls.Add(this.chkAllowAV);
            this.Name = "AVOptions";
            this.Size = new System.Drawing.Size(320, 236);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox chkAllowAV;
        private System.Windows.Forms.TextBox txtAVFileName;
        private System.Windows.Forms.Button btnSelAV;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtFileTypes;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtParameter;
        private System.Windows.Forms.OpenFileDialog openFileDlg;
    }
}
