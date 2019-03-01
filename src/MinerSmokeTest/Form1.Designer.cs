namespace MinerSmokeTest
{
    partial class Form1
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
            this.tbx_info = new System.Windows.Forms.TextBox();
            this.btn_startTest = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.dgv_devices = new System.Windows.Forms.DataGridView();
            this.dgv_deviceEnabled = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.dgv_deviceName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgv_algo = new System.Windows.Forms.DataGridView();
            this.Enabled = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.Algorithm = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Miner = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_devices)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_algo)).BeginInit();
            this.SuspendLayout();
            // 
            // tbx_info
            // 
            this.tbx_info.Location = new System.Drawing.Point(800, 224);
            this.tbx_info.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tbx_info.Multiline = true;
            this.tbx_info.Name = "tbx_info";
            this.tbx_info.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.tbx_info.Size = new System.Drawing.Size(787, 314);
            this.tbx_info.TabIndex = 4;
            // 
            // btn_startTest
            // 
            this.btn_startTest.Location = new System.Drawing.Point(16, 26);
            this.btn_startTest.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btn_startTest.Name = "btn_startTest";
            this.btn_startTest.Size = new System.Drawing.Size(165, 28);
            this.btn_startTest.TabIndex = 5;
            this.btn_startTest.Text = "Start test";
            this.btn_startTest.UseVisualStyleBackColor = true;
            this.btn_startTest.Click += new System.EventHandler(this.btn_startTest_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(227, 26);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(101, 17);
            this.label1.TabIndex = 6;
            this.label1.Text = "Testing status:";
            // 
            // dgv_devices
            // 
            this.dgv_devices.AllowUserToAddRows = false;
            this.dgv_devices.AllowUserToDeleteRows = false;
            this.dgv_devices.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_devices.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dgv_deviceEnabled,
            this.dgv_deviceName});
            this.dgv_devices.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.dgv_devices.Location = new System.Drawing.Point(16, 225);
            this.dgv_devices.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.dgv_devices.Name = "dgv_devices";
            this.dgv_devices.ReadOnly = true;
            this.dgv_devices.RowHeadersVisible = false;
            this.dgv_devices.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgv_devices.Size = new System.Drawing.Size(349, 314);
            this.dgv_devices.TabIndex = 12;
            // 
            // dgv_deviceEnabled
            // 
            this.dgv_deviceEnabled.FalseValue = "\"NO\"";
            this.dgv_deviceEnabled.HeaderText = "Enabled";
            this.dgv_deviceEnabled.Name = "dgv_deviceEnabled";
            this.dgv_deviceEnabled.ReadOnly = true;
            this.dgv_deviceEnabled.Width = 50;
            // 
            // dgv_deviceName
            // 
            this.dgv_deviceName.HeaderText = "Name";
            this.dgv_deviceName.Name = "dgv_deviceName";
            this.dgv_deviceName.ReadOnly = true;
            this.dgv_deviceName.Width = 200;
            // 
            // dgv_algo
            // 
            this.dgv_algo.AllowUserToAddRows = false;
            this.dgv_algo.AllowUserToDeleteRows = false;
            this.dgv_algo.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_algo.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Enabled,
            this.Algorithm,
            this.Miner});
            this.dgv_algo.Location = new System.Drawing.Point(373, 224);
            this.dgv_algo.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.dgv_algo.Name = "dgv_algo";
            this.dgv_algo.ReadOnly = true;
            this.dgv_algo.RowHeadersVisible = false;
            this.dgv_algo.Size = new System.Drawing.Size(417, 315);
            this.dgv_algo.TabIndex = 13;
            // 
            // Enabled
            // 
            this.Enabled.HeaderText = "Enabled";
            this.Enabled.Name = "Enabled";
            this.Enabled.ReadOnly = true;
            // 
            // Algorithm
            // 
            this.Algorithm.HeaderText = "Algorithm";
            this.Algorithm.Name = "Algorithm";
            this.Algorithm.ReadOnly = true;
            // 
            // Miner
            // 
            this.Miner.HeaderText = "Miner";
            this.Miner.Name = "Miner";
            this.Miner.ReadOnly = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1604, 554);
            this.Controls.Add(this.dgv_algo);
            this.Controls.Add(this.dgv_devices);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btn_startTest);
            this.Controls.Add(this.tbx_info);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "Form1";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.dgv_devices)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_algo)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox tbx_info;
        private System.Windows.Forms.Button btn_startTest;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DataGridView dgv_devices;
        private System.Windows.Forms.DataGridView dgv_algo;
        private System.Windows.Forms.DataGridViewCheckBoxColumn Enabled;
        private System.Windows.Forms.DataGridViewTextBoxColumn Algorithm;
        private System.Windows.Forms.DataGridViewTextBoxColumn Miner;
        private System.Windows.Forms.DataGridViewCheckBoxColumn dgv_deviceEnabled;
        private System.Windows.Forms.DataGridViewTextBoxColumn dgv_deviceName;
    }
}

