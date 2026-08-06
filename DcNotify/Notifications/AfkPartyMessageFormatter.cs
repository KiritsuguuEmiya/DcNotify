namespace Dnc.Notifications;

public static class AfkPartyMessageFormatter
{
    public const string DefaultMessage = "brb in 5 minutes";

    public static string Format(Configuration config)
        => string.IsNullOrWhiteSpace(config.AfkPartyMessageTemplate)
            ? DefaultMessage
            : config.AfkPartyMessageTemplate.Trim();
}
