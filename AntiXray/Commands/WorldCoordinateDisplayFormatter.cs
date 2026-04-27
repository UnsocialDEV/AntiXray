using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace AntiXray.Commands;

public sealed class WorldCoordinateDisplayFormatter
{
    private readonly IWorldManagerAPI? worldManager;

    public WorldCoordinateDisplayFormatter(ICoreServerAPI api)
        : this(api?.WorldManager)
    {
    }

    public WorldCoordinateDisplayFormatter(IWorldManagerAPI? worldManager)
    {
        this.worldManager = worldManager;
    }

    public BlockPos ToDisplayPosition(BlockPos worldPosition)
    {
        return new BlockPos(
            worldPosition.X - GetHorizontalOffsetX(),
            worldPosition.Y,
            worldPosition.Z - GetHorizontalOffsetZ(),
            worldPosition.dimension);
    }

    public string Format(BlockPos worldPosition)
    {
        BlockPos displayPosition = ToDisplayPosition(worldPosition);
        return $"{displayPosition.X},{displayPosition.Y},{displayPosition.Z}";
    }

    public string Format(int x, int y, int z)
    {
        return Format(new BlockPos(x, y, z));
    }

    private int GetHorizontalOffsetX()
    {
        int[]? defaultSpawnPosition = GetDefaultSpawnPosition();
        return defaultSpawnPosition?.Length >= 1
            ? defaultSpawnPosition[0]
            : GetMapCenterOffset(GetMapSizeX());
    }

    private int GetHorizontalOffsetZ()
    {
        int[]? defaultSpawnPosition = GetDefaultSpawnPosition();
        return defaultSpawnPosition?.Length >= 3
            ? defaultSpawnPosition[2]
            : GetMapCenterOffset(GetMapSizeZ());
    }

    private int[]? GetDefaultSpawnPosition()
    {
        try
        {
            return worldManager?.DefaultSpawnPosition;
        }
        catch (NullReferenceException)
        {
            return null;
        }
    }

    private int GetMapSizeX()
    {
        try
        {
            return worldManager?.MapSizeX ?? 0;
        }
        catch (NullReferenceException)
        {
            return 0;
        }
    }

    private int GetMapSizeZ()
    {
        try
        {
            return worldManager?.MapSizeZ ?? 0;
        }
        catch (NullReferenceException)
        {
            return 0;
        }
    }

    private static int GetMapCenterOffset(int mapSize)
    {
        return mapSize > 0 ? mapSize / 2 : 0;
    }
}
