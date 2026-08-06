namespace Dnc.Delivery;

public sealed class DiscordNotificationSink : INotificationSink
{
    public static DiscordNotificationSink Instance { get; } = new();

    private DiscordNotificationSink()
    {
    }

    public void Deliver(string title, string text, byte[]? compositionPng = null)
        => DncDelivery.Deliver(title, text, compositionPng);
}
