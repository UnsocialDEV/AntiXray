using AntiXray.Commands;
using AntiXray.Config;
using AntiXray.Systems;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace AntiXray.ModSystem;

public sealed class AntiXraySystem : Vintagestory.API.Common.ModSystem
{
    private ICoreServerAPI? serverApi;
    private HiddenOreManager? hiddenOreManager;
    private RevealService? revealService;
    private ProspectingPickRevealController? prospectingPickRevealController;
    private BlockBreakRevealService? blockBreakRevealService;
    private HiddenOreExplosionRevealService? hiddenOreExplosionRevealService;
    private PlayerProximityRevealService? playerProximityRevealService;
    private ChunkOreConverter? chunkOreConverter;
    private HiddenOrePersistence? hiddenOrePersistence;
    private AntiXrayConfig? config;

    public override bool ShouldLoad(EnumAppSide forSide)
    {
        return forSide == EnumAppSide.Server;
    }

    public override void StartServerSide(ICoreServerAPI api)
    {
        serverApi = api;
        int chunkSize = GlobalConstants.ChunkSize;
        var chunkIndex = new ChunkIndex(chunkSize);

        hiddenOreManager = new HiddenOreManager(chunkIndex);
        hiddenOrePersistence = new HiddenOrePersistence(hiddenOreManager);
        config = AntiXrayConfigLoader.Load(api);
        var debugLogger = new DebugLogger(api.Logger, config.DebugLoggingEnabled);
        var oreBlockClassifier = new OreBlockClassifier(config.OreCodePatterns);
        var orePlaceholderResolver = new OrePlaceholderResolver(api.World, config.PlaceholderFallbackBlockCode, debugLogger);
        var debugSessions = new AdminChatDebugSessions();
        var debugReporter = new OreRevealDebugReporter(
            api,
            debugSessions,
            new OreRevealDebugFormatter(new WorldCoordinateDisplayFormatter(api)));
        revealService = new RevealService(api.World, hiddenOreManager, hiddenOrePersistence, debugReporter);
        blockBreakRevealService = new BlockBreakRevealService(
            hiddenOreManager,
            revealService,
            new HiddenOreVeinCollector(hiddenOreManager),
            new BlockBreakRevealRadius(config.BlockBreakRevealDistance).Value,
            config.BlockBreakRevealConnectedVein,
            config.BlockBreakRevealMaxOreBlocks,
            debugReporter);
        hiddenOreExplosionRevealService = new HiddenOreExplosionRevealService(
            hiddenOreManager,
            revealService,
            new HiddenOreVeinCollector(hiddenOreManager),
            new BlockBreakRevealRadius(config.BlockBreakRevealDistance).Value,
            config.BlockBreakRevealConnectedVein,
            config.BlockBreakRevealMaxOreBlocks,
            debugReporter);
        HiddenOreExplosionBridge.Register(hiddenOreExplosionRevealService);
        new HiddenOreExplosionBehaviorInstaller().Install(api.World);
        playerProximityRevealService = new PlayerProximityRevealService(
            config.RevealAirExposedOreNearPlayers,
            new HiddenOreProximityQuery(hiddenOreManager, chunkSize),
            revealService,
            new PlayerOreRevealRadius(config.PlayerAirExposedOreRevealDistance).Value,
            config.PlayerAirExposedOreRevealMaxPerEvent,
            debugReporter);
        var prospectingPickDetector = new ProspectingPickDetector(config.ProspectingPickCodePatterns);
        int prospectingRevealRadius = new ProspectingRevealRadius(config.ProspectingPickRevealRadius).Resolve(api.World);
        var prospectingRevealService = new ProspectingRevealService(revealService, prospectingRevealRadius, hiddenOreManager, debugReporter);
        prospectingPickRevealController = new ProspectingPickRevealController(config.RevealOreOnProspectingPick, prospectingPickDetector, prospectingRevealService);
        chunkOreConverter = new ChunkOreConverter(
            api.World,
            hiddenOreManager,
            oreBlockClassifier,
            orePlaceholderResolver,
            hiddenOrePersistence,
            new AirExposedOreHidingPolicy(config.HideAirExposedOreBelowY));

        api.Event.ChunkColumnGeneration(OnChunkColumnGeneration, EnumWorldGenPass.PreDone, "standard");
        api.Event.ChunkColumnLoaded += OnChunkColumnLoaded;
        api.Event.HandInteract += OnHandInteract;
        api.Event.BreakBlock += OnBreakBlock;
        api.Event.DidBreakBlock += OnDidBreakBlock;
        api.Event.DidPlaceBlock += OnDidPlaceBlock;
        api.Event.DidUseBlock += OnDidUseBlock;
        api.Event.PlayerNowPlaying += OnPlayerNowPlaying;

        new AdminDebugCommandRegistrar(
            api,
            hiddenOreManager,
            revealService,
            new AdminOreHideService(
                hiddenOreManager,
                oreBlockClassifier,
                new AdminOreHideWorld(api.World, orePlaceholderResolver, hiddenOrePersistence)),
            debugSessions,
            debugReporter,
            config.AdminCommandDefaultRadius,
            config.AdminCommandMaxRadius).Register();
    }

