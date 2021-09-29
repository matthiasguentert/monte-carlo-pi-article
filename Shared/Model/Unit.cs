namespace Shared.Model
{
    public class Unit
    {
        public Square Square {  get; set; }

        public Alignment Alignment {  get; set; }

        public ulong NumRandomPoints { get; set; }

        public ulong CircleHits { get; set; }

        public long ElapsedMilliseconds { get; set; }

        public override string ToString()
        {
            return $"Square: {Square} NumRandomPoints: {NumRandomPoints}";
        }
    }
}
