namespace Dnc.Notifications;

public static class DutyNotificationPolicy
{
    public static bool ShouldNotify(Configuration config, bool isClientAfk)
        => config.Enabled && config.EnableForDutyPops && isClientAfk;
}
