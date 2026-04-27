using Vintagestory.API.MathTools;

namespace AntiXray.Systems;

public static class HiddenOreExplosionBridge
{
    private static HiddenOreExplosionRevealService? revealService;

    public static void Register(HiddenOreExplosionRevealService service)
    {
        revealService = service;
    }

    public static void Unregister(HiddenOreExplosionRevealService service)
    {
        if (ReferenceEquals(revealService, service))
        {
            revealService = null;
        }
    }

    public static ExplosionRevealResult RevealBeforeExplosion(BlockPos pos)
    {
        return revealService?.RevealBeforeExplosion(pos) ?? new ExplosionRevealResult(false, 0);
    }

    public static int RevealNearExplosion(BlockPos pos)
    {
        return revealService?.RevealNearExplosion(pos) ?? 0;
    }
}
