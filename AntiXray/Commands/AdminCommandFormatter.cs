using AntiXray.Models;
using Vintagestory.API.MathTools;

namespace AntiXray.Commands;

public sealed class AdminCommandFormatter
{
    private const int HiddenListLimit = 12;
    private readonly WorldCoordinateDisplayFormatter coordinateFormatter;

    public AdminCommandFormatter()
        : this(new WorldCoordinateDisplayFormatter((Vintagestory.API.Server.IWorldManagerAPI?)null))
    {
    }

    public AdminCommandFormatter(WorldCoordinateDisplayFormatter coordinateFormatter)
    {
        this.coordinateFormatter = coordinateFormatter;
    }

    public string Usage()
    {
        return "Usage: /antixray inspect|hidden|reveal|hide|count [radius] or /antixray debug on|off|status";
    }

    public string Count(BlockPos center, int radius, int count)
    {
        return $"Hidden ore within radius {radius} of {Format(center)}: {count}";
    }

    public string Reveal(BlockPos center, int radius, int found, int revealed)
    {
        return $"Revealed {revealed}/{found} hidden ore blocks within radius {radius} of {Format(center)}.";
    }

    public string Hide(BlockPos center, int radius, AdminOreHideResult result)
    {
        return $"Hid {result.Hidden}/{result.OreFound} real ore blocks within radius {radius} of {Format(center)}. scanned={result.Scanned}; alreadyHidden={result.AlreadyHidden}; missingPlaceholder={result.PlaceholderMissing}.";
    }

    public string DebugEnabled()
    {
        return "AntiXray debug chat enabled for this admin session.";
    }

    public string DebugDisabled()
    {
        return "AntiXray debug chat disabled for this admin session.";
    }

    public string DebugStatus(bool enabled)
    {
        return enabled
            ? "AntiXray debug chat is enabled for this admin session."
            : "AntiXray debug chat is disabled for this admin session.";
    }

    public string Inspect(BlockPos center, int radius, int count, string targetBlock)
    {
        return $"Inspect radius {radius} around {Format(center)}: hidden={count}; target={targetBlock}";
    }

    public string Hidden(BlockPos center, int radius, List<HiddenOreData> entries)
    {
        if (entries.Count == 0)
        {
            return $"No hidden ore found within radius {radius} of {Format(center)}.";
        }

        var lines = new List<string>(HiddenListLimit + 2)
        {
            $"Hidden ore within radius {radius} of {Format(center)}: {entries.Count}"
        };

        int limit = entries.Count < HiddenListLimit ? entries.Count : HiddenListLimit;
        for (int i = 0; i < limit; i++)
        {
            HiddenOreData data = entries[i];
            lines.Add($"{Format(data.Position)} {data.OreBlockCode}");
        }

        if (entries.Count > HiddenListLimit)
        {
            lines.Add($"... truncated {entries.Count - HiddenListLimit} more");
        }

        return string.Join("\n", lines);
    }

    public string TargetBlock(BlockPos pos, int blockId, string blockCode)
    {
        return $"{Format(pos)} id={blockId} code={blockCode}";
    }

    private string Format(BlockPos pos)
    {
        return coordinateFormatter.Format(pos);
    }

    private string Format(BlockPosKey pos)
    {
        return coordinateFormatter.Format(pos.X, pos.Y, pos.Z);
    }
}
