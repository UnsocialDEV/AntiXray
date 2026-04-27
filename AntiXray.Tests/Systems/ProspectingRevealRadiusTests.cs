using AntiXray.Systems;
using Xunit;

namespace AntiXray.Tests.Systems;

public sealed class ProspectingRevealRadiusTests
{
    [Theory]
    [InlineData(-1, 0)]
    [InlineData(0, 0)]
    [InlineData(6, 6)]
    [InlineData(12, 8)]
    public void Clamp_RestrictsRadiusToSupportedNodeSearchRange(int input, int expected)
    {
        Assert.Equal(expected, ProspectingRevealRadius.Clamp(input));
    }
}
