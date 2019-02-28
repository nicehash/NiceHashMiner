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
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.lbl_testingMinerVersion = new System.Windows.Forms.Label();
            this.lbl_minerName = new System.Windows.Forms.Label();
            this.dgv_devices = new System.Windows.Forms.DataGridView();
            this.dgv_algo = new System.Windows.Forms.DataGridView();
            this.Enabled = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.Algorithm = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Miner = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.lbl_status = new System.Windows.Forms.Label();
            this.lbl_algoName = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.dgv_deviceEnabled = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.dgv_deviceName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_devices)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_algo)).BeginInit();
            this.SuspendLayout();
            // 
            // tbx_info
            // 
            this.tbx_info.Location = new System.Drawing.Point(600, 182);
            this.tbx_info.Multiline = true;
            this.tbx_info.Name = "tbx_info";
            this.tbx_info.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.tbx_info.Size = new System.Drawing.Size(591, 256);
            this.tbx_info.TabIndex = 4;
            // 
            // btn_startTest
            // 
            this.btn_startTest.Location = new System.Drawing.Point(12, 21);
            this.btn_startTest.Name = "btn_startTest";
            this.btn_startTest.Size = new System.Drawing.Size(124, 23);
            this.btn_startTest.TabIndex = 5;
            this.btn_startTest.Text = "Start test";
            this.btn_startTest.UseVisualStyleBackColor = true;
            this.btn_startTest.Click += new System.EventHandler(this.btn_startTest_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(1000, 43);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(76, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Testing status:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(950, 120);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(45, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "Version:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(950, 94);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(36, 13);
            this.label4.TabIndex = 9;
            this.label4.Text = "Miner:";
            // 
            // lbl_testingMinerVersion
            // 
            this.lbl_testingMinerVersion.AutoSize = true;
            this.lbl_testingMinerVersion.Location = new System.Drawing.Point(1009, 120);
            this.lbl_testingMinerVersion.Name = "lbl_testingMinerVersion";
            this.lbl_testingMinerVersion.Size = new System.Drawing.Size(0, 13);
            this.lbl_testingMinerVersion.TabIndex = 10;
            // 
            // lbl_minerName
            // 
            this.lbl_minerName.AutoSize = true;
            this.lbl_minerName.Location = new System.Drawing.Point(1009, 94);
            this.lbl_minerName.Name = "lbl_minerName";
            this.lbl_minerName.Size = new System.Drawing.Size(0, 13);
            this.lbl_minerName.TabIndex = 11;
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
            this.dgv_devices.Location = new System.Drawing.Point(12, 183);
            this.dgv_devices.Name = "dgv_devices";
            this.dgv_devices.ReadOnly = true;
            this.dgv_devices.RowHeadersVisible = false;
            this.dgv_devices.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgv_devices.Size = new System.Drawing.Size(262, 255);
            this.dgv_devices.TabIndex = 12;
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
            this.dgv_algo.Location = new System.Drawing.Point(280, 182);
            this.dgv_algo.Name = "dgv_algo";
            this.dgv_algo.ReadOnly = true;
            this.dgv_algo.RowHeadersVisible = false;
            this.dgv_algo.Size = new System.Drawing.Size(313, 256);
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
            // lbl_status
            // 
            this.lbl_status.AutoSize = true;
            this.lbl_status.Location = new System.Drawing.Point(1082, 43);
            this.lbl_status.Name = "lbl_status";
            this.lbl_status.Size = new System.Drawing.Size(0, 13);
            this.lbl_status.TabIndex = 14;
            // 
            // lbl_algoName
            // 
            this.lbl_algoName.AutoSize = true;
            this.lbl_algoName.Location = new System.Drawing.Point(1009, 151);
            this.lbl_algoName.Name = "lbl_algoName";
            this.lbl_algoName.Size = new System.Drawing.Size(0, 13);
            this.lbl_algoName.TabIndex = 16;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(950, 151);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(53, 13);
            this.label5.TabIndex = 15;
            this.label5.Text = "Algorithm:";
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
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1203, 450);
            this.Controls.Add(this.lbl_algoName);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.lbl_status);
            this.Controls.Add(this.dgv_algo);
            this.Controls.Add(this.dgv_devices);
            this.Controls.Add(this.lbl_minerName);
            this.Controls.Add(this.lbl_testingMinerVersion);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btn_startTest);
            this.Controls.Add(this.tbx_info);
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
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label lbl_testingMinerVersion;
        private System.Windows.Forms.Label lbl_minerName;
        private System.Windows.Forms.DataGridView dgv_devices;
        private System.Windows.Forms.DataGridView dgv_algo;
        private System.Windows.Forms.DataGridViewCheckBoxColumn Enabled;
        private System.Windows.Forms.DataGridViewTextBoxColumn Algorithm;
        private System.Windows.Forms.DataGridViewTextBoxColumn Miner;
        private System.Windows.Forms.Label lbl_status;
        private System.Windows.Forms.Label lbl_algoName;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.DataGridViewCheckBoxColumn dgv_deviceEnabled;
        private System.Windows.Forms.DataGridViewTextBoxColumn dgv_deviceName;
    }
}

