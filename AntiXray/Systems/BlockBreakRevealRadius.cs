namespace AntiXray.Systems;

public sealed class BlockBreakRevealRadius
{
    private const int MinRadius = 1;
    private const int MaxRadius = 3;

    public BlockBreakRevealRadius(int configuredRadius)
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
