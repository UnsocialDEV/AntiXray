using Vintagestory.API.Common;

namespace AntiXray.Systems;

public sealed class OrePlaceholderResolver
{
    private readonly IWorldAccessor world;
    private readonly string? fallbackBlockCode;
    private readonly DebugLogger debugLogger;
    private readonly Dictionary<string, int> blockIdCache = new(StringComparer.Ordinal);

    public OrePlaceholderResolver(IWorldAccessor world)
        : this(world, string.Empty, new DebugLogger(world.Logger, false))
    {
    }

    public OrePlaceholderResolver(IWorldAccessor world, string fallbackBlockCode, DebugLogger debugLogger)
    {
        this.world = world;
        this.fallbackBlockCode = string.IsNullOrWhiteSpace(fallbackBlockCode) ? null : fallbackBlockCode;
        this.debugLogger = debugLogger;
    }

    public int ResolveBlockId(Block oreBlock)
    {
        string? path = oreBlock.Code?.Path;
        if (path == null)
        {
            debugLogger.WarningOnce("Skipped ore block without a block code path.");
            return 0;
        }

        string? rockCode = OreHostRockCodeResolver.Resolve(path);
        if (rockCode != null && TryResolveBlockId(rockCode, out int hostRockBlockId))
        {
            return hostRockBlockId;
        }

        if (fallbackBlockCode != null && TryResolveBlockId(fallbackBlockCode, out int fallbackBlockId))
        {
            return fallbackBlockId;
        }

        debugLogger.WarningOnce($"Skipped ore block {oreBlock.Code} because no placeholder block could be resolved.");
        return 0;
    }

    private bool TryResolveBlockId(string blockCode, out int blockId)
    {
        if (blockIdCache.TryGetValue(blockCode, out blockId))
        {
            return blockId != 0;
        }

        Block? block = world.GetBlock(new AssetLocation(blockCode));
        blockId = block?.Id ?? 0;
        blockIdCache.Add(blockCode, blockId);
        return blockId != 0;
    }
}
