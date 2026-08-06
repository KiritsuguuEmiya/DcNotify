using System.Collections.Generic;
using Dnc.Delivery;

namespace DcNotify.TestSupport;

public sealed class FakePartyChatSender : IPartyChatSender
{
    public List<string> Messages { get; } = new();

    public bool AllowSend { get; set; } = true;

    public string? RejectReason { get; set; }

    public PartyChatSendResult TrySendPartyMessage(string message)
    {
        if (!AllowSend)
            return PartyChatSendResult.Fail(RejectReason ?? "Send rejected.");

        Messages.Add(message);
        return PartyChatSendResult.Ok();
    }

    public void Clear() => Messages.Clear();
}
