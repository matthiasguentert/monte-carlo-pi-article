using ClusterHead.Model;
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
        public static string GenerateJobId(string prefix, ulong iterationsTotal, uint unitsTotal)
        {
            var guid = Guid.NewGuid().ToString();

            return $"{prefix}-{unitsTotal}-{iterationsTotal}-{guid.Substring(0, 4)}";
        }

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

        public static IList<Unit> GenerateSquareUnits(ulong iterationsTotal, uint unitsTotal)
        {
            if (Math.Sqrt(unitsTotal) % 2 != 0)
                throw new InvalidOperationException("unitsTotal should be a power of two");

            var stepSize = 2.0 / Math.Sqrt(unitsTotal);
            var iterationsPerUnit = iterationsTotal / unitsTotal;
            var units = new List<Unit>();

            for (double y = 1; y > -1; y -= stepSize)
            {
                for (double x = -1; x < 1; x += stepSize)
                {
                    var s = new Square1
                    {
                        Points = new List<Point>
                        {
                            new Point(x, y),
                            new Point(x + stepSize, y),
                            new Point(x, y - stepSize),
                            new Point(x + stepSize, y - stepSize)
                        }
                    };
                    

                    var square = new Square
                    {
                        Point1 = (x, y),
                        Point2 = (x + stepSize, y),
                        Point3 = (x, y - stepSize),
                        Point4 = (x + stepSize, y - stepSize)
                    };

                    var unit = new Unit
                    {
                        Square = square,
                        NumRandomPoints = iterationsPerUnit,
                        Alignment = CalculateSquareAlignment(square)
                    };
                                   
                    units.Add(unit);
                }
            }

            return units;
        }

        public static Alignment CalculateSquareAlignment(Square square)
        {
            var inside = 0;
            var outside = 0;

            if (square.Point1.x * square.Point1.x + square.Point1.y * square.Point1.y <= 1.0)
                inside++;
            else 
                outside++;

            if (square.Point2.x * square.Point2.x + square.Point2.y * square.Point2.y <= 1.0)
                inside++;
            else
                outside++;

            if (square.Point3.x * square.Point3.x + square.Point3.y * square.Point3.y <= 1.0)
                inside++;
            else
                outside++;

            if (square.Point4.x * square.Point4.x + square.Point4.y * square.Point4.y <= 1.0)
                inside++;
            else
                outside++;

            if (inside == 4)
                return Alignment.SquareInsideCircle;

            if (outside == 4)
                return Alignment.SquareOutsideCircle;

            return Alignment.SquareOverlapsCircle;
        }

        public static PoolInformation UseAutoPool()
        {
            var imageReference = new ImageReference(
                    Offer.WINDOWSSERVER,
                    Publisher.MICROSOFTWINDOWSSERVER,
                    Sku.DATACENTER_SMALLDISK_2012_R2);

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
                VirtualMachineSize = VirtualMachineSize.STANDARD_A1
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
