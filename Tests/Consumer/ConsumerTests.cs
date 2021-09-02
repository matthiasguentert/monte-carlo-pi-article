using Consumer;
using FluentAssertions;
using Shared.Model;
using Xunit;

namespace Tests.Consumer
{
    public class ConsumerTests
    {
        [Fact]
        public void ShouldExecuteWorkloadRunner()
        {
            // Arrange 
            var unit = new Unit
            {
                Area = new Area
                {
                    LowerX = -1,
                    UpperX = 1,
                    LowerY = -1,
                    UpperY = 1
                },
                NumRandomPoints = 100000
            };

            // Act
            var result = WorkloadRunner.GenerateRandomPoints(unit);

            // Assert 
            result.CircleHits.Should().NotBe(0);
        }

        [Theory]
        [InlineData(0, 0, true)]
        [InlineData(1, 1, false)]
        [InlineData(-1, -1, false)]
        [InlineData(-1, 1, false)]
        [InlineData(1, -1, false)]
        public void ShouldTestIfInCircle(double x, double y, bool expected)
        {
            var result = WorkloadRunner.IsInCircle(x, y);
            result.Should().Be(expected);
        }
    }
}
