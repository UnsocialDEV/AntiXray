using AntiXray.Models;
using AntiXray.Systems;
using Vintagestory.API.MathTools;

namespace AntiXray.Commands;

public sealed class AdminRadiusQuery
{
    private readonly HiddenOreProximityQuery proximityQuery;
    private readonly List<HiddenOreData> reusableCandidates = new(128);
    private readonly BlockPos scratchPos;

    public AdminRadiusQuery(HiddenOreProximityQuery proximityQuery)
    {
        this.proximityQuery = proximityQuery;
        scratchPos = new BlockPos(0);
    }

    public void CopyHiddenOreWithin(BlockPos center, int radius, List<HiddenOreData> output)
    {
        reusableCandidates.Clear();
        proximityQuery.CopyCandidates(center, radius, reusableCandidates);

        int radiusSquared = radius * radius;
        for (int i = 0; i < reusableCandidates.Count; i++)
        {
            HiddenOreData data = reusableCandidates[i];
            scratchPos.Set(data.Position.X, data.Position.Y, data.Position.Z);
            if (DistanceSquared(center, scratchPos) <= radiusSquared)
            {
                output.Add(data);
            }
        }
    }

    private static int DistanceSquared(BlockPos left, BlockPos right)
    {
        int dx = left.X - right.X;
        int dy = left.InternalY - right.InternalY;
        int dz = left.Z - right.Z;
        return (dx * dx) + (dy * dy) + (dz * dz);
    }
}
