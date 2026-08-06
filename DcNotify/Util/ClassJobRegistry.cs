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
    public IReadOnlyList<ClassJobEntry> Jobs { get; init; } = [];
    public IReadOnlyList<ClassJobEntry> Classes { get; init; } = [];

    public IEnumerable<uint> AllIds => Jobs.Select(j => j.RowId).Concat(Classes.Select(c => c.RowId));
}

public static class ClassJobRegistry
{
    private static readonly Vector4 TankColor = new(0.45f, 0.65f, 1.0f, 1.0f);
    private static readonly Vector4 HealerColor = new(0.45f, 0.95f, 0.55f, 1.0f);
    private static readonly Vector4 DpsColor = new(0.95f, 0.45f, 0.45f, 1.0f);
    private static readonly Vector4 FreeColor = new(0.75f, 0.75f, 0.75f, 1.0f);

    // Party Finder layout: ordered row IDs per role, matching in-game JOB / CLASS columns.
    private static readonly (PfRoleGroup Group, string Label, Vector4 Color, uint[] Jobs, uint[] Classes)[] PfLayout =
    [
        (PfRoleGroup.Tank, "Tank", TankColor, [19, 21, 32, 37], [1, 3]),
        (PfRoleGroup.Healer, "Healer", HealerColor, [24, 28, 33, 40], [6]),
        (PfRoleGroup.MeleeDps, "Melee DPS", DpsColor, [20, 22, 30, 34, 39, 41], [2, 4, 29]),
        (PfRoleGroup.PhysicalRangedDps, "Physical Ranged DPS", DpsColor, [23, 31, 38], [5]),
        (PfRoleGroup.MagicalRangedDps, "Magical Ranged DPS", DpsColor, [25, 27, 35, 42], [7, 26]),
        (PfRoleGroup.Free, "Free", FreeColor, [0], []),
    ];

    private static IReadOnlyList<PfRoleRow>? rows;
    private static Dictionary<uint, PfRoleGroup>? rowToGroup;

    public static IReadOnlyList<PfRoleRow> Rows => rows ??= BuildRows();

    public static PfRoleGroup? GetRoleGroup(uint classJobRowId)
    {
        rowToGroup ??= BuildRowToGroupMap();
        return rowToGroup.TryGetValue(classJobRowId, out var group) ? group : null;
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
                LabelColor = layout.Color,
                Jobs = ResolveEntries(layout.Jobs, byId),
                Classes = ResolveEntries(layout.Classes, byId),
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

    private static uint GetClassJobIconId(uint rowId)
        => rowId is >= 1 and <= 42 ? 62000u + rowId : 0u;
}
