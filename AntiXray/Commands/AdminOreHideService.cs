using AntiXray.Systems;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace AntiXray.Commands;

public sealed class AdminOreHideService
{
    private readonly HiddenOreManager hiddenOreManager;
    private readonly OreBlockClassifier oreBlockClassifier;
    private readonly IAdminOreHideWorld world;
    private readonly BlockPos scratchPos;

    public AdminOreHideService(
        HiddenOreManager hiddenOreManager,
        OreBlockClassifier oreBlockClassifier,
        IAdminOreHideWorld world)
    {
        this.hiddenOreManager = hiddenOreManager;
        this.oreBlockClassifier = oreBlockClassifier;
        this.world = world;
        scratchPos = new BlockPos(0);
    }

    public AdminOreHideResult HideRealOreWithin(BlockPos center, int radius)
    {
        int radiusSquared = radius * radius;
        int scanned = 0;
        int oreFound = 0;
        int hidden = 0;
        int alreadyHidden = 0;
        int placeholderMissing = 0;

        for (int dx = -radius; dx <= radius; dx++)
        {
            for (int dy = -radius; dy <= radius; dy++)
            {
                for (int dz = -radius; dz <= radius; dz++)
                {
                    if (DistanceSquared(dx, dy, dz) > radiusSquared)
                    {
                        continue;
                    }

                    scanned++;
                    scratchPos.Set(center.X + dx, center.InternalY + dy, center.Z + dz);
                    if (hiddenOreManager.TryGet(scratchPos, out _))
                    {
                        alreadyHidden++;
                        continue;
                    }

                    Block? block = world.GetBlock(scratchPos);
                    if (block == null || !oreBlockClassifier.IsOre(block))
                    {
                        continue;
                    }

                    oreFound++;
                    int placeholderBlockId = world.ResolvePlaceholderBlockId(block);
                    if (placeholderBlockId == 0)
                    {
                        placeholderMissing++;
                        continue;
                    }

                    hiddenOreManager.Store(scratchPos, block.Code.ToString());
                    world.SetBlock(placeholderBlockId, scratchPos);
                    world.SaveHiddenOreColumn(scratchPos);
                    hidden++;
                }
            }
        }

        return new AdminOreHideResult(scanned, oreFound, hidden, alreadyHidden, placeholderMissing);
    }

    private static int DistanceSquared(int dx, int dy, int dz)
    {
        return (dx * dx) + (dy * dy) + (dz * dz);
    }
}
