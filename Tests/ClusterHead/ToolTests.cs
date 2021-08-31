using ClusterHead;
using FluentAssertions;
using Xunit;

namespace Tests.ClusterHead
{
    public class ToolTests
    {
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
    }
}
