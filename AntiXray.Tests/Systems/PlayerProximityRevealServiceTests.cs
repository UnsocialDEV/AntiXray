using AntiXray.Systems;
using Vintagestory.API.MathTools;
using Xunit;

namespace AntiXray.Tests.Systems;

public sealed class PlayerProximityRevealServiceTests
{
    [Fact]
    public void RevealNear_DoesNothingWhenDisabled()
    {
        var manager = CreateManager();
        manager.Store(new BlockPos(1, 0, 0, 0), "game:ore-copper-granite");
        var revealer = new RecordingRevealer(true);
        var service = CreateService(manager, revealer, enabled: false, radius: 24, maxPerEvent: 64);

        int revealed = service.RevealNear(new BlockPos(0, 0, 0, 0));

        Assert.Equal(0, revealed);
        Assert.Empty(revealer.Attempts);
    }

    [Fact]
    public void RevealNear_RevealsExposedHiddenOreWithinDistance()
    {
        var manager = CreateManager();
        manager.Store(new BlockPos(24, 0, 0, 0), "game:ore-copper-granite");
        var revealer = new RecordingRevealer(true);
        var service = CreateService(manager, revealer, enabled: true, radius: 24, maxPerEvent: 64);

        int revealed = service.RevealNear(new BlockPos(0, 0, 0, 0));

        Assert.Equal(1, revealed);
        Assert.Single(revealer.Attempts);
    }

    [Fact]
    public void RevealNear_DoesNotRevealHiddenOreOutsideDistance()
    {
        var manager = CreateManager();
        manager.Store(new BlockPos(25, 0, 0, 0), "game:ore-copper-granite");
        var revealer = new RecordingRevealer(true);
        var service = CreateService(manager, revealer, enabled: true, radius: 24, maxPerEvent: 64);

        int revealed = service.RevealNear(new BlockPos(0, 0, 0, 0));

        Assert.Equal(0, revealed);
        Assert.Empty(revealer.Attempts);
    }

    [Fact]
    public void RevealNear_DoesNotCountEnclosedHiddenOre()
    {
        var manager = CreateManager();
        manager.Store(new BlockPos(1, 0, 0, 0), "game:ore-copper-granite");
        var revealer = new RecordingRevealer(false);
        var service = CreateService(manager, revealer, enabled: true, radius: 24, maxPerEvent: 64);

        int revealed = service.RevealNear(new BlockPos(0, 0, 0, 0));

        Assert.Equal(0, revealed);
        Assert.Single(revealer.Attempts);
    }

    [Fact]
    public void RevealNear_StopsAtPerEventCap()
    {
        var manager = CreateManager();
        manager.Store(new BlockPos(1, 0, 0, 0), "game:ore-copper-granite");
        manager.Store(new BlockPos(2, 0, 0, 0), "game:ore-tin-granite");
        manager.Store(new BlockPos(3, 0, 0, 0), "game:ore-gold-granite");
        var revealer = new RecordingRevealer(true);
        var service = CreateService(manager, revealer, enabled: true, radius: 24, maxPerEvent: 2);

        int revealed = service.RevealNear(new BlockPos(0, 0, 0, 0));

        Assert.Equal(2, revealed);
        Assert.Equal(2, revealer.Attempts.Count);
    }

    [Fact]
    public void RevealNear_ReportsPlayerProximityReason()
    {
        var manager = CreateManager();
        manager.Store(new BlockPos(1, 0, 0, 0), "game:ore-copper-granite");
        var revealer = new RecordingRevealer(true);
        var debugReporter = new RecordingDebugReporter();
        var service = new PlayerProximityRevealService(
            enabled: true,
            new HiddenOreProximityQuery(manager, 32),
            revealer,
            radius: 24,
            maxPerEvent: 64,
            debugReporter);

        int revealed = service.RevealNear(new BlockPos(0, 0, 0, 0));

        Assert.Equal(1, revealed);
        Assert.Single(debugReporter.Reveals);
        Assert.Contains("moved close", debugReporter.Reveals[0].Reason);
        Assert.Equal("game:ore-copper-granite", debugReporter.Reveals[0].OreCode);
    }

    private static PlayerProximityRevealService CreateService(
        HiddenOreManager manager,
        RecordingRevealer revealer,
        bool enabled,
        int radius,
        int maxPerEvent)
    {
        return new PlayerProximityRevealService(
            enabled,
            new HiddenOreProximityQuery(manager, 32),
            revealer,
            radius,
            maxPerEvent);
    }

    private static HiddenOreManager CreateManager()
    {
        return new HiddenOreManager(new ChunkIndex(32));
    }

    private sealed class RecordingRevealer : IExposedOreRevealer
    {
        private readonly bool revealResult;

        public RecordingRevealer(bool revealResult)
        {
            this.revealResult = revealResult;
        }

        public List<BlockPos> Attempts { get; } = new();

        public bool TryRevealIfExposed(BlockPos pos)
        {
            Attempts.Add(pos.Copy());
            return revealResult;
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
