using AntiXray.Models;
using Vintagestory.API.MathTools;

namespace AntiXray.Systems;

public sealed class ChunkIndex
{
    private readonly Dictionary<ChunkPos, List<BlockPos>> hiddenOrePositions = new();
    private readonly int chunkSize;

    public ChunkIndex(int chunkSize)
    {
        this.chunkSize = chunkSize;
    }

    public ChunkPos GetChunkPos(BlockPos pos)
    {
        return new ChunkPos(FloorDiv(pos.X, chunkSize), FloorDiv(pos.InternalY, chunkSize), FloorDiv(pos.Z, chunkSize));
    }

    public void Add(BlockPos pos)
    {
        ChunkPos chunkPos = GetChunkPos(pos);

        if (!hiddenOrePositions.TryGetValue(chunkPos, out List<BlockPos>? positions))
        {
            positions = new List<BlockPos>(8);
            hiddenOrePositions.Add(chunkPos, positions);
        }

        positions.Add(pos.Copy());
    }

    public void Remove(BlockPos pos)
    {
        ChunkPos chunkPos = GetChunkPos(pos);

        if (!hiddenOrePositions.TryGetValue(chunkPos, out List<BlockPos>? positions))
        {
            return;
        }

        for (int i = positions.Count - 1; i >= 0; i--)
        {
            BlockPos candidate = positions[i];
            if (candidate.X != pos.X || candidate.InternalY != pos.InternalY || candidate.Z != pos.Z)
            {
                continue;
            }

            int lastIndex = positions.Count - 1;
            positions[i] = positions[lastIndex];
            positions.RemoveAt(lastIndex);
            break;
        }

        if (positions.Count == 0)
        {
            hiddenOrePositions.Remove(chunkPos);
        }
    }

    public bool TryGetChunkPositions(ChunkPos chunkPos, out List<BlockPos> positions)
    {
        return hiddenOrePositions.TryGetValue(chunkPos, out positions!);
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