    public override void Dispose()
    {
        if (hiddenOreExplosionRevealService != null)
        {
            HiddenOreExplosionBridge.Unregister(hiddenOreExplosionRevealService);
        }

        base.Dispose();
    }

    private void OnChunkColumnGeneration(IChunkColumnGenerateRequest request)
    {
        chunkOreConverter?.ConvertGeneratedColumn(request);
    }

    private void OnChunkColumnLoaded(Vec2i chunkCoord, IWorldChunk[] chunks)
    {
        bool hasStoredData = hiddenOrePersistence?.LoadColumn(chunks) == true;
        if (hasStoredData)
        {
            RevealNearOnlinePlayers();
            return;
        }

        if (config?.ConvertExistingChunks != true)
        {
            RevealNearOnlinePlayers();
            return;
        }

        chunkOreConverter?.ConvertLoadedColumn(chunks, chunkCoord.X, chunkCoord.Y);
        RevealNearOnlinePlayers();
    }

    private void OnBreakBlock(IServerPlayer byPlayer, BlockSelection blockSel, ref float dropQuantityMultiplier, ref EnumHandling handling)
    {
        if (blockSel?.Position == null)
        {
            return;
        }

        blockBreakRevealService?.RevealNear(blockSel.Position);
    }

    private void OnDidBreakBlock(IServerPlayer byPlayer, int oldBlockId, BlockSelection blockSel)
    {
        if (blockSel?.Position == null)
        {
            return;
        }

        revealService?.RevealAdjacentTo(blockSel.Position);
    }

    private void OnDidPlaceBlock(IServerPlayer byPlayer, int oldBlockId, BlockSelection blockSel, ItemStack withItemStack)
    {
        if (blockSel?.Position == null)
        {
            return;
        }

        revealService?.RevealAdjacentTo(blockSel.Position);
    }

    private void OnDidUseBlock(IServerPlayer byPlayer, BlockSelection blockSel)
    {
        if (blockSel?.Position == null)
        {
            return;
        }

        TryRevealAfterProspectingPickUse(byPlayer, blockSel.Position);
    }

    private void OnHandInteract(IServerPlayer player, EnumHandInteractNw enumHandInteract, float secondsPassed, ref EnumHandling handling)
    {
        if (enumHandInteract != EnumHandInteractNw.StopBlockUse)
        {
            return;
        }

        if (player.CurrentBlockSelection?.Position == null)
        {
            return;
        }

        TryRevealAfterProspectingPickUse(player, player.CurrentBlockSelection.Position);
    }

    private void TryRevealAfterProspectingPickUse(IServerPlayer player, BlockPos pos)
    {
        prospectingPickRevealController?.TryReveal(player, pos);
    }

    private void OnPlayerNowPlaying(IServerPlayer player)
    {
        RevealNearPlayer(player);
    }

    private void RevealNearPlayer(IServerPlayer player)
    {
        BlockPos? pos = player.Entity?.Pos?.AsBlockPos;
        if (pos == null)
        {
            return;
        }

        playerProximityRevealService?.RevealNear(pos);
    }

    private void RevealNearOnlinePlayers()
    {
        if (serverApi == null)
        {
            return;
        }

        IPlayer[] players = serverApi.World.AllOnlinePlayers;
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i] is IServerPlayer serverPlayer)
            {
                RevealNearPlayer(serverPlayer);
            }
        }
    }
}
