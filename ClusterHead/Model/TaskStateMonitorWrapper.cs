using Microsoft.Azure.Batch;
using Microsoft.Azure.Batch.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClusterHead.Model
{
    public interface ITaskStateMonitorWrapper
    {
        public Task WhenAll(List<ICloudTaskWrapper> cloudTasks, TaskState taskState, TimeSpan timeSpan);
    }

    public class TaskStateMonitorWrapper : ITaskStateMonitorWrapper
    {
        private readonly TaskStateMonitor taskStateMonitor;

        public TaskStateMonitorWrapper(TaskStateMonitor taskStateMonitor) => this.taskStateMonitor = taskStateMonitor;

        public async Task WhenAll(List<ICloudTaskWrapper> cloudTasks, TaskState taskState, TimeSpan timeSpan)
        {
            var tasks = new List<CloudTask>();
            foreach (var cloudTask in cloudTasks)
            {
                tasks.Add(cloudTask.CloudTask);
            }

            await this.taskStateMonitor.WhenAll(tasks, taskState, timeSpan);
        }
    }
}
