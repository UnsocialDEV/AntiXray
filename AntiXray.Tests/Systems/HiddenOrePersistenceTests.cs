using System.Reflection;
using AntiXray.Systems;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Xunit;

namespace AntiXray.Tests.Systems;

public sealed class HiddenOrePersistenceTests
{
    [Fact]
    public void SaveColumn_WritesProcessedMarkerForEmptyColumn()
    {
        var manager = new HiddenOreManager(new ChunkIndex(32));
        var persistence = new HiddenOrePersistence(manager);
        var mapChunk = TestMapChunk.Create();

        persistence.SaveColumn(mapChunk.Instance, 0, 0);

        Assert.NotNull(mapChunk.ModData);
        Assert.NotEmpty(mapChunk.ModData);
        Assert.True(mapChunk.MarkedDirty);
    }

    [Fact]
    public void LoadColumn_TreatsEmptyProcessedColumnAsProcessed()
    {
        var manager = new HiddenOreManager(new ChunkIndex(32));
        var persistence = new HiddenOrePersistence(manager);
        var mapChunk = TestMapChunk.Create();
        var worldChunk = TestWorldChunk.Create(mapChunk.Instance);

        persistence.SaveColumn(mapChunk.Instance, 0, 0);
        bool loaded = persistence.LoadColumn([worldChunk.Instance]);

        Assert.True(loaded);
    }

    [Fact]
    public void LoadColumn_MigratesLegacyModDataKey()
    {
        var manager = new HiddenOreManager(new ChunkIndex(32));
        var persistence = new HiddenOrePersistence(manager);
        var legacyMapChunk = TestMapChunk.Create();
        var legacyWriter = new HiddenOrePersistence(manager);
        var pos = new BlockPos(4, 10, 8, 0);
        manager.Store(pos, "game:ore-poor-nativecopper-granite");
        legacyWriter.SaveColumn(legacyMapChunk.Instance, 0, 0);
        byte[] legacyBytes = legacyMapChunk.ModData!;

        var migratedManager = new HiddenOreManager(new ChunkIndex(32));
        var migratedPersistence = new HiddenOrePersistence(migratedManager);
        var mapChunk = TestMapChunk.Create();
        mapChunk.SetLegacyModData(legacyBytes);
        var worldChunk = TestWorldChunk.Create(mapChunk.Instance);

        bool loaded = migratedPersistence.LoadColumn([worldChunk.Instance]);

        Assert.True(loaded);
        Assert.True(migratedManager.TryGet(pos, out _));
        Assert.NotNull(mapChunk.ModData);
        Assert.True(mapChunk.MarkedDirty);
    }

    [Fact]
    public void SaveColumn_KeepsColumnProcessedAfterLastHiddenOreIsRemoved()
    {
        var manager = new HiddenOreManager(new ChunkIndex(32));
        var persistence = new HiddenOrePersistence(manager);
        var mapChunk = TestMapChunk.Create();
        var pos = new BlockPos(4, 10, 8, 0);

        manager.Store(pos, "game:ore-poor-nativecopper-granite");
        manager.Remove(pos);
        persistence.SaveColumn(mapChunk.Instance, 0, 0);

        Assert.NotNull(mapChunk.ModData);
        Assert.Equal(0, BitConverter.ToInt32(mapChunk.ModData, 0));
    }

    private class TestMapChunk : DispatchProxy
    {
        public IMapChunk Instance { get; private set; } = null!;

        public byte[]? ModData { get; private set; }

        private byte[]? legacyModData;

        public bool MarkedDirty { get; private set; }

        public static TestMapChunk Create()
        {
            IMapChunk instance = DispatchProxy.Create<IMapChunk, TestMapChunk>();
            var proxy = (TestMapChunk)(object)instance;
            proxy.Instance = instance;
            return proxy;
        }

        public void SetLegacyModData(byte[] bytes)
        {
            legacyModData = bytes;
        }

        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            if (targetMethod?.Name == nameof(IMapChunk.SetModdata))
            {
                ModData = (byte[])args![1]!;
                return null;
            }

            if (targetMethod?.Name == nameof(IMapChunk.GetModdata))
            {
                string key = (string)args![0]!;
                return key == "antiblockoverlay:hiddenores" ? legacyModData : ModData;
            }

            if (targetMethod?.Name == nameof(IMapChunk.MarkDirty))
            {
                MarkedDirty = true;
                return null;
            }

            return GetDefault(targetMethod?.ReturnType);
        }
    }

    private class TestWorldChunk : DispatchProxy
    {
        private IMapChunk? mapChunk;

        public IWorldChunk Instance { get; private set; } = null!;

        public static TestWorldChunk Create(IMapChunk mapChunk)
        {
            IWorldChunk instance = DispatchProxy.Create<IWorldChunk, TestWorldChunk>();
            var proxy = (TestWorldChunk)(object)instance;
            proxy.Instance = instance;
            proxy.mapChunk = mapChunk;
            return proxy;
        }

        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            if (targetMethod?.Name == "get_MapChunk")
            {
                return mapChunk;
            }

            return GetDefault(targetMethod?.ReturnType);
        }
    }

    private static object? GetDefault(Type? type)
    {
        if (type == null || type == typeof(void))
        {
            return null;
        }

        return type.IsValueType ? Activator.CreateInstance(type) : null;
    }
}
