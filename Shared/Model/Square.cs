using System.Collections.Generic;

namespace Shared.Model
{
    public class Square
    {
        public List<Point> Points { get; set; } = new List<Point>();

        public override string ToString()
        {
            return $"p1: {Points[0]} p2: {Points[1]} p3: {Points[2]} p4: {Points[3]}";
        }
    }
}
