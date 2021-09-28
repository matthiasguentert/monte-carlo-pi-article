using ClusterHead;
using FluentAssertions;
using Shared.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
        public void ShouldGenerateSquareUnits()
        {
            // Arrange & Act
            var units = Tools.GenerateSquareUnits(iterationsTotal: ulong.MaxValue, unitsTotal: 16);

            // Assert
            units.Should().HaveCount(16);
            
            units[0].NumRandomPoints.Should().Be(1152921504606846975);
            units[0].Square.Point1.Should().Be((-1.0, 1.0));
            units[0].Square.Point2.Should().Be((-0.5, 1.0));
            units[0].Square.Point3.Should().Be((-1.0, 0.5));
            units[0].Square.Point4.Should().Be((-0.5, 0.5));
            units[0].Alignment.Should().Be(Alignment.SquareOverlapsCircle);

            units[5].NumRandomPoints.Should().Be(1152921504606846975);
            units[5].Square.Point1.Should().Be((-0.5, 0.5));
            units[5].Square.Point2.Should().Be(( 0.0, 0.5));
            units[5].Square.Point3.Should().Be((-0.5, 0.0));
            units[5].Square.Point4.Should().Be(( 0.0, 0.0));
            units[5].Alignment.Should().Be(Alignment.SquareInsideCircle);

            units[15].NumRandomPoints.Should().Be(1152921504606846975);
            units[15].Square.Point1.Should().Be((0.5, -0.5));
            units[15].Square.Point2.Should().Be((1.0, -0.5));
            units[15].Square.Point3.Should().Be((0.5, -1.0));
            units[15].Square.Point4.Should().Be((1.0, -1.0));
            units[15].Alignment.Should().Be(Alignment.SquareOverlapsCircle);
        }

        [Theory]
        [InlineData(16, 0, 12, 4)]
        [InlineData(64, 4, 28, 32)]
        [InlineData(256, 32, 60, 164)]
        public void ShouldCalculateSquareAlignment(uint unitsTotal, int expectedOut, int expectedOverlap, int expectedIn)
        {
            // Arrange
            var units = Tools.GenerateSquareUnits(iterationsTotal: ulong.MaxValue, unitsTotal: unitsTotal);
            var results = new List<Alignment>();

            // Act
            foreach (var unit in units)
            {
                results.Add(Tools.CalculateSquareAlignment(unit.Square));
            }

            // Assert
            results.Count(r => r == Alignment.SquareOutsideCircle).Should().Be(expectedOut);
            results.Count(r => r == Alignment.SquareOverlapsCircle).Should().Be(expectedOverlap);
            results.Count(r => r == Alignment.SquareInsideCircle).Should().Be(expectedIn);
        }

        [Fact]
        public void GenerateSquareUnits_Should_Throw_If_UnitsTotal_Not_PowerOfTwo()
        {
            // Arrange
            Action act = () => Tools.GenerateSquareUnits(iterationsTotal: ulong.MaxValue, unitsTotal: 12);

            // Act & Assert
            act.Should().Throw<InvalidOperationException>("unitsTotal should be a power of two");
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

