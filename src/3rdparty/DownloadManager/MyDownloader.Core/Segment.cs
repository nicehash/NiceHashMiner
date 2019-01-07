using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace MyDownloader.Core
{
    public class Segment
    {
        private long startPosition;
        private int index;
        private string currentURL;
        private long initialStartPosition;
        private long endPosition;
        private Stream outputStream;
        private Stream inputStream;
        private Exception lastError;
        private SegmentState state;
        private bool started = false;
        private DateTime lastReception = DateTime.MinValue;
        private DateTime lastErrorDateTime = DateTime.MinValue;
        private double rate;
        private long start;
        private TimeSpan left = TimeSpan.Zero;
        private int currentTry;
        
        public int CurrentTry
        {
            get { return currentTry; }
            set { currentTry = value; }
        }
        
        public SegmentState State
        {
            get
            {
                return state;
            }
            set
            {
                state = value;

                switch (state)
                {
                    case SegmentState.Downloading:
                        BeginWork();
                        break;

                    case SegmentState.Connecting:
                    case SegmentState.Paused:
                    case SegmentState.Finished:
                    case SegmentState.Error:
                        rate = 0.0;
                        left = TimeSpan.Zero;
                        break;
                }
            }
        }

        public DateTime LastErrorDateTime
        {
            get
            {
                return lastErrorDateTime;
            }
        }

        public Exception LastError
        {
            get
            {
                return lastError;
            }
            set
            {
                if (value != null)
                {
                    lastErrorDateTime = DateTime.Now;
                }
                else
                {
                    lastErrorDateTime = DateTime.MinValue;
                }
                lastError = value;
            }
        }

        public int Index
        {
            get
            {
                return index;
            }
            set
            {
                index = value;
            }
        }

        public long InitialStartPosition
        {
            get
            {
                return initialStartPosition;
            }
            set
            {
                initialStartPosition = value;
            }
        }

        public long StartPosition
        {
            get
            {
                return startPosition;
            }
            set
            {
                startPosition = value;
            }
        }

        public long Transfered
        {
            get 
            {
                return this.StartPosition - this.InitialStartPosition; 
            }
        }

        public long TotalToTransfer
        {
            get
            {
                return (this.EndPosition <= 0? 0: this.EndPosition - this.InitialStartPosition);
            }
        }

        public long MissingTransfer
        {
            get
            {
                return (this.EndPosition <= 0? 0: this.EndPosition - this.StartPosition);
            }
        }


        public double Progress
        {
            get
            {
                return (this.EndPosition <= 0? 0: ((double)Transfered / (double)TotalToTransfer * 100.0f));
            }
        }

        public long EndPosition
        {
            get
            {
                return endPosition;
            }
            set
            {
                endPosition = value;
            }
        }

        public Stream OutputStream
        {
            get
            {
                return outputStream;
            }
            set
            {
                outputStream = value;
            }
        }

        public Stream InputStream
        {
            get
            {
                return inputStream;
            }
            set
            {
                inputStream = value;
            }
        }

        public string CurrentURL
        {
            get
            {
                return currentURL;
            }
            set
            {
                currentURL = value;
            }
        }

        public double Rate
        {
            get
            {
                if (this.State == SegmentState.Downloading)
                {
                    IncreaseStartPosition(0);
                    return rate;
                }
                else
                {
                    return 0;
                }
            }
        }

        public TimeSpan Left
        {
            get
            {
                return left;
            }
        }

        public void BeginWork()
        {
            start = startPosition;
            lastReception = DateTime.Now;
            started = true;
        }

        public void IncreaseStartPosition(long size)
        {
            lock (this)
            {
                DateTime now = DateTime.Now;

                startPosition += size;

                if (started)
                {
                    TimeSpan ts = (now - lastReception);
                    if (ts.TotalSeconds == 0)
                    {
                        return;
                    }

                    // bytes per seconds
                    rate = ((double)(startPosition - start)) / ts.TotalSeconds;

                    if (rate > 0.0)
                    {
                        left = TimeSpan.FromSeconds(MissingTransfer / rate);
                    }
                    else
                    {
                        left = TimeSpan.MaxValue;
                    }
                }
                else
                {
                    start = startPosition;
                    lastReception = now;
                    started = true;
                }
            }
        }
    }
}
