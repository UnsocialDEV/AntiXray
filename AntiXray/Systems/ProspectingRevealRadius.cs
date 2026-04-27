using Vintagestory.API.Common;

namespace AntiXray.Systems;

public sealed class ProspectingRevealRadius
{
    private const int MinRadius = 0;
    private const int MaxRadius = 8;

    private readonly int configuredRadius;

    public ProspectingRevealRadius(int configuredRadius)
    {
        this.configuredRadius = Clamp(configuredRadius);
    }

    public int Resolve(IWorldAccessor world)
    {
        int? worldRadius = world.Config.TryGetInt("propickNodeSearchRadius");
        if (worldRadius == null)
        {
            return configuredRadius;
        }

        int clampedWorldRadius = Clamp(worldRadius.Value);
        return configuredRadius < clampedWorldRadius ? configuredRadius : clampedWorldRadius;
    }

    public static int Clamp(int radius)
    {
        if (radius < MinRadius)
        {
            return MinRadius;
        }

        if (radius > MaxRadius)
        {
            return MaxRadius;
        }

        return radius;
    }
}
