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
            this.groupBoxTDPMode = new System.Windows.Forms.GroupBox();
            this.tb_Simple = new System.Windows.Forms.TextBox();
            this.tb_Percentage = new System.Windows.Forms.TextBox();
            this.tb_Raw = new System.Windows.Forms.TextBox();
            this.rb_Simple = new System.Windows.Forms.RadioButton();
            this.rb_Percentage = new System.Windows.Forms.RadioButton();
            this.rb_Raw = new System.Windows.Forms.RadioButton();
            this.btn_Refresh = new System.Windows.Forms.Button();
            this.listView1 = new System.Windows.Forms.ListView();
            this.DevicesHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.btn_SaveData = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.groupBoxTDPMode.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxTDPMode
            // 
            this.groupBoxTDPMode.Controls.Add(this.tb_Simple);
            this.groupBoxTDPMode.Controls.Add(this.tb_Percentage);
            this.groupBoxTDPMode.Controls.Add(this.tb_Raw);
            this.groupBoxTDPMode.Controls.Add(this.rb_Simple);
            this.groupBoxTDPMode.Controls.Add(this.rb_Percentage);
            this.groupBoxTDPMode.Controls.Add(this.rb_Raw);
            this.groupBoxTDPMode.Location = new System.Drawing.Point(423, 12);
            this.groupBoxTDPMode.Name = "groupBoxTDPMode";
            this.groupBoxTDPMode.Size = new System.Drawing.Size(263, 100);
            this.groupBoxTDPMode.TabIndex = 1;
            this.groupBoxTDPMode.TabStop = false;
            this.groupBoxTDPMode.Text = "groupBox1";
            // 
            // tb_Simple
            // 
            this.tb_Simple.Location = new System.Drawing.Point(125, 71);
            this.tb_Simple.Name = "tb_Simple";
            this.tb_Simple.Size = new System.Drawing.Size(100, 20);
            this.tb_Simple.TabIndex = 5;
            // 
            // tb_Percentage
            // 
            this.tb_Percentage.Location = new System.Drawing.Point(125, 45);
            this.tb_Percentage.Name = "tb_Percentage";
            this.tb_Percentage.Size = new System.Drawing.Size(100, 20);
            this.tb_Percentage.TabIndex = 4;
            // 
            // tb_Raw
            // 
            this.tb_Raw.Location = new System.Drawing.Point(125, 19);
            this.tb_Raw.Name = "tb_Raw";
            this.tb_Raw.Size = new System.Drawing.Size(100, 20);
            this.tb_Raw.TabIndex = 3;
            // 
            // rb_Simple
            // 
            this.rb_Simple.AutoSize = true;
            this.rb_Simple.Location = new System.Drawing.Point(6, 65);
            this.rb_Simple.Name = "rb_Simple";
            this.rb_Simple.Size = new System.Drawing.Size(56, 17);
            this.rb_Simple.TabIndex = 2;
            this.rb_Simple.TabStop = true;
            this.rb_Simple.Text = "Simple";
            this.rb_Simple.UseVisualStyleBackColor = true;
            // 
            // rb_Percentage
            // 
            this.rb_Percentage.AutoSize = true;
            this.rb_Percentage.Location = new System.Drawing.Point(6, 42);
            this.rb_Percentage.Name = "rb_Percentage";
            this.rb_Percentage.Size = new System.Drawing.Size(80, 17);
            this.rb_Percentage.TabIndex = 1;
            this.rb_Percentage.TabStop = true;
            this.rb_Percentage.Text = "Percentage";
            this.rb_Percentage.UseVisualStyleBackColor = true;
            // 
            // rb_Raw
            // 
            this.rb_Raw.AutoSize = true;
            this.rb_Raw.Location = new System.Drawing.Point(6, 19);
            this.rb_Raw.Name = "rb_Raw";
            this.rb_Raw.Size = new System.Drawing.Size(47, 17);
            this.rb_Raw.TabIndex = 0;
            this.rb_Raw.TabStop = true;
            this.rb_Raw.Text = "Raw";
            this.rb_Raw.UseVisualStyleBackColor = true;
            // 
            // btn_Refresh
            // 
            this.btn_Refresh.Location = new System.Drawing.Point(579, 258);
            this.btn_Refresh.Name = "btn_Refresh";
            this.btn_Refresh.Size = new System.Drawing.Size(107, 23);
            this.btn_Refresh.TabIndex = 2;
            this.btn_Refresh.Text = "RefreshDevices";
            this.btn_Refresh.UseVisualStyleBackColor = true;
            this.btn_Refresh.Click += new System.EventHandler(this.Btn_Refresh_Click);
            // 
            // listView1
            // 
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.DevicesHeader});
            this.listView1.FullRowSelect = true;
            this.listView1.GridLines = true;
            this.listView1.HideSelection = false;
            this.listView1.Location = new System.Drawing.Point(12, 12);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(405, 269);
            this.listView1.TabIndex = 3;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            // 
            // DevicesHeader
            // 
            this.DevicesHeader.Text = "Devices";
            this.DevicesHeader.Width = 400;
            // 
            // btn_SaveData
            // 
            this.btn_SaveData.Location = new System.Drawing.Point(423, 118);
            this.btn_SaveData.Name = "btn_SaveData";
            this.btn_SaveData.Size = new System.Drawing.Size(75, 23);
            this.btn_SaveData.TabIndex = 4;
            this.btn_SaveData.Text = "Save Data";
            this.btn_SaveData.UseVisualStyleBackColor = true;
            this.btn_SaveData.Click += new System.EventHandler(this.Btn_SaveData_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(573, 118);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 5;
            this.button1.Text = "Save Data";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // Form_TDPSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(698, 293);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.btn_SaveData);
            this.Controls.Add(this.listView1);
            this.Controls.Add(this.btn_Refresh);
            this.Controls.Add(this.groupBoxTDPMode);
            this.Name = "Form_TDPSettings";
            this.Text = "Form_TDPSettings";
            this.groupBoxTDPMode.ResumeLayout(false);
            this.groupBoxTDPMode.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.GroupBox groupBoxTDPMode;
        private System.Windows.Forms.TextBox tb_Simple;
        private System.Windows.Forms.TextBox tb_Percentage;
        private System.Windows.Forms.TextBox tb_Raw;
        private System.Windows.Forms.RadioButton rb_Simple;
        private System.Windows.Forms.RadioButton rb_Percentage;
        private System.Windows.Forms.RadioButton rb_Raw;
        private System.Windows.Forms.Button btn_Refresh;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ColumnHeader DevicesHeader;
        private System.Windows.Forms.Button btn_SaveData;
        private System.Windows.Forms.Button button1;
    }
}
