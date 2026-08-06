using System;
using System.Numerics;
using System.Threading.Tasks;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Dnc.Delivery;
using Dnc.Util;

namespace Dnc.Windows;

public class ConfigWindow : Window, IDisposable
{
    private readonly Configuration Configuration;

    private readonly TimedBool notifSentMessageTimer = new(3.0f);

    public ConfigWindow(Plugin plugin) : base(
        "DcN Configuration",
        ImGuiWindowFlags.NoCollapse)
    {
        Configuration = Plugin.Configuration;
        Size = new Vector2(560, 420);
        SizeCondition = ImGuiCond.FirstUseEver;
    }

    public void Dispose() { }

    public override void Draw()
    {
        using (var tabBar = ImRaii.TabBar("DcNConfigTabs"))
        {
            if (!tabBar)
                return;

            using (var applicationTab = ImRaii.TabItem("Application"))
            {
                if (applicationTab)
                    DrawApplicationTab();
            }

            using (var apiTab = ImRaii.TabItem("API"))
            {
                if (apiTab)
                    DrawApiTab();
            }

            using (var classFilterTab = ImRaii.TabItem("Class Filter"))
            {
                if (classFilterTab)
                    DrawClassFilterTab();
            }
        }

        ImGui.Spacing();

        if (PluginInterface.IsInProfile)
        {
            ImGui.TextColored(new Vector4(0.6f, 0.8f, 1.0f, 1.0f),
                "This plugin uses your active Dalamud collection profile for settings.");
        }

        if (ImGui.Button("Save"))
            Configuration.Save();

        ImGui.SameLine();

        if (ImGui.Button("Save and close"))
        {
            Configuration.Save();
            IsOpen = false;
        }
    }

    private void DrawApplicationTab()
    {
        {
            var cfg = Configuration.Enabled;
            if (ImGui.Checkbox("Enable/Disable Plugin", ref cfg))
                Configuration.Enabled = cfg;
        }

        {
            var cfg = Configuration.EnableForDutyPops;
            if (ImGui.Checkbox("Send message for duty pop?", ref cfg))
                Configuration.EnableForDutyPops = cfg;
        }

        {
            var cfg = Configuration.NotifyOnFilteredLeave;
            if (ImGui.Checkbox("Notify when filtered people leave", ref cfg))
                Configuration.NotifyOnFilteredLeave = cfg;
        }

        {
            var cfg = Configuration.NotifyOnPartyChatMessages;
            if (ImGui.Checkbox("Send party chat messages as notifications", ref cfg))
                Configuration.NotifyOnPartyChatMessages = cfg;
        }

        {
            var cfg = Configuration.IgnoreAfkStatus;
            if (ImGui.Checkbox("Ignore AFK status and always notify", ref cfg))
                Configuration.IgnoreAfkStatus = cfg;
        }

        if (!Configuration.IgnoreAfkStatus)
        {
            if (!CharacterUtil.IsClientAfk())
            {
                var red = new Vector4(1.0f, 0.0f, 0.0f, 1.0f);
                ImGui.TextColored(red, "This plugin will only function while your client is AFK (/afk, red icon)!");

                if (ImGui.IsItemHovered())
                {
                    ImGui.BeginTooltip();
                    ImGui.Text("The reasoning for this is that if you are not AFK, you are assumed to");
                    ImGui.Text("be at your computer, and ready to respond to a join or a duty pop.");
                    ImGui.Text("Notifications would be bothersome, so they are disabled.");
                    ImGui.EndTooltip();
                }
            }
            else
            {
                var green = new Vector4(0.0f, 1.0f, 0.0f, 1.0f);
                ImGui.TextColored(green, "You are AFK. The plugin is active and notifications will be served.");
            }
        }
    }

    private void DrawApiTab()
    {
        ImGui.TextUnformatted("Discord API");
        ImGui.Separator();

        {
            var cfg = Configuration.DcHook ?? string.Empty;
            if (ImGui.InputText("Webhook URL", ref cfg, 2048))
                Configuration.DcHook = cfg;
        }

        if (ImGui.Button("Send test notification"))
        {
            notifSentMessageTimer.Start();
            _ = SendTestNotificationAsync();
        }

        if (notifSentMessageTimer.Value)
        {
            ImGui.SameLine();
            ImGui.Text("Notification sent!");
        }

        ImGui.Spacing();
        ImGui.TextUnformatted("User API");
        ImGui.Separator();

        {
            var cfg = Configuration.DiscordMentionEnabled;
            if (ImGui.Checkbox("Include Discord mention (for mobile push)", ref cfg))
                Configuration.DiscordMentionEnabled = cfg;
        }

        ImGui.BeginDisabled(!Configuration.DiscordMentionEnabled);
        {
            var cfg = Configuration.DiscordMentionTarget ?? string.Empty;
            if (ImGui.InputText("Mention target (user or role ID)", ref cfg, 128))
                Configuration.DiscordMentionTarget = cfg;
        }
        ImGui.EndDisabled();

        ImGui.TextWrapped(
            "Enable Discord Developer Mode, then right-click a user or role and choose Copy User/Role ID. " +
            "Paste the raw ID or full mention string (<@...> or <@&...>). " +
            "Set the channel notification preference to Only @mentions for mobile push.");
    }

    private static async Task SendTestNotificationAsync()
    {
        var slots = PartyCompositionBuilder.BuildRandomSample();
        var filled = PartyCompositionBuilder.CountFilled(slots);
        var remaining = Math.Max(0, 8 - filled);

        byte[]? composition = null;
        try
        {
            composition = await PartyCompositionRenderer.RenderAsync(slots);
        }
        catch (Exception ex)
        {
            Service.PluginLog.Warning(ex, "Failed to render test party composition.");
        }

        DncDelivery.Deliver(
            "Test notification",
            $"**Preview Player** (Lv100) joins the party.\n\n**{filled}/8 filled · {remaining} players remaining**",
            composition);
    }

    private void DrawClassFilterTab()
    {
        ClassJobSelector.Draw(Configuration);
    }

    private static IDalamudPluginInterface PluginInterface => Service.PluginInterface;
}
