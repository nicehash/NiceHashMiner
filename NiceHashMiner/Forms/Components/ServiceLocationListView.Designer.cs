namespace NiceHashMiner.Forms.Components
{
    partial class ServiceLocationListView
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
            this.listView_ServiceLocations = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.buttonUp = new System.Windows.Forms.Button();
            this.buttonDown = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // listView_ServiceLocations
            // 
            this.listView_ServiceLocations.CheckBoxes = true;
            this.listView_ServiceLocations.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
            this.listView_ServiceLocations.FullRowSelect = true;
            this.listView_ServiceLocations.GridLines = true;
            this.listView_ServiceLocations.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.listView_ServiceLocations.Location = new System.Drawing.Point(0, 0);
            this.listView_ServiceLocations.MultiSelect = false;
            this.listView_ServiceLocations.Name = "listView_ServiceLocations";
            this.listView_ServiceLocations.Size = new System.Drawing.Size(210, 135);
            this.listView_ServiceLocations.TabIndex = 0;
            this.listView_ServiceLocations.UseCompatibleStateImageBehavior = false;
            this.listView_ServiceLocations.View = System.Windows.Forms.View.Details;
            this.listView_ServiceLocations.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.listView_ServiceLocations_ItemCheck);
            this.listView_ServiceLocations.Enter += new System.EventHandler(this.listView_ServiceLocations_Enter);
            this.listView_ServiceLocations.Leave += new System.EventHandler(this.listView_ServiceLocations_Leave);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "ServiceLocation";
            this.columnHeader1.Width = 130;
            // 
            // buttonUp
            // 
            this.buttonUp.Location = new System.Drawing.Point(213, 37);
            this.buttonUp.Name = "buttonUp";
            this.buttonUp.Size = new System.Drawing.Size(23, 23);
            this.buttonUp.TabIndex = 1;
            this.buttonUp.Text = "˄";
            this.buttonUp.UseVisualStyleBackColor = true;
            this.buttonUp.Click += new System.EventHandler(this.buttonUp_Click);
            // 
            // buttonDown
            // 
            this.buttonDown.Location = new System.Drawing.Point(213, 66);
            this.buttonDown.Name = "buttonDown";
            this.buttonDown.Size = new System.Drawing.Size(23, 23);
            this.buttonDown.TabIndex = 2;
            this.buttonDown.Text = "˅";
            this.buttonDown.UseVisualStyleBackColor = true;
            this.buttonDown.Click += new System.EventHandler(this.buttonDown_Click);
            // 
            // ServiceLocationListView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.buttonDown);
            this.Controls.Add(this.buttonUp);
            this.Controls.Add(this.listView_ServiceLocations);
            this.Name = "ServiceLocationListView";
            this.Size = new System.Drawing.Size(240, 135);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView listView_ServiceLocations;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.Button buttonUp;
        private System.Windows.Forms.Button buttonDown;
    }
}
