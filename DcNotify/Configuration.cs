using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Configuration;
using Dalamud.Plugin;

namespace Dnc;

public enum ClassFilterMode
{
    All = 0,
    None = 1,
    Selected = 2,
}

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 6;

    public bool EnableForDutyPops { get; set; } = true;
    public bool IgnoreAfkStatus { get; set; } = false;
    public bool NotifyOnFilteredLeave { get; set; } = false;
    public bool NotifyOnPartyChatMessages { get; set; } = false;

    public bool Enabled { get; set; } = true;

    public string DcHook { get; set; } = "";

    public bool DiscordMentionEnabled { get; set; } = false;

    public string DiscordMentionTarget { get; set; } = "";

    public ClassFilterMode ClassFilterMode { get; set; } = ClassFilterMode.All;

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
        return ClassFilterMode switch
        {
            ClassFilterMode.All => true,
            ClassFilterMode.None => false,
            ClassFilterMode.Selected => SelectedClassJobIds.Contains(classJobId),
            _ => true,
        };
    }

    public bool ShouldNotifyLeaveForClassJob(uint classJobId)
        => NotifyOnFilteredLeave && ShouldNotifyForClassJob(classJobId);

    public bool IsClassJobSelected(uint classJobId) => SelectedClassJobIds.Contains(classJobId);

    public void SetClassFilterAll()
    {
        ClassFilterMode = ClassFilterMode.All;
        SelectedClassJobIds.Clear();
    }

    public void SetClassFilterNone()
    {
        ClassFilterMode = ClassFilterMode.None;
        SelectedClassJobIds.Clear();
    }

    public void ToggleClassJob(uint classJobId)
    {
        ClassFilterMode = ClassFilterMode.Selected;
        if (SelectedClassJobIds.Contains(classJobId))
            SelectedClassJobIds.Remove(classJobId);
        else
            SelectedClassJobIds.Add(classJobId);
    }

    public void SetRoleGroup(IEnumerable<uint> classJobIds, bool selected)
    {
        ClassFilterMode = ClassFilterMode.Selected;
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

    public void ClearClassJobSelection() => SetClassFilterAll();

    public bool IsRoleGroupFullySelected(IEnumerable<uint> classJobIds)
    {
        var ids = classJobIds.ToList();
        return ids.Count > 0 && ids.All(IsClassJobSelected);
    }
}
