using AntiXray.Models;
using Vintagestory.API.MathTools;

namespace AntiXray.Systems;

public sealed class HiddenOreProximityQuery
{
    private readonly HiddenOreManager hiddenOreManager;
    private readonly int chunkSize;
    private readonly BlockPos minPos;
    private readonly BlockPos maxPos;

    public HiddenOreProximityQuery(HiddenOreManager hiddenOreManager, int chunkSize)
    {
        this.hiddenOreManager = hiddenOreManager;
        this.chunkSize = chunkSize;
        minPos = new BlockPos(0);
        maxPos = new BlockPos(0);
    }

    public void CopyCandidates(BlockPos center, int radius, List<HiddenOreData> output)
    {
        minPos.Set(center.X - radius, center.InternalY - radius, center.Z - radius);
        maxPos.Set(center.X + radius, center.InternalY + radius, center.Z + radius);

        ChunkPos minChunk = hiddenOreManager.ChunkIndex.GetChunkPos(minPos);
        ChunkPos maxChunk = hiddenOreManager.ChunkIndex.GetChunkPos(maxPos);

        for (int chunkX = minChunk.X; chunkX <= maxChunk.X; chunkX++)
        {
            for (int chunkY = minChunk.Y; chunkY <= maxChunk.Y; chunkY++)
            {
                if (chunkY < 0)
                {
                    continue;
                }

                for (int chunkZ = minChunk.Z; chunkZ <= maxChunk.Z; chunkZ++)
                {
                    hiddenOreManager.CopyChunkEntries(new ChunkPos(chunkX, chunkY, chunkZ), output);
                }
            }
        }
    }
}
