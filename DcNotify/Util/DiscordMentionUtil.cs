using System;
using System.Text.RegularExpressions;

namespace Dnc.Util;

public sealed record DiscordMentionPayload(string Content, object AllowedMentions);

public static class DiscordMentionUtil
{
    private static readonly Regex RoleMentionRegex = new(@"^<@&(\d{17,20})>$", RegexOptions.Compiled);
    private static readonly Regex UserMentionRegex = new(@"^<@(\d{17,20})>$", RegexOptions.Compiled);
    private static readonly Regex RawIdRegex = new(@"^(\d{17,20})$", RegexOptions.Compiled);

    public static DiscordMentionPayload? TryParse(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return null;

        var trimmed = input.Trim();
        Match match;
        var isRole = false;

        if ((match = RoleMentionRegex.Match(trimmed)).Success)
            isRole = true;
        else if ((match = UserMentionRegex.Match(trimmed)).Success)
            isRole = false;
        else if ((match = RawIdRegex.Match(trimmed)).Success)
            isRole = false;
        else
            return null;

        var id = match.Groups[1].Value;
        var content = isRole ? $"<@&{id}>" : $"<@{id}>";
        var allowedMentions = isRole
            ? (object)new { parse = Array.Empty<string>(), roles = new[] { id } }
            : new { parse = Array.Empty<string>(), users = new[] { id } };

        return new DiscordMentionPayload(content, allowedMentions);
    }
}
