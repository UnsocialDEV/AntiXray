using Vintagestory.API.Config;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace AntiXray.Commands;

public sealed class OreRevealDebugReporter : IOreRevealDebugReporter
{
    private readonly ICoreServerAPI api;
    private readonly AdminChatDebugSessions sessions;
    private readonly OreRevealDebugFormatter formatter;

    public OreRevealDebugReporter(
        ICoreServerAPI api,
        AdminChatDebugSessions sessions,
        OreRevealDebugFormatter formatter)
    {
        this.api = api;
        this.sessions = sessions;
        this.formatter = formatter;
    }

    public void ReportReveal(string reason, BlockPos pos, string oreCode, BlockPos? triggerPos = null)
    {
        Send(formatter.Reveal(reason, pos, oreCode, triggerPos));
    }

    public void ReportBatch(string reason, int count, BlockPos triggerPos)
    {
        if (count <= 1)
        {
            return;
        }

        Send(formatter.Batch(reason, count, triggerPos));
    }

    private void Send(string message)
    {
        IPlayer[] players = api.World.AllOnlinePlayers;
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i] is not IServerPlayer serverPlayer || !sessions.IsEnabled(serverPlayer))
            {
                continue;
            }

            serverPlayer.SendMessage(GlobalConstants.GeneralChatGroup, message, EnumChatType.Notification);
        }
    }
}
