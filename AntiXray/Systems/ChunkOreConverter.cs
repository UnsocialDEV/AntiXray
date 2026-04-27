using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace AntiXray.Systems;

public sealed class ChunkOreConverter
{
    private readonly IWorldAccessor world;
    private readonly HiddenOreManager hiddenOreManager;
    private readonly OreBlockClassifier oreBlockClassifier;
    private readonly OrePlaceholderResolver orePlaceholderResolver;
    private readonly HiddenOrePersistence hiddenOrePersistence;
    private readonly AirExposedOreHidingPolicy airExposedOreHidingPolicy;
    private readonly ExposureBlockClassifier exposureBlockClassifier;
    private readonly BlockPos scratchPos;

    public ChunkOreConverter(
        IWorldAccessor world,
        HiddenOreManager hiddenOreManager,
        OreBlockClassifier oreBlockClassifier,
        OrePlaceholderResolver orePlaceholderResolver,
        HiddenOrePersistence hiddenOrePersistence,
        AirExposedOreHidingPolicy airExposedOreHidingPolicy)
    {
        this.world = world;
        this.hiddenOreManager = hiddenOreManager;
        this.oreBlockClassifier = oreBlockClassifier;
        this.orePlaceholderResolver = orePlaceholderResolver;
        this.hiddenOrePersistence = hiddenOrePersistence;
        this.airExposedOreHidingPolicy = airExposedOreHidingPolicy;
        exposureBlockClassifier = new ExposureBlockClassifier();
        scratchPos = new BlockPos(0);
    }

    public void ConvertGeneratedColumn(IChunkColumnGenerateRequest request)
    {
        ConvertColumn(request.Chunks, request.ChunkX, request.ChunkZ);
    }

    public void ConvertLoadedColumn(IWorldChunk[] chunks, int chunkX, int chunkZ)
    {
        ConvertColumn(chunks, chunkX, chunkZ);
    }

    private void ConvertColumn(IWorldChunk[] chunks, int chunkX, int chunkZ)
    {
        int chunkSize = GlobalConstants.ChunkSize;
        int chunkVolume = chunkSize * chunkSize * chunkSize;
        int baseX = chunkX * chunkSize;
        int baseZ = chunkZ * chunkSize;

        for (int chunkY = 0; chunkY < chunks.Length; chunkY++)
        {
            IWorldChunk? chunk = chunks[chunkY];
            if (chunk == null)
            {
                continue;
            }

            chunk.Unpack();
            if (chunk.Data == null)
            {
                continue;
            }

            bool modified = false;
            int baseY = chunkY * chunkSize;

            for (int index = 0; index < chunkVolume; index++)
            {
                int blockId = chunk.Data.GetBlockIdUnsafe(index);
                if (blockId == 0)
                {
                    continue;
                }

                Block? block = world.BlockAccessor.GetBlock(blockId);
                if (block == null || !oreBlockClassifier.IsOre(block))
                {
                    continue;
                }

                int placeholderBlockId = orePlaceholderResolver.ResolveBlockId(block);
                if (placeholderBlockId == 0)
                {
                    continue;
                }

                int localX = index % chunkSize;
                int localZ = index / chunkSize % chunkSize;
                int localY = index / (chunkSize * chunkSize);

                scratchPos.Set(baseX + localX, baseY + localY, baseZ + localZ);
                bool isExposed = IsExposureAdjacentInColumn(chunks, chunkY, localX, localY, localZ);
                if (!airExposedOreHidingPolicy.ShouldHide(isExposed, scratchPos.InternalY))
                {
                    continue;
                }

                hiddenOreManager.Store(scratchPos, block.Code.ToString());
                chunk.Data.SetBlockUnsafe(index, placeholderBlockId);
                modified = true;
            }

            if (modified)
            {
                chunk.MarkModified();
            }
        }

        for (int i = 0; i < chunks.Length; i++)
        {
            IWorldChunk? chunk = chunks[i];
            if (chunk?.MapChunk == null)
            {
                continue;
            }

            hiddenOrePersistence.SaveColumn(chunk.MapChunk, chunkX, chunkZ);
            return;
        }
    }

    private bool IsExposureAdjacentInColumn(IWorldChunk[] chunks, int chunkY, int localX, int localY, int localZ)
    {
        return IsExposureAt(chunks, chunkY, localX + 1, localY, localZ)
            || IsExposureAt(chunks, chunkY, localX - 1, localY, localZ)
            || IsExposureAt(chunks, chunkY, localX, localY + 1, localZ)
            || IsExposureAt(chunks, chunkY, localX, localY - 1, localZ)
            || IsExposureAt(chunks, chunkY, localX, localY, localZ + 1)
            || IsExposureAt(chunks, chunkY, localX, localY, localZ - 1);
    }

    private bool IsExposureAt(IWorldChunk[] chunks, int chunkY, int localX, int localY, int localZ)
    {
        int chunkSize = GlobalConstants.ChunkSize;
        int targetChunkY = chunkY;
        int targetY = localY;

        if (targetY < 0)
        {
            targetChunkY--;
            targetY += chunkSize;
        }
        else if (targetY >= chunkSize)
        {
            targetChunkY++;
            targetY -= chunkSize;
        }

        if (targetChunkY < 0 || targetChunkY >= chunks.Length)
        {
            return false;
        }

        if (localX < 0 || localX >= chunkSize || localZ < 0 || localZ >= chunkSize)
        {
            return false;
        }

        int index = (targetY * chunkSize + localZ) * chunkSize + localX;
        IWorldChunk? chunk = chunks[targetChunkY];
        if (chunk?.Data == null)
        {
            return false;
        }

        int blockId = chunk.Data.GetBlockIdUnsafe(index);
        if (blockId == 0)
        {
            return true;
        }

        Block? block = world.BlockAccessor.GetBlock(blockId);
        return block != null && exposureBlockClassifier.IsExposure(block);
    }
}
