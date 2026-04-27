using AntiXray.Config;
using Xunit;

namespace AntiXray.Tests.Config;

public sealed class AntiXrayConfigTests
{
    [Fact]
    public void Defaults_DisableProspectingPickReveal()
    {
        var config = new AntiXrayConfig();

        Assert.False(config.RevealOreOnProspectingPick);
        Assert.Equal(6, config.ProspectingPickRevealRadius);
        Assert.Contains("prospectingpick-*", config.ProspectingPickCodePatterns);
    }

    [Fact]
    public void Defaults_EnableHardenedBlockBreakVeinReveal()
    {
        var config = new AntiXrayConfig();

        Assert.Equal(3, config.BlockBreakRevealDistance);
        Assert.True(config.BlockBreakRevealConnectedVein);
        Assert.Equal(128, config.BlockBreakRevealMaxOreBlocks);
    }

    [Fact]
    public void Defaults_EnablePlayerProximityCaveOreReveal()
    {
        var config = new AntiXrayConfig();

        Assert.True(config.RevealAirExposedOreNearPlayers);
        Assert.Equal(24, config.PlayerAirExposedOreRevealDistance);
        Assert.Equal(64, config.PlayerAirExposedOreRevealMaxPerEvent);
    }

    [Fact]
    public void Defaults_SetAdminCommandRadiusLimits()
    {
        var config = new AntiXrayConfig();

        Assert.Equal(16, config.AdminCommandDefaultRadius);
        Assert.Equal(64, config.AdminCommandMaxRadius);
    }
}
