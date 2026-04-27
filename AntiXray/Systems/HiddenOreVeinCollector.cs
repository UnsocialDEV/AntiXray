using AntiXray.Models;
using Vintagestory.API.MathTools;

namespace AntiXray.Systems;

public sealed class HiddenOreVeinCollector
{
    private static readonly int[] Offsets =
    [
        1, 0, 0,
        -1, 0, 0,
        0, 1, 0,
        0, -1, 0,
        0, 0, 1,
        0, 0, -1
    ];

    private readonly HiddenOreManager hiddenOreManager;
    private readonly Queue<BlockPosKey> pending = new();
    private readonly HashSet<BlockPosKey> visited = new();

    public HiddenOreVeinCollector(HiddenOreManager hiddenOreManager)
    {
        this.hiddenOreManager = hiddenOreManager;
    }

    public int CollectConnected(BlockPos seed, int maxBlocks, List<BlockPos> output)
    {
        if (maxBlocks <= 0)
        {
            return 0;
        }

        if (!hiddenOreManager.TryGet(seed, out HiddenOreData seedData))
        {
            return 0;
        }

        pending.Clear();
        visited.Clear();

        BlockPosKey seedKey = BlockPosKey.From(seed);
        pending.Enqueue(seedKey);
        visited.Add(seedKey);

        while (pending.Count > 0 && output.Count < maxBlocks)
        {
            BlockPosKey key = pending.Dequeue();
            var pos = new BlockPos(key.X, key.Y, key.Z);

            if (!hiddenOreManager.TryGet(pos, out HiddenOreData data))
            {
                continue;
            }

            if (!StringComparer.Ordinal.Equals(data.OreBlockCode, seedData.OreBlockCode))
            {
                continue;
            }

            output.Add(pos);

            for (int i = 0; i < Offsets.Length; i += 3)
            {
                var neighborKey = new BlockPosKey(key.X + Offsets[i], key.Y + Offsets[i + 1], key.Z + Offsets[i + 2]);
                if (visited.Add(neighborKey))
                {
                    pending.Enqueue(neighborKey);
                }
            }
        }

        return output.Count;
    }
}
