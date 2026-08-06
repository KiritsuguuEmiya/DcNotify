using System;
using Dalamud.Configuration;
using Dalamud.Plugin;

namespace Dnc;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 2;

    public bool EnableForDutyPops { get; set; } = true;
    public bool IgnoreAfkStatus { get; set; } = false;

    public bool Enabled { get; set; } = true;

    public string DcHook { get; set; } = "";

    [NonSerialized]
    private IDalamudPluginInterface? PluginInterface;

    public void Initialize(IDalamudPluginInterface pluginInterface)
    {
        PluginInterface = pluginInterface;
    }

    public void Save()
    {
        PluginInterface!.SavePluginConfig(this);
    }
}
