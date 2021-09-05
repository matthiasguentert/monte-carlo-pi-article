using Microsoft.Azure.Batch;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClusterHead.Wrappers
{
    public interface ICloudPoolWrapper 
    {
        public Task CommitAsync();

        public void SetApplicationPackageReferences(IList<ApplicationPackageReference> applications);

        public void SetTaskSlotsPerNode(int taskSlotsPerNode);
    }

    public class CloudPoolWrapper : ICloudPoolWrapper
    {
        private readonly CloudPool cloudPool;

        public CloudPoolWrapper(CloudPool pool) => this.cloudPool = pool;

        public async Task CommitAsync()
        {
            await this.cloudPool.CommitAsync();
        }

        public void SetApplicationPackageReferences(IList<ApplicationPackageReference> applications)
        {
            this.cloudPool.ApplicationPackageReferences = applications;
        }

        public void SetTaskSlotsPerNode(int taskSlotsPerNode)
        {
            this.cloudPool.TaskSlotsPerNode = taskSlotsPerNode;
        }
    }
}
