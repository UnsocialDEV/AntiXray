using AntiXray.Commands;
using AntiXray.Models;
using AntiXray.Systems;
using Vintagestory.API.MathTools;
using Xunit;

namespace AntiXray.Tests.Commands;

public sealed class AdminRadiusQueryTests
{
    [Fact]
    public void CopyHiddenOreWithin_IncludesHiddenOreInsideRadius()
    {
        var manager = CreateManager();
        manager.Store(new BlockPos(15, 0, 0, 0), "game:ore-copper-granite");
        var query = CreateQuery(manager);
        var output = new List<HiddenOreData>();

        query.CopyHiddenOreWithin(new BlockPos(0, 0, 0, 0), 16, output);

        Assert.Single(output);
        Assert.Equal("game:ore-copper-granite", output[0].OreBlockCode);
    }

    [Fact]
    public void CopyHiddenOreWithin_ExcludesHiddenOreOutsideRadius()
    {
        var manager = CreateManager();
        manager.Store(new BlockPos(17, 0, 0, 0), "game:ore-copper-granite");
        var query = CreateQuery(manager);
        var output = new List<HiddenOreData>();

        query.CopyHiddenOreWithin(new BlockPos(0, 0, 0, 0), 16, output);

        Assert.Empty(output);
    }

    [Fact]
    public void CopyHiddenOreWithin_UsesSphericalDistance()
    {
        var manager = CreateManager();
        manager.Store(new BlockPos(12, 12, 0, 0), "game:ore-copper-granite");
        var query = CreateQuery(manager);
        var output = new List<HiddenOreData>();

        query.CopyHiddenOreWithin(new BlockPos(0, 0, 0, 0), 16, output);

        Assert.Empty(output);
    }

    private static AdminRadiusQuery CreateQuery(HiddenOreManager manager)
    {
        return new AdminRadiusQuery(new HiddenOreProximityQuery(manager, 32));
    }

    private static HiddenOreManager CreateManager()
    {
        return new HiddenOreManager(new ChunkIndex(32));
    }
}
