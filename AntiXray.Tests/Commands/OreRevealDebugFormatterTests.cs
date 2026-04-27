using System.Reflection;
using AntiXray.Commands;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Xunit;

namespace AntiXray.Tests.Commands;

public sealed class OreRevealDebugFormatterTests
{
    [Fact]
    public void Reveal_UsesDisplayCoordinatesAndReason()
    {
        var formatter = new OreRevealDebugFormatter(new WorldCoordinateDisplayFormatter(CreateWorldManager()));

        string message = formatter.Reveal(
            "a player broke a block near hidden ore",
            new BlockPos(512005, 4, 512140, 0),
            "game:ore-copper-granite",
            new BlockPos(512003, 3, 512139, 0));

        Assert.Contains("game:ore-copper-granite", message);
        Assert.Contains("5,4,140", message);
        Assert.Contains("trigger=3,3,139", message);
        Assert.Contains("a player broke a block near hidden ore", message);
        Assert.DoesNotContain("512005", message);
    }

    [Fact]
    public void Batch_UsesDisplayTriggerCoordinates()
    {
        var formatter = new OreRevealDebugFormatter(new WorldCoordinateDisplayFormatter(CreateWorldManager()));

        string message = formatter.Batch("a prospecting pick revealed hidden ore", 3, new BlockPos(512003, 3, 512139, 0));

        Assert.Contains("Revealed 3 hidden ore blocks", message);
        Assert.Contains("trigger=3,3,139", message);
        Assert.DoesNotContain("512003", message);
    }

    private static IWorldManagerAPI CreateWorldManager()
    {
        IWorldManagerAPI worldManager = DispatchProxy.Create<IWorldManagerAPI, WorldManagerProxy>();
        var proxy = (WorldManagerProxy)(object)worldManager;
        proxy.MapSizeX = 1024000;
        proxy.MapSizeZ = 1024000;
        return worldManager;
    }

    private class WorldManagerProxy : DispatchProxy
    {
        public int MapSizeX { get; set; }

        public int MapSizeZ { get; set; }

        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            return targetMethod?.Name switch
            {
                "get_MapSizeX" => MapSizeX,
                "get_MapSizeZ" => MapSizeZ,
                "get_DefaultSpawnPosition" => null,
                _ => targetMethod?.ReturnType.IsValueType == true
                    ? Activator.CreateInstance(targetMethod.ReturnType)
                    : null
            };
        }
    }
}
