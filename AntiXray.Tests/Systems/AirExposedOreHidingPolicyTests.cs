using AntiXray.Systems;
using Xunit;

namespace AntiXray.Tests.Systems;

public sealed class AirExposedOreHidingPolicyTests
{
    [Fact]
    public void ShouldHide_HidesEnclosedOreRegardlessOfY()
    {
        var policy = new AirExposedOreHidingPolicy(80);

        Assert.True(policy.ShouldHide(false, 120));
    }

    [Fact]
    public void ShouldHide_HidesAirExposedOreBelowConfiguredY()
    {
        var policy = new AirExposedOreHidingPolicy(80);

        Assert.True(policy.ShouldHide(true, 79));
    }

    [Fact]
    public void ShouldHide_KeepsAirExposedOreAtConfiguredYVisible()
    {
        var policy = new AirExposedOreHidingPolicy(80);

        Assert.False(policy.ShouldHide(true, 80));
    }

    [Fact]
    public void ShouldHide_KeepsAirExposedOreAboveConfiguredYVisible()
    {
        var policy = new AirExposedOreHidingPolicy(80);

        Assert.False(policy.ShouldHide(true, 81));
    }
}
