using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using Dnc.Util;
using Flurl.Http;

namespace Dnc.Delivery;

public sealed class DiscordNotificationSink : INotificationSink
{
    private const string CompositionFileName = "party.png";

    private readonly Configuration configuration;
    private readonly IPluginLog log;

    public DiscordNotificationSink(Configuration configuration, IPluginLog log)
    {
        this.configuration = configuration;
        this.log = log;
    }

    public void Deliver(string title, string text, byte[]? compositionPng = null)
    {
        if (configuration.DcHook.IsNullOrWhitespace())
        {
            log.Debug("Discord webhook URL not configured, skipping notification.");
            return;
        }

        if (!Uri.IsWellFormedUriString(configuration.DcHook, UriKind.Absolute))
        {
            log.Warning("Discord webhook URL is invalid, skipping notification.");
            return;
        }

        _ = Task.Run(() => DeliverAsync(title, text, compositionPng));
    }

    private async Task DeliverAsync(string title, string text, byte[]? compositionPng)
    {
        var discordWebhookUrl = configuration.DcHook;

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

            log.Debug("Sent Discord notification.");
        }
        catch (FlurlHttpException e)
        {
            if (e.Call.Response != null)
            {
                log.Error(
                    $"Failed to send notification to Discord webhook. Status code: {e.Call.Response.StatusCode}, response body: {await e.GetResponseStringAsync()}");
            }
            else
            {
                log.Error($"Failed to send notification to Discord webhook: '{e.Message}'");
            }
        }
    }

    private Task DeliverWithAttachmentAsync(string webhookUrl, string title, string text, byte[] compositionPng)
    {
        var payload = BuildPayload(title, text, $"attachment://{CompositionFileName}");
        var payloadJson = JsonSerializer.Serialize(payload);

        return webhookUrl.PostMultipartAsync(mp => mp
            .AddString("payload_json", payloadJson)
            .AddFile("files[0]", new MemoryStream(compositionPng), CompositionFileName, "image/png"));
    }

    private Task DeliverJsonAsync(string webhookUrl, string title, string text, string? imageUrl)
        => webhookUrl.PostJsonAsync(BuildPayload(title, text, imageUrl));

    private object BuildPayload(string title, string text, string? imageUrl)
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

    private DiscordMentionPayload? TryBuildMentionPayload()
    {
        if (!configuration.DiscordMentionEnabled || configuration.DiscordMentionTarget.IsNullOrWhitespace())
            return null;

        var mention = DiscordMentionUtil.TryParse(configuration.DiscordMentionTarget);
        if (mention == null)
            log.Warning("Discord mention target is invalid, sending without ping.");

        return mention;
    }
}
