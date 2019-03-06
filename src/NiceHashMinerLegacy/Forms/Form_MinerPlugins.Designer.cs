namespace NiceHashMiner.Forms
{
    partial class Form_MinerPlugins
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
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.ColumnName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnAuthor = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnOnlineVersion = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnInstalledVersion = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridView1
            // 
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ColumnName,
            this.ColumnAuthor,
            this.ColumnOnlineVersion,
            this.ColumnInstalledVersion});
            this.dataGridView1.Location = new System.Drawing.Point(12, 42);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.Size = new System.Drawing.Size(492, 396);
            this.dataGridView1.TabIndex = 0;
            // 
            // ColumnName
            // 
            this.ColumnName.HeaderText = "Name";
            this.ColumnName.Name = "ColumnName";
            // 
            // ColumnAuthor
            // 
            this.ColumnAuthor.HeaderText = "Author";
            this.ColumnAuthor.Name = "ColumnAuthor";
            // 
            // ColumnOnlineVersion
            // 
            this.ColumnOnlineVersion.HeaderText = "Online Version";
            this.ColumnOnlineVersion.Name = "ColumnOnlineVersion";
            // 
            // ColumnInstalledVersion
            // 
            this.ColumnInstalledVersion.HeaderText = "Installed Version";
            this.ColumnInstalledVersion.Name = "ColumnInstalledVersion";
            // 
            // Form_MinerPlugins
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(914, 450);
            this.Controls.Add(this.dataGridView1);
            this.Name = "Form_MinerPlugins";
            this.Text = "Form_MinerPlugins";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnName;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnAuthor;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnOnlineVersion;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnInstalledVersion;
    }
}