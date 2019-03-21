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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.flowLayoutPanelPluginsLV = new System.Windows.Forms.FlowLayoutPanel();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.pluginInfoItemRow1 = new NiceHashMiner.Forms.Components.PluginInfoItemRow();
            this.pluginInfoItemRow2 = new NiceHashMiner.Forms.Components.PluginInfoItemRow();
            this.pluginInfoItemRow3 = new NiceHashMiner.Forms.Components.PluginInfoItemRow();
            this.groupBox1.SuspendLayout();
            this.flowLayoutPanelPluginsLV.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox1.Controls.Add(this.flowLayoutPanelPluginsLV);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(619, 582);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Plugins:";
            // 
            // flowLayoutPanelPluginsLV
            // 
            this.flowLayoutPanelPluginsLV.AutoScroll = true;
            this.flowLayoutPanelPluginsLV.Controls.Add(this.pluginInfoItemRow1);
            this.flowLayoutPanelPluginsLV.Controls.Add(this.pluginInfoItemRow2);
            this.flowLayoutPanelPluginsLV.Controls.Add(this.pluginInfoItemRow3);
            this.flowLayoutPanelPluginsLV.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanelPluginsLV.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanelPluginsLV.Location = new System.Drawing.Point(3, 16);
            this.flowLayoutPanelPluginsLV.Name = "flowLayoutPanelPluginsLV";
            this.flowLayoutPanelPluginsLV.Size = new System.Drawing.Size(613, 563);
            this.flowLayoutPanelPluginsLV.TabIndex = 0;
            this.flowLayoutPanelPluginsLV.WrapContents = false;
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox2.Controls.Add(this.richTextBox1);
            this.groupBox2.Location = new System.Drawing.Point(839, 74);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(231, 292);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "groupBox - Plugin description";
            // 
            // richTextBox1
            // 
            this.richTextBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBox1.Location = new System.Drawing.Point(3, 16);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(225, 273);
            this.richTextBox1.TabIndex = 0;
            this.richTextBox1.Text = "";
            // 
            // pluginInfoItemRow1
            // 
            this.pluginInfoItemRow1.Location = new System.Drawing.Point(3, 3);
            this.pluginInfoItemRow1.Name = "pluginInfoItemRow1";
            this.pluginInfoItemRow1.Size = new System.Drawing.Size(606, 201);
            this.pluginInfoItemRow1.TabIndex = 0;
            // 
            // pluginInfoItemRow2
            // 
            this.pluginInfoItemRow2.Location = new System.Drawing.Point(3, 210);
            this.pluginInfoItemRow2.Name = "pluginInfoItemRow2";
            this.pluginInfoItemRow2.Size = new System.Drawing.Size(606, 201);
            this.pluginInfoItemRow2.TabIndex = 1;
            // 
            // pluginInfoItemRow3
            // 
            this.pluginInfoItemRow3.Location = new System.Drawing.Point(3, 417);
            this.pluginInfoItemRow3.Name = "pluginInfoItemRow3";
            this.pluginInfoItemRow3.Size = new System.Drawing.Size(606, 201);
            this.pluginInfoItemRow3.TabIndex = 2;
            // 
            // Form_MinerPlugins
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1551, 599);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "Form_MinerPlugins";
            this.Text = "Form_MinerPlugins";
            this.groupBox1.ResumeLayout(false);
            this.flowLayoutPanelPluginsLV.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelPluginsLV;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private Components.PluginInfoItem pluginInfoItem1;
        private Components.PluginInfoItemRow pluginInfoItemRow1;
        private Components.PluginInfoItemRow pluginInfoItemRow2;
        private Components.PluginInfoItemRow pluginInfoItemRow3;
    }
}