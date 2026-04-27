using AntiXray.Systems;
using Vintagestory.API.MathTools;
using Xunit;

namespace AntiXray.Tests.Systems;

public sealed class HiddenOreVeinCollectorTests
{
    [Fact]
    public void CollectConnected_IncludesFaceConnectedSameCodeOre()
    {
        var manager = CreateManager();
        manager.Store(new BlockPos(0, 0, 0, 0), "game:ore-copper-granite");
        manager.Store(new BlockPos(1, 0, 0, 0), "game:ore-copper-granite");
        var collector = new HiddenOreVeinCollector(manager);
        var output = new List<BlockPos>();

        collector.CollectConnected(new BlockPos(0, 0, 0, 0), 128, output);

        Assert.Equal(2, output.Count);
    }

    [Fact]
    public void CollectConnected_StopsAtConfiguredCap()
    {
        var manager = CreateManager();
        manager.Store(new BlockPos(0, 0, 0, 0), "game:ore-copper-granite");
        manager.Store(new BlockPos(1, 0, 0, 0), "game:ore-copper-granite");
        manager.Store(new BlockPos(2, 0, 0, 0), "game:ore-copper-granite");
        var collector = new HiddenOreVeinCollector(manager);
        var output = new List<BlockPos>();

        collector.CollectConnected(new BlockPos(0, 0, 0, 0), 2, output);

        Assert.Equal(2, output.Count);
    }

    [Fact]
    public void CollectConnected_LeavesDisconnectedOreOutOfVein()
    {
        var manager = CreateManager();
        manager.Store(new BlockPos(0, 0, 0, 0), "game:ore-copper-granite");
        manager.Store(new BlockPos(1, 0, 0, 0), "game:ore-copper-granite");
        manager.Store(new BlockPos(5, 0, 0, 0), "game:ore-copper-granite");
        var collector = new HiddenOreVeinCollector(manager);
        var output = new List<BlockPos>();

        collector.CollectConnected(new BlockPos(0, 0, 0, 0), 128, output);

        Assert.Equal(2, output.Count);
        Assert.DoesNotContain(output, pos => pos.X == 5);
    }

    [Fact]
    public void CollectConnected_DoesNotMergeDifferentOreCodes()
    {
        var manager = CreateManager();
        manager.Store(new BlockPos(0, 0, 0, 0), "game:ore-copper-granite");
        manager.Store(new BlockPos(1, 0, 0, 0), "game:ore-tin-granite");
        var collector = new HiddenOreVeinCollector(manager);
        var output = new List<BlockPos>();

        collector.CollectConnected(new BlockPos(0, 0, 0, 0), 128, output);

        Assert.Single(output);
        Assert.Equal(0, output[0].X);
    }

    private static HiddenOreManager CreateManager()
    {
        return new HiddenOreManager(new ChunkIndex(32));
    }
}
