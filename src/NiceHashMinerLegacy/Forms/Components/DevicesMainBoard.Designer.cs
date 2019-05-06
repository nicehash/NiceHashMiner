namespace NiceHashMiner.Forms.Components
{
    partial class DevicesMainBoard
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
            this.devicesDataGridView = new System.Windows.Forms.DataGridView();
            this.Enable = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.deviceHeader = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.StatusColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnTemperature = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnLoad = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnRMP = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Run = new System.Windows.Forms.DataGridViewButtonColumn();
            this.algorithmsEnabled = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.devicesDataGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // devicesDataGridView
            // 
            this.devicesDataGridView.AllowUserToAddRows = false;
            this.devicesDataGridView.AllowUserToDeleteRows = false;
            this.devicesDataGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.devicesDataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.devicesDataGridView.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.devicesDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.devicesDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Enable,
            this.deviceHeader,
            this.StatusColumn,
            this.ColumnTemperature,
            this.ColumnLoad,
            this.ColumnRMP,
            this.Run,
            this.algorithmsEnabled});
            this.devicesDataGridView.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.devicesDataGridView.Location = new System.Drawing.Point(0, 0);
            this.devicesDataGridView.Margin = new System.Windows.Forms.Padding(10);
            this.devicesDataGridView.Name = "devicesDataGridView";
            this.devicesDataGridView.ReadOnly = true;
            this.devicesDataGridView.RowHeadersVisible = false;
            this.devicesDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.devicesDataGridView.Size = new System.Drawing.Size(813, 223);
            this.devicesDataGridView.TabIndex = 114;
            // 
            // Enable
            // 
            this.Enable.FalseValue = "NO";
            this.Enable.HeaderText = "Enabled";
            this.Enable.Name = "Enable";
            this.Enable.ReadOnly = true;
            this.Enable.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.Enable.TrueValue = "Ye43s";
            this.Enable.Width = 52;
            // 
            // deviceHeader
            // 
            this.deviceHeader.HeaderText = "Device";
            this.deviceHeader.Name = "deviceHeader";
            this.deviceHeader.ReadOnly = true;
            this.deviceHeader.Width = 66;
            // 
            // StatusColumn
            // 
            this.StatusColumn.HeaderText = "Status";
            this.StatusColumn.Name = "StatusColumn";
            this.StatusColumn.ReadOnly = true;
            this.StatusColumn.Width = 62;
            // 
            // ColumnTemperature
            // 
            this.ColumnTemperature.HeaderText = "Temp (°C)";
            this.ColumnTemperature.Name = "ColumnTemperature";
            this.ColumnTemperature.ReadOnly = true;
            this.ColumnTemperature.Width = 79;
            // 
            // ColumnLoad
            // 
            this.ColumnLoad.HeaderText = "Load (%)";
            this.ColumnLoad.Name = "ColumnLoad";
            this.ColumnLoad.ReadOnly = true;
            this.ColumnLoad.Width = 73;
            // 
            // ColumnRMP
            // 
            this.ColumnRMP.HeaderText = "RPM";
            this.ColumnRMP.Name = "ColumnRMP";
            this.ColumnRMP.ReadOnly = true;
            this.ColumnRMP.Width = 56;
            // 
            // Run
            // 
            this.Run.HeaderText = "Start / Stop";
            this.Run.Name = "Run";
            this.Run.ReadOnly = true;
            this.Run.Text = "Start";
            this.Run.Width = 68;
            // 
            // algorithmsEnabled
            // 
            this.algorithmsEnabled.HeaderText = "Algorithms/Enabled/Benchmarked";
            this.algorithmsEnabled.Name = "algorithmsEnabled";
            this.algorithmsEnabled.ReadOnly = true;
            this.algorithmsEnabled.Width = 195;
            // 
            // DevicesMainBoard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.devicesDataGridView);
            this.Name = "DevicesMainBoard";
            this.Size = new System.Drawing.Size(813, 223);
            ((System.ComponentModel.ISupportInitialize)(this.devicesDataGridView)).EndInit();
            this.ResumeLayout(false);

        }

#endregion

        private System.Windows.Forms.DataGridView devicesDataGridView;
        private System.Windows.Forms.DataGridViewCheckBoxColumn Enable;
        private System.Windows.Forms.DataGridViewTextBoxColumn deviceHeader;
        private System.Windows.Forms.DataGridViewTextBoxColumn StatusColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnTemperature;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnLoad;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnRMP;
        private System.Windows.Forms.DataGridViewButtonColumn Run;
        private System.Windows.Forms.DataGridViewTextBoxColumn algorithmsEnabled;
    }
}
