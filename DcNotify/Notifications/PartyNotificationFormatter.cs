using System;
using Dnc.Util;

namespace Dnc.Notifications;

public static class PartyNotificationFormatter
{
    public static string FormatJoinTitle(
        CrossWorldPartyListSystem.CrossWorldMember member,
        JoinSnapshot? snapshot)
    {
        if (member.PartyCount == 8)
            return "Party full";

        var roleLabel = GetRoleLabel(snapshot);
        return roleLabel != null
            ? $"{member.PartyCount}/8: {roleLabel} join"
            : $"{member.PartyCount}/8: Party join";
    }

    public static string FormatLeaveTitle(
        CrossWorldPartyListSystem.CrossWorldMember member,
        int remaining,
        JoinSnapshot? snapshot)
    {
        var roleLabel = GetRoleLabel(snapshot);
        return roleLabel != null
            ? $"{remaining}/8: {roleLabel} leave"
            : $"{remaining}/8: Party leave";
    }

    public static string FormatJoinDescription(
        CrossWorldPartyListSystem.CrossWorldMember member,
        JoinSnapshot? snapshot,
        string jobAbbreviation)
    {
        var status = FormatStatusLine(member.PartyCount);
        var action = member.PartyCount == 8
            ? $"{FormatMemberLine(member, snapshot, jobAbbreviation)} joins the party.\n\nParty recruitment ended."
            : $"{FormatMemberLine(member, snapshot, jobAbbreviation)} joins the party.";

        return $"{action}\n\n{status}";
    }

    public static string FormatLeaveDescription(
        CrossWorldPartyListSystem.CrossWorldMember member,
        int remaining,
        JoinSnapshot? snapshot,
        string jobAbbreviation)
        => $"{FormatMemberLine(member, snapshot, jobAbbreviation)} has left the party.\n\n{FormatStatusLine(remaining)}";

    public static string FormatMemberLine(
        CrossWorldPartyListSystem.CrossWorldMember member,
        JoinSnapshot? snapshot,
        string jobAbbreviation)
    {
        var jobId = snapshot?.JoinJobId ?? member.JobId;

        if (snapshot?.SlotRole is { } role)
        {
            var roleLabel = ClassJobRegistry.GetRoleLabel(role);
            return $"**{roleLabel}** — **{member.Name}** (Lv{member.Level} {jobAbbreviation})";
        }

        return $"**{member.Name}** (Lv{member.Level} {jobAbbreviation})";
    }

    private static string? GetRoleLabel(JoinSnapshot? snapshot)
        => snapshot?.SlotRole is { } role ? ClassJobRegistry.GetRoleLabel(role) : null;

    private static string FormatStatusLine(int filledCount)
    {
        var remaining = Math.Max(0, 8 - filledCount);
        return $"**{filledCount}/8 filled · {remaining} players remaining**";
    }
}
