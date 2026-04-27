using Vintagestory.API.Server;

namespace AntiXray.Commands;

public sealed class AdminChatDebugSessions
{
    private readonly HashSet<string> enabledPlayerUids = new(StringComparer.Ordinal);

    public void Enable(IServerPlayer player)
    {
        enabledPlayerUids.Add(player.PlayerUID);
    }

    public void Disable(IServerPlayer player)
    {
        enabledPlayerUids.Remove(player.PlayerUID);
    }

    public bool IsEnabled(IServerPlayer player)
    {
        return enabledPlayerUids.Contains(player.PlayerUID);
    }
}
