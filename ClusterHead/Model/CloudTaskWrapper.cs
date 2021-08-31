using Microsoft.Azure.Batch;
using System.Threading.Tasks;

namespace ClusterHead.Model
{
    public interface ICloudTaskWrapper
    {
        public CloudTask CloudTask { get; }

        public Task<INodeFileWrapper> GetNodeFileAsync(string filePath);

        public string GetId();
    }

    public class CloudTaskWrapper : ICloudTaskWrapper
    {
        public CloudTaskWrapper(CloudTask cloudTask) => this.CloudTask = cloudTask;

        public CloudTask CloudTask { get; }

        public async Task<INodeFileWrapper> GetNodeFileAsync(string filePath)
        {
            var nodeFile = await this.CloudTask.GetNodeFileAsync(filePath);

            return new NodeFileWrapper(nodeFile);
        }

        public string GetId()
        {
            return this.CloudTask.Id;
        }
    }
}
