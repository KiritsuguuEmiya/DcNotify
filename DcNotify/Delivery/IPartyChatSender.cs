namespace Dnc.Delivery;

public interface IPartyChatSender
{
    PartyChatSendResult TrySendPartyMessage(string message);
}
