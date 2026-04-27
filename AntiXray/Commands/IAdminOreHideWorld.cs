using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace AntiXray.Commands;

public interface IAdminOreHideWorld
{
    Block? GetBlock(BlockPos pos);

    int ResolvePlaceholderBlockId(Block oreBlock);

    void SetBlock(int blockId, BlockPos pos);

    void SaveHiddenOreColumn(BlockPos pos);
}
