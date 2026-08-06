namespace Dnc.Delivery;

public readonly record struct PartyChatSendResult(bool Success, string? ErrorMessage)
{
    public static PartyChatSendResult Ok() => new(true, null);

    public static PartyChatSendResult Fail(string errorMessage) => new(false, errorMessage);
}
