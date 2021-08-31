namespace Shared.Model
{
    public class Unit
    {
        public Area Area { get; set; }

        public ulong NumRandomPoints { get; set; }

        public ulong CircleHits { get; set; }

        public long ElapsedMilliseconds { get; set; }

        public override string ToString()
        {
            return $"Area: {Area} NumRandomPoints: {NumRandomPoints}";
        }
    }
}
