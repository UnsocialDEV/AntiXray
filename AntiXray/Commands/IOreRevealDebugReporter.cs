using Vintagestory.API.MathTools;

namespace AntiXray.Commands;

public interface IOreRevealDebugReporter
{
    void ReportReveal(string reason, BlockPos pos, string oreCode, BlockPos? triggerPos = null);

    void ReportBatch(string reason, int count, BlockPos triggerPos);
}
