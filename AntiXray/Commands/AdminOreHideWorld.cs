using AntiXray.Systems;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace AntiXray.Commands;

public sealed class AdminOreHideWorld : IAdminOreHideWorld
{
    private readonly IWorldAccessor world;
    private readonly OrePlaceholderResolver placeholderResolver;
    private readonly HiddenOrePersistence hiddenOrePersistence;

    public AdminOreHideWorld(
        IWorldAccessor world,
        OrePlaceholderResolver placeholderResolver,
        HiddenOrePersistence hiddenOrePersistence)
    {
        this.world = world;
        this.placeholderResolver = placeholderResolver;
        this.hiddenOrePersistence = hiddenOrePersistence;
    }

    public Block? GetBlock(BlockPos pos)
    {
        return world.BlockAccessor.GetBlock(pos);
    }

    public int ResolvePlaceholderBlockId(Block oreBlock)
    {
        return placeholderResolver.ResolveBlockId(oreBlock);
    }

    public void SetBlock(int blockId, BlockPos pos)
    {
        world.BlockAccessor.SetBlock(blockId, pos);
    }

    public void SaveHiddenOreColumn(BlockPos pos)
    {
        hiddenOrePersistence.SaveColumnAt(world, pos);
    }
}
