using AntiXray.Models;
using AntiXray.Systems;
using Vintagestory.API.MathTools;
using Xunit;

namespace AntiXray.Tests.Systems;

public sealed class HiddenOreManagerTests
{
    [Fact]
    public void Store_ProvidesO1LookupByPosition()
    {
        var manager = new HiddenOreManager(new ChunkIndex(32));
        var pos = new BlockPos(12, 24, 36, 0);

        manager.Store(pos, "game:ore-copper-native-granite");

        bool found = manager.TryGet(pos, out HiddenOreData data);
        Assert.True(found);
        Assert.Equal("game:ore-copper-native-granite", data.OreBlockCode);
    }

    [Fact]
    public void Store_IsIdempotentForSamePosition()
    {
        var manager = new HiddenOreManager(new ChunkIndex(32));
        var pos = new BlockPos(12, 24, 36, 0);
        var entries = new List<HiddenOreData>();

        manager.Store(pos, "game:ore-copper-native-granite");
        manager.Store(pos, "game:ore-gold-native-granite");
        manager.CopyColumnEntries(0, 1, entries);

        Assert.Single(entries);
        Assert.Equal("game:ore-copper-native-granite", entries[0].OreBlockCode);
    }

    [Fact]
    public void Remove_DeletesMetadataAndIndexEntry()
    {
        var manager = new HiddenOreManager(new ChunkIndex(32));
        var pos = new BlockPos(12, 24, 36, 0);
        var entries = new List<HiddenOreData>();

        manager.Store(pos, "game:ore-copper-native-granite");
        bool removed = manager.Remove(pos);
        manager.CopyColumnEntries(0, 1, entries);

        Assert.True(removed);
        Assert.False(manager.TryGet(pos, out _));
        Assert.Empty(entries);
    }

    [Fact]
    public void CountColumnEntries_CountsOnlyRequestedColumn()
    {
        var manager = new HiddenOreManager(new ChunkIndex(32));

        manager.Store(new BlockPos(12, 24, 36, 0), "game:ore-copper-native-granite");
        manager.Store(new BlockPos(18, 48, 40, 0), "game:ore-gold-native-granite");
        manager.Store(new BlockPos(70, 24, 36, 0), "game:ore-tin-cassiterite-granite");

        int count = manager.CountColumnEntries(0, 1);

        Assert.Equal(2, count);
    }
}
