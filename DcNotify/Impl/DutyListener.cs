using Dalamud.Utility;
using Dnc.Delivery;
using Dnc.Util;
using Lumina.Excel.Sheets;

namespace Dnc.Impl;

public class DutyListener
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

    private static void OnDutyPop(ContentFinderCondition e)
    {
        if (!Plugin.Configuration.EnableForDutyPops)
            return;

        if (!CharacterUtil.IsClientAfk())
            return;

        if (!Plugin.Configuration.Enabled) return;

        var dutyName = e.RowId == 0 ? "Duty Roulette" : e.Name.ToDalamudString().TextValue;
        DncDelivery.Deliver("Duty pop", $"Duty registered: '{dutyName}'.");
    }
}
