namespace AntiXray.Models;

public sealed class HiddenOreData
{
    public HiddenOreData(BlockPosKey position, string oreBlockCode)
    {
        Position = position;
        OreBlockCode = oreBlockCode;
    }

    public BlockPosKey Position { get; }

    public string OreBlockCode { get; }
}
