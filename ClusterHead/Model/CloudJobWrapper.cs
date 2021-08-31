using Microsoft.Azure.Batch;
using Microsoft.Azure.Batch.Common;
using Microsoft.Azure.Batch.Conventions.Files;
using Microsoft.WindowsAzure.Storage;
using System.Threading.Tasks;

namespace ClusterHead.Model
{
    public interface ICloudJobWrapper 
    {
        public Task CommitAsync();

        public string GetOutputStorageContainerUrl(CloudStorageAccount cloudStorageAccount);

        public Task PrepareOutputStorageAsync(CloudStorageAccount cloudStorageAccount);

        public void AddTask(CloudTask cloudTask);

        public Task<ICloudTaskWrapper> GetTaskAsync(string taskId);

        public void SetOnAllTasksComplete(OnAllTasksComplete onAllTasksComplete);
    }

    public class CloudJobWrapper : ICloudJobWrapper
    {
        private readonly CloudJob cloudJob;

        public CloudJobWrapper(CloudJob cloudJob) => this.cloudJob = cloudJob;

        public async Task CommitAsync()
        {
            await this.cloudJob.CommitAsync();
        }

        public string GetOutputStorageContainerUrl(CloudStorageAccount cloudStorageAccount)
        {
            return this.cloudJob.GetOutputStorageContainerUrl(cloudStorageAccount);
        }

        public async Task PrepareOutputStorageAsync(CloudStorageAccount cloudStorageAccount)
        {
            await this.cloudJob.PrepareOutputStorageAsync(cloudStorageAccount);
        }

        public void AddTask(CloudTask cloudTask)
        {
            this.cloudJob.AddTask(cloudTask);
        }

        public async Task<ICloudTaskWrapper> GetTaskAsync(string taskId)
        {
            var task = await this.cloudJob.GetTaskAsync(taskId);

            return new CloudTaskWrapper(task);
        }

        public void SetOnAllTasksComplete(OnAllTasksComplete onAllTasksComplete)
        {
            this.cloudJob.OnAllTasksComplete = onAllTasksComplete;
        }
    }
}
