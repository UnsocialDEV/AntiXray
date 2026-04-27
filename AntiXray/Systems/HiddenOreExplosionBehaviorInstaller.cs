using Vintagestory.API.Common;

namespace AntiXray.Systems;

public sealed class HiddenOreExplosionBehaviorInstaller
{
    public int Install(IWorldAccessor world)
    {
        int installed = 0;
        IList<Block> blocks = world.Blocks;

        for (int i = 0; i < blocks.Count; i++)
        {
            Block block = blocks[i];
            if (!ShouldInstall(block))
            {
                continue;
            }

            if (HasBehavior(block))
            {
                continue;
            }

            AddBehavior(block);
            installed++;
        }

        return installed;
    }

    public bool ShouldInstall(Block block)
    {
        string? domain = block.Code?.Domain;
        string? path = block.Code?.Path;
        return string.Equals(domain, "game", StringComparison.Ordinal)
            && path != null
            && path.StartsWith("rock-", StringComparison.Ordinal);
    }

    private static bool HasBehavior(Block block)
    {
        BlockBehavior[]? behaviors = block.BlockBehaviors;
        if (behaviors == null)
        {
            return false;
        }

        for (int i = 0; i < behaviors.Length; i++)
        {
            if (behaviors[i] is HiddenOreExplosionBlockBehavior)
            {
                return true;
            }
        }

        return false;
    }

    private static void AddBehavior(Block block)
    {
        BlockBehavior[]? existingBehaviors = block.BlockBehaviors;
        if (existingBehaviors == null || existingBehaviors.Length == 0)
        {
            block.BlockBehaviors = [new HiddenOreExplosionBlockBehavior(block)];
            return;
        }

        var updatedBehaviors = new BlockBehavior[existingBehaviors.Length + 1];
        Array.Copy(existingBehaviors, updatedBehaviors, existingBehaviors.Length);
        updatedBehaviors[^1] = new HiddenOreExplosionBlockBehavior(block);
        block.BlockBehaviors = updatedBehaviors;
    }
}
