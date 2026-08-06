using Dnc.Notifications;
using Dnc.Util;

namespace Dnc.Impl;

public static class AfkPartyMessageListener
{
    public static void On()
    {
        Service.PluginLog.Debug("AfkPartyMessageListener On");
        CrossWorldPartyListSystem.OnJoin += OnJoin;
        CrossWorldPartyListSystem.OnLeave += OnLeave;
        Service.Framework.Update += OnFrameworkUpdate;
    }

    public static void Off()
    {
        Service.PluginLog.Debug("AfkPartyMessageListener Off");
        CrossWorldPartyListSystem.OnJoin -= OnJoin;
        CrossWorldPartyListSystem.OnLeave -= OnLeave;
        Service.Framework.Update -= OnFrameworkUpdate;
        AfkPartyMessageHandler.Default.Dispose();
    }

    private static void OnJoin(CrossWorldPartyListSystem.CrossWorldMember member)
        => AfkPartyMessageHandler.Default.HandleJoin(
            member,
            Plugin.Configuration,
            CharacterUtil.IsClientAfk());

    private static void OnLeave(CrossWorldPartyListSystem.CrossWorldMember member)
        => AfkPartyMessageHandler.Default.HandleLeave(member);

    private static void OnFrameworkUpdate(Dalamud.Plugin.Services.IFramework framework)
        => AfkPartyMessageHandler.Default.OnFrameworkUpdate(CharacterUtil.IsClientAfk());
}
