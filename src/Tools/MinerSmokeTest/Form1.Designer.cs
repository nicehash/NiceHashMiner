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
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.tbx_minTimeM = new System.Windows.Forms.TextBox();
            this.tbx_minTimeS = new System.Windows.Forms.TextBox();
            this.tbx_minTimeMS = new System.Windows.Forms.TextBox();
            this.tbx_stopDelayMS = new System.Windows.Forms.TextBox();
            this.tbx_stopDelayS = new System.Windows.Forms.TextBox();
            this.tbx_stopDelayM = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.rb_endMining = new System.Windows.Forms.RadioButton();
            this.rb_stopMining = new System.Windows.Forms.RadioButton();
            this.gb_stopMiningBy = new System.Windows.Forms.GroupBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.radioButtonTest01 = new System.Windows.Forms.RadioButton();
            this.radioButtonTest02 = new System.Windows.Forms.RadioButton();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_devices)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_algo)).BeginInit();
            this.gb_stopMiningBy.SuspendLayout();
            this.groupBox1.SuspendLayout();
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
            this.label1.Location = new System.Drawing.Point(170, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(76, 13);
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
            this.dgv_devices.Location = new System.Drawing.Point(12, 183);
            this.dgv_devices.Name = "dgv_devices";
            this.dgv_devices.ReadOnly = true;
            this.dgv_devices.RowHeadersVisible = false;
            this.dgv_devices.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgv_devices.Size = new System.Drawing.Size(262, 255);
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
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(411, 21);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(63, 13);
            this.label2.TabIndex = 14;
            this.label2.Text = "Mining time:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(589, 21);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(12, 13);
            this.label3.TabIndex = 15;
            this.label3.Text = "s";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(522, 21);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(23, 13);
            this.label4.TabIndex = 16;
            this.label4.Text = "min";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(653, 21);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(20, 13);
            this.label5.TabIndex = 17;
            this.label5.Text = "ms";
            // 
            // tbx_minTimeM
            // 
            this.tbx_minTimeM.Location = new System.Drawing.Point(480, 14);
            this.tbx_minTimeM.Name = "tbx_minTimeM";
            this.tbx_minTimeM.Size = new System.Drawing.Size(40, 20);
            this.tbx_minTimeM.TabIndex = 18;
            this.tbx_minTimeM.Text = "0";
            this.tbx_minTimeM.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // tbx_minTimeS
            // 
            this.tbx_minTimeS.Location = new System.Drawing.Point(551, 14);
            this.tbx_minTimeS.Name = "tbx_minTimeS";
            this.tbx_minTimeS.Size = new System.Drawing.Size(40, 20);
            this.tbx_minTimeS.TabIndex = 19;
            this.tbx_minTimeS.Text = "0";
            this.tbx_minTimeS.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // tbx_minTimeMS
            // 
            this.tbx_minTimeMS.Location = new System.Drawing.Point(607, 14);
            this.tbx_minTimeMS.Name = "tbx_minTimeMS";
            this.tbx_minTimeMS.Size = new System.Drawing.Size(40, 20);
            this.tbx_minTimeMS.TabIndex = 20;
            this.tbx_minTimeMS.Text = "0";
            this.tbx_minTimeMS.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // tbx_stopDelayMS
            // 
            this.tbx_stopDelayMS.Location = new System.Drawing.Point(607, 56);
            this.tbx_stopDelayMS.Name = "tbx_stopDelayMS";
            this.tbx_stopDelayMS.Size = new System.Drawing.Size(40, 20);
            this.tbx_stopDelayMS.TabIndex = 27;
            this.tbx_stopDelayMS.Text = "0";
            this.tbx_stopDelayMS.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // tbx_stopDelayS
            // 
            this.tbx_stopDelayS.Location = new System.Drawing.Point(551, 56);
            this.tbx_stopDelayS.Name = "tbx_stopDelayS";
            this.tbx_stopDelayS.Size = new System.Drawing.Size(40, 20);
            this.tbx_stopDelayS.TabIndex = 26;
            this.tbx_stopDelayS.Text = "0";
            this.tbx_stopDelayS.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // tbx_stopDelayM
            // 
            this.tbx_stopDelayM.Location = new System.Drawing.Point(480, 56);
            this.tbx_stopDelayM.Name = "tbx_stopDelayM";
            this.tbx_stopDelayM.Size = new System.Drawing.Size(40, 20);
            this.tbx_stopDelayM.TabIndex = 25;
            this.tbx_stopDelayM.Text = "0";
            this.tbx_stopDelayM.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(653, 63);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(20, 13);
            this.label6.TabIndex = 24;
            this.label6.Text = "ms";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(522, 63);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(23, 13);
            this.label7.TabIndex = 23;
            this.label7.Text = "min";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(589, 63);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(12, 13);
            this.label8.TabIndex = 22;
            this.label8.Text = "s";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(392, 63);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(82, 13);
            this.label9.TabIndex = 21;
            this.label9.Text = "Stop delay time:";
            // 
            // rb_endMining
            // 
            this.rb_endMining.AutoSize = true;
            this.rb_endMining.Location = new System.Drawing.Point(6, 20);
            this.rb_endMining.Name = "rb_endMining";
            this.rb_endMining.Size = new System.Drawing.Size(44, 17);
            this.rb_endMining.TabIndex = 28;
            this.rb_endMining.Text = "End";
            this.rb_endMining.UseVisualStyleBackColor = true;
            // 
            // rb_stopMining
            // 
            this.rb_stopMining.AutoSize = true;
            this.rb_stopMining.Checked = true;
            this.rb_stopMining.Location = new System.Drawing.Point(6, 43);
            this.rb_stopMining.Name = "rb_stopMining";
            this.rb_stopMining.Size = new System.Drawing.Size(47, 17);
            this.rb_stopMining.TabIndex = 29;
            this.rb_stopMining.TabStop = true;
            this.rb_stopMining.Text = "Stop";
            this.rb_stopMining.UseVisualStyleBackColor = true;
            // 
            // gb_stopMiningBy
            // 
            this.gb_stopMiningBy.Controls.Add(this.rb_stopMining);
            this.gb_stopMiningBy.Controls.Add(this.rb_endMining);
            this.gb_stopMiningBy.Location = new System.Drawing.Point(755, 14);
            this.gb_stopMiningBy.Name = "gb_stopMiningBy";
            this.gb_stopMiningBy.Size = new System.Drawing.Size(200, 100);
            this.gb_stopMiningBy.TabIndex = 31;
            this.gb_stopMiningBy.TabStop = false;
            this.gb_stopMiningBy.Text = "Stop by";
            this.gb_stopMiningBy.Enter += new System.EventHandler(this.Gb_stopMiningBy_Enter);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.radioButtonTest02);
            this.groupBox1.Controls.Add(this.radioButtonTest01);
            this.groupBox1.Location = new System.Drawing.Point(961, 14);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(200, 100);
            this.groupBox1.TabIndex = 32;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Tests";
            // 
            // radioButtonTest01
            // 
            this.radioButtonTest01.AutoSize = true;
            this.radioButtonTest01.Checked = true;
            this.radioButtonTest01.Location = new System.Drawing.Point(7, 20);
            this.radioButtonTest01.Name = "radioButtonTest01";
            this.radioButtonTest01.Size = new System.Drawing.Size(58, 17);
            this.radioButtonTest01.TabIndex = 0;
            this.radioButtonTest01.TabStop = true;
            this.radioButtonTest01.Text = "Test01";
            this.radioButtonTest01.UseVisualStyleBackColor = true;
            // 
            // radioButtonTest02
            // 
            this.radioButtonTest02.AutoSize = true;
            this.radioButtonTest02.Location = new System.Drawing.Point(7, 42);
            this.radioButtonTest02.Name = "radioButtonTest02";
            this.radioButtonTest02.Size = new System.Drawing.Size(58, 17);
            this.radioButtonTest02.TabIndex = 1;
            this.radioButtonTest02.TabStop = true;
            this.radioButtonTest02.Text = "Test02";
            this.radioButtonTest02.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1203, 450);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.gb_stopMiningBy);
            this.Controls.Add(this.tbx_stopDelayMS);
            this.Controls.Add(this.tbx_stopDelayS);
            this.Controls.Add(this.tbx_stopDelayM);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.tbx_minTimeMS);
            this.Controls.Add(this.tbx_minTimeS);
            this.Controls.Add(this.tbx_minTimeM);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.dgv_algo);
            this.Controls.Add(this.dgv_devices);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btn_startTest);
            this.Controls.Add(this.tbx_info);
            this.Name = "Form1";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.dgv_devices)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_algo)).EndInit();
            this.gb_stopMiningBy.ResumeLayout(false);
            this.gb_stopMiningBy.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
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
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox tbx_minTimeM;
        private System.Windows.Forms.TextBox tbx_minTimeS;
        private System.Windows.Forms.TextBox tbx_minTimeMS;
        private System.Windows.Forms.TextBox tbx_stopDelayMS;
        private System.Windows.Forms.TextBox tbx_stopDelayS;
        private System.Windows.Forms.TextBox tbx_stopDelayM;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.RadioButton rb_endMining;
        private System.Windows.Forms.RadioButton rb_stopMining;
        private System.Windows.Forms.GroupBox gb_stopMiningBy;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton radioButtonTest02;
        private System.Windows.Forms.RadioButton radioButtonTest01;
    }
}

