using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Configuration;
using Dalamud.Plugin;

namespace Dnc;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 3;

    public bool EnableForDutyPops { get; set; } = true;
    public bool IgnoreAfkStatus { get; set; } = false;

    public bool Enabled { get; set; } = true;

    public string DcHook { get; set; } = "";

    public List<uint> SelectedClassJobIds { get; set; } = new();

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

    public bool ShouldNotifyForClassJob(uint classJobId)
    {
        if (SelectedClassJobIds.Count == 0)
            return true;

        return SelectedClassJobIds.Contains(classJobId);
    }

    public bool IsClassJobSelected(uint classJobId) => SelectedClassJobIds.Contains(classJobId);

    public void ToggleClassJob(uint classJobId)
    {
        if (SelectedClassJobIds.Contains(classJobId))
            SelectedClassJobIds.Remove(classJobId);
        else
            SelectedClassJobIds.Add(classJobId);
    }

    public void SetRoleGroup(IEnumerable<uint> classJobIds, bool selected)
    {
        foreach (var classJobId in classJobIds)
        {
            if (selected)
            {
                if (!SelectedClassJobIds.Contains(classJobId))
                    SelectedClassJobIds.Add(classJobId);
            }
            else
            {
                SelectedClassJobIds.Remove(classJobId);
            }
        }
    }

    public void ClearClassJobSelection() => SelectedClassJobIds.Clear();

    public bool IsRoleGroupFullySelected(IEnumerable<uint> classJobIds)
    {
        var ids = classJobIds.ToList();
        return ids.Count > 0 && ids.All(IsClassJobSelected);
    }
}
