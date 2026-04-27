using AntiXray.Commands;
using AntiXray.Models;
using Vintagestory.API.MathTools;

namespace AntiXray.Systems;

public sealed class ProspectingRevealService
{
    private readonly IProspectingOreRevealer oreRevealer;
    private readonly HiddenOreManager? hiddenOreManager;
    private readonly IOreRevealDebugReporter? debugReporter;
    private readonly int radius;
    private readonly BlockPos scratchPos;

    public ProspectingRevealService(IProspectingOreRevealer oreRevealer, int radius)
        : this(oreRevealer, radius, null, null)
    {
    }

    public ProspectingRevealService(
        IProspectingOreRevealer oreRevealer,
        int radius,
        HiddenOreManager? hiddenOreManager,
        IOreRevealDebugReporter? debugReporter)
    {
        this.oreRevealer = oreRevealer;
        this.hiddenOreManager = hiddenOreManager;
        this.debugReporter = debugReporter;
        this.radius = ProspectingRevealRadius.Clamp(radius);
        scratchPos = new BlockPos(0);
    }

    public int RevealAround(BlockPos center)
    {
        int revealed = 0;

        for (int dx = -radius; dx <= radius; dx++)
        {
            for (int dy = -radius; dy <= radius; dy++)
            {
                for (int dz = -radius; dz <= radius; dz++)
                {
                    scratchPos.Set(center.X + dx, center.InternalY + dy, center.Z + dz);
                    HiddenOreData? data = null;
                    if (debugReporter != null && hiddenOreManager?.TryGet(scratchPos, out HiddenOreData hiddenOreData) == true)
                    {
                        data = hiddenOreData;
                    }

                    if (oreRevealer.RevealForProspectingPick(scratchPos))
                    {
                        revealed++;
                        if (data != null)
                        {
                            debugReporter?.ReportReveal("a prospecting pick revealed hidden ore", scratchPos, data.OreBlockCode, center);
                        }
                    }
                }
            }
        }

        debugReporter?.ReportBatch("a prospecting pick revealed hidden ore in its configured radius", revealed, center);
        return revealed;
    }
}
