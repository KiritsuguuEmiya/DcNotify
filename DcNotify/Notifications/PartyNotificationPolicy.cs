using Dnc.Util;

namespace Dnc.Notifications;

public static class PartyNotificationPolicy
{
    public static bool ShouldNotify(bool enabled, bool isClientAfk)
        => enabled && isClientAfk;

    public static bool ShouldNotifyJoin(Configuration config, CrossWorldPartyListSystem.CrossWorldMember member)
    {
        if (member.PartyCount == 8)
            return true;

        return config.ShouldNotifyForClassJob(member.JobId);
    }

    public static bool ShouldNotifyLeave(Configuration config, CrossWorldPartyListSystem.CrossWorldMember member)
        => config.ShouldNotifyLeaveForClassJob(member.JobId);
}
