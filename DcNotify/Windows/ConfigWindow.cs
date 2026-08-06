using System;
using System.Numerics;
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

            using (var settingsTab = ImRaii.TabItem("Settings"))
            {
                if (settingsTab)
                    DrawSettingsTab();
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

    private void DrawSettingsTab()
    {
        {
            var cfg = Configuration.Enabled;
            if (ImGui.Checkbox("Enable/Disable Plugin", ref cfg))
                Configuration.Enabled = cfg;
        }

        {
            var cfg = Configuration.DcHook ?? string.Empty;
            if (ImGui.InputText("Webhook URL", ref cfg, 2048))
                Configuration.DcHook = cfg;
        }

        {
            var cfg = Configuration.EnableForDutyPops;
            if (ImGui.Checkbox("Send message for duty pop?", ref cfg))
                Configuration.EnableForDutyPops = cfg;
        }

        if (ImGui.Button("Send test notification"))
        {
            notifSentMessageTimer.Start();
            DncDelivery.Deliver("Test notification",
                "If you received this, DcN is configured correctly.");
        }

        if (notifSentMessageTimer.Value)
        {
            ImGui.SameLine();
            ImGui.Text("Notification sent!");
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

    private void DrawClassFilterTab()
    {
        ClassJobSelector.Draw(Configuration);
    }

    private static IDalamudPluginInterface PluginInterface => Service.PluginInterface;
}
