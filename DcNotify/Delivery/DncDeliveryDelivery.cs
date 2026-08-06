using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Dalamud.Utility;
using Dnc.Util;
using Flurl.Http;

namespace Dnc.Delivery;

public static class DncDelivery
{
    private const string CompositionFileName = "party.png";

    public static void Deliver(string title, string text = "", byte[]? compositionPng = null)
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

        Task.Run(() => DeliverAsync(title, text, compositionPng));
    }

    private static async Task DeliverAsync(string title, string text, byte[]? compositionPng)
    {
        var discordWebhookUrl = Plugin.Configuration.DcHook;

        try
        {
            if (compositionPng is { Length: > 0 })
            {
                await DeliverWithAttachmentAsync(discordWebhookUrl, title, text, compositionPng);
            }
            else
            {
                await DeliverJsonAsync(discordWebhookUrl, title, text, null);
            }

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

    private static Task DeliverWithAttachmentAsync(string webhookUrl, string title, string text, byte[] compositionPng)
    {
        var payload = BuildPayload(title, text, $"attachment://{CompositionFileName}");
        var payloadJson = JsonSerializer.Serialize(payload);

        return webhookUrl.PostMultipartAsync(mp => mp
            .AddString("payload_json", payloadJson)
            .AddFile("files[0]", new MemoryStream(compositionPng), CompositionFileName, "image/png"));
    }

    private static Task DeliverJsonAsync(string webhookUrl, string title, string text, string? imageUrl)
        => webhookUrl.PostJsonAsync(BuildPayload(title, text, imageUrl));

    private static object BuildPayload(string title, string text, string? imageUrl)
    {
        var embed = new
        {
            title,
            description = text,
            color = 16711680,
            image = imageUrl == null ? null : new { url = imageUrl },
        };

        var mention = TryBuildMentionPayload();
        if (mention != null)
        {
            return new
            {
                username = "DcN",
                avatar_url = "https://i.imgur.com/wAhXLxp.png",
                content = mention.Content,
                allowed_mentions = mention.AllowedMentions,
                embeds = new[] { embed },
            };
        }

        return new
        {
            username = "DcN",
            avatar_url = "https://i.imgur.com/wAhXLxp.png",
            embeds = new[] { embed },
        };
    }

    private static DiscordMentionPayload? TryBuildMentionPayload()
    {
        var config = Plugin.Configuration;
        if (!config.DiscordMentionEnabled || config.DiscordMentionTarget.IsNullOrWhitespace())
            return null;

        var mention = DiscordMentionUtil.TryParse(config.DiscordMentionTarget);
        if (mention == null)
            Service.PluginLog.Warning("Discord mention target is invalid, sending without ping.");

        return mention;
    }
}
