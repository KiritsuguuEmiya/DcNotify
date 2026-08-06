using Dalamud.Game.Chat;
using Dalamud.Game.Text;
using Dnc.Notifications;
using Dnc.Util;

namespace Dnc.Impl;

public static class PartyChatListener
{
    public static void On()
    {
        Service.PluginLog.Debug("PartyChatListener On");
        Service.ChatGui.ChatMessage += OnChatMessage;
    }

    public static void Off()
    {
        Service.PluginLog.Debug("PartyChatListener Off");
        Service.ChatGui.ChatMessage -= OnChatMessage;
    }

    private static void OnChatMessage(IHandleableChatMessage message)
    {
        if (!PartyChatNotificationPolicy.IsPartyChatType(message.LogKind))
            return;

        var senderName = message.Sender.TextValue;
        var body = message.Message.TextValue;
        var localPlayerName = Service.ObjectTable.LocalPlayer?.Name.TextValue;

        PartyChatNotificationHandler.Default.Handle(
            senderName,
            body,
            Plugin.Configuration,
            CharacterUtil.IsClientAfk(),
            localPlayerName);
    }
}
