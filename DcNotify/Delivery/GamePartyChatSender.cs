using System;
using System.Runtime.CompilerServices;
using Dnc.Util;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Shell;

namespace Dnc.Delivery;

public sealed class GamePartyChatSender : IPartyChatSender
{
    public const int MaxMessageLength = 200;

    public PartyChatSendResult TrySendPartyMessage(string message)
    {
        message = Sanitize(message);
        if (string.IsNullOrWhiteSpace(message))
            return PartyChatSendResult.Fail("Enter a party chat message first.");

        if (!Service.ClientState.IsLoggedIn)
            return PartyChatSendResult.Fail("Log in to send a party chat message.");

        if (!PartyUtil.IsInParty())
            return PartyChatSendResult.Fail("Join a party to send a party chat message.");

        try
        {
            Service.Framework.RunOnFrameworkThread(() => SendUnsafe(message));
            return PartyChatSendResult.Ok();
        }
        catch (Exception ex)
        {
            Service.PluginLog.Warning(ex, "Failed to queue party chat message.");
            return PartyChatSendResult.Fail("Failed to send party chat message.");
        }
    }

    private static unsafe void SendUnsafe(string message)
    {
        var uiModule = UIModule.Instance();
        if (uiModule == null)
            return;

        var shellModule = uiModule->GetRaptureShellModule();
        if (shellModule == null)
            return;

        Utf8String utf8 = new($"/p {message}");
        try
        {
            shellModule->ExecuteCommandInner((Utf8String*)Unsafe.AsPointer(ref utf8), uiModule);
        }
        finally
        {
            utf8.Dtor();
        }
    }

    internal static string Sanitize(string message)
    {
        message = message.Replace("\r\n", " ").Replace('\r', ' ').Replace('\n', ' ').Trim();
        if (message.Length > MaxMessageLength)
            message = message[..MaxMessageLength];

        return message;
    }
}
