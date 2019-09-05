namespace NiceHashMiner.Forms
{
    partial class Form_TDPSettings
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
            this.listViewDevicesTDP = new System.Windows.Forms.ListView();
            this.DevicesHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.button_set_simple = new System.Windows.Forms.Button();
            this.label_simple = new System.Windows.Forms.Label();
            this.label_value_simple = new System.Windows.Forms.Label();
            this.button_get_simple = new System.Windows.Forms.Button();
            this.textBox_simple = new System.Windows.Forms.TextBox();
            this.button_set_percentage = new System.Windows.Forms.Button();
            this.label_percentage = new System.Windows.Forms.Label();
            this.label_value_percentage = new System.Windows.Forms.Label();
            this.button_get_percentage = new System.Windows.Forms.Button();
            this.textBox_percentage = new System.Windows.Forms.TextBox();
            this.button_set_raw = new System.Windows.Forms.Button();
            this.label_raw = new System.Windows.Forms.Label();
            this.label_value_raw = new System.Windows.Forms.Label();
            this.button_get_raw = new System.Windows.Forms.Button();
            this.textBox_raw = new System.Windows.Forms.TextBox();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.labelSelectedDevice = new System.Windows.Forms.Label();
            this.labelSelectedTDPSetting = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // listViewDevicesTDP
            // 
            this.listViewDevicesTDP.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.listViewDevicesTDP.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.DevicesHeader});
            this.listViewDevicesTDP.FullRowSelect = true;
            this.listViewDevicesTDP.GridLines = true;
            this.listViewDevicesTDP.HideSelection = false;
            this.listViewDevicesTDP.Location = new System.Drawing.Point(12, 12);
            this.listViewDevicesTDP.Name = "listViewDevicesTDP";
            this.listViewDevicesTDP.Size = new System.Drawing.Size(405, 380);
            this.listViewDevicesTDP.TabIndex = 3;
            this.listViewDevicesTDP.UseCompatibleStateImageBehavior = false;
            this.listViewDevicesTDP.View = System.Windows.Forms.View.Details;
            // 
            // DevicesHeader
            // 
            this.DevicesHeader.Text = "Devices";
            this.DevicesHeader.Width = 400;
            // 
            // button_set_simple
            // 
            this.button_set_simple.Location = new System.Drawing.Point(436, 132);
            this.button_set_simple.Name = "button_set_simple";
            this.button_set_simple.Size = new System.Drawing.Size(75, 23);
            this.button_set_simple.TabIndex = 4;
            this.button_set_simple.Text = "Set";
            this.button_set_simple.UseVisualStyleBackColor = true;
            this.button_set_simple.Click += new System.EventHandler(this.Button_set_simple_Click);
            // 
            // label_simple
            // 
            this.label_simple.AutoSize = true;
            this.label_simple.Location = new System.Drawing.Point(433, 58);
            this.label_simple.Name = "label_simple";
            this.label_simple.Size = new System.Drawing.Size(49, 13);
            this.label_simple.TabIndex = 5;
            this.label_simple.Text = "SIMPLE:";
            // 
            // label_value_simple
            // 
            this.label_value_simple.AutoSize = true;
            this.label_value_simple.Location = new System.Drawing.Point(523, 95);
            this.label_value_simple.Name = "label_value_simple";
            this.label_value_simple.Size = new System.Drawing.Size(87, 13);
            this.label_value_simple.TabIndex = 6;
            this.label_value_simple.Text = "SIMPLE_VALUE";
            // 
            // button_get_simple
            // 
            this.button_get_simple.Location = new System.Drawing.Point(436, 90);
            this.button_get_simple.Name = "button_get_simple";
            this.button_get_simple.Size = new System.Drawing.Size(75, 23);
            this.button_get_simple.TabIndex = 7;
            this.button_get_simple.Text = "Get";
            this.button_get_simple.UseVisualStyleBackColor = true;
            this.button_get_simple.Click += new System.EventHandler(this.Button_get_simple_Click);
            // 
            // textBox_simple
            // 
            this.textBox_simple.Location = new System.Drawing.Point(526, 134);
            this.textBox_simple.Name = "textBox_simple";
            this.textBox_simple.Size = new System.Drawing.Size(100, 20);
            this.textBox_simple.TabIndex = 8;
            // 
            // button_set_percentage
            // 
            this.button_set_percentage.Location = new System.Drawing.Point(436, 250);
            this.button_set_percentage.Name = "button_set_percentage";
            this.button_set_percentage.Size = new System.Drawing.Size(75, 23);
            this.button_set_percentage.TabIndex = 4;
            this.button_set_percentage.Text = "Set";
            this.button_set_percentage.UseVisualStyleBackColor = true;
            this.button_set_percentage.Click += new System.EventHandler(this.Button_set_percentage_Click);
            // 
            // label_percentage
            // 
            this.label_percentage.AutoSize = true;
            this.label_percentage.Location = new System.Drawing.Point(433, 176);
            this.label_percentage.Name = "label_percentage";
            this.label_percentage.Size = new System.Drawing.Size(83, 13);
            this.label_percentage.TabIndex = 5;
            this.label_percentage.Text = "PERCENTAGE:";
            // 
            // label_value_percentage
            // 
            this.label_value_percentage.AutoSize = true;
            this.label_value_percentage.Location = new System.Drawing.Point(523, 213);
            this.label_value_percentage.Name = "label_value_percentage";
            this.label_value_percentage.Size = new System.Drawing.Size(121, 13);
            this.label_value_percentage.TabIndex = 6;
            this.label_value_percentage.Text = "PERCENTAGE_VALUE";
            // 
            // button_get_percentage
            // 
            this.button_get_percentage.Location = new System.Drawing.Point(436, 208);
            this.button_get_percentage.Name = "button_get_percentage";
            this.button_get_percentage.Size = new System.Drawing.Size(75, 23);
            this.button_get_percentage.TabIndex = 7;
            this.button_get_percentage.Text = "Get";
            this.button_get_percentage.UseVisualStyleBackColor = true;
            this.button_get_percentage.Click += new System.EventHandler(this.Button_get_percentage_Click);
            // 
            // textBox_percentage
            // 
            this.textBox_percentage.Location = new System.Drawing.Point(526, 252);
            this.textBox_percentage.Name = "textBox_percentage";
            this.textBox_percentage.Size = new System.Drawing.Size(100, 20);
            this.textBox_percentage.TabIndex = 8;
            // 
            // button_set_raw
            // 
            this.button_set_raw.Location = new System.Drawing.Point(436, 368);
            this.button_set_raw.Name = "button_set_raw";
            this.button_set_raw.Size = new System.Drawing.Size(75, 23);
            this.button_set_raw.TabIndex = 4;
            this.button_set_raw.Text = "Set";
            this.button_set_raw.UseVisualStyleBackColor = true;
            this.button_set_raw.Click += new System.EventHandler(this.Button_set_raw_Click);
            // 
            // label_raw
            // 
            this.label_raw.AutoSize = true;
            this.label_raw.Location = new System.Drawing.Point(433, 294);
            this.label_raw.Name = "label_raw";
            this.label_raw.Size = new System.Drawing.Size(36, 13);
            this.label_raw.TabIndex = 5;
            this.label_raw.Text = "RAW:";
            // 
            // label_value_raw
            // 
            this.label_value_raw.AutoSize = true;
            this.label_value_raw.Location = new System.Drawing.Point(523, 331);
            this.label_value_raw.Name = "label_value_raw";
            this.label_value_raw.Size = new System.Drawing.Size(74, 13);
            this.label_value_raw.TabIndex = 6;
            this.label_value_raw.Text = "RAW_VALUE";
            // 
            // button_get_raw
            // 
            this.button_get_raw.Location = new System.Drawing.Point(436, 326);
            this.button_get_raw.Name = "button_get_raw";
            this.button_get_raw.Size = new System.Drawing.Size(75, 23);
            this.button_get_raw.TabIndex = 7;
            this.button_get_raw.Text = "Get";
            this.button_get_raw.UseVisualStyleBackColor = true;
            this.button_get_raw.Click += new System.EventHandler(this.Button_get_raw_Click);
            // 
            // textBox_raw
            // 
            this.textBox_raw.Location = new System.Drawing.Point(526, 370);
            this.textBox_raw.Name = "textBox_raw";
            this.textBox_raw.Size = new System.Drawing.Size(100, 20);
            this.textBox_raw.TabIndex = 8;
            // 
            // splitter1
            // 
            this.splitter1.Location = new System.Drawing.Point(0, 0);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(3, 399);
            this.splitter1.TabIndex = 9;
            this.splitter1.TabStop = false;
            // 
            // labelSelectedDevice
            // 
            this.labelSelectedDevice.AutoSize = true;
            this.labelSelectedDevice.Location = new System.Drawing.Point(436, 13);
            this.labelSelectedDevice.Name = "labelSelectedDevice";
            this.labelSelectedDevice.Size = new System.Drawing.Size(109, 13);
            this.labelSelectedDevice.TabIndex = 10;
            this.labelSelectedDevice.Text = "Selected Device N/A";
            // 
            // labelSelectedTDPSetting
            // 
            this.labelSelectedTDPSetting.AutoSize = true;
            this.labelSelectedTDPSetting.Location = new System.Drawing.Point(436, 36);
            this.labelSelectedTDPSetting.Name = "labelSelectedTDPSetting";
            this.labelSelectedTDPSetting.Size = new System.Drawing.Size(128, 13);
            this.labelSelectedTDPSetting.TabIndex = 10;
            this.labelSelectedTDPSetting.Text = "Selected TDP Setting => ";
            // 
            // Form_TDPSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(831, 399);
            this.Controls.Add(this.labelSelectedTDPSetting);
            this.Controls.Add(this.labelSelectedDevice);
            this.Controls.Add(this.splitter1);
            this.Controls.Add(this.textBox_raw);
            this.Controls.Add(this.textBox_percentage);
            this.Controls.Add(this.textBox_simple);
            this.Controls.Add(this.button_get_raw);
            this.Controls.Add(this.button_get_percentage);
            this.Controls.Add(this.label_value_raw);
            this.Controls.Add(this.label_value_percentage);
            this.Controls.Add(this.button_get_simple);
            this.Controls.Add(this.label_raw);
            this.Controls.Add(this.label_percentage);
            this.Controls.Add(this.button_set_raw);
            this.Controls.Add(this.label_value_simple);
            this.Controls.Add(this.button_set_percentage);
            this.Controls.Add(this.label_simple);
            this.Controls.Add(this.button_set_simple);
            this.Controls.Add(this.listViewDevicesTDP);
            this.Name = "Form_TDPSettings";
            this.Text = "Form_TDPSettings";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ListView listViewDevicesTDP;
        private System.Windows.Forms.ColumnHeader DevicesHeader;
        private System.Windows.Forms.Button button_set_simple;
        private System.Windows.Forms.Label label_simple;
        private System.Windows.Forms.Label label_value_simple;
        private System.Windows.Forms.Button button_get_simple;
        private System.Windows.Forms.TextBox textBox_simple;
        private System.Windows.Forms.Button button_set_percentage;
        private System.Windows.Forms.Label label_percentage;
        private System.Windows.Forms.Label label_value_percentage;
        private System.Windows.Forms.Button button_get_percentage;
        private System.Windows.Forms.TextBox textBox_percentage;
        private System.Windows.Forms.Button button_set_raw;
        private System.Windows.Forms.Label label_raw;
        private System.Windows.Forms.Label label_value_raw;
        private System.Windows.Forms.Button button_get_raw;
        private System.Windows.Forms.TextBox textBox_raw;
        private System.Windows.Forms.Splitter splitter1;
        private System.Windows.Forms.Label labelSelectedDevice;
        private System.Windows.Forms.Label labelSelectedTDPSetting;
    }
}
