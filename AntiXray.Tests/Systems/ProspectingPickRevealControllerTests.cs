using AntiXray.Systems;
using Vintagestory.API.MathTools;
using Xunit;

namespace AntiXray.Tests.Systems;

public sealed class ProspectingPickRevealControllerTests
{
    [Fact]
    public void TryReveal_DoesNothingWhenConfigIsDisabled()
    {
        var service = new ProspectingRevealService(new CountingRevealer(), 1);
        var controller = new ProspectingPickRevealController(false, new ProspectingPickDetector(), service);

        int revealed = controller.TryReveal(null!, new BlockPos(0, 0, 0, 0));

        Assert.Equal(0, revealed);
    }

    private sealed class CountingRevealer : IProspectingOreRevealer
    {
        public bool RevealForProspectingPick(BlockPos pos)
        {
            return true;
        }
    }
}
