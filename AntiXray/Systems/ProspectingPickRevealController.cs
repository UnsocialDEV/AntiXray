using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace AntiXray.Systems;

public sealed class ProspectingPickRevealController
{
    private readonly bool enabled;
    private readonly ProspectingPickDetector detector;
    private readonly ProspectingRevealService revealService;

    public ProspectingPickRevealController(bool enabled, ProspectingPickDetector detector, ProspectingRevealService revealService)
    {
        this.enabled = enabled;
        this.detector = detector;
        this.revealService = revealService;
    }

    public int TryReveal(IServerPlayer player, BlockPos pos)
    {
        if (!enabled)
        {
            return 0;
        }

        if (!detector.IsHoldingProspectingPick(player))
        {
            return 0;
        }

        return revealService.RevealAround(pos);
    }
}
