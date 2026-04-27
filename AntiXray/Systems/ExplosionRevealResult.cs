namespace AntiXray.Systems;

public readonly struct ExplosionRevealResult
{
    public ExplosionRevealResult(bool handled, int revealed)
    {
        Handled = handled;
        Revealed = revealed;
    }

    public bool Handled { get; }

    public int Revealed { get; }
}
