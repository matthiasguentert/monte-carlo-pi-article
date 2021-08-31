using System.Text;

namespace ClusterHead
{
    public class AppConfig
    {
        public ulong IterationsTotal { get; set; }

        public uint UnitsTotal { get; set; }

        public string BatchAccountUrl { get; set; }

        public string BatchAccountName { get; set; }

        public string ConnectionString { get; set; }

        public string BatchAccountKey { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine($"IterationsTotal: {IterationsTotal}");
            sb.AppendLine($"UnitsToGenerate: {UnitsTotal}");
            sb.AppendLine($"BatchAccountUrl: {BatchAccountUrl}");
            sb.AppendLine($"BatchAccountName: {BatchAccountName}");
            sb.AppendLine($"BatchAccountKey: {BatchAccountKey.Substring(0, 20)}...");
            sb.AppendLine($"StorageAccount: {ConnectionString.Substring(0, 80)}...");

            return sb.ToString();
        }
    }
}
