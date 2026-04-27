using AntiXray.Systems;
using Vintagestory.API.Config;
using Vintagestory.API.Server;

namespace AntiXray.Commands;

public sealed class AdminDebugCommandRegistrar
{
    private readonly ICoreServerAPI api;
    private readonly HiddenOreManager hiddenOreManager;
    private readonly RevealService revealService;
    private readonly AdminOreHideService oreHideService;
    private readonly AdminChatDebugSessions debugSessions;
    private readonly OreRevealDebugReporter debugReporter;
    private readonly int defaultRadius;
    private readonly int maxRadius;

    public AdminDebugCommandRegistrar(
        ICoreServerAPI api,
        HiddenOreManager hiddenOreManager,
        RevealService revealService,
        AdminOreHideService oreHideService,
        AdminChatDebugSessions debugSessions,
        OreRevealDebugReporter debugReporter,
        int defaultRadius,
        int maxRadius)
    {
        this.api = api;
        this.hiddenOreManager = hiddenOreManager;
        this.revealService = revealService;
        this.oreHideService = oreHideService;
        this.debugSessions = debugSessions;
        this.debugReporter = debugReporter;
        this.defaultRadius = defaultRadius;
        this.maxRadius = maxRadius;
    }

    public void Register()
    {
        var handler = new AdminDebugCommandHandler(
            api,
            revealService,
            oreHideService,
            debugSessions,
            debugReporter,
            new AdminRadiusQuery(new HiddenOreProximityQuery(hiddenOreManager, GlobalConstants.ChunkSize)),
            new AdminCommandRadius(defaultRadius, maxRadius),
            new AdminCommandFormatter(new WorldCoordinateDisplayFormatter(api)));

        var command = api.ChatCommands
            .Create("antixray")
            .WithDescription("AntiXray admin diagnostics")
            .RequiresPrivilege(Privilege.root)
            .RequiresPlayer()
            .HandleWith(handler.Handle);

        command.BeginSubCommand("inspect")
            .WithDescription("Inspect hidden ore metadata around you and the targeted block.")
            .WithArgs(api.ChatCommands.Parsers.OptionalInt("radius", defaultRadius))
            .HandleWith(handler.HandleInspect)
            .EndSubCommand();

        command.BeginSubCommand("hidden")
            .WithDescription("List hidden ore metadata around you.")
            .WithArgs(api.ChatCommands.Parsers.OptionalInt("radius", defaultRadius))
            .HandleWith(handler.HandleHidden)
            .EndSubCommand();

        command.BeginSubCommand("reveal")
            .WithDescription("Force-reveal hidden ore around you.")
            .WithArgs(api.ChatCommands.Parsers.OptionalInt("radius", defaultRadius))
            .HandleWith(handler.HandleReveal)
            .EndSubCommand();

        command.BeginSubCommand("hide")
            .WithDescription("Hide real ore around you for debug testing.")
            .WithArgs(api.ChatCommands.Parsers.OptionalInt("radius", defaultRadius))
            .HandleWith(handler.HandleHide)
            .EndSubCommand();

        command.BeginSubCommand("count")
            .WithDescription("Count hidden ore metadata around you.")
            .WithArgs(api.ChatCommands.Parsers.OptionalInt("radius", defaultRadius))
            .HandleWith(handler.HandleCount)
            .EndSubCommand();

        command.BeginSubCommand("debug")
            .WithDescription("Toggle AntiXray reveal debug chat for your admin session.")
            .BeginSubCommand("on")
                .WithDescription("Enable reveal debug chat for this admin session.")
                .HandleWith(handler.HandleDebugOn)
                .EndSubCommand()
            .BeginSubCommand("off")
                .WithDescription("Disable reveal debug chat for this admin session.")
                .HandleWith(handler.HandleDebugOff)
                .EndSubCommand()
            .BeginSubCommand("status")
                .WithDescription("Show reveal debug chat status for this admin session.")
                .HandleWith(handler.HandleDebugStatus)
                .EndSubCommand()
            .EndSubCommand();
    }
}
