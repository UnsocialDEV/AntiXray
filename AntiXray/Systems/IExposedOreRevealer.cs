using Vintagestory.API.MathTools;

namespace AntiXray.Systems;

public interface IExposedOreRevealer
{
    bool TryRevealIfExposed(BlockPos pos);
}
