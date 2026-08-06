using System;
using Dalamud.Game.Text;

namespace Dnc.Notifications;

public static class PartyChatNotificationPolicy
{
    public static bool ShouldNotify(Configuration config, bool isClientAfk)
        => config.Enabled && config.NotifyOnPartyChatMessages && isClientAfk;

    public static bool IsPartyChatType(XivChatType type)
        => type is XivChatType.Party or XivChatType.CrossParty;

    public static bool IsOwnMessage(string senderName, string? localPlayerName)
    {
        if (string.IsNullOrWhiteSpace(localPlayerName))
            return false;

        return string.Equals(senderName, localPlayerName, StringComparison.Ordinal);
    }
}
