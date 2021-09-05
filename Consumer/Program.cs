using Microsoft.Azure.Batch.Conventions.Files;
using Shared.Model;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Consumer
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            try
            {
                var path = args[0];

                if (!File.Exists(path))
                {
                    Console.Error.WriteLine($"ERROR: No file found at path: {path}");
                    Environment.Exit(1);
                }

                Console.WriteLine($"Reading input from {path}");
                var jsonInput = await File.ReadAllTextAsync(path);

                Console.WriteLine("Deserializing input...");
                var unit = JsonSerializer.Deserialize<Unit>(jsonInput);

                Console.WriteLine($"Starting to work on: {unit}");
                var result = WorkloadRunner.GenerateRandomPoints(unit);
                Console.WriteLine($"Finished task in {result.ElapsedMilliseconds} [ms] with {result.CircleHits} circle hits");

                Console.WriteLine("Uploading task output & logs...");
                var taskId = Environment.GetEnvironmentVariable("AZ_BATCH_TASK_ID");
                var jobOutputContainerUri = Environment.GetEnvironmentVariable("JOB_OUTPUT_CONTAINER_URI");

                var taskOutputStorage = new TaskOutputStorage(new Uri(jobOutputContainerUri), taskId);

                if (string.IsNullOrEmpty(taskId))
                {
                    Console.Error.WriteLine("ERROR: AZ_BATCH_TASK_ID not set!");
                    Environment.Exit(1);
                }

                if (string.IsNullOrEmpty(jobOutputContainerUri))
                {
                    Console.Error.WriteLine("ERROR: JOB_OUTPUT_CONTAINER_URI not set!");
                    Environment.Exit(1);
                }

                WorkloadRunner.UploadTaskOutput(result, taskOutputStorage);

            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.ToString());
            }
        }
    }
}
