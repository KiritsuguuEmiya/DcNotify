using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Dnc.Delivery;
using Dnc.Impl;
using Dnc.Notifications;
using Dnc.Util;
using Dnc.Windows;

namespace Dnc;

public sealed class Plugin : IDalamudPlugin
{
    public string Name => "DcN";
    private const string CommandName = "/dcn";

    private IDalamudPluginInterface PluginInterface { get; init; }
    private ICommandManager CommandManager { get; init; }

#pragma warning disable CS8618
    public static Configuration Configuration { get; private set; }
#pragma warning restore

    public WindowSystem WindowSystem = new("DcN");

    private ConfigWindow ConfigWindow { get; init; }

    public Plugin(
        IDalamudPluginInterface pluginInterface,
        ICommandManager commandManager)
    {
        pluginInterface.Create<Service>();

        PluginInterface = pluginInterface;
        CommandManager = commandManager;

        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        Configuration.Initialize(PluginInterface);

        ClassJobRegistry.Initialize();

        var notificationSink = new DiscordNotificationSink(Configuration, Service.PluginLog);
        PartyNotificationHandler.Initialize(notificationSink);
        DutyNotificationHandler.Initialize(notificationSink);
        PartyChatNotificationHandler.Initialize(notificationSink);
        AfkPartyMessageHandler.Initialize(new GamePartyChatSender());

        ConfigWindow = new ConfigWindow(this);

        WindowSystem.AddWindow(ConfigWindow);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Opens settings\n'version' shows loaded build\n't' toggles whether it's enabled.\n'on' enables the plugin\n'off' disables the plugin."
        });

        PluginInterface.UiBuilder.Draw += DrawUI;
        PluginInterface.UiBuilder.OpenMainUi += DrawMainUI;
        PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;

        CrossWorldPartyListSystem.Start();
        PartyListener.On();
        PartyChatListener.On();
        AfkPartyMessageListener.On();
        DutyListener.On();
    }

    public void Dispose()
    {
        WindowSystem.RemoveAllWindows();

        ConfigWindow.Dispose();

        CrossWorldPartyListSystem.Stop();
        PartyListener.Off();
        PartyChatListener.Off();
        AfkPartyMessageListener.Off();
        DutyListener.Off();

        CommandManager.RemoveHandler(CommandName);
    }

    private void OnCommand(string command, string args)
    {
        switch (args.Trim())
        {
            case "t" or "toggle":
                Configuration.Enabled = !Configuration.Enabled;
                Service.ChatGui.Print($"DcN plugin {(Configuration.Enabled ? "enabled" : "disabled")}.");
                break;
            case "on":
                Configuration.Enabled = true;
                Service.ChatGui.Print("DcN plugin enabled.");
                break;
            case "off":
                Configuration.Enabled = false;
                Service.ChatGui.Print("DcN plugin disabled.");
                break;
            case "version" or "v":
                Service.ChatGui.Print($"DcN v{typeof(Plugin).Assembly.GetName().Version} loaded.");
                break;
            case "":
                ConfigWindow.IsOpen = true;
                break;
        }
    }

    private void DrawUI()
    {
        WindowSystem.Draw();
    }

    public void DrawMainUI() => ConfigWindow.IsOpen = true;

    public void DrawConfigUI() => ConfigWindow.IsOpen = true;
}
