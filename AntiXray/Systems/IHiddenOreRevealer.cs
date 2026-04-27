using Vintagestory.API.MathTools;

namespace AntiXray.Systems;

public interface IHiddenOreRevealer
{
    bool ForceReveal(BlockPos pos);
}
