using AntiXray.Systems;
using Vintagestory.API.MathTools;
using Xunit;

namespace AntiXray.Tests.Systems;

public sealed class BlockBreakRevealServiceTests
{
    [Fact]
    public void RevealNear_RevealsHiddenOreAtConfiguredDistance()
    {
        var manager = CreateManager();
        var revealer = new RecordingRevealer();
        manager.Store(new BlockPos(3, 0, 0, 0), "game:ore-copper-granite");
        var service = CreateService(manager, revealer, radius: 3, revealConnectedVein: false);

        int revealed = service.RevealNear(new BlockPos(0, 0, 0, 0));

        Assert.Equal(1, revealed);
        Assert.Contains(revealer.Revealed, pos => pos.X == 3);
    }

    [Fact]
    public void RevealNear_DoesNotRevealHiddenOreOutsideConfiguredDistance()
    {
        var manager = CreateManager();
        var revealer = new RecordingRevealer();
        manager.Store(new BlockPos(4, 0, 0, 0), "game:ore-copper-granite");
        var service = CreateService(manager, revealer, radius: 3, revealConnectedVein: false);

        int revealed = service.RevealNear(new BlockPos(0, 0, 0, 0));

        Assert.Equal(0, revealed);
        Assert.Empty(revealer.Revealed);
    }

    [Fact]
    public void RevealNear_RevealsSelectedHiddenOreBeforeBreakHandling()
    {
        var manager = CreateManager();
        var revealer = new RecordingRevealer();
        manager.Store(new BlockPos(0, 0, 0, 0), "game:ore-copper-granite");
        var service = CreateService(manager, revealer, radius: 3, revealConnectedVein: false);

        int revealed = service.RevealNear(new BlockPos(0, 0, 0, 0));

        Assert.Equal(1, revealed);
        Assert.Contains(revealer.Revealed, pos => pos.X == 0 && pos.InternalY == 0 && pos.Z == 0);
    }

    [Fact]
    public void RevealNear_PrioritizesSelectedHiddenOreOverNearbyOre()
    {
        var manager = CreateManager();
        var revealer = new RecordingRevealer();
        manager.Store(new BlockPos(-3, -3, -3, 0), "game:ore-tin-granite");
        manager.Store(new BlockPos(0, 0, 0, 0), "game:ore-copper-granite");
        var service = CreateService(manager, revealer, radius: 3, revealConnectedVein: false);

        int revealed = service.RevealNear(new BlockPos(0, 0, 0, 0));

        Assert.Equal(1, revealed);
        Assert.Single(revealer.Revealed);
        Assert.Equal(0, revealer.Revealed[0].X);
        Assert.Equal(0, revealer.Revealed[0].InternalY);
        Assert.Equal(0, revealer.Revealed[0].Z);
    }

    [Fact]
    public void RevealNear_RevealsConnectedVeinThroughCollector()
    {
        var manager = CreateManager();
        var revealer = new RecordingRevealer();
        manager.Store(new BlockPos(2, 0, 0, 0), "game:ore-copper-granite");
        manager.Store(new BlockPos(3, 0, 0, 0), "game:ore-copper-granite");
        var service = CreateService(manager, revealer, radius: 3, revealConnectedVein: true);

        int revealed = service.RevealNear(new BlockPos(0, 0, 0, 0));

        Assert.Equal(2, revealed);
    }

    [Fact]
    public void RevealNear_ReportsDebugReasonWhenOreIsRevealed()
    {
        var manager = CreateManager();
        var revealer = new RecordingRevealer();
        var debugReporter = new RecordingDebugReporter();
        manager.Store(new BlockPos(3, 0, 0, 0), "game:ore-copper-granite");
        var service = new BlockBreakRevealService(
            manager,
            revealer,
            new HiddenOreVeinCollector(manager),
            3,
            revealConnectedVein: false,
            128,
            debugReporter);

        int revealed = service.RevealNear(new BlockPos(0, 0, 0, 0));

        Assert.Equal(1, revealed);
        Assert.Single(debugReporter.Reveals);
        Assert.Contains("broke a block", debugReporter.Reveals[0].Reason);
        Assert.Equal("game:ore-copper-granite", debugReporter.Reveals[0].OreCode);
    }

    private static BlockBreakRevealService CreateService(
        HiddenOreManager manager,
        RecordingRevealer revealer,
        int radius,
        bool revealConnectedVein)
    {
        return new BlockBreakRevealService(
            manager,
            revealer,
            new HiddenOreVeinCollector(manager),
            radius,
            revealConnectedVein,
            128);
    }

    private static HiddenOreManager CreateManager()
    {
        return new HiddenOreManager(new ChunkIndex(32));
    }

    private sealed class RecordingRevealer : IHiddenOreRevealer
    {
        public List<BlockPos> Revealed { get; } = new();

        public bool ForceReveal(BlockPos pos)
        {
            Revealed.Add(pos.Copy());
            return true;
        }
    }

    private sealed class RecordingDebugReporter : AntiXray.Commands.IOreRevealDebugReporter
    {
        public List<(string Reason, BlockPos Pos, string OreCode, BlockPos? Trigger)> Reveals { get; } = new();

        public void ReportReveal(string reason, BlockPos pos, string oreCode, BlockPos? triggerPos = null)
        {
            Reveals.Add((reason, pos.Copy(), oreCode, triggerPos?.Copy()));
        }

        public void ReportBatch(string reason, int count, BlockPos triggerPos)
        {
        }
    }
}
