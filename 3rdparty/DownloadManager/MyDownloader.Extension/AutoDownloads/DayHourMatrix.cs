using System;
using System.Collections.Generic;
using System.Text;

namespace MyDownloader.Extension.AutoDownloads
{
    [Serializable]
    public enum EnableMode: int
    {
        Active = 1,
        ActiveWithLimit,
        Disabled
    }

    public class DayHourMatrix
    {
        public const int DAYS = 7;
        public const int HOURS = 24;

        private EnableMode[,] enabledAt = new EnableMode[DAYS, HOURS];

        public DayHourMatrix()
        {
        }

        public DayHourMatrix(string data)
        {
            if (data == null || (data = data.Trim()).Length == 0)
            {
                return;
            }

            string[] days = data.Split('|');

            for (int i = 0; i < days.Length; i++)
            {
                string[] values = days[i].Split(',');
                if (values.Length == 3)
                {
                    int day = int.Parse(values[0]);
                    int hour = int.Parse(values[1]);
                    EnableMode em = (EnableMode)int.Parse(values[2]);

                    this[(DayOfWeek)day, hour] = em;
                }
            }
        }

        public EnableMode this[DayOfWeek day, int hour]
        {
            get
            {
                return enabledAt[(int)day, hour];
            }
            set
            {
                enabledAt[(int)day, hour] = value;
            }
        }

        public override string ToString()
        {
            string selected = String.Empty;

            for (int i = 0; i < DayHourMatrix.DAYS; i++)
            {
                for (int j = 0; j < DayHourMatrix.HOURS; j++)
                {
                    if (this[(DayOfWeek)i, j] != EnableMode.Disabled)
                    {
                        if (selected.Length > 0)
                        {
                            selected += "|";
                        }

                        // day,hour,mode|day,hour,mode....
                        selected += i + "," + j + "," + (int)this[(DayOfWeek)i, j];
                    }
                }
            }

            return selected;
        }
    }
}
