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
            this.pluginInfoItemRow4 = new NiceHashMiner.Forms.Components.PluginInfoItemRow();
            this.groupBox1.SuspendLayout();
            this.flowLayoutPanelPluginsLV.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox1.Controls.Add(this.flowLayoutPanelPluginsLV);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(590, 327);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Plugins:";
            // 
            // flowLayoutPanelPluginsLV
            // 
            this.flowLayoutPanelPluginsLV.AutoScroll = true;
            this.flowLayoutPanelPluginsLV.Controls.Add(this.pluginInfoItemRow4);
            this.flowLayoutPanelPluginsLV.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanelPluginsLV.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanelPluginsLV.Location = new System.Drawing.Point(3, 16);
            this.flowLayoutPanelPluginsLV.Name = "flowLayoutPanelPluginsLV";
            this.flowLayoutPanelPluginsLV.Size = new System.Drawing.Size(584, 308);
            this.flowLayoutPanelPluginsLV.TabIndex = 0;
            this.flowLayoutPanelPluginsLV.WrapContents = false;
            // 
            // pluginInfoItemRow4
            // 
            this.pluginInfoItemRow4.Location = new System.Drawing.Point(3, 3);
            this.pluginInfoItemRow4.Name = "pluginInfoItemRow4";
            this.pluginInfoItemRow4.Size = new System.Drawing.Size(560, 182);
            this.pluginInfoItemRow4.TabIndex = 0;
            // 
            // Form_MinerPlugins
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(614, 351);
            this.Controls.Add(this.groupBox1);
            this.MaximumSize = new System.Drawing.Size(630, 1200);
            this.MinimumSize = new System.Drawing.Size(630, 390);
            this.Name = "Form_MinerPlugins";
            this.Text = "Miner Plugins";
            this.groupBox1.ResumeLayout(false);
            this.flowLayoutPanelPluginsLV.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelPluginsLV;
        private Components.PluginInfoItem pluginInfoItem1;
        private Components.PluginInfoItemRow pluginInfoItemRow1;
        private Components.PluginInfoItemRow pluginInfoItemRow2;
        private Components.PluginInfoItemRow pluginInfoItemRow3;
        private Components.PluginInfoItemRow pluginInfoItemRow4;
    }
}