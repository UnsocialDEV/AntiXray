using AntiXray.Systems;
using Vintagestory.API.Common;
using Xunit;

namespace AntiXray.Tests.Systems;

public sealed class ProspectingPickDetectorTests
{
    [Theory]
    [InlineData("game:prospectingpick-copper")]
    [InlineData("game:prospectingpick-steel")]
    public void IsProspectingPick_AcceptsConfiguredProspectingPickCodes(string code)
    {
        var detector = new ProspectingPickDetector(["prospectingpick-*"]);

        Assert.True(detector.IsProspectingPick(CreateItemStack(code)));
    }

    [Theory]
    [InlineData("game:pickaxe-copper")]
    [InlineData("game:ore-poor-nativecopper-granite")]
    public void IsProspectingPick_RejectsNonProspectingPickCodes(string code)
    {
        var detector = new ProspectingPickDetector(["prospectingpick-*"]);

        Assert.False(detector.IsProspectingPick(CreateItemStack(code)));
    }

    private static ItemStack CreateItemStack(string code)
    {
        return new ItemStack(new Item { Code = new AssetLocation(code) });
    }
}
