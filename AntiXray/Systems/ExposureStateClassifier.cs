namespace AntiXray.Systems;

public static class ExposureStateClassifier
{
    public static bool IsExposure(int blockId, bool forFluidsLayer, string? liquidCode)
    {
        return blockId == 0 || forFluidsLayer || liquidCode != null;
    }
}
