using System.Reflection;
using AntiXray.Commands;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Xunit;

namespace AntiXray.Tests.Commands;

public sealed class WorldCoordinateDisplayFormatterTests
{
    [Fact]
    public void ToDisplayPosition_SubtractsDefaultSpawnOffset()
    {
        var formatter = new WorldCoordinateDisplayFormatter(CreateWorldManager(1024000, 1024000, [511831, 110, 511954]));

        BlockPos displayPosition = formatter.ToDisplayPosition(new BlockPos(511873, 114, 511926, 0));

        Assert.Equal(42, displayPosition.X);
        Assert.Equal(114, displayPosition.Y);
        Assert.Equal(-28, displayPosition.Z);
    }

    [Fact]
    public void ToDisplayPosition_FallsBackToMapCenterOffset()
    {
        var formatter = new WorldCoordinateDisplayFormatter(CreateWorldManager(1024000, 1024000));

        BlockPos displayPosition = formatter.ToDisplayPosition(new BlockPos(512003, 3, 512139, 0));

        Assert.Equal(3, displayPosition.X);
        Assert.Equal(3, displayPosition.Y);
        Assert.Equal(139, displayPosition.Z);
    }

    [Fact]
    public void Format_UsesDisplayCoordinates()
    {
        var formatter = new WorldCoordinateDisplayFormatter(CreateWorldManager(1024000, 1024000));

        string formatted = formatter.Format(new BlockPos(512003, 3, 512139, 0));

        Assert.Equal("3,3,139", formatted);
    }

    [Fact]
    public void ToDisplayPosition_FallsBackToMapCenter_WhenDefaultSpawnGetterThrows()
    {
        var formatter = new WorldCoordinateDisplayFormatter(CreateWorldManager(1024000, 1024000, throwsOnDefaultSpawn: true));

        BlockPos displayPosition = formatter.ToDisplayPosition(new BlockPos(512003, 3, 512139, 0));

        Assert.Equal(3, displayPosition.X);
        Assert.Equal(3, displayPosition.Y);
        Assert.Equal(139, displayPosition.Z);
    }

    private static IWorldManagerAPI CreateWorldManager(
        int mapSizeX,
        int mapSizeZ,
        int[]? defaultSpawnPosition = null,
        bool throwsOnDefaultSpawn = false)
    {
        var worldManager = DispatchProxy.Create<IWorldManagerAPI, WorldManagerProxy>();
        var proxy = (WorldManagerProxy)(object)worldManager;
        proxy.MapSizeX = mapSizeX;
        proxy.MapSizeZ = mapSizeZ;
        proxy.DefaultSpawnPosition = defaultSpawnPosition;
        proxy.ThrowsOnDefaultSpawn = throwsOnDefaultSpawn;
        return worldManager;
    }

    private class WorldManagerProxy : DispatchProxy
    {
        public int MapSizeX { get; set; }

        public int MapSizeZ { get; set; }

        public int[]? DefaultSpawnPosition { get; set; }

        public bool ThrowsOnDefaultSpawn { get; set; }

        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            return targetMethod?.Name switch
            {
                "get_MapSizeX" => MapSizeX,
                "get_MapSizeZ" => MapSizeZ,
                "get_DefaultSpawnPosition" => ThrowsOnDefaultSpawn
                    ? throw new NullReferenceException()
                    : DefaultSpawnPosition,
                _ => targetMethod?.ReturnType.IsValueType == true
                    ? Activator.CreateInstance(targetMethod.ReturnType)
                    : null
            };
        }
    }
}
