using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Model
{
    public class Area
    {
        public double LowerX { get; set; }

        public double UpperX { get; set; }

        public double LowerY { get; set; }

        public double UpperY { get; set; }

        public override string ToString()
        {
            return $"LowerX: {LowerX} UpperX: {UpperX} LowerY: {LowerY} UpperY: {UpperY}";
        }
    }
}
