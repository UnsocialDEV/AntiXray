using AntiXray.Systems;
using Vintagestory.API.MathTools;
using Xunit;

namespace AntiXray.Tests.Systems;

public sealed class HiddenOreProximityQueryTests
{
    [Fact]
    public void CopyCandidates_CopiesHiddenOreFromChunksIntersectingRadius()
    {
        var manager = CreateManager();
        manager.Store(new BlockPos(8, 8, 8, 0), "game:ore-copper-granite");
        manager.Store(new BlockPos(36, 8, 8, 0), "game:ore-tin-granite");
        var query = new HiddenOreProximityQuery(manager, 32);
        var output = new List<AntiXray.Models.HiddenOreData>();

        query.CopyCandidates(new BlockPos(16, 8, 8, 0), 24, output);

        Assert.Equal(2, output.Count);
    }

    [Fact]
    public void CopyCandidates_DoesNotCopyHiddenOreFromDistantChunks()
    {
        var manager = CreateManager();
        manager.Store(new BlockPos(8, 8, 8, 0), "game:ore-copper-granite");
        manager.Store(new BlockPos(96, 8, 8, 0), "game:ore-tin-granite");
        var query = new HiddenOreProximityQuery(manager, 32);
        var output = new List<AntiXray.Models.HiddenOreData>();

        query.CopyCandidates(new BlockPos(16, 8, 8, 0), 24, output);

        Assert.Single(output);
        Assert.Equal("game:ore-copper-granite", output[0].OreBlockCode);
    }

    private static HiddenOreManager CreateManager()
    {
        return new HiddenOreManager(new ChunkIndex(32));
    }
}
