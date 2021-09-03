using Microsoft.Azure.Batch;
using Microsoft.Azure.Batch.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClusterHead.Wrappers
{
    public interface ITaskStateMonitorWrapper
    {
        public Task WhenAll(IEnumerable<ICloudTaskWrapper> cloudTasks, TaskState taskState, TimeSpan timeSpan);

        public void WaitAll(IEnumerable<ICloudTaskWrapper> cloudTasks, TaskState taskState, TimeSpan timeSpan, ODATAMonitorControl controlParams = null);
    }

    public class TaskStateMonitorWrapper : ITaskStateMonitorWrapper
    {
        private readonly TaskStateMonitor taskStateMonitor;

        public TaskStateMonitorWrapper(TaskStateMonitor taskStateMonitor) => this.taskStateMonitor = taskStateMonitor;

        public void WaitAll(IEnumerable<ICloudTaskWrapper> cloudTasks, TaskState taskState, TimeSpan timeSpan, ODATAMonitorControl controlParams = null)
        {
            var tasks = new List<CloudTask>();
            foreach (var cloudTask in cloudTasks)
            {
                tasks.Add(cloudTask.CloudTask);
            }

            this.taskStateMonitor.WaitAll(tasks, taskState, timeSpan, controlParams);
        }

        public async Task WhenAll(IEnumerable<ICloudTaskWrapper> cloudTasks, TaskState taskState, TimeSpan timeSpan)
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
