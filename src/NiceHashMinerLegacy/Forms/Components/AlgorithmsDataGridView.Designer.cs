namespace NiceHashMiner.Forms.Components
{
    partial class AlgorithmsDataGridView
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
            this.dataGridView = new System.Windows.Forms.DataGridView();
            this.Enable = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.deviceHeader = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnMiner = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnSpeed = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnSMARate = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnBTCPerDay = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.StatusColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridView
            // 
            this.dataGridView.AllowUserToAddRows = false;
            this.dataGridView.AllowUserToDeleteRows = false;
            this.dataGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dataGridView.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.dataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Enable,
            this.deviceHeader,
            this.ColumnMiner,
            this.ColumnSpeed,
            this.ColumnSMARate,
            this.ColumnBTCPerDay,
            this.StatusColumn});
            this.dataGridView.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.dataGridView.Location = new System.Drawing.Point(0, 0);
            this.dataGridView.Margin = new System.Windows.Forms.Padding(10);
            this.dataGridView.Name = "dataGridView";
            this.dataGridView.ReadOnly = true;
            this.dataGridView.RowHeadersVisible = false;
            this.dataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridView.Size = new System.Drawing.Size(743, 177);
            this.dataGridView.TabIndex = 115;
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
            this.deviceHeader.HeaderText = "Algorithm";
            this.deviceHeader.Name = "deviceHeader";
            this.deviceHeader.ReadOnly = true;
            this.deviceHeader.Width = 75;
            // 
            // ColumnMiner
            // 
            this.ColumnMiner.HeaderText = "Miner";
            this.ColumnMiner.Name = "ColumnMiner";
            this.ColumnMiner.ReadOnly = true;
            this.ColumnMiner.Width = 58;
            // 
            // ColumnSpeed
            // 
            this.ColumnSpeed.HeaderText = "Speed";
            this.ColumnSpeed.Name = "ColumnSpeed";
            this.ColumnSpeed.ReadOnly = true;
            this.ColumnSpeed.Width = 63;
            // 
            // ColumnSMARate
            // 
            this.ColumnSMARate.HeaderText = "SMA BTC/GH/Day";
            this.ColumnSMARate.Name = "ColumnSMARate";
            this.ColumnSMARate.ReadOnly = true;
            this.ColumnSMARate.Width = 114;
            // 
            // ColumnBTCPerDay
            // 
            this.ColumnBTCPerDay.HeaderText = "BTC/Day";
            this.ColumnBTCPerDay.Name = "ColumnBTCPerDay";
            this.ColumnBTCPerDay.ReadOnly = true;
            this.ColumnBTCPerDay.Width = 77;
            // 
            // StatusColumn
            // 
            this.StatusColumn.HeaderText = "Status";
            this.StatusColumn.Name = "StatusColumn";
            this.StatusColumn.ReadOnly = true;
            this.StatusColumn.Width = 62;
            // 
            // AlgorithmsDataGridView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.dataGridView);
            this.Name = "AlgorithmsDataGridView";
            this.Size = new System.Drawing.Size(775, 209);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridView;
        private System.Windows.Forms.DataGridViewCheckBoxColumn Enable;
        private System.Windows.Forms.DataGridViewTextBoxColumn deviceHeader;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnMiner;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnSpeed;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnSMARate;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnBTCPerDay;
        private System.Windows.Forms.DataGridViewTextBoxColumn StatusColumn;
    }
}
