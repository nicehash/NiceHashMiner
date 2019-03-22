namespace NiceHashMiner.Forms.Components
{
    partial class PluginInfoItemRow
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
            this.pluginInfoItem1 = new NiceHashMiner.Forms.Components.PluginInfoItem();
            this.pluginInfoItem2 = new NiceHashMiner.Forms.Components.PluginInfoItem();
            this.SuspendLayout();
            // 
            // pluginInfoItem1
            // 
            this.pluginInfoItem1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pluginInfoItem1.ButtonInstallRemoveEnabled = true;
            this.pluginInfoItem1.ButtonInstallRemoveText = "Install/Remove";
            this.pluginInfoItem1.ButtonUpdateText = "Update";
            this.pluginInfoItem1.Location = new System.Drawing.Point(3, 3);
            this.pluginInfoItem1.Name = "pluginInfoItem1";
            this.pluginInfoItem1.PluginAuthor = "labelAuthor";
            this.pluginInfoItem1.PluginName = "Name";
            this.pluginInfoItem1.PluginUUID = null;
            this.pluginInfoItem1.PluginVersion = "version";
            this.pluginInfoItem1.ProgressBarValue = 0;
            this.pluginInfoItem1.ProgressBarVisible = false;
            this.pluginInfoItem1.Size = new System.Drawing.Size(274, 176);
            this.pluginInfoItem1.StatusText = "Status";
            this.pluginInfoItem1.TabIndex = 0;
            // 
            // pluginInfoItem2
            // 
            this.pluginInfoItem2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pluginInfoItem2.ButtonInstallRemoveEnabled = true;
            this.pluginInfoItem2.ButtonInstallRemoveText = "Install/Remove";
            this.pluginInfoItem2.ButtonUpdateText = "Update";
            this.pluginInfoItem2.Location = new System.Drawing.Point(283, 3);
            this.pluginInfoItem2.Name = "pluginInfoItem2";
            this.pluginInfoItem2.PluginAuthor = "labelAuthor";
            this.pluginInfoItem2.PluginName = "Name";
            this.pluginInfoItem2.PluginUUID = null;
            this.pluginInfoItem2.PluginVersion = "version";
            this.pluginInfoItem2.ProgressBarValue = 0;
            this.pluginInfoItem2.ProgressBarVisible = false;
            this.pluginInfoItem2.Size = new System.Drawing.Size(274, 176);
            this.pluginInfoItem2.StatusText = "Status";
            this.pluginInfoItem2.TabIndex = 1;
            // 
            // PluginInfoItemRow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pluginInfoItem2);
            this.Controls.Add(this.pluginInfoItem1);
            this.Name = "PluginInfoItemRow";
            this.Size = new System.Drawing.Size(560, 182);
            this.ResumeLayout(false);

        }

        #endregion

        private PluginInfoItem pluginInfoItem1;
        private PluginInfoItem pluginInfoItem2;
    }
}
