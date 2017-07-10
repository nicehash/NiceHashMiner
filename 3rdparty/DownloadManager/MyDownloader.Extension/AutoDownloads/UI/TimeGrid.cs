using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace MyDownloader.Extension.AutoDownloads.UI
{
    public partial class TimeGrid : UserControl
    {
        object Selected = new object();
        object Unselected = new object();
        object SelectedWithLimit = new object();

        Color UnselectedColor = Color.DarkGray;
        Color SelectedColor = Color.Green;
        Color SelectedWithLimitColor = Color.PaleGreen;

        private Point startPosition;

        // days x hours
        PictureBox[,] timePanels = new PictureBox[DayHourMatrix.DAYS, DayHourMatrix.HOURS];
        Label[] timeLbs = new Label[DayHourMatrix.HOURS];
        Label[] dayLbl = new Label[DayHourMatrix.DAYS];
        PictureBox lastPanel;
        DayHourMatrix matrix = new DayHourMatrix();

        public TimeGrid()
        {
            InitializeComponent();

            this.SuspendLayout();

            for (int i = 0; i < DayHourMatrix.DAYS; i++)
            {
                for (int j = 0; j < DayHourMatrix.HOURS; j++)
                {
                    timePanels[i, j] = new PictureBox();
                    InitPanel(timePanels[i, j]);
                }
            }

            for (int i = 0; i < DayHourMatrix.DAYS; i++)
            {
                dayLbl[i] = new Label();
                dayLbl[i].AutoSize = false;
                dayLbl[i].TextAlign = ContentAlignment.MiddleLeft;
                dayLbl[i].Text = ((DayOfWeek)i).ToString().Substring(0, 3);
                dayLbl[i].Visible = true;
                this.Controls.Add(dayLbl[i]);
            }

            for (int i = 0; i < DayHourMatrix.HOURS; i++)
            {
                timeLbs[i] = new Label();
                timeLbs[i].AutoSize = true;
                //timeLbs[i].TextAlign = ContentAlignment.MiddleCenter;
                timeLbs[i].Text = i.ToString("0#");
                timeLbs[i].Visible = true;
                this.Controls.Add(timeLbs[i]);
            }

            this.ResumeLayout(true);
        }

        public Point StartPosition
        {
            get 
            { 
                return startPosition; 
            }
            set 
            { 
                startPosition = value;
                this.layoutPanels();
            }
        }

        public DayHourMatrix SelectedTimes
        {
            get
            {
                return matrix;
            }
            set
            {
                matrix = value;

                // unselect all
                for (int i = 0; i < DayHourMatrix.DAYS; i++)
                {
                    for (int j = 0; j < DayHourMatrix.HOURS; j++)
                    {
                        object v = Unselected;

                        if (matrix != null)
                        {
                            EnableMode em = matrix[(DayOfWeek)i, j];
                            if (em == EnableMode.Active)
                            {
                                v = Selected;
                            }
                            if (em == EnableMode.ActiveWithLimit)
                            {
                                v = SelectedWithLimit;
                            }
                        }

                        SetPanel(timePanels[i, j], v);
                    }
                }
            }
        }

        private void InitPanel(PictureBox p)
        {
            p.BackColor = UnselectedColor;
            p.BorderStyle = BorderStyle.FixedSingle;
            p.Visible = true;
            p.MouseMove += new MouseEventHandler(panel_MouseMove);
            p.Tag = Unselected;
            this.Controls.Add(p);
        }

        void panel_MouseMove(object sender, MouseEventArgs e)
        {
            Point pt = PointToClient(Cursor.Position);

            pt.Offset(-this.StartPosition.X, -this.StartPosition.Y);

            int hour = Math.Max(0, Math.Min(pt.X / timePanels[0, 0].Width, DayHourMatrix.HOURS - 1));
            int day = Math.Max(0, Math.Min(pt.Y / timePanels[0, 0].Height, DayHourMatrix.DAYS - 1));

            PictureBox p = timePanels[day, hour];

            if (p == lastPanel) return;

            if (e.Button == MouseButtons.Left)
            {
                if ((Control.ModifierKeys == Keys.Shift))
                {
                    SetPanel(p, SelectedWithLimit);
                    matrix[(DayOfWeek)day, hour] = EnableMode.ActiveWithLimit;
                }
                else
                {
                    SetPanel(p, Selected);
                    matrix[(DayOfWeek)day, hour] = EnableMode.Active;
                }
            }
            else if (e.Button == MouseButtons.Right)
            {
                SetPanel(p, Unselected);
                matrix[(DayOfWeek)day, hour] = EnableMode.Disabled;
            }            
        }

        private void SetPanel(PictureBox p, object action)
        {
            if (action == null) action = p.Tag;

            if (action == Selected)
            {
                p.Tag = Selected;
                p.BackColor = SelectedColor;
            }
            else if (action == Unselected)
            {
                p.Tag = Unselected;
                p.BackColor = UnselectedColor;
            }
            else 
            {
                p.Tag = SelectedWithLimit;
                p.BackColor = SelectedWithLimitColor;
            }

            lastPanel = p;
        }

        private void layoutPanels()
        {
            int width = (this.Width - StartPosition.X) / DayHourMatrix.HOURS;
            int height = (this.Height - StartPosition.Y) / DayHourMatrix.DAYS;

            for (int i = 0; i < DayHourMatrix.DAYS; i++)
            {
                dayLbl[i].Location = new Point(0, i * height + StartPosition.Y);
                dayLbl[i].Size = new Size(StartPosition.X, height);

                for (int j = 0; j < DayHourMatrix.HOURS; j++)
                {
                    timePanels[i, j].Location = new Point(j * width + StartPosition.X, i * height + StartPosition.Y);
                    timePanels[i, j].Size = new Size(width, height);
                }
            }

            for (int j = 0; j < DayHourMatrix.HOURS; j++)
            {
                timeLbs[j].Location = new Point(j * width + StartPosition.X, 0);
                timeLbs[j].Size = new Size(width, StartPosition.Y);
            }
        }

        private void TimeGrid_Layout(object sender, LayoutEventArgs e)
        {
            layoutPanels();
        }

        private void TimeGrid_Load(object sender, EventArgs e)
        {
            layoutPanels();
        }
    }
}
