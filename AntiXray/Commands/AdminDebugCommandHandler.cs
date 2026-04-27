using AntiXray.Models;
using AntiXray.Systems;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace AntiXray.Commands;

public sealed class AdminDebugCommandHandler
{
    private readonly ICoreServerAPI api;
    private readonly RevealService revealService;
    private readonly AdminOreHideService oreHideService;
    private readonly AdminChatDebugSessions debugSessions;
    private readonly OreRevealDebugReporter debugReporter;
    private readonly AdminRadiusQuery radiusQuery;
    private readonly AdminCommandRadius radius;
    private readonly AdminCommandFormatter formatter;
    private readonly List<HiddenOreData> reusableEntries = new(128);
    private readonly BlockPos scratchPos;

    public AdminDebugCommandHandler(
        ICoreServerAPI api,
        RevealService revealService,
        AdminOreHideService oreHideService,
        AdminChatDebugSessions debugSessions,
        OreRevealDebugReporter debugReporter,
        AdminRadiusQuery radiusQuery,
        AdminCommandRadius radius,
        AdminCommandFormatter formatter)
    {
        this.api = api;
        this.revealService = revealService;
        this.oreHideService = oreHideService;
        this.debugSessions = debugSessions;
        this.debugReporter = debugReporter;
        this.radiusQuery = radiusQuery;
        this.radius = radius;
        this.formatter = formatter;
        scratchPos = new BlockPos(0);
    }

    public TextCommandResult Handle(TextCommandCallingArgs args)
    {
        return TextCommandResult.Error(formatter.Usage(), "usage");
    }

    public TextCommandResult HandleInspect(TextCommandCallingArgs args)
    {
        TextCommandResult? error = TryGetPlayer(args, out IServerPlayer player);
        if (error != null)
        {
            return error;
        }

        return Inspect(player, player.Entity.Pos.AsBlockPos, ResolveRadius(args));
    }

    public TextCommandResult HandleHidden(TextCommandCallingArgs args)
    {
        TextCommandResult? error = TryGetPlayer(args, out IServerPlayer player);
        if (error != null)
        {
            return error;
        }

        return Hidden(player.Entity.Pos.AsBlockPos, ResolveRadius(args));
    }

    public TextCommandResult HandleReveal(TextCommandCallingArgs args)
    {
        TextCommandResult? error = TryGetPlayer(args, out IServerPlayer player);
        if (error != null)
        {
            return error;
        }

        return ForceReveal(player.Entity.Pos.AsBlockPos, ResolveRadius(args));
    }

    public TextCommandResult HandleCount(TextCommandCallingArgs args)
    {
        TextCommandResult? error = TryGetPlayer(args, out IServerPlayer player);
        if (error != null)
        {
            return error;
        }

        return Count(player.Entity.Pos.AsBlockPos, ResolveRadius(args));
    }

    public TextCommandResult HandleHide(TextCommandCallingArgs args)
    {
        TextCommandResult? error = TryGetPlayer(args, out IServerPlayer player);
        if (error != null)
        {
            return error;
        }

        return Hide(player.Entity.Pos.AsBlockPos, ResolveRadius(args));
    }

    public TextCommandResult HandleDebugOn(TextCommandCallingArgs args)
    {
        TextCommandResult? error = TryGetPlayer(args, out IServerPlayer player);
        if (error != null)
        {
            return error;
        }

        debugSessions.Enable(player);
        return TextCommandResult.Success(formatter.DebugEnabled(), null);
    }

    public TextCommandResult HandleDebugOff(TextCommandCallingArgs args)
    {
        TextCommandResult? error = TryGetPlayer(args, out IServerPlayer player);
        if (error != null)
        {
            return error;
        }

        debugSessions.Disable(player);
        return TextCommandResult.Success(formatter.DebugDisabled(), null);
    }

    public TextCommandResult HandleDebugStatus(TextCommandCallingArgs args)
    {
        TextCommandResult? error = TryGetPlayer(args, out IServerPlayer player);
        if (error != null)
        {
            return error;
        }

        return TextCommandResult.Success(formatter.DebugStatus(debugSessions.IsEnabled(player)), null);
    }

    private static TextCommandResult? TryGetPlayer(
        TextCommandCallingArgs args,
        out IServerPlayer player)
    {
        if (args.Caller.Player is IServerPlayer serverPlayer)
        {
            player = serverPlayer;
            return null;
        }

        player = null!;
        return TextCommandResult.Error("This command can only be used by a server player.", "player-required");
    }

    private int ResolveRadius(TextCommandCallingArgs args)
    {
        if (args.ArgCount > 0 && args[0] is int parsedRadius)
        {
            return radius.Resolve(parsedRadius);
        }

        return radius.Resolve(null);
    }

    private TextCommandResult Inspect(IServerPlayer player, BlockPos center, int radius)
    {
        CopyEntries(center, radius);
        string targetBlock = GetTargetBlockDescription(player);
        return TextCommandResult.Success(formatter.Inspect(center, radius, reusableEntries.Count, targetBlock), null);
    }

    private TextCommandResult Hidden(BlockPos center, int radius)
    {
        CopyEntries(center, radius);
        return TextCommandResult.Success(formatter.Hidden(center, radius, reusableEntries), null);
    }

    private TextCommandResult ForceReveal(BlockPos center, int radius)
    {
        CopyEntries(center, radius);
        int found = reusableEntries.Count;
        int revealed = 0;

        for (int i = 0; i < reusableEntries.Count; i++)
        {
            HiddenOreData data = reusableEntries[i];
            scratchPos.Set(data.Position.X, data.Position.Y, data.Position.Z);
            if (revealService.ForceReveal(scratchPos))
            {
                revealed++;
                debugReporter.ReportReveal("an admin used /antixray reveal", scratchPos, data.OreBlockCode, center);
            }
        }

        return TextCommandResult.Success(formatter.Reveal(center, radius, found, revealed), null);
    }

    private TextCommandResult Count(BlockPos center, int radius)
    {
        CopyEntries(center, radius);
        return TextCommandResult.Success(formatter.Count(center, radius, reusableEntries.Count), null);
    }

    private TextCommandResult Hide(BlockPos center, int radius)
    {
        AdminOreHideResult result = oreHideService.HideRealOreWithin(center, radius);
        return TextCommandResult.Success(formatter.Hide(center, radius, result), null);
    }

    private void CopyEntries(BlockPos center, int radius)
    {
        reusableEntries.Clear();
        radiusQuery.CopyHiddenOreWithin(center, radius, reusableEntries);
    }

    private string GetTargetBlockDescription(IServerPlayer player)
    {
        BlockSelection? blockSelection = player.CurrentBlockSelection;
        if (blockSelection?.Position == null)
        {
            return "none";
        }

        Block block = api.World.BlockAccessor.GetBlock(blockSelection.Position);
        string blockCode = block.Code?.ToString() ?? "unknown";
        return formatter.TargetBlock(blockSelection.Position, block.Id, blockCode);
    }
}
