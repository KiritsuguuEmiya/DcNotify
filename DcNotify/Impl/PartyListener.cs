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
            FormatJoinTitle(m),
            FormatJoinDescription(m));
    }

    private static void OnLeave(CrossWorldPartyListSystem.CrossWorldMember m)
    {
        if (!CharacterUtil.IsClientAfk()) return;
        if (!Plugin.Configuration.Enabled) return;

        if (!Plugin.Configuration.ShouldNotifyForLeave()) return;

        var remaining = Math.Max(0, m.PartyCount - 1);
        SendNotification(
            FormatLeaveTitle(m, remaining),
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

    private static string FormatJoinTitle(CrossWorldPartyListSystem.CrossWorldMember m)
    {
        if (m.PartyCount == 8)
            return "Party full";

        var roleLabel = GetStoredRoleLabel(m);
        return roleLabel != null
            ? $"{m.PartyCount}/8: {roleLabel} join"
            : $"{m.PartyCount}/8: Party join";
    }

    private static string FormatLeaveTitle(CrossWorldPartyListSystem.CrossWorldMember m, int remaining)
    {
        var roleLabel = GetStoredRoleLabel(m);
        return roleLabel != null
            ? $"{remaining}/8: {roleLabel} leave"
            : $"{remaining}/8: Party leave";
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
    {
        var snapshot = PfRoleTracker.Get(m.ContentId, m.Name);
        var jobId = snapshot?.JoinJobId ?? m.JobId;
        var jobAbbr = LuminaDataUtil.GetJobAbbreviation(jobId);

        if (snapshot?.SlotRole is { } role)
        {
            var roleLabel = ClassJobRegistry.GetRoleLabel(role);
            return $"**{roleLabel}** — **{m.Name}** (Lv{m.Level} {jobAbbr})";
        }

        return $"**{m.Name}** (Lv{m.Level} {jobAbbr})";
    }

    private static string? GetStoredRoleLabel(CrossWorldPartyListSystem.CrossWorldMember m)
    {
        var snapshot = PfRoleTracker.Get(m.ContentId, m.Name);
        return snapshot?.SlotRole is { } role ? ClassJobRegistry.GetRoleLabel(role) : null;
    }

    private static string FormatStatusLine(int filledCount)
    {
        var remaining = Math.Max(0, 8 - filledCount);
        return $"**{filledCount}/8 filled · {remaining} players remaining**";
    }
}
