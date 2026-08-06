using System.Collections.Generic;

namespace Dnc.Util;

public readonly record struct JoinSnapshot(PfRoleGroup? SlotRole, uint JoinJobId, uint JoinJobIconId);

public static class PfRoleTracker
{
    private static readonly Dictionary<ulong, JoinSnapshot> ByContentId = new();
    private static readonly Dictionary<string, JoinSnapshot> ByName = new();

    public static void RecordJoin(ulong contentId, string name, uint jobId, PfRoleGroup? slotRole)
    {
        var snapshot = new JoinSnapshot(slotRole, jobId, ClassJobRegistry.GetClassJobIconId(jobId));

        if (contentId != 0)
            ByContentId[contentId] = snapshot;

        if (!string.IsNullOrEmpty(name))
            ByName[name] = snapshot;
    }

    public static JoinSnapshot? Get(ulong contentId, string name)
    {
        if (contentId != 0 && ByContentId.TryGetValue(contentId, out var byId))
            return byId;

        if (!string.IsNullOrEmpty(name) && ByName.TryGetValue(name, out var byName))
            return byName;

        return null;
    }

    public static void Remove(ulong contentId, string name)
    {
        if (contentId != 0)
            ByContentId.Remove(contentId);

        if (!string.IsNullOrEmpty(name))
            ByName.Remove(name);
    }

    public static void Clear()
    {
        ByContentId.Clear();
        ByName.Clear();
    }
}
