using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace AntiXray.Systems;

public sealed class HiddenOreExplosionBlockBehavior : BlockBehavior
{
    public HiddenOreExplosionBlockBehavior(Block block)
        : base(block)
    {
    }

    public override void OnBlockExploded(
        IWorldAccessor world,
        BlockPos pos,
        BlockPos explosionCenter,
        EnumBlastType blastType,
        ref EnumHandling handling)
    {
        ExplosionRevealResult result = HiddenOreExplosionBridge.RevealBeforeExplosion(pos);
        if (!result.Handled)
        {
            return;
        }

        handling = EnumHandling.PreventDefault;

        Block restoredBlock = world.BlockAccessor.GetBlock(pos);
        if (restoredBlock.Id != block.Id)
        {
            restoredBlock.OnBlockExploded(world, pos, explosionCenter, blastType, string.Empty);
        }
    }

    public override void OnNeighbourBlockChange(
        IWorldAccessor world,
        BlockPos pos,
        BlockPos neibpos,
        ref EnumHandling handling)
    {
        HiddenOreExplosionBridge.RevealNearExplosion(pos);
    }
}
