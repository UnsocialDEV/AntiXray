using Vintagestory.API.Common;

namespace AntiXray.Systems;

public sealed class DebugLogger
{
    private readonly ILogger logger;
    private readonly bool enabled;
    private readonly HashSet<string> loggedMessages = new(StringComparer.Ordinal);

    public DebugLogger(ILogger logger, bool enabled)
    {
        this.logger = logger;
        this.enabled = enabled;
    }

    public void WarningOnce(string message)
    {
        if (!enabled || !loggedMessages.Add(message))
        {
            return;
        }

        logger.Warning("[AntiXray] " + message);
    }
}
