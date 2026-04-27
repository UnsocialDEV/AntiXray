using Vintagestory.API.MathTools;

namespace AntiXray.Commands;

public sealed class OreRevealDebugFormatter
{
    private readonly WorldCoordinateDisplayFormatter coordinateFormatter;

    public OreRevealDebugFormatter(WorldCoordinateDisplayFormatter coordinateFormatter)
    {
        this.coordinateFormatter = coordinateFormatter;
    }

    public string Reveal(string reason, BlockPos pos, string oreCode, BlockPos? triggerPos)
    {
        string message = $"[AntiXray Debug] Revealed {oreCode} at {coordinateFormatter.Format(pos)} because {reason}.";
        if (triggerPos == null)
        {
            return message;
        }

        return message + $" trigger={coordinateFormatter.Format(triggerPos)}.";
    }

    public string Batch(string reason, int count, BlockPos triggerPos)
    {
        return $"[AntiXray Debug] Revealed {count} hidden ore blocks because {reason}. trigger={coordinateFormatter.Format(triggerPos)}.";
    }
}
