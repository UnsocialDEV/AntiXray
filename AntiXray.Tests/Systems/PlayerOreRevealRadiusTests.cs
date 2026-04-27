using AntiXray.Systems;
using Xunit;

namespace AntiXray.Tests.Systems;

public sealed class PlayerOreRevealRadiusTests
{
    [Theory]
    [InlineData(4, 8)]
    [InlineData(8, 8)]
    [InlineData(24, 24)]
    [InlineData(32, 32)]
    [InlineData(40, 32)]
    public void Clamp_RestrictsRadiusToConfiguredPlayerRevealRange(int input, int expected)
    {
        Assert.Equal(expected, PlayerOreRevealRadius.Clamp(input));
    }
}
