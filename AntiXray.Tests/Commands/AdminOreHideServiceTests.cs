using AntiXray.Commands;
using AntiXray.Systems;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Xunit;

namespace AntiXray.Tests.Commands;

public sealed class AdminOreHideServiceTests
{
    [Fact]
    public void HideRealOreWithin_HidesRealOreInsideRadius()
    {
        var manager = CreateManager();
        var world = new RecordingWorld();
        var orePos = new BlockPos(2, 0, 0, 0);
        world.SetExistingBlock(orePos, CreateBlock("game:ore-poor-nativecopper-granite"));
        world.PlaceholderBlockId = 4;
        var service = CreateService(manager, world);

        AdminOreHideResult result = service.HideRealOreWithin(new BlockPos(0, 0, 0, 0), 3);

        Assert.Equal(1, result.OreFound);
        Assert.Equal(1, result.Hidden);
        Assert.True(manager.TryGet(orePos, out var data));
        Assert.Equal("game:ore-poor-nativecopper-granite", data.OreBlockCode);
        Assert.Contains(world.SetBlocks, entry => entry.Pos.X == 2 && entry.BlockId == 4);
        Assert.Contains(world.SavedColumns, pos => pos.X == 2 && pos.InternalY == 0 && pos.Z == 0);
    }

    [Fact]
    public void HideRealOreWithin_DoesNotHideOreOutsideRadius()
    {
        var manager = CreateManager();
        var world = new RecordingWorld();
        var orePos = new BlockPos(4, 0, 0, 0);
        world.SetExistingBlock(orePos, CreateBlock("game:ore-poor-nativecopper-granite"));
        world.PlaceholderBlockId = 4;
        var service = CreateService(manager, world);

        AdminOreHideResult result = service.HideRealOreWithin(new BlockPos(0, 0, 0, 0), 3);

        Assert.Equal(0, result.OreFound);
        Assert.Equal(0, result.Hidden);
        Assert.Empty(world.SetBlocks);
        Assert.False(manager.TryGet(orePos, out _));
    }

    [Fact]
    public void HideRealOreWithin_SkipsAlreadyHiddenOre()
    {
        var manager = CreateManager();
        var world = new RecordingWorld();
        var orePos = new BlockPos(2, 0, 0, 0);
        manager.Store(orePos, "game:ore-poor-nativecopper-granite");
        world.SetExistingBlock(orePos, CreateBlock("game:ore-poor-nativecopper-granite"));
        world.PlaceholderBlockId = 4;
        var service = CreateService(manager, world);

        AdminOreHideResult result = service.HideRealOreWithin(new BlockPos(0, 0, 0, 0), 3);

        Assert.Equal(1, result.AlreadyHidden);
        Assert.Equal(0, result.Hidden);
        Assert.Empty(world.SetBlocks);
    }

    [Fact]
    public void HideRealOreWithin_SkipsNonOreBlocks()
    {
        var manager = CreateManager();
        var world = new RecordingWorld();
        world.SetExistingBlock(new BlockPos(2, 0, 0, 0), CreateBlock("game:rock-granite"));
        world.PlaceholderBlockId = 4;
        var service = CreateService(manager, world);

        AdminOreHideResult result = service.HideRealOreWithin(new BlockPos(0, 0, 0, 0), 3);

        Assert.Equal(0, result.OreFound);
        Assert.Equal(0, result.Hidden);
        Assert.Empty(world.SetBlocks);
    }

    [Fact]
    public void HideRealOreWithin_CountsPlaceholderFailures()
    {
        var manager = CreateManager();
        var world = new RecordingWorld();
        var orePos = new BlockPos(2, 0, 0, 0);
        world.SetExistingBlock(orePos, CreateBlock("game:ore-poor-nativecopper-granite"));
        world.PlaceholderBlockId = 0;
        var service = CreateService(manager, world);

        AdminOreHideResult result = service.HideRealOreWithin(new BlockPos(0, 0, 0, 0), 3);

        Assert.Equal(1, result.OreFound);
        Assert.Equal(1, result.PlaceholderMissing);
        Assert.Equal(0, result.Hidden);
        Assert.Empty(world.SetBlocks);
        Assert.False(manager.TryGet(orePos, out _));
    }

    private static AdminOreHideService CreateService(HiddenOreManager manager, RecordingWorld world)
    {
        return new AdminOreHideService(manager, new OreBlockClassifier(), world);
    }

    private static HiddenOreManager CreateManager()
    {
        return new HiddenOreManager(new ChunkIndex(32));
    }

    private static Block CreateBlock(string code)
    {
        return new Block
        {
            Code = new AssetLocation(code)
        };
    }

    private sealed class RecordingWorld : IAdminOreHideWorld
    {
        private readonly Dictionary<string, Block> blocks = new(StringComparer.Ordinal);

        public int PlaceholderBlockId { get; set; }

        public List<(int BlockId, BlockPos Pos)> SetBlocks { get; } = new();

        public List<BlockPos> SavedColumns { get; } = new();

        public Block? GetBlock(BlockPos pos)
        {
            return blocks.TryGetValue(Key(pos), out Block? block) ? block : null;
        }

        public int ResolvePlaceholderBlockId(Block oreBlock)
        {
            return PlaceholderBlockId;
        }

        public void SetBlock(int blockId, BlockPos pos)
        {
            SetBlocks.Add((blockId, pos.Copy()));
        }

        public void SaveHiddenOreColumn(BlockPos pos)
        {
            SavedColumns.Add(pos.Copy());
        }

        public void SetExistingBlock(BlockPos pos, Block block)
        {
            blocks[Key(pos)] = block;
        }

        private static string Key(BlockPos pos)
        {
            return $"{pos.X}/{pos.InternalY}/{pos.Z}";
        }
    }
}
