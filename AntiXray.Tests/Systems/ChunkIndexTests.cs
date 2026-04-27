using AntiXray.Models;
using AntiXray.Systems;
using Vintagestory.API.MathTools;
using Xunit;

namespace AntiXray.Tests.Systems;

public sealed class ChunkIndexTests
{
    [Fact]
    public void GetChunkPos_UsesFloorDivisionForNegativeCoordinates()
    {
        var index = new ChunkIndex(32);
        var pos = new BlockPos(-1, 10, -33, 0);

        ChunkPos chunkPos = index.GetChunkPos(pos);

        Assert.Equal(-1, chunkPos.X);
        Assert.Equal(0, chunkPos.Y);
        Assert.Equal(-2, chunkPos.Z);
    }

    [Fact]
    public void Remove_DropsEmptyChunkEntry()
    {
        var index = new ChunkIndex(32);
        var pos = new BlockPos(4, 10, 8, 0);
        ChunkPos chunkPos = index.GetChunkPos(pos);

        index.Add(pos);
        index.Remove(pos);

        Assert.False(index.TryGetChunkPositions(chunkPos, out _));
    }
}
