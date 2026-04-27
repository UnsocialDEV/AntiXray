using AntiXray.Models;
using AntiXray.Commands;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace AntiXray.Systems;

public sealed class RevealService : IProspectingOreRevealer, IHiddenOreRevealer, IExposedOreRevealer
{
    private static readonly int[] Offsets =
    [
        1, 0, 0,
        -1, 0, 0,
        0, 1, 0,
        0, -1, 0,
        0, 0, 1,
        0, 0, -1
    ];

    private readonly IWorldAccessor world;
    private readonly HiddenOreManager hiddenOreManager;
    private readonly HiddenOrePersistence hiddenOrePersistence;
    private readonly IOreRevealDebugReporter? debugReporter;
    private readonly ExposureBlockClassifier exposureBlockClassifier;
    private readonly BlockPos scratchPos;

    public RevealService(IWorldAccessor world, HiddenOreManager hiddenOreManager, HiddenOrePersistence hiddenOrePersistence)
        : this(world, hiddenOreManager, hiddenOrePersistence, null)
    {
    }

    public RevealService(
        IWorldAccessor world,
        HiddenOreManager hiddenOreManager,
        HiddenOrePersistence hiddenOrePersistence,
        IOreRevealDebugReporter? debugReporter)
    {
        this.world = world;
        this.hiddenOreManager = hiddenOreManager;
        this.hiddenOrePersistence = hiddenOrePersistence;
        this.debugReporter = debugReporter;
        exposureBlockClassifier = new ExposureBlockClassifier();
        scratchPos = new BlockPos(0);
    }

    public void RevealAdjacentTo(BlockPos changedPos)
    {
        for (int i = 0; i < Offsets.Length; i += 3)
        {
            scratchPos.Set(changedPos.X + Offsets[i], changedPos.InternalY + Offsets[i + 1], changedPos.Z + Offsets[i + 2]);
            if (!hiddenOreManager.TryGet(scratchPos, out HiddenOreData data))
            {
                continue;
            }

            if (TryRevealIfExposed(scratchPos))
            {
                debugReporter?.ReportReveal("it became exposed after a block changed", scratchPos, data.OreBlockCode, changedPos);
            }
        }
    }

    public bool TryRevealIfExposed(BlockPos pos)
    {
        if (!hiddenOreManager.TryGet(pos, out HiddenOreData data))
        {
            return false;
        }

        if (!HasAirAdjacent(pos))
        {
            return false;
        }

        return RestoreHiddenOre(pos, data);
    }

    public bool ForceReveal(BlockPos pos)
    {
        if (!hiddenOreManager.TryGet(pos, out HiddenOreData data))
        {
            return false;
        }

        return RestoreHiddenOre(pos, data);
    }

    public bool RevealForProspectingPick(BlockPos pos)
    {
        return ForceReveal(pos);
    }

    private bool HasAirAdjacent(BlockPos pos)
    {
        for (int i = 0; i < Offsets.Length; i += 3)
        {
            scratchPos.Set(pos.X + Offsets[i], pos.InternalY + Offsets[i + 1], pos.Z + Offsets[i + 2]);

            Block block = world.BlockAccessor.GetBlock(scratchPos);
            if (exposureBlockClassifier.IsExposure(block))
            {
                return true;
            }
        }

        return false;
    }

    private bool RestoreHiddenOre(BlockPos pos, HiddenOreData data)
    {
        Block? oreBlock = world.GetBlock(new AssetLocation(data.OreBlockCode));
        if (oreBlock == null)
        {
            return false;
        }

        world.BlockAccessor.SetBlock(oreBlock.Id, pos);
        hiddenOreManager.Remove(pos);
        hiddenOrePersistence.SaveColumnAt(world, pos);
        return true;
    }
}
