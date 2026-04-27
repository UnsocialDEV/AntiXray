using AntiXray.Systems;
using Xunit;

namespace AntiXray.Tests.Systems;

public sealed class ExposureStateClassifierTests
{
    [Fact]
    public void IsExposure_ReturnsTrueForAir()
    {
        Assert.True(ExposureStateClassifier.IsExposure(0, false, null));
    }

    [Fact]
    public void IsExposure_ReturnsTrueForLiquidBlocks()
    {
        Assert.True(ExposureStateClassifier.IsExposure(100, false, "water"));
        Assert.True(ExposureStateClassifier.IsExposure(100, true, null));
    }

    [Fact]
    public void IsExposure_ReturnsFalseForSolidBlocks()
    {
        Assert.False(ExposureStateClassifier.IsExposure(100, false, null));
    }
}
