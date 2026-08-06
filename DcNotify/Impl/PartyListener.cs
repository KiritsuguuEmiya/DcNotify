using System;
using System.Threading.Tasks;
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

        SendNotification(
            m.PartyCount == 8 ? "Party full" : $"{m.PartyCount}/8: Party join",
            FormatJoinDescription(m));
    }

    private static void OnLeave(CrossWorldPartyListSystem.CrossWorldMember m)
    {
        if (!CharacterUtil.IsClientAfk()) return;
        if (!Plugin.Configuration.Enabled) return;

        var remaining = Math.Max(0, m.PartyCount - 1);
        SendNotification(
            $"{remaining}/8: Party leave",
            FormatLeaveDescription(m, remaining));
    }

    private static void SendNotification(string title, string description)
    {
        _ = SendNotificationAsync(title, description);
    }

    private static async Task SendNotificationAsync(string title, string description)
    {
        byte[]? composition = null;

        try
        {
            var slots = PartyCompositionBuilder.Build();
            composition = await PartyCompositionRenderer.RenderAsync(slots);
        }
        catch (Exception ex)
        {
            Service.PluginLog.Warning(ex, "Failed to build party composition for webhook.");
        }

        DncDelivery.Deliver(title, description, composition);
    }

    private static string FormatJoinDescription(CrossWorldPartyListSystem.CrossWorldMember m)
    {
        var status = FormatStatusLine(m.PartyCount);
        var action = m.PartyCount == 8
            ? $"{FormatMemberLine(m)} joins the party.\n\nParty recruitment ended."
            : $"{FormatMemberLine(m)} joins the party.";

        return $"{action}\n\n{status}";
    }

    private static string FormatLeaveDescription(CrossWorldPartyListSystem.CrossWorldMember m, int remaining)
    {
        return $"{FormatMemberLine(m)} has left the party.\n\n{FormatStatusLine(remaining)}";
    }

    private static string FormatMemberLine(CrossWorldPartyListSystem.CrossWorldMember m)
        => $"**{m.Name}** (Lv{m.Level})";

    private static string FormatStatusLine(int filledCount)
    {
        var remaining = Math.Max(0, 8 - filledCount);
        return $"**{filledCount}/8 filled · {remaining} players remaining**";
    }
}
