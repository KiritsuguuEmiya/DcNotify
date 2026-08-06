using Dnc.Util;

namespace Dnc.Notifications;

public static class AfkPartyMessagePolicy
{
    public static bool ShouldTrigger(Configuration config, bool isClientAfk)
        => config.Enabled && config.AfkPartyMessageEnabled && isClientAfk;

    public static bool IsPartyFull(CrossWorldPartyListSystem.CrossWorldMember member)
        => member.PartyCount >= PartyConstants.SlotCount;

    public static bool IsPartyNoLongerFullAfterLeave(CrossWorldPartyListSystem.CrossWorldMember member)
        => member.PartyCount >= PartyConstants.SlotCount;
}
