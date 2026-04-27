using AntiXray.Systems;
using Xunit;

namespace AntiXray.Tests.Systems;

public sealed class BlockBreakRevealRadiusTests
{
    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 1)]
    [InlineData(2, 2)]
    [InlineData(3, 3)]
    [InlineData(4, 3)]
    public void Clamp_RestrictsRadiusToSupportedBreakRevealRange(int input, int expected)
    {
        Assert.Equal(expected, BlockBreakRevealRadius.Clamp(input));
    }
}
