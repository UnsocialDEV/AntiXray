namespace AntiXray.Systems;

public static class OreHostRockCodeResolver
{
    public static string? Resolve(string oreCodePath)
    {
        int separatorIndex = oreCodePath.LastIndexOf('-');
        if (separatorIndex < 0 || separatorIndex == oreCodePath.Length - 1)
        {
            return null;
        }

        return "game:rock-" + oreCodePath[(separatorIndex + 1)..];
    }
}
