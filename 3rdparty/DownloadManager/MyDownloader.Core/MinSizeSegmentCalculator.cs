using System;
using System.Collections.Generic;
using System.Text;

namespace MyDownloader.Core
{
    public class MinSizeSegmentCalculator: ISegmentCalculator
    {
        #region ISegmentCalculator Members

        public CalculatedSegment[] GetSegments(int segmentCount, RemoteFileInfo remoteFileInfo)
        {
            long minSize = Settings.Default.MinSegmentSize;
            long segmentSize = remoteFileInfo.FileSize / (long)segmentCount;

            while (segmentCount > 1 && segmentSize < minSize)
            {
                segmentCount--;
                segmentSize = remoteFileInfo.FileSize / (long)segmentCount;
            }

            long startPosition = 0;

            List<CalculatedSegment> segments = new List<CalculatedSegment>();

            for (int i = 0; i < segmentCount; i++)
            {
                if (segmentCount - 1 == i)
                {
                    segments.Add(new CalculatedSegment(startPosition, remoteFileInfo.FileSize));
                }
                else
                {
                    segments.Add(new CalculatedSegment(startPosition, startPosition + (int)segmentSize));
                }

                startPosition = segments[segments.Count - 1].EndPosition;
            }

            return segments.ToArray();
        }

        #endregion
    }
}
