namespace NiceHashMiner.Forms.Components
{
    partial class StartupLoadingControl
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
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.label_LoadStepMessageText = new System.Windows.Forms.Label();
            this.label_LoadingTitle = new System.Windows.Forms.Label();
            this.label_LoadStepMessageText2 = new System.Windows.Forms.Label();
            this.progressBar2 = new System.Windows.Forms.ProgressBar();
            this.label_Title2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // progressBar1
            // 
            this.progressBar1.ForeColor = System.Drawing.Color.Blue;
            this.progressBar1.Location = new System.Drawing.Point(12, 25);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(286, 23);
            this.progressBar1.Step = 1;
            this.progressBar1.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.progressBar1.TabIndex = 1;
            // 
            // label_LoadStepMessageText
            // 
            this.label_LoadStepMessageText.AutoSize = true;
            this.label_LoadStepMessageText.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label_LoadStepMessageText.Location = new System.Drawing.Point(9, 51);
            this.label_LoadStepMessageText.Name = "label_LoadStepMessageText";
            this.label_LoadStepMessageText.Size = new System.Drawing.Size(119, 13);
            this.label_LoadStepMessageText.TabIndex = 2;
            this.label_LoadStepMessageText.Text = "Load message step text";
            // 
            // label_LoadingTitle
            // 
            this.label_LoadingTitle.AutoSize = true;
            this.label_LoadingTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label_LoadingTitle.ForeColor = System.Drawing.Color.Black;
            this.label_LoadingTitle.Location = new System.Drawing.Point(84, 9);
            this.label_LoadingTitle.Name = "label_LoadingTitle";
            this.label_LoadingTitle.Size = new System.Drawing.Size(136, 13);
            this.label_LoadingTitle.TabIndex = 0;
            this.label_LoadingTitle.Text = "Loading, please wait...";
            // 
            // label_LoadStepMessageText2
            // 
            this.label_LoadStepMessageText2.AutoSize = true;
            this.label_LoadStepMessageText2.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label_LoadStepMessageText2.Location = new System.Drawing.Point(9, 124);
            this.label_LoadStepMessageText2.Name = "label_LoadStepMessageText2";
            this.label_LoadStepMessageText2.Size = new System.Drawing.Size(119, 13);
            this.label_LoadStepMessageText2.TabIndex = 5;
            this.label_LoadStepMessageText2.Text = "Load message step text";
            // 
            // progressBar2
            // 
            this.progressBar2.ForeColor = System.Drawing.Color.Blue;
            this.progressBar2.Location = new System.Drawing.Point(12, 98);
            this.progressBar2.Name = "progressBar2";
            this.progressBar2.Size = new System.Drawing.Size(286, 23);
            this.progressBar2.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.progressBar2.TabIndex = 4;
            // 
            // label_Title2
            // 
            this.label_Title2.AutoSize = true;
            this.label_Title2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label_Title2.ForeColor = System.Drawing.Color.Black;
            this.label_Title2.Location = new System.Drawing.Point(84, 82);
            this.label_Title2.Name = "label_Title2";
            this.label_Title2.Size = new System.Drawing.Size(136, 13);
            this.label_Title2.TabIndex = 3;
            this.label_Title2.Text = "Loading, please wait...";
            // 
            // StartupLoadingControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.label_LoadStepMessageText2);
            this.Controls.Add(this.progressBar2);
            this.Controls.Add(this.label_Title2);
            this.Controls.Add(this.label_LoadStepMessageText);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.label_LoadingTitle);
            this.Name = "StartupLoadingControl";
            this.Size = new System.Drawing.Size(310, 146);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label label_LoadStepMessageText;
        private System.Windows.Forms.Label label_LoadingTitle;
        private System.Windows.Forms.Label label_LoadStepMessageText2;
        private System.Windows.Forms.ProgressBar progressBar2;
        private System.Windows.Forms.Label label_Title2;
    }
}
