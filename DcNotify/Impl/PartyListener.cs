using Dnc.Delivery;
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

    private static void OnJoin(CrossWorldPartyListSystem.CrossWorldMember m)
    {
        if (!CharacterUtil.IsClientAfk()) return;
        if (!Plugin.Configuration.Enabled) return;

        if (m.PartyCount != 8 && !Plugin.Configuration.ShouldNotifyForClassJob(m.JobId))
            return;

        var jobAbbr = LuminaDataUtil.GetJobAbbreviation(m.JobId);

        if (m.PartyCount == 8)
        {
            DncDelivery.Deliver("Party full",
                $"{m.Name} (Lv{m.Level} {jobAbbr}) joins the party.\nParty recruitment ended. All spots have been filled.");
        }
        else
        {
            DncDelivery.Deliver($"{m.PartyCount}/8: Party join",
                $"{m.Name} (Lv{m.Level} {jobAbbr}) joins the party.");
        }
    }

    private static void OnLeave(CrossWorldPartyListSystem.CrossWorldMember m)
    {
        if (!CharacterUtil.IsClientAfk()) return;
        if (!Plugin.Configuration.Enabled) return;

        var jobAbbr = LuminaDataUtil.GetJobAbbreviation(m.JobId);

        DncDelivery.Deliver($"{m.PartyCount - 1}/8: Party leave",
            $"{m.Name} (Lv{m.Level} {jobAbbr}) has left the party.");
    }
}
