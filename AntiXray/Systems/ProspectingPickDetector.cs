using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace AntiXray.Systems;

public sealed class ProspectingPickDetector
{
    private readonly string[] codePatterns;

    public ProspectingPickDetector(string[]? codePatterns = null)
    {
        this.codePatterns = codePatterns is { Length: > 0 } ? codePatterns : ["prospectingpick-*"];
    }

    public bool IsHoldingProspectingPick(IServerPlayer player)
    {
        if (player.Entity?.RightHandItemSlot?.Itemstack is ItemStack rightHandStack && IsProspectingPick(rightHandStack))
        {
            return true;
        }

        if (player.Entity?.LeftHandItemSlot?.Itemstack is ItemStack leftHandStack && IsProspectingPick(leftHandStack))
        {
            return true;
        }

        return false;
    }

    public bool IsProspectingPick(ItemStack? itemStack)
    {
        AssetLocation? code = itemStack?.Collectible?.Code;
        if (code == null)
        {
            return false;
        }

        string path = code.Path;
        string fullCode = code.ToString();

        for (int i = 0; i < codePatterns.Length; i++)
        {
            string pattern = codePatterns[i];
            if (OreCodePattern.Matches(path, pattern) || OreCodePattern.Matches(fullCode, pattern))
            {
                return true;
            }
        }

        return false;
    }
}
