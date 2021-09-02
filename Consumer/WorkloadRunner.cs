using Microsoft.Azure.Batch;
using Microsoft.Azure.Batch.Conventions.Files;
using Shared.Model;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Consumer
{
    public static class WorkloadRunner
    {
        public static bool IsInCircle(double x, double y) => x * x + y * y <= 1.0;

        public static Unit GenerateRandomPoints(Unit unit)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var random = new Random();
            uint circleHits = 0;

            for (ulong i = 0; i < unit.NumRandomPoints; i++)
            {
                var x = random.NextDouble() * (unit.Area.UpperX - unit.Area.LowerX) + unit.Area.LowerX;
                var y = random.NextDouble() * (unit.Area.UpperY - unit.Area.LowerY) + unit.Area.LowerY;

                if (IsInCircle(x, y))
                    circleHits++;
            }
            stopwatch.Stop();

            unit.ElapsedMilliseconds = stopwatch.ElapsedMilliseconds;
            unit.CircleHits = circleHits;

            return unit;
        }

        public static void UploadTaskOutput(Unit unit, TaskOutputStorage taskOutputStorage)
        {
            try
            {
                var contents = JsonSerializer.Serialize(unit);
                File.WriteAllText(@"output.txt", contents);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.ToString());
            }

            // Copying log files to working directory to prevent locking issues
            File.Copy(sourceFileName: $@"..\{Constants.StandardOutFileName}", destFileName: Constants.StandardOutFileName);
            File.Copy(sourceFileName: $@"..\{Constants.StandardErrorFileName}", destFileName: Constants.StandardErrorFileName);

            Task.WaitAll(
                taskOutputStorage.SaveAsync(TaskOutputKind.TaskLog, Constants.StandardOutFileName),
                taskOutputStorage.SaveAsync(TaskOutputKind.TaskLog, Constants.StandardErrorFileName),
                taskOutputStorage.SaveAsync(TaskOutputKind.TaskOutput, @"output.txt"));
        }
    }
}
