using AntiXray.Systems;
using Vintagestory.API.Common;
using Xunit;

namespace AntiXray.Tests.Systems;

public sealed class OreBlockClassifierTests
{
    [Theory]
    [InlineData("game:ore-poor-nativecopper-granite")]
    [InlineData("game:ore-quartz-andesite")]
    [InlineData("game:ore-low-diamond-kimberlite")]
    public void IsOre_MatchesVanillaOreCodes(string code)
    {
        var classifier = new OreBlockClassifier();

        Assert.True(classifier.IsOre(CreateBlock(code)));
    }

    [Theory]
    [InlineData("game:rock-granite")]
    [InlineData("game:looseores-nativecopper")]
    [InlineData("game:orebits-nativecopper")]
    public void IsOre_RejectsNonOreBlocks(string code)
    {
        var classifier = new OreBlockClassifier();

        Assert.False(classifier.IsOre(CreateBlock(code)));
    }

    [Theory]
    [InlineData("game:rich-copper-ore")]
    [InlineData("game:deepslate-tin-ore")]
    public void IsOre_UsesConfiguredModdedPatterns(string code)
    {
        var classifier = new OreBlockClassifier(["ore-*", "*-ore"]);

        Assert.True(classifier.IsOre(CreateBlock(code)));
    }

    private static Block CreateBlock(string code)
    {
        return new Block
        {
            Code = new AssetLocation(code)
        };
    }
}
