using System;
using System.Threading.Tasks;
using Dalamud.Utility;
using Flurl.Http;

namespace Dnc.Delivery;

public static class DncDelivery
{
    public static void Deliver(string title, string text = "")
    {
        if (Plugin.Configuration.DcHook.IsNullOrWhitespace())
        {
            Service.PluginLog.Debug("Discord webhook URL not configured, skipping notification.");
            return;
        }

        if (!Uri.IsWellFormedUriString(Plugin.Configuration.DcHook, UriKind.Absolute))
        {
            Service.PluginLog.Warning("Discord webhook URL is invalid, skipping notification.");
            return;
        }

        Task.Run(() => DeliverAsync(title, text));
    }

    private static async Task DeliverAsync(string title, string text)
    {
        var discordWebhookUrl = Plugin.Configuration.DcHook;

        var payload = new
        {
            username = "DcN",
            avatar_url = "https://i.imgur.com/wAhXLxp.png",
            embeds = new[]
            {
                new
                {
                    title = title,
                    description = text,
                    color = 16711680
                }
            }
        };

        try
        {
            await discordWebhookUrl.PostJsonAsync(payload);
            Service.PluginLog.Debug("Sent Discord notification.");
        }
        catch (FlurlHttpException e)
        {
            if (e.Call.Response != null)
            {
                Service.PluginLog.Error(
                    $"Failed to send notification to Discord webhook. Status code: {e.Call.Response.StatusCode}, response body: {await e.GetResponseStringAsync()}");
            }
            else
            {
                Service.PluginLog.Error($"Failed to send notification to Discord webhook: '{e.Message}'");
            }
        }
    }
}
