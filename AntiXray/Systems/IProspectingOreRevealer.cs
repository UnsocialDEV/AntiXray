using Vintagestory.API.MathTools;

namespace AntiXray.Systems;

public interface IProspectingOreRevealer
{
    bool RevealForProspectingPick(BlockPos pos);
}
