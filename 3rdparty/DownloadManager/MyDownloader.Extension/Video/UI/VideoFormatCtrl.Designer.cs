namespace MyDownloader.Extension.Video.UI
{
    partial class VideoFormatCtrl
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
            this.chkConvert = new System.Windows.Forms.CheckBox();
            this.cboTypes = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // chkConvert
            // 
            this.chkConvert.AutoSize = true;
            this.chkConvert.Dock = System.Windows.Forms.DockStyle.Top;
            this.chkConvert.Location = new System.Drawing.Point(0, 0);
            this.chkConvert.Name = "chkConvert";
            this.chkConvert.Size = new System.Drawing.Size(202, 17);
            this.chkConvert.TabIndex = 0;
            this.chkConvert.Text = "Convert when done:";
            this.chkConvert.UseVisualStyleBackColor = true;
            this.chkConvert.CheckedChanged += new System.EventHandler(this.chkConvert_CheckedChanged);
            // 
            // cboTypes
            // 
            this.cboTypes.Dock = System.Windows.Forms.DockStyle.Top;
            this.cboTypes.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboTypes.Items.AddRange(new object[] {
            "AVI",
            "MPEG",
            "MP3 (Audio Only)"});
            this.cboTypes.Location = new System.Drawing.Point(0, 17);
            this.cboTypes.Name = "cboTypes";
            this.cboTypes.Size = new System.Drawing.Size(202, 21);
            this.cboTypes.TabIndex = 1;
            this.cboTypes.SelectedIndexChanged += new System.EventHandler(this.cboTypes_SelectedIndexChanged);
            // 
            // VideoFormatCtrl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.cboTypes);
            this.Controls.Add(this.chkConvert);
            this.Name = "VideoFormatCtrl";
            this.Size = new System.Drawing.Size(202, 52);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox chkConvert;
        private System.Windows.Forms.ComboBox cboTypes;
    }
}
