using System.Collections.Generic;
using System.Linq;

namespace Dnc.Util;

internal static class PartyMemberChangeDetector
{
    internal static bool MembersEqual(CrossWorldPartyListSystem.CrossWorldMember a, CrossWorldPartyListSystem.CrossWorldMember b)
    {
        if (a.ContentId != 0 && b.ContentId != 0)
            return a.ContentId == b.ContentId;

        return a.Name == b.Name;
    }

    internal static IEnumerable<CrossWorldPartyListSystem.CrossWorldMember> DetectJoins(
        IReadOnlyList<CrossWorldPartyListSystem.CrossWorldMember> current,
        IReadOnlyList<CrossWorldPartyListSystem.CrossWorldMember> previous)
        => current.Where(member => !previous.Any(old => MembersEqual(old, member)));

    internal static IEnumerable<CrossWorldPartyListSystem.CrossWorldMember> DetectLeaves(
        IReadOnlyList<CrossWorldPartyListSystem.CrossWorldMember> current,
        IReadOnlyList<CrossWorldPartyListSystem.CrossWorldMember> previous)
        => previous.Where(member => !current.Any(currentMember => MembersEqual(currentMember, member)));
}
