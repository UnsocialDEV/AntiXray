using Vintagestory.API.Common;

namespace AntiXray.Systems;

public sealed class ExposureBlockClassifier
{
    public bool IsExposure(Block block)
    {
        return ExposureStateClassifier.IsExposure(block.Id, block.ForFluidsLayer, block.LiquidCode);
    }
}
