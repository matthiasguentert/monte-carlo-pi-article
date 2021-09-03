using ClusterHead;
using FluentAssertions;
using Shared.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Xunit;

namespace Tests.ClusterHead
{
    public class ToolTests
    {
        [Fact]
        public void ShouldGetTotalCalculationTime()
        {
            // Arrange
            var units = new List<Unit>
            {
                new Unit() { ElapsedMilliseconds = 1000000 },
                new Unit() { ElapsedMilliseconds = 1000000 },
                new Unit() { ElapsedMilliseconds = 1000000 },
                new Unit() { ElapsedMilliseconds = 1000000 },
                new Unit() { ElapsedMilliseconds = 100 }
            };

            // Act
            var totalTime = Tools.GetTotalCalculationTime(units);

            // Asset
            totalTime.Should().Be("01h:06m:40s:100ms");
        }

        [Fact]
        public void ShouldGetTotalRuntime()
        {
            // Arrange
            var elapsed = TimeSpan.FromSeconds(100);

            // Act
            var totalTime = Tools.GetTotalRuntime(elapsed);

            // Asset
            totalTime.Should().Be("00h:01m:40s:000ms");
        }

        [Fact]
        public void ShouldGenerateUnits()
        {
            // Arrange & Act
            var units = Tools.GenerateUnits(iterationsTotal: ulong.MaxValue, unitsTotal: 16);

            // Assert
            units.Should().HaveCount(16);

            units[0].NumRandomPoints.Should().Be(1152921504606846975);

            units[0].Area.LowerY.Should().Be(-1);
            units[0].Area.UpperY.Should().Be(1);
            units[0].Area.LowerX.Should().Be(-1);
            units[0].Area.UpperX.Should().Be(-0.875);
        }

        [Fact]
        public void ShouldCalculatePi()
        {
            // Arrange
            var units = new List<Unit>
            {
                new Unit() { NumRandomPoints = 8589934590, CircleHits = 5275799930 },
                new Unit() { NumRandomPoints = 8589934590, CircleHits = 8217225901 },
                new Unit() { NumRandomPoints = 8589934590, CircleHits = 8217225498 },
                new Unit() { NumRandomPoints = 8589934590, CircleHits = 5275798973 }
            };

            // Act
            var pi = Tools.CalculatePi(units);

            // Assert
            pi.Should().Be(3.1415897314766398005831613672M);
            pi.Should().BeApproximately((decimal)Math.PI, 0.00001M);
        }
    }
}
