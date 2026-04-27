using AntiXray.Systems;
using Vintagestory.API.MathTools;
using Xunit;

namespace AntiXray.Tests.Systems;

public sealed class HiddenOreExplosionRevealServiceTests
{
    [Fact]
    public void RevealBeforeExplosion_DoesNothingForNonHiddenBlock()
    {
        var manager = CreateManager();
        var revealer = new RecordingRevealer();
        var service = CreateService(manager, revealer, revealConnectedVein: true, maxOreBlocks: 128);

        ExplosionRevealResult result = service.RevealBeforeExplosion(new BlockPos(0, 0, 0, 0));

        Assert.False(result.Handled);
        Assert.Equal(0, result.Revealed);
        Assert.Empty(revealer.Revealed);
    }

    [Fact]
    public void RevealBeforeExplosion_RevealsNearbyHiddenOreWithoutHandlingPlaceholderExplosion()
    {
        var manager = CreateManager();
        var revealer = new RecordingRevealer();
        manager.Store(new BlockPos(3, 0, 0, 0), "game:ore-copper-granite");
        var service = CreateService(manager, revealer, revealConnectedVein: false, maxOreBlocks: 128);

        ExplosionRevealResult result = service.RevealBeforeExplosion(new BlockPos(0, 0, 0, 0));

        Assert.False(result.Handled);
        Assert.Equal(1, result.Revealed);
        Assert.Contains(revealer.Revealed, pos => pos.X == 3 && pos.InternalY == 0 && pos.Z == 0);
    }

    [Fact]
    public void RevealNearExplosion_DoesNotRevealHiddenOreOutsideRadius()
    {
        var manager = CreateManager();
        var revealer = new RecordingRevealer();
        manager.Store(new BlockPos(4, 0, 0, 0), "game:ore-copper-granite");
        var service = CreateService(manager, revealer, revealConnectedVein: false, maxOreBlocks: 128);

        int revealed = service.RevealNearExplosion(new BlockPos(0, 0, 0, 0));

        Assert.Equal(0, revealed);
        Assert.Empty(revealer.Revealed);
    }

    [Fact]
    public void RevealBeforeExplosion_RevealsHiddenOreAtExplodedPosition()
    {
        var manager = CreateManager();
        var revealer = new RecordingRevealer();
        manager.Store(new BlockPos(0, 0, 0, 0), "game:ore-copper-granite");
        var service = CreateService(manager, revealer, revealConnectedVein: false, maxOreBlocks: 128);

        ExplosionRevealResult result = service.RevealBeforeExplosion(new BlockPos(0, 0, 0, 0));

        Assert.True(result.Handled);
        Assert.Equal(1, result.Revealed);
        Assert.Contains(revealer.Revealed, pos => pos.X == 0 && pos.InternalY == 0 && pos.Z == 0);
    }

    [Fact]
    public void RevealBeforeExplosion_RevealsConnectedSameCodeVeinUpToCap()
    {
        var manager = CreateManager();
        var revealer = new RecordingRevealer();
        manager.Store(new BlockPos(0, 0, 0, 0), "game:ore-copper-granite");
        manager.Store(new BlockPos(1, 0, 0, 0), "game:ore-copper-granite");
        manager.Store(new BlockPos(2, 0, 0, 0), "game:ore-copper-granite");
        var service = CreateService(manager, revealer, revealConnectedVein: true, maxOreBlocks: 2);

        ExplosionRevealResult result = service.RevealBeforeExplosion(new BlockPos(0, 0, 0, 0));

        Assert.True(result.Handled);
        Assert.Equal(2, result.Revealed);
        Assert.Equal(2, revealer.Revealed.Count);
    }

    [Fact]
    public void RevealBeforeExplosion_DoesNotMergeDifferentOreCode()
    {
        var manager = CreateManager();
        var revealer = new RecordingRevealer();
        manager.Store(new BlockPos(0, 0, 0, 0), "game:ore-copper-granite");
        manager.Store(new BlockPos(1, 0, 0, 0), "game:ore-tin-granite");
        var service = CreateService(manager, revealer, revealConnectedVein: true, maxOreBlocks: 128);

        ExplosionRevealResult result = service.RevealBeforeExplosion(new BlockPos(0, 0, 0, 0));

        Assert.True(result.Handled);
        Assert.Equal(1, result.Revealed);
        Assert.Single(revealer.Revealed);
    }

    [Fact]
    public void RevealBeforeExplosion_IsNotHandledWhenRevealFails()
    {
        var manager = CreateManager();
        var revealer = new RecordingRevealer { ShouldReveal = false };
        manager.Store(new BlockPos(0, 0, 0, 0), "game:ore-copper-granite");
        var service = CreateService(manager, revealer, revealConnectedVein: false, maxOreBlocks: 128);

        ExplosionRevealResult result = service.RevealBeforeExplosion(new BlockPos(0, 0, 0, 0));

        Assert.False(result.Handled);
        Assert.Equal(0, result.Revealed);
        Assert.Single(revealer.Revealed);
    }

    [Fact]
    public void RevealBeforeExplosion_ReportsExactExplosionReason()
    {
        var manager = CreateManager();
        var revealer = new RecordingRevealer();
        var debugReporter = new RecordingDebugReporter();
        manager.Store(new BlockPos(0, 0, 0, 0), "game:ore-copper-granite");
        var service = new HiddenOreExplosionRevealService(
            manager,
            revealer,
            new HiddenOreVeinCollector(manager),
            3,
            revealConnectedVein: false,
            128,
            debugReporter);

        ExplosionRevealResult result = service.RevealBeforeExplosion(new BlockPos(0, 0, 0, 0));

        Assert.True(result.Handled);
        Assert.Single(debugReporter.Reveals);
        Assert.Contains("explosion", debugReporter.Reveals[0].Reason);
        Assert.Equal("game:ore-copper-granite", debugReporter.Reveals[0].OreCode);
    }

    private static HiddenOreExplosionRevealService CreateService(
        HiddenOreManager manager,
        RecordingRevealer revealer,
        bool revealConnectedVein,
        int maxOreBlocks)
    {
        return new HiddenOreExplosionRevealService(
            manager,
            revealer,
            new HiddenOreVeinCollector(manager),
            3,
            revealConnectedVein,
            maxOreBlocks);
    }

    private static HiddenOreManager CreateManager()
    {
        return new HiddenOreManager(new ChunkIndex(32));
    }

    private sealed class RecordingRevealer : IHiddenOreRevealer
    {
        public List<BlockPos> Revealed { get; } = new();

        public bool ShouldReveal { get; init; } = true;

        public bool ForceReveal(BlockPos pos)
        {
            Revealed.Add(pos.Copy());
            return ShouldReveal;
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
