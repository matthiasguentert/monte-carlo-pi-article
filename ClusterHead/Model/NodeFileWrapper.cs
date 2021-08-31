using Microsoft.Azure.Batch;
using System.Threading.Tasks;

namespace ClusterHead.Model
{
    public interface INodeFileWrapper
    {
        public Task<string> ReadAsStringAsync();
    }

    public class NodeFileWrapper : INodeFileWrapper
    {
        private readonly NodeFile nodeFile;

        public NodeFileWrapper(NodeFile nodeFile) => this.nodeFile = nodeFile;

        public async Task<string> ReadAsStringAsync()
        {
            return await this.nodeFile.ReadAsStringAsync();
        }
    }
}
