using Azure.Storage.Blobs;
using ClusterHead.Wrappers;
using Microsoft.Azure.Batch;
using Microsoft.WindowsAzure.Storage;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClusterHead
{
    public interface IClusterService
    {
        ICloudPoolWrapper CreatePool(string poolId, string virtualMachineSize, VirtualMachineConfiguration virtualMachineConfiguration, int targetLowPriorityComputeNodes);

        ICloudJobWrapper CreateJob(string jobId, PoolInformation poolInformation);

        ICloudJobWrapper GetJob(string jobId);

        CloudStorageAccount GetStorageAccount();

        BlobContainerClient GetBlobContainerClient(string containerName);

        void AddTasks(string jobId, IEnumerable<CloudTask> tasksToAdd);

        public Task<IEnumerable<ICloudTaskWrapper>> GetTasksAsync(string jobId);

        ITaskStateMonitorWrapper CreateTaskStateMonitor();

        public JobExecutionInformation GetJobExecutionInformation(string jobId);
    }
}
