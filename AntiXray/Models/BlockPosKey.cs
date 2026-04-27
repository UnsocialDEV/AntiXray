using Vintagestory.API.MathTools;

namespace AntiXray.Models;

public readonly struct BlockPosKey : IEquatable<BlockPosKey>
{
    public BlockPosKey(int x, int y, int z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public int X { get; }

    public int Y { get; }

    public int Z { get; }

    public static BlockPosKey From(BlockPos pos)
    {
        return new BlockPosKey(pos.X, pos.InternalY, pos.Z);
    }

    public bool Equals(BlockPosKey other)
    {
        return X == other.X && Y == other.Y && Z == other.Z;
    }

    public override bool Equals(object? obj)
    {
        return obj is BlockPosKey other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y, Z);
    }
}
