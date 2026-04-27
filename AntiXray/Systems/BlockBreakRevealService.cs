using AntiXray.Models;
using AntiXray.Commands;
using Vintagestory.API.MathTools;

namespace AntiXray.Systems;

public sealed class BlockBreakRevealService
{
    private readonly HiddenOreManager hiddenOreManager;
    private readonly IHiddenOreRevealer revealService;
    private readonly HiddenOreVeinCollector veinCollector;
    private readonly int radius;
    private readonly bool revealConnectedVein;
    private readonly int maxOreBlocks;
    private readonly IOreRevealDebugReporter? debugReporter;
    private BlockPos? lastTriggerPos;
    private readonly List<BlockPos> reusableRevealPositions = new(128);
    private readonly BlockPos scratchPos;

    public BlockBreakRevealService(
        HiddenOreManager hiddenOreManager,
        IHiddenOreRevealer revealService,
        HiddenOreVeinCollector veinCollector,
        int radius,
        bool revealConnectedVein,
        int maxOreBlocks)
        : this(hiddenOreManager, revealService, veinCollector, radius, revealConnectedVein, maxOreBlocks, null)
    {
    }

    public BlockBreakRevealService(
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

    public int RevealNear(BlockPos center)
    {
        reusableRevealPositions.Clear();
        lastTriggerPos = center.Copy();

        if (hiddenOreManager.TryGet(center, out _))
        {
            CollectRevealPositions(center);
            return RevealCollected();
        }

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
            return;
        }

        reusableRevealPositions.Add(seed.Copy());
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

        debugReporter?.ReportBatch("a player broke a block near a hidden ore vein", revealed, lastTriggerPos!);
        return revealed;
    }

    private void ReportReveal(BlockPos pos, HiddenOreData? data)
    {
        if (debugReporter == null || data == null)
        {
            return;
        }

        debugReporter.ReportReveal("a player broke a block near hidden ore", pos, data.OreBlockCode, lastTriggerPos);
    }
}
