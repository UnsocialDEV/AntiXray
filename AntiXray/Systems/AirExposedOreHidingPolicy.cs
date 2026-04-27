namespace AntiXray.Systems;

public sealed class AirExposedOreHidingPolicy
{
    private readonly int hideAirExposedOreBelowY;

    public AirExposedOreHidingPolicy(int hideAirExposedOreBelowY)
    {
        this.hideAirExposedOreBelowY = hideAirExposedOreBelowY;
    }

    public bool ShouldHide(bool isAirAdjacent, int oreY)
    {
        return !isAirAdjacent || oreY < hideAirExposedOreBelowY;
    }
}
