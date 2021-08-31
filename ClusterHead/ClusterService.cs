using Azure.Storage.Blobs;
using ClusterHead.Model;
using Microsoft.Azure.Batch;
using Microsoft.Azure.Batch.Auth;
using Microsoft.WindowsAzure.Storage;
using System.Collections.Generic;

namespace ClusterHead
{
    public class ClusterService : IClusterService
    {
        private readonly BatchClient batchClient;
        private readonly BlobServiceClient blobServiceClient;
        private readonly AppConfig config;

        public ClusterService(AppConfig config)
        {
            this.config = config;

            var credentials = new BatchSharedKeyCredentials(
                  config.BatchAccountUrl,
                  config.BatchAccountName,
                  config.BatchAccountKey);

            this.batchClient = BatchClient.Open(credentials);

            this.blobServiceClient = new BlobServiceClient(config.ConnectionString);
        }

        public ClusterService(BatchClient batchClient)
        {
            this.batchClient = batchClient;
        }

        public ICloudPoolWrapper CreatePool(string poolId, string virtualMachineSize, VirtualMachineConfiguration virtualMachineConfiguration, int targetLowPriorityComputeNodes)
        {
            var cloudPool = this.batchClient.PoolOperations.CreatePool(poolId, virtualMachineSize, virtualMachineConfiguration, targetLowPriorityComputeNodes: targetLowPriorityComputeNodes);

            return new CloudPoolWrapper(cloudPool);
        }

        public ICloudJobWrapper CreateJob(string jobId, PoolInformation poolInformation)
        {
            var cloudJob = this.batchClient.JobOperations.CreateJob(jobId, poolInformation);

            return new CloudJobWrapper(cloudJob);
        }

        public ICloudJobWrapper GetJob(string jobId)
        {
            var cloudJob = this.batchClient.JobOperations.GetJob(jobId);

            return new CloudJobWrapper(cloudJob);
        }

        public CloudStorageAccount GetStorageAccount()
        {
            return CloudStorageAccount.Parse(this.config.ConnectionString);
        }

        public BlobContainerClient GetBlobContainerClient(string containerName)
        {
            return this.blobServiceClient.GetBlobContainerClient(containerName);
        }

        public void AddTask(string jobId, IEnumerable<CloudTask> tasksToAdd)
        {
            this.batchClient.JobOperations.AddTask(jobId, tasksToAdd);
        }

        public ITaskStateMonitorWrapper CreateTaskStateMonitor()
        {
            var taskStateMonitor = this.batchClient.Utilities.CreateTaskStateMonitor();

            return new TaskStateMonitorWrapper(taskStateMonitor);
        }
    }
}
