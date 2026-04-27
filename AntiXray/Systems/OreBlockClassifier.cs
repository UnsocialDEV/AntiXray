using Vintagestory.API.Common;

namespace AntiXray.Systems;

public sealed class OreBlockClassifier
{
    private static readonly string[] DefaultPatterns = ["ore-*"];
    private readonly string[] oreCodePatterns;

    public OreBlockClassifier()
        : this(DefaultPatterns)
    {
    }

    public OreBlockClassifier(string[] oreCodePatterns)
    {
        if (oreCodePatterns.Length == 0)
        {
            this.oreCodePatterns = DefaultPatterns;
            return;
        }

        this.oreCodePatterns = (string[])oreCodePatterns.Clone();
    }

    public bool IsOre(Block block)
    {
        string? path = block.Code?.Path;
        if (path == null)
        {
            return false;
        }

        for (int i = 0; i < oreCodePatterns.Length; i++)
        {
            if (OreCodePattern.Matches(path, oreCodePatterns[i]))
            {
                return true;
            }
        }

        return false;
    }
}
