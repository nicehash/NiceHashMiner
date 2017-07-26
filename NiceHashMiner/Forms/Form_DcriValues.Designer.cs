namespace NiceHashMiner.Forms
{
    partial class Form_DcriValues
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.listView_Intensities = new System.Windows.Forms.ListView();
            this.columnHeader_DcriValue = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_Speed = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_SecondarySpeed = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_Profit = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.button_Close = new System.Windows.Forms.Button();
            this.button_Save = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // listView_Intensities
            // 
            this.listView_Intensities.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView_Intensities.BackColor = System.Drawing.SystemColors.Window;
            this.listView_Intensities.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_DcriValue,
            this.columnHeader_Speed,
            this.columnHeader_SecondarySpeed,
            this.columnHeader_Profit});
            this.listView_Intensities.FullRowSelect = true;
            this.listView_Intensities.GridLines = true;
            this.listView_Intensities.Location = new System.Drawing.Point(12, 12);
            this.listView_Intensities.MultiSelect = false;
            this.listView_Intensities.Name = "listView_Intensities";
            this.listView_Intensities.Size = new System.Drawing.Size(738, 663);
            this.listView_Intensities.TabIndex = 0;
            this.listView_Intensities.UseCompatibleStateImageBehavior = false;
            this.listView_Intensities.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader_DcriValue
            // 
            this.columnHeader_DcriValue.Text = "Dcri Value";
            this.columnHeader_DcriValue.Width = 91;
            // 
            // columnHeader_Speed
            // 
            this.columnHeader_Speed.Text = "Speed";
            this.columnHeader_Speed.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeader_Speed.Width = 90;
            // 
            // columnHeader_SecondarySpeed
            // 
            this.columnHeader_SecondarySpeed.Text = "Secondary Speed";
            this.columnHeader_SecondarySpeed.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeader_SecondarySpeed.Width = 146;
            // 
            // columnHeader_Profit
            // 
            this.columnHeader_Profit.Text = "Profit (BTC/Day)";
            this.columnHeader_Profit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeader_Profit.Width = 128;
            // 
            // button_Close
            // 
            this.button_Close.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Close.Location = new System.Drawing.Point(616, 681);
            this.button_Close.Name = "button_Close";
            this.button_Close.Size = new System.Drawing.Size(134, 35);
            this.button_Close.TabIndex = 1;
            this.button_Close.Text = "Close";
            this.button_Close.UseVisualStyleBackColor = true;
            // 
            // button_Save
            // 
            this.button_Save.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Save.Location = new System.Drawing.Point(476, 681);
            this.button_Save.Name = "button_Save";
            this.button_Save.Size = new System.Drawing.Size(134, 35);
            this.button_Save.TabIndex = 2;
            this.button_Save.Text = "Save";
            this.button_Save.UseVisualStyleBackColor = true;
            // 
            // Form_DcriValues
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(762, 730);
            this.Controls.Add(this.button_Save);
            this.Controls.Add(this.button_Close);
            this.Controls.Add(this.listView_Intensities);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "Form_DcriValues";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Dcri Values";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView listView_Intensities;
        private System.Windows.Forms.Button button_Close;
        private System.Windows.Forms.Button button_Save;
        private System.Windows.Forms.ColumnHeader columnHeader_DcriValue;
        private System.Windows.Forms.ColumnHeader columnHeader_Speed;
        private System.Windows.Forms.ColumnHeader columnHeader_SecondarySpeed;
        private System.Windows.Forms.ColumnHeader columnHeader_Profit;
    }
}