namespace AntiXray.Commands;

public sealed class AdminCommandRadius
{
    private readonly int defaultRadius;
    private readonly int maxRadius;

    public AdminCommandRadius(int defaultRadius, int maxRadius)
    {
        this.maxRadius = maxRadius < 1 ? 1 : maxRadius;
        this.defaultRadius = Clamp(defaultRadius, this.maxRadius);
    }

    public int DefaultRadius => defaultRadius;

    public int MaxRadius => maxRadius;

    public int Resolve(int? requestedRadius)
    {
        if (requestedRadius == null)
        {
            return defaultRadius;
        }

        return Clamp(requestedRadius.Value, maxRadius);
    }

    public static int Clamp(int radius, int maxRadius)
    {
        int effectiveMax = maxRadius < 1 ? 1 : maxRadius;
        if (radius < 1)
        {
            return 1;
        }

        if (radius > effectiveMax)
        {
            return effectiveMax;
        }

        return radius;
    }
}
