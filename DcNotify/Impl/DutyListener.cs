using Dnc.Notifications;
using Dnc.Util;
using Lumina.Excel.Sheets;

namespace Dnc.Impl;

public static class DutyListener
{
    public static void On()
    {
        Service.PluginLog.Debug("DutyListener On");
        Service.ClientState.CfPop += OnDutyPop;
    }

    public static void Off()
    {
        Service.PluginLog.Debug("DutyListener Off");
        Service.ClientState.CfPop -= OnDutyPop;
    }

    private static void OnDutyPop(ContentFinderCondition duty)
        => DutyNotificationHandler.Default.Handle(
            duty,
            Plugin.Configuration,
            CharacterUtil.IsClientAfk());
}
