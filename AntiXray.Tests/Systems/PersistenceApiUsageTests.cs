using Xunit;

namespace AntiXray.Tests.Systems;

public sealed class PersistenceApiUsageTests
{
    [Fact]
    public void HiddenOrePersistence_UsesMapChunkModdataOnly()
    {
        string? directory = AppContext.BaseDirectory;
        while (directory != null && !File.Exists(Path.Combine(directory, "AntiXray.slnx")))
        {
            directory = Directory.GetParent(directory)?.FullName;
        }

        Assert.NotNull(directory);

        string persistencePath = Path.Combine(
            directory,
            "AntiXray",
            "Systems",
            "HiddenOrePersistence.cs");

        string persistenceSource = File.ReadAllText(persistencePath);

        Assert.Contains("IMapChunk", persistenceSource);
        Assert.Contains("mapChunk.SetModdata", persistenceSource);
        Assert.Contains("mapChunk.GetModdata", persistenceSource);
        Assert.DoesNotContain("IWorldChunk.SetModdata", persistenceSource);
        Assert.DoesNotContain("chunk.SetModdata", persistenceSource);
    }
}
