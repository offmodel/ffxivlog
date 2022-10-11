using System;

namespace Offmodel.FFXIV.Log.Model
{
    public class Position
    {
        public double X { get; }
        public double Y { get; }
        public double Z { get; }
        /** Direction facing in radians, 0 = south */
        public double R { get; }

        public Position(LogLine line, int startPos)
        {
            X = Double.Parse(line.Text(startPos++));
            Y = Double.Parse(line.Text(startPos++));
            Z = Double.Parse(line.Text(startPos++));
            R = Double.Parse(line.Text(startPos++));
        }
    }
}
