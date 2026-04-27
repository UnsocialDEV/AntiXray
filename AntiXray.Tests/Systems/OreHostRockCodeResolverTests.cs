using AntiXray.Systems;
using Xunit;

namespace AntiXray.Tests.Systems;

public sealed class OreHostRockCodeResolverTests
{
    [Theory]
    [InlineData("ore-poor-nativecopper-granite", "game:rock-granite")]
    [InlineData("ore-quartz_wolframite-granite", "game:rock-granite")]
    [InlineData("ore-low-diamond-kimberlite", "game:rock-kimberlite")]
    [InlineData("ore-flint-suevite", "game:rock-suevite")]
    public void Resolve_UsesLastOreCodeSegmentAsHostRock(string oreCodePath, string expectedRockCode)
    {
        Assert.Equal(expectedRockCode, OreHostRockCodeResolver.Resolve(oreCodePath));
    }

    [Theory]
    [InlineData("ore")]
    [InlineData("ore-")]
    public void Resolve_ReturnsNullWhenHostRockCannotBeDerived(string oreCodePath)
    {
        Assert.Null(OreHostRockCodeResolver.Resolve(oreCodePath));
    }
}
