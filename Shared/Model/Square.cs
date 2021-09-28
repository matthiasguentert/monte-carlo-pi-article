using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Shared.Model
{
    public class Square
    {
        public (double x, double y) Point1;

        public (double x, double y) Point2;

        public (double x, double y) Point3;

        public (double x, double y) Point4;
    }

    public class Square1
    {
        public List<Point> Points { get; set; } = new List<Point>();

        public override string ToString()
        {
            return $"p1: {Points[0]} p2: {Points[1]} p3: {Points[2]} p4: {Points[3]}";
        }
    }

    public class Point
    {
        public Point (double x, double y)
        {
            this.X = x;
            this.Y = y;
        }

        public double X { get; set; }

        public double Y { get; set; } 

        public override string ToString()
        {
            return $"{X},{Y}";
        }
    }
}
