namespace NiceHashMiner.Forms.Components
{
    partial class PluginInfoItem
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
            this.labelName = new System.Windows.Forms.Label();
            this.labelVersion = new System.Windows.Forms.Label();
            this.labelAuthor = new System.Windows.Forms.Label();
            this.buttonInstallRemove = new System.Windows.Forms.Button();
            this.buttonUpdate = new System.Windows.Forms.Button();
            this.labelStatus = new System.Windows.Forms.Label();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.SuspendLayout();
            // 
            // labelName
            // 
            this.labelName.AutoSize = true;
            this.labelName.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.labelName.Location = new System.Drawing.Point(19, 12);
            this.labelName.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelName.Name = "labelName";
            this.labelName.Size = new System.Drawing.Size(64, 25);
            this.labelName.TabIndex = 0;
            this.labelName.Text = "Name";
            // 
            // labelVersion
            // 
            this.labelVersion.AutoSize = true;
            this.labelVersion.Location = new System.Drawing.Point(20, 48);
            this.labelVersion.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelVersion.Name = "labelVersion";
            this.labelVersion.Size = new System.Drawing.Size(54, 17);
            this.labelVersion.TabIndex = 1;
            this.labelVersion.Text = "version";
            // 
            // labelAuthor
            // 
            this.labelAuthor.AutoSize = true;
            this.labelAuthor.Location = new System.Drawing.Point(20, 75);
            this.labelAuthor.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelAuthor.Name = "labelAuthor";
            this.labelAuthor.Size = new System.Drawing.Size(80, 17);
            this.labelAuthor.TabIndex = 2;
            this.labelAuthor.Text = "labelAuthor";
            // 
            // buttonInstallRemove
            // 
            this.buttonInstallRemove.Location = new System.Drawing.Point(24, 111);
            this.buttonInstallRemove.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.buttonInstallRemove.Name = "buttonInstallRemove";
            this.buttonInstallRemove.Size = new System.Drawing.Size(192, 28);
            this.buttonInstallRemove.TabIndex = 3;
            this.buttonInstallRemove.Text = "Install/Remove";
            this.buttonInstallRemove.UseVisualStyleBackColor = true;
            // 
            // buttonUpdate
            // 
            this.buttonUpdate.Location = new System.Drawing.Point(281, 111);
            this.buttonUpdate.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.buttonUpdate.Name = "buttonUpdate";
            this.buttonUpdate.Size = new System.Drawing.Size(179, 28);
            this.buttonUpdate.TabIndex = 4;
            this.buttonUpdate.Text = "Update";
            this.buttonUpdate.UseVisualStyleBackColor = true;
            // 
            // labelStatus
            // 
            this.labelStatus.AutoSize = true;
            this.labelStatus.Location = new System.Drawing.Point(365, 21);
            this.labelStatus.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(48, 17);
            this.labelStatus.TabIndex = 5;
            this.labelStatus.Text = "Status";
            // 
            // progressBar1
            // 
            this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar1.Location = new System.Drawing.Point(0, 149);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(509, 10);
            this.progressBar1.TabIndex = 6;
            // 
            // PluginInfoItem
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.labelStatus);
            this.Controls.Add(this.buttonUpdate);
            this.Controls.Add(this.buttonInstallRemove);
            this.Controls.Add(this.labelAuthor);
            this.Controls.Add(this.labelVersion);
            this.Controls.Add(this.labelName);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "PluginInfoItem";
            this.Size = new System.Drawing.Size(509, 158);
            this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.PluginInfoItem_MouseClick);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelName;
        private System.Windows.Forms.Label labelVersion;
        private System.Windows.Forms.Label labelAuthor;
        private System.Windows.Forms.Button buttonInstallRemove;
        private System.Windows.Forms.Button buttonUpdate;
        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.ProgressBar progressBar1;
    }
}
