namespace AntiXray.Systems;

public sealed class PlayerOreRevealRadius
{
    private const int MinRadius = 8;
    private const int MaxRadius = 32;

    public PlayerOreRevealRadius(int configuredRadius)
    {
        Value = Clamp(configuredRadius);
    }

    public int Value { get; }

    public static int Clamp(int radius)
    {
        if (radius < MinRadius)
        {
            return MinRadius;
        }

        if (radius > MaxRadius)
        {
            return MaxRadius;
        }

        return radius;
    }
}
