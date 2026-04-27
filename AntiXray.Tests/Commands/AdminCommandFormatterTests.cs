using System.Reflection;
using AntiXray.Commands;
using AntiXray.Models;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Xunit;

namespace AntiXray.Tests.Commands;

public sealed class AdminCommandFormatterTests
{
    [Fact]
    public void Usage_ReferencesAntixrayCommand()
    {
        var formatter = new AdminCommandFormatter();

        string usage = formatter.Usage();

        Assert.Contains("/antixray", usage);
        Assert.Contains("hide", usage);
        Assert.Contains("debug", usage);
        Assert.DoesNotContain("/abodebug", usage);
    }

    [Fact]
    public void DebugMessages_ReportSessionState()
    {
        var formatter = new AdminCommandFormatter();

        Assert.Contains("enabled", formatter.DebugEnabled());
        Assert.Contains("disabled", formatter.DebugDisabled());
        Assert.Contains("enabled", formatter.DebugStatus(true));
        Assert.Contains("disabled", formatter.DebugStatus(false));
    }

    [Fact]
    public void Count_UsesDisplayCenterCoordinates()
    {
        var formatter = CreateFormatter();

        string message = formatter.Count(new BlockPos(512003, 3, 512139, 0), 16, 4);

        Assert.Contains("3,3,139", message);
        Assert.DoesNotContain("512003", message);
    }

    [Fact]
    public void Reveal_UsesDisplayCenterCoordinates()
    {
        var formatter = CreateFormatter();

        string message = formatter.Reveal(new BlockPos(512003, 3, 512139, 0), 16, 4, 3);

        Assert.Contains("3,3,139", message);
        Assert.DoesNotContain("512003", message);
    }

    [Fact]
    public void Hide_UsesDisplayCenterCoordinates()
    {
        var formatter = CreateFormatter();
        var result = new AdminOreHideResult(17, 2, 1, 3, 1);

        string message = formatter.Hide(new BlockPos(512003, 3, 512139, 0), 16, result);

        Assert.Contains("3,3,139", message);
        Assert.Contains("Hid 1/2 real ore blocks", message);
        Assert.Contains("alreadyHidden=3", message);
        Assert.Contains("missingPlaceholder=1", message);
        Assert.DoesNotContain("512003", message);
    }

    [Fact]
    public void Hidden_UsesDisplayCoordinatesForCenterAndEntries()
    {
        var formatter = CreateFormatter();
        var entries = new List<HiddenOreData>
        {
            new(new BlockPosKey(512005, 4, 512140), "game:ore-copper-granite")
        };

        string message = formatter.Hidden(new BlockPos(512003, 3, 512139, 0), 16, entries);

        Assert.Contains("3,3,139", message);
        Assert.Contains("5,4,140", message);
        Assert.DoesNotContain("512005", message);
    }

    [Fact]
    public void Inspect_UsesDisplayCenterAndTargetCoordinates()
    {
        var formatter = CreateFormatter();
        string target = formatter.TargetBlock(new BlockPos(512004, 3, 512142, 0), 10, "game:rock-granite");

        string message = formatter.Inspect(new BlockPos(512003, 3, 512139, 0), 16, 1, target);

        Assert.Contains("3,3,139", message);
        Assert.Contains("4,3,142 id=10 code=game:rock-granite", message);
        Assert.DoesNotContain("512004", message);
    }

    private static AdminCommandFormatter CreateFormatter()
    {
        return new AdminCommandFormatter(new WorldCoordinateDisplayFormatter(CreateWorldManager()));
    }

    private static IWorldManagerAPI CreateWorldManager()
    {
        var worldManager = DispatchProxy.Create<IWorldManagerAPI, WorldManagerProxy>();
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
