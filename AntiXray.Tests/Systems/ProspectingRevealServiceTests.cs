using AntiXray.Systems;
using Vintagestory.API.MathTools;
using Xunit;

namespace AntiXray.Tests.Systems;

public sealed class ProspectingRevealServiceTests
{
    [Fact]
    public void RevealAround_RevealsHiddenOreInsideRadius()
    {
        var revealer = new RecordingRevealer(new BlockPos(2, 0, 0, 0));
        var service = new ProspectingRevealService(revealer, 2);

        int revealed = service.RevealAround(new BlockPos(0, 0, 0, 0));

        Assert.Equal(1, revealed);
        Assert.Equal(125, revealer.Attempts);
    }

    [Fact]
    public void RevealAround_DoesNotRevealHiddenOreOutsideRadius()
    {
        var revealer = new RecordingRevealer(new BlockPos(3, 0, 0, 0));
        var service = new ProspectingRevealService(revealer, 2);

        int revealed = service.RevealAround(new BlockPos(0, 0, 0, 0));

        Assert.Equal(0, revealed);
    }

    [Fact]
    public void RevealAround_IsIdempotentWhenOreWasAlreadyRevealed()
    {
        var hiddenPos = new BlockPos(1, 0, 0, 0);
        var revealer = new RecordingRevealer(hiddenPos);
        var service = new ProspectingRevealService(revealer, 1);

        int firstReveal = service.RevealAround(new BlockPos(0, 0, 0, 0));
        int secondReveal = service.RevealAround(new BlockPos(0, 0, 0, 0));

        Assert.Equal(1, firstReveal);
        Assert.Equal(0, secondReveal);
    }

    [Fact]
    public void RevealAround_ReportsProspectingPickReason()
    {
        var hiddenPos = new BlockPos(1, 0, 0, 0);
        var manager = new HiddenOreManager(new ChunkIndex(32));
        manager.Store(hiddenPos, "game:ore-copper-granite");
        var revealer = new RecordingRevealer(hiddenPos);
        var debugReporter = new RecordingDebugReporter();
        var service = new ProspectingRevealService(revealer, 1, manager, debugReporter);

        int revealed = service.RevealAround(new BlockPos(0, 0, 0, 0));

        Assert.Equal(1, revealed);
        Assert.Single(debugReporter.Reveals);
        Assert.Contains("prospecting pick", debugReporter.Reveals[0].Reason);
        Assert.Equal("game:ore-copper-granite", debugReporter.Reveals[0].OreCode);
    }

    private sealed class RecordingRevealer : IProspectingOreRevealer
    {
        private readonly BlockPos hiddenPos;
        private bool revealed;

        public RecordingRevealer(BlockPos hiddenPos)
        {
            this.hiddenPos = hiddenPos;
        }

        public int Attempts { get; private set; }

        public bool RevealForProspectingPick(BlockPos pos)
        {
            Attempts++;
            if (revealed)
            {
                return false;
            }

            if (pos.X != hiddenPos.X || pos.InternalY != hiddenPos.InternalY || pos.Z != hiddenPos.Z)
            {
                return false;
            }

            revealed = true;
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
