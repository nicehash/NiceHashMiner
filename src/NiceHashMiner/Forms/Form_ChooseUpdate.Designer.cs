namespace NiceHashMiner.Forms
{
    partial class Form_ChooseUpdate
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
            this.UpdaterBtn = new System.Windows.Forms.Button();
            this.GithubBtn = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.SuspendLayout();
            // 
            // UpdaterBtn
            // 
            this.UpdaterBtn.Location = new System.Drawing.Point(12, 26);
            this.UpdaterBtn.Name = "UpdaterBtn";
            this.UpdaterBtn.Size = new System.Drawing.Size(75, 23);
            this.UpdaterBtn.TabIndex = 0;
            this.UpdaterBtn.Text = "Updater";
            this.UpdaterBtn.UseVisualStyleBackColor = true;
            this.UpdaterBtn.Click += new System.EventHandler(this.UpdaterBtn_Click);
            // 
            // GithubBtn
            // 
            this.GithubBtn.Location = new System.Drawing.Point(147, 26);
            this.GithubBtn.Name = "GithubBtn";
            this.GithubBtn.Size = new System.Drawing.Size(75, 23);
            this.GithubBtn.TabIndex = 1;
            this.GithubBtn.Text = "GitHub";
            this.GithubBtn.UseVisualStyleBackColor = true;
            this.GithubBtn.Click += new System.EventHandler(this.GithubBtn_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(209, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Please choose wanted option for updating:";
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(0, 55);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(233, 10);
            this.progressBar1.TabIndex = 3;
            // 
            // Form_ChooseUpdate
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(234, 69);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.GithubBtn);
            this.Controls.Add(this.UpdaterBtn);
            this.Name = "Form_ChooseUpdate";
            this.Text = "Update";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button UpdaterBtn;
        private System.Windows.Forms.Button GithubBtn;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ProgressBar progressBar1;
    }
}