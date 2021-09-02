using Microsoft.Azure.Batch;
using Microsoft.Azure.Batch.Common;
using Shared.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ClusterHead
{
    public static class Tools
    {
        public static string GetTotalCalculationTime(IEnumerable<Unit> units)
        {
            var totalElapsedMilliseconds = units.Sum(u => u.ElapsedMilliseconds);
            var t = TimeSpan.FromMilliseconds(totalElapsedMilliseconds);

            return string.Format("{0:D2}h:{1:D2}m:{2:D2}s:{3:D3}ms", t.Hours, t.Minutes, t.Seconds, t.Milliseconds);
        }

        public static string GetTotalRuntime(TimeSpan elapsed)
        {
            var t = TimeSpan.FromMilliseconds(elapsed.TotalMilliseconds);

            return string.Format("{0:D2}h:{1:D2}m:{2:D2}s:{3:D3}ms", t.Hours, t.Minutes, t.Seconds, t.Milliseconds);
        }

        public static decimal CalculatePi(IEnumerable<Unit> units)
        {
            ulong circleHitsTotal = 0;
            ulong numRandomPointsTotal = 0;

            foreach (var unit in units)
            {
                circleHitsTotal += unit.CircleHits;
                numRandomPointsTotal += unit.NumRandomPoints;
            }

            return (decimal)circleHitsTotal / numRandomPointsTotal * 4.0m;
        }

        public static IList<Unit> GenerateUnits(ulong iterationsTotal, uint unitsTotal)
        {
            var stepSizeX = 2.0 / unitsTotal;
            var iterationsPerUnit = iterationsTotal / unitsTotal;
            var units = new List<Unit>();

            for (double boundaryX = -1.0; boundaryX < 1.0; boundaryX += stepSizeX)
            {
                var area = new Area
                {
                    LowerX = boundaryX,
                    UpperX = boundaryX + stepSizeX,
                    LowerY = -1.0,
                    UpperY = 1.0
                };

                var unit = new Unit
                {
                    Area = area,
                    NumRandomPoints = iterationsPerUnit
                };

                units.Add(unit);
            }

            return units;
        }

        public static PoolInformation UseAutoPool()
        {
            var imageReference = new ImageReference(
                    offer: "windowsserver",
                    publisher: "microsoftwindowsserver",
                    sku: Sku.DATACENTER_SMALLDISK_2012_R2);

            var applications = new List<ApplicationPackageReference>()
            {
                new ApplicationPackageReference { ApplicationId = "consumer", Version = "1.0.0" }
            };

            var poolSpecification = new PoolSpecification
            {
                ApplicationPackageReferences = applications,
                TargetLowPriorityComputeNodes = 1,
                TaskSlotsPerNode = 2,
                VirtualMachineConfiguration = new VirtualMachineConfiguration(imageReference, nodeAgentSkuId: "batch.node.windows amd64"),
                VirtualMachineSize = "standard_a1"
            };

            var autoPoolSpecification = new AutoPoolSpecification
            {
                AutoPoolIdPrefix = "autopool",
                PoolLifetimeOption = PoolLifetimeOption.Job,
                PoolSpecification = poolSpecification
            };

            return new PoolInformation { AutoPoolSpecification = autoPoolSpecification };
        }

        public static PoolInformation UseStaticPool(string poolId)
        {
            return new PoolInformation { PoolId = poolId };
        }
    }
}
