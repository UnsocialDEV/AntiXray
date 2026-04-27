using AntiXray.Commands;
using Xunit;

namespace AntiXray.Tests.Commands;

public sealed class AdminCommandRadiusTests
{
    [Theory]
    [InlineData(-1, 1)]
    [InlineData(0, 1)]
    [InlineData(8, 8)]
    [InlineData(128, 64)]
    public void Clamp_RestrictsRadiusToOneThroughMax(int requested, int expected)
    {
        Assert.Equal(expected, AdminCommandRadius.Clamp(requested, 64));
    }

    [Fact]
    public void Resolve_UsesDefaultRadiusWhenRequestIsMissing()
    {
        var radius = new AdminCommandRadius(16, 64);

        Assert.Equal(16, radius.Resolve(null));
    }
}
