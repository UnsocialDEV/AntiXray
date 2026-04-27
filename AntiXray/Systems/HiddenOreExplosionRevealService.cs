using AntiXray.Commands;
using AntiXray.Models;
using Vintagestory.API.MathTools;

namespace AntiXray.Systems;

public sealed class HiddenOreExplosionRevealService
{
    private readonly HiddenOreManager hiddenOreManager;
    private readonly IHiddenOreRevealer revealService;
    private readonly HiddenOreVeinCollector veinCollector;
    private readonly int radius;
    private readonly bool revealConnectedVein;
    private readonly int maxOreBlocks;
    private readonly IOreRevealDebugReporter? debugReporter;
    private BlockPos? lastTriggerPos;
    private string lastReason = string.Empty;
    private readonly List<BlockPos> reusableRevealPositions = new(128);
    private readonly BlockPos scratchPos;

    public HiddenOreExplosionRevealService(
        HiddenOreManager hiddenOreManager,
        IHiddenOreRevealer revealService,
        HiddenOreVeinCollector veinCollector,
        int radius,
        bool revealConnectedVein,
        int maxOreBlocks)
        : this(hiddenOreManager, revealService, veinCollector, radius, revealConnectedVein, maxOreBlocks, null)
    {
    }

    public HiddenOreExplosionRevealService(
        HiddenOreManager hiddenOreManager,
        IHiddenOreRevealer revealService,
        HiddenOreVeinCollector veinCollector,
        int radius,
        bool revealConnectedVein,
        int maxOreBlocks,
        IOreRevealDebugReporter? debugReporter)
    {
        this.hiddenOreManager = hiddenOreManager;
        this.revealService = revealService;
        this.veinCollector = veinCollector;
        this.radius = BlockBreakRevealRadius.Clamp(radius);
        this.revealConnectedVein = revealConnectedVein;
        this.maxOreBlocks = maxOreBlocks < 1 ? 1 : maxOreBlocks;
        this.debugReporter = debugReporter;
        scratchPos = new BlockPos(0);
    }

    public ExplosionRevealResult RevealBeforeExplosion(BlockPos pos)
    {
        reusableRevealPositions.Clear();

        if (hiddenOreManager.TryGet(pos, out _))
        {
            lastTriggerPos = pos.Copy();
            lastReason = "an explosion reached hidden ore directly";
            CollectRevealPositions(pos);
            int revealed = RevealCollected();
            return new ExplosionRevealResult(revealed > 0, revealed);
        }

        int nearbyRevealed = RevealNearExplosion(pos);
        return new ExplosionRevealResult(false, nearbyRevealed);
    }

    public int RevealNearExplosion(BlockPos center)
    {
        reusableRevealPositions.Clear();
        lastTriggerPos = center.Copy();
        lastReason = "an explosion broke stone near hidden ore";

        for (int dx = -radius; dx <= radius; dx++)
        {
            for (int dy = -radius; dy <= radius; dy++)
            {
                for (int dz = -radius; dz <= radius; dz++)
                {
                    scratchPos.Set(center.X + dx, center.InternalY + dy, center.Z + dz);
                    if (scratchPos.X == center.X && scratchPos.InternalY == center.InternalY && scratchPos.Z == center.Z)
                    {
                        continue;
                    }

                    if (!hiddenOreManager.TryGet(scratchPos, out _))
                    {
                        continue;
                    }

                    CollectRevealPositions(scratchPos);
                    return RevealCollected();
                }
            }
        }

        return 0;
    }

    private void CollectRevealPositions(BlockPos seed)
    {
        if (revealConnectedVein)
        {
            veinCollector.CollectConnected(seed, maxOreBlocks, reusableRevealPositions);
        }
        else
        {
            reusableRevealPositions.Add(seed.Copy());
        }
    }

    private int RevealCollected()
    {
        int revealed = 0;
        for (int i = 0; i < reusableRevealPositions.Count; i++)
        {
            BlockPos pos = reusableRevealPositions[i];
            HiddenOreData? data = null;
            if (debugReporter != null && hiddenOreManager.TryGet(pos, out HiddenOreData hiddenOreData))
            {
                data = hiddenOreData;
            }

            if (revealService.ForceReveal(pos))
            {
                revealed++;
                ReportReveal(pos, data);
            }
        }

        debugReporter?.ReportBatch(lastReason, revealed, lastTriggerPos!);
        return revealed;
    }

    private void ReportReveal(BlockPos pos, HiddenOreData? data)
    {
        if (debugReporter == null || data == null)
        {
            return;
        }

        debugReporter.ReportReveal(lastReason, pos, data.OreBlockCode, lastTriggerPos);
    }
}
