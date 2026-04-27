using AntiXray.Models;
using AntiXray.Commands;
using Vintagestory.API.MathTools;

namespace AntiXray.Systems;

public sealed class PlayerProximityRevealService
{
    private readonly bool enabled;
    private readonly HiddenOreProximityQuery query;
    private readonly IExposedOreRevealer revealer;
    private readonly int radius;
    private readonly int maxPerEvent;
    private readonly IOreRevealDebugReporter? debugReporter;
    private readonly List<HiddenOreData> reusableCandidates = new(128);
    private readonly BlockPos scratchPos;
    private readonly int radiusSquared;

    public PlayerProximityRevealService(
        bool enabled,
        HiddenOreProximityQuery query,
        IExposedOreRevealer revealer,
        int radius,
        int maxPerEvent)
        : this(enabled, query, revealer, radius, maxPerEvent, null)
    {
    }

    public PlayerProximityRevealService(
        bool enabled,
        HiddenOreProximityQuery query,
        IExposedOreRevealer revealer,
        int radius,
        int maxPerEvent,
        IOreRevealDebugReporter? debugReporter)
    {
        this.enabled = enabled;
        this.query = query;
        this.revealer = revealer;
        this.radius = PlayerOreRevealRadius.Clamp(radius);
        this.maxPerEvent = maxPerEvent < 1 ? 1 : maxPerEvent;
        this.debugReporter = debugReporter;
        radiusSquared = this.radius * this.radius;
        scratchPos = new BlockPos(0);
    }

    public int RevealNear(BlockPos playerPos)
    {
        if (!enabled)
        {
            return 0;
        }

        reusableCandidates.Clear();
        query.CopyCandidates(playerPos, radius, reusableCandidates);

        int revealed = 0;
        for (int i = 0; i < reusableCandidates.Count; i++)
        {
            HiddenOreData data = reusableCandidates[i];
            scratchPos.Set(data.Position.X, data.Position.Y, data.Position.Z);

            if (DistanceSquared(playerPos, scratchPos) > radiusSquared)
            {
                continue;
            }

            if (!revealer.TryRevealIfExposed(scratchPos))
            {
                continue;
            }

            revealed++;
            debugReporter?.ReportReveal("a player moved close to air-exposed hidden ore", scratchPos, data.OreBlockCode, playerPos);
            if (revealed >= maxPerEvent)
            {
                break;
            }
        }

        return revealed;
    }

    private static int DistanceSquared(BlockPos left, BlockPos right)
    {
        int dx = left.X - right.X;
        int dy = left.InternalY - right.InternalY;
        int dz = left.Z - right.Z;
        return (dx * dx) + (dy * dy) + (dz * dz);
    }
}
