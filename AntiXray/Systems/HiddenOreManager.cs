using AntiXray.Models;
using Vintagestory.API.MathTools;

namespace AntiXray.Systems;

public sealed class HiddenOreManager
{
    private readonly Dictionary<ChunkPos, Dictionary<BlockPosKey, HiddenOreData>> hiddenOreData = new();
    private readonly object syncRoot = new();

    public HiddenOreManager(ChunkIndex chunkIndex)
    {
        ChunkIndex = chunkIndex;
    }

    public ChunkIndex ChunkIndex { get; }

    public void Store(BlockPos pos, string oreBlockCode)
    {
        lock (syncRoot)
        {
            ChunkPos chunkPos = ChunkIndex.GetChunkPos(pos);
            BlockPosKey key = BlockPosKey.From(pos);

            if (!hiddenOreData.TryGetValue(chunkPos, out Dictionary<BlockPosKey, HiddenOreData>? chunkData))
            {
                chunkData = new Dictionary<BlockPosKey, HiddenOreData>(8);
                hiddenOreData.Add(chunkPos, chunkData);
            }

            if (chunkData.ContainsKey(key))
            {
                return;
            }

            chunkData.Add(key, new HiddenOreData(key, oreBlockCode));
            ChunkIndex.Add(pos);
        }
    }

    public void CopyColumnEntries(int chunkX, int chunkZ, List<HiddenOreData> entries)
    {
        lock (syncRoot)
        {
            foreach (KeyValuePair<ChunkPos, Dictionary<BlockPosKey, HiddenOreData>> chunkEntry in hiddenOreData)
            {
                ChunkPos chunkPos = chunkEntry.Key;
                if (chunkPos.X != chunkX || chunkPos.Z != chunkZ)
                {
                    continue;
                }

                foreach (KeyValuePair<BlockPosKey, HiddenOreData> dataEntry in chunkEntry.Value)
                {
                    entries.Add(dataEntry.Value);
                }
            }
        }
    }

    public void CopyChunkEntries(ChunkPos chunkPos, List<HiddenOreData> entries)
    {
        lock (syncRoot)
        {
            if (!hiddenOreData.TryGetValue(chunkPos, out Dictionary<BlockPosKey, HiddenOreData>? chunkData))
            {
                return;
            }

            foreach (KeyValuePair<BlockPosKey, HiddenOreData> dataEntry in chunkData)
            {
                entries.Add(dataEntry.Value);
            }
        }
    }

    public int CountColumnEntries(int chunkX, int chunkZ)
    {
        lock (syncRoot)
        {
            int count = 0;

            foreach (KeyValuePair<ChunkPos, Dictionary<BlockPosKey, HiddenOreData>> chunkEntry in hiddenOreData)
            {
                ChunkPos chunkPos = chunkEntry.Key;
                if (chunkPos.X != chunkX || chunkPos.Z != chunkZ)
                {
                    continue;
                }

                count += chunkEntry.Value.Count;
            }

            return count;
        }
    }

    public bool TryGet(BlockPos pos, out HiddenOreData data)
    {
        lock (syncRoot)
        {
            ChunkPos chunkPos = ChunkIndex.GetChunkPos(pos);

            if (!hiddenOreData.TryGetValue(chunkPos, out Dictionary<BlockPosKey, HiddenOreData>? chunkData))
            {
                data = null!;
                return false;
            }

            return chunkData.TryGetValue(BlockPosKey.From(pos), out data!);
        }
    }

    public bool Remove(BlockPos pos)
    {
        lock (syncRoot)
        {
            ChunkPos chunkPos = ChunkIndex.GetChunkPos(pos);

            if (!hiddenOreData.TryGetValue(chunkPos, out Dictionary<BlockPosKey, HiddenOreData>? chunkData))
            {
                return false;
            }

            bool removed = chunkData.Remove(BlockPosKey.From(pos));
            if (!removed)
            {
                return false;
            }

            ChunkIndex.Remove(pos);

            if (chunkData.Count == 0)
            {
                hiddenOreData.Remove(chunkPos);
            }

            return true;
        }
    }
}
