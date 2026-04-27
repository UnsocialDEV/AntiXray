using System.Reflection;
using AntiXray.Commands;
using Vintagestory.API.Server;
using Xunit;

namespace AntiXray.Tests.Commands;

public sealed class AdminChatDebugSessionsTests
{
    [Fact]
    public void Enable_DoesNotEnableOtherAdmins()
    {
        var sessions = new AdminChatDebugSessions();
        IServerPlayer first = CreatePlayer("first");
        IServerPlayer second = CreatePlayer("second");

        sessions.Enable(first);

        Assert.True(sessions.IsEnabled(first));
        Assert.False(sessions.IsEnabled(second));
    }

    [Fact]
    public void Disable_RemovesOnlyThatAdmin()
    {
        var sessions = new AdminChatDebugSessions();
        IServerPlayer first = CreatePlayer("first");
        IServerPlayer second = CreatePlayer("second");
        sessions.Enable(first);
        sessions.Enable(second);

        sessions.Disable(first);

        Assert.False(sessions.IsEnabled(first));
        Assert.True(sessions.IsEnabled(second));
    }

    [Fact]
    public void UnknownAdmin_DefaultsToDisabled()
    {
        var sessions = new AdminChatDebugSessions();

        Assert.False(sessions.IsEnabled(CreatePlayer("admin")));
    }

    private static IServerPlayer CreatePlayer(string playerUid)
    {
        IServerPlayer player = DispatchProxy.Create<IServerPlayer, ServerPlayerProxy>();
        ((ServerPlayerProxy)(object)player).PlayerUid = playerUid;
        return player;
    }

    private class ServerPlayerProxy : DispatchProxy
    {
        public string PlayerUid { get; set; } = string.Empty;

        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            return targetMethod?.Name == "get_PlayerUID" ? PlayerUid : GetDefault(targetMethod?.ReturnType);
        }

        private static object? GetDefault(Type? type)
        {
            if (type == null || type == typeof(void))
            {
                return null;
            }

            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }
    }
}
