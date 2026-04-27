using AntiXray.Models;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;

namespace AntiXray.Systems;

public sealed class HiddenOrePersistence
{
    private const string ModDataKey = "antixray:hiddenores";
    private const string LegacyModDataKey = "antiblockoverlay:hiddenores";

    private readonly HiddenOreManager hiddenOreManager;
    private readonly List<HiddenOreData> reusableEntries = new(128);
    private readonly BlockPos scratchPos;
    private readonly object syncRoot = new();

    public HiddenOrePersistence(HiddenOreManager hiddenOreManager)
    {
        this.hiddenOreManager = hiddenOreManager;
        scratchPos = new BlockPos(0);
    }

    public bool LoadColumn(IWorldChunk[] chunks)
    {
        lock (syncRoot)
        {
            if (chunks.Length == 0)
            {
                return false;
            }

            IMapChunk? mapChunk = null;
            for (int i = 0; i < chunks.Length; i++)
            {
                if (chunks[i]?.MapChunk == null)
                {
                    continue;
                }

                mapChunk = chunks[i].MapChunk;
                break;
            }

            if (mapChunk == null)
            {
                return false;
            }

            byte[]? bytes = mapChunk.GetModdata(ModDataKey);
            bool loadedLegacyData = false;
            if (bytes == null || bytes.Length == 0)
            {
                bytes = mapChunk.GetModdata(LegacyModDataKey);
                loadedLegacyData = bytes != null && bytes.Length > 0;
            }

            if (bytes == null || bytes.Length == 0)
            {
                return false;
            }

            if (loadedLegacyData)
            {
                mapChunk.SetModdata(ModDataKey, bytes);
                mapChunk.MarkDirty();
            }

            using var stream = new MemoryStream(bytes);
            using var reader = new BinaryReader(stream);

            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                int x = reader.ReadInt32();
                int y = reader.ReadInt32();
                int z = reader.ReadInt32();
                string code = reader.ReadString();

                scratchPos.Set(x, y, z);
                hiddenOreManager.Store(scratchPos, code);
            }

            return true;
        }
    }

    public void SaveColumn(IMapChunk mapChunk, int chunkX, int chunkZ)
    {
        lock (syncRoot)
        {
            reusableEntries.Clear();
            hiddenOreManager.CopyColumnEntries(chunkX, chunkZ, reusableEntries);

            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream);

            writer.Write(reusableEntries.Count);
            for (int i = 0; i < reusableEntries.Count; i++)
            {
                HiddenOreData data = reusableEntries[i];
                writer.Write(data.Position.X);
                writer.Write(data.Position.Y);
                writer.Write(data.Position.Z);
                writer.Write(data.OreBlockCode);
            }

            writer.Flush();
            mapChunk.SetModdata(ModDataKey, stream.ToArray());
            mapChunk.MarkDirty();
        }
    }

    public void SaveColumnAt(IWorldAccessor world, BlockPos pos)
    {
        IMapChunk? mapChunk = world.BlockAccessor.GetMapChunkAtBlockPos(pos);
        if (mapChunk == null)
        {
            return;
        }

        int chunkX = FloorDiv(pos.X, GlobalConstants.ChunkSize);
        int chunkZ = FloorDiv(pos.Z, GlobalConstants.ChunkSize);
        SaveColumn(mapChunk, chunkX, chunkZ);
    }

    private static int FloorDiv(int value, int divisor)
    {
        int quotient = value / divisor;
        int remainder = value % divisor;

        if (remainder != 0 && ((remainder < 0) != (divisor < 0)))
        {
            quotient--;
        }

        return quotient;
    }
}
