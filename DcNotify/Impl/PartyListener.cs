using Dnc.Notifications;
using Dnc.Util;

namespace Dnc.Impl;

public static class PartyListener
{
    public static void On()
    {
        Service.PluginLog.Debug("PartyListener On");
        CrossWorldPartyListSystem.OnJoin += OnJoin;
        CrossWorldPartyListSystem.OnLeave += OnLeave;
    }

    public static void Off()
    {
        Service.PluginLog.Debug("PartyListener Off");
        CrossWorldPartyListSystem.OnJoin -= OnJoin;
        CrossWorldPartyListSystem.OnLeave -= OnLeave;
    }

    private static void OnJoin(CrossWorldPartyListSystem.CrossWorldMember member)
        => PartyNotificationHandler.Default.HandleJoin(
            member,
            Plugin.Configuration,
            CharacterUtil.IsClientAfk());

    private static void OnLeave(CrossWorldPartyListSystem.CrossWorldMember member)
        => PartyNotificationHandler.Default.HandleLeave(
            member,
            Plugin.Configuration,
            CharacterUtil.IsClientAfk());
}
