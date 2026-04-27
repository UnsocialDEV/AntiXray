using AntiXray.Systems;
using Vintagestory.API.Common;
using Xunit;

namespace AntiXray.Tests.Systems;

public sealed class HiddenOreExplosionBehaviorInstallerTests
{
    [Theory]
    [InlineData("game:rock-granite", true)]
    [InlineData("game:rock-andesite", true)]
    [InlineData("game:ore-poor-nativecopper-granite", false)]
    [InlineData("other:rock-granite", false)]
    public void ShouldInstall_TargetsGameRockPlaceholders(string code, bool expected)
    {
        var installer = new HiddenOreExplosionBehaviorInstaller();
        var block = new Block { Code = new AssetLocation(code) };

        Assert.Equal(expected, installer.ShouldInstall(block));
    }
}
