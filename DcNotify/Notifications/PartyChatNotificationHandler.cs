using Dnc.Delivery;

namespace Dnc.Notifications;

public sealed class PartyChatNotificationHandler
{
    public static PartyChatNotificationHandler Default { get; } = new(DiscordNotificationSink.Instance);

    private readonly INotificationSink sink;

    public PartyChatNotificationHandler(INotificationSink sink)
    {
        this.sink = sink;
    }

    public void Handle(string senderName, string message, Configuration config, bool isClientAfk, string? localPlayerName)
    {
        if (!PartyChatNotificationPolicy.ShouldNotify(config, isClientAfk))
            return;

        if (PartyChatNotificationPolicy.IsOwnMessage(senderName, localPlayerName))
            return;

        if (string.IsNullOrWhiteSpace(message))
            return;

        sink.Deliver("Party chat", $"**{senderName}**: {message}");
    }
}
