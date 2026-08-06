using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Lumina.Excel.Sheets;

namespace Dnc.Util;

public enum PfRoleGroup
{
    Tank,
    Healer,
    MeleeDps,
    PhysicalRangedDps,
    MagicalRangedDps,
    Free,
}

public readonly record struct ClassJobEntry(uint RowId, string Abbreviation, uint IconId);

public sealed class PfRoleRow
{
    public PfRoleGroup Group { get; init; }
    public string Label { get; init; } = string.Empty;
    public Vector4 LabelColor { get; init; }
    public Vector4 BackgroundColor { get; init; }
    public IReadOnlyList<ClassJobEntry> Jobs { get; init; } = [];

    public IEnumerable<uint> AllIds => Jobs.Select(j => j.RowId);
}

public static class ClassJobRegistry
{
    private static readonly Vector4 TankLabelColor = new(0.85f, 0.92f, 1.0f, 1.0f);
    private static readonly Vector4 HealerLabelColor = new(0.85f, 1.0f, 0.88f, 1.0f);
    private static readonly Vector4 DpsLabelColor = new(1.0f, 0.85f, 0.85f, 1.0f);

    // FFXIV PF role slot colors: tank blue, healer green, DPS red.
    private static readonly Vector4 TankBgColor = new(0.20f, 0.40f, 0.72f, 1.0f);
    private static readonly Vector4 HealerBgColor = new(0.18f, 0.55f, 0.28f, 1.0f);
    private static readonly Vector4 DpsBgColor = new(0.65f, 0.18f, 0.18f, 1.0f);

    private static readonly (PfRoleGroup Group, string Label, Vector4 LabelColor, Vector4 BgColor, uint[] Jobs)[] PfLayout =
    [
        (PfRoleGroup.Tank, "Tank", TankLabelColor, TankBgColor, [19, 21, 32, 37]),
        (PfRoleGroup.Healer, "Healer", HealerLabelColor, HealerBgColor, [24, 28, 33, 40]),
        (PfRoleGroup.MeleeDps, "Melee DPS", DpsLabelColor, DpsBgColor, [20, 22, 30, 34, 39, 41]),
        (PfRoleGroup.PhysicalRangedDps, "Physical Ranged DPS", DpsLabelColor, DpsBgColor, [23, 31, 38]),
        (PfRoleGroup.MagicalRangedDps, "Magical Ranged DPS", DpsLabelColor, DpsBgColor, [25, 27, 35, 42]),
    ];

    private static IReadOnlyList<PfRoleRow>? rows;
    private static Dictionary<uint, PfRoleGroup>? rowToGroup;

    public static IReadOnlyList<PfRoleRow> Rows => rows ??= BuildRows();

    public static PfRoleGroup? GetRoleGroup(uint classJobRowId)
    {
        rowToGroup ??= BuildRowToGroupMap();
        return rowToGroup.TryGetValue(classJobRowId, out var group) ? group : null;
    }

    public static uint GetClassJobIconId(uint rowId)
        => rowId is >= 1 and <= 42 ? 62000u + rowId : 0u;

    public static PfRoleGroup? GetRoleFromSlotFlags(ulong slotFlags)
    {
        if (slotFlags == 0)
            return null;

        PfRoleGroup? matched = null;
        var groupsFound = new HashSet<PfRoleGroup>();
        var jobBits = 0;

        for (uint jobId = 1; jobId <= 42; jobId++)
        {
            if ((slotFlags & (1ul << (int)jobId)) == 0)
                continue;

            jobBits++;
            var group = GetRoleGroup(jobId);
            if (group == null)
                continue;

            groupsFound.Add(group.Value);
            matched = group;
        }

        if (groupsFound.Count == 0)
            return null;

        if (groupsFound.Count > 1 || jobBits >= 20)
            return PfRoleGroup.Free;

        return matched;
    }

    public static uint GetRolePlaceholderIconId(PfRoleGroup? role)
    {
        return role switch
        {
            PfRoleGroup.Tank => GetClassJobIconId(19),
            PfRoleGroup.Healer => GetClassJobIconId(24),
            PfRoleGroup.MeleeDps => GetClassJobIconId(20),
            PfRoleGroup.PhysicalRangedDps => GetClassJobIconId(23),
            PfRoleGroup.MagicalRangedDps => GetClassJobIconId(25),
            PfRoleGroup.Free => 62001u,
            _ => 62001u,
        };
    }

    public static void Initialize()
    {
        rowToGroup = null;
        rows = BuildRows();
    }

    private static IReadOnlyList<PfRoleRow> BuildRows()
    {
        var sheet = Service.DataManager.GetExcelSheet<ClassJob>();
        var byId = sheet.ToDictionary(cj => cj.RowId);

        return PfLayout
            .Select(layout => new PfRoleRow
            {
                Group = layout.Group,
                Label = layout.Label,
                LabelColor = layout.LabelColor,
                BackgroundColor = layout.BgColor,
                Jobs = ResolveEntries(layout.Jobs, byId),
            })
            .ToList();
    }

    private static Dictionary<uint, PfRoleGroup> BuildRowToGroupMap()
    {
        var map = new Dictionary<uint, PfRoleGroup>();
        foreach (var row in Rows)
        {
            foreach (var id in row.AllIds)
                map[id] = row.Group;
        }

        return map;
    }

    private static List<ClassJobEntry> ResolveEntries(uint[] rowIds, Dictionary<uint, ClassJob> byId)
    {
        var entries = new List<ClassJobEntry>(rowIds.Length);
        foreach (var rowId in rowIds)
        {
            if (!byId.TryGetValue(rowId, out var classJob))
            {
                Service.PluginLog.Warning($"ClassJob row {rowId} missing from Lumina sheet; skipping PF layout entry.");
                continue;
            }

            entries.Add(ToEntry(classJob));
        }

        return entries;
    }

    private static ClassJobEntry ToEntry(ClassJob classJob)
        => new(classJob.RowId, classJob.Abbreviation.ToString(), GetClassJobIconId(classJob.RowId));
}
