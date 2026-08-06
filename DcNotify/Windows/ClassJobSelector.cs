using System.Collections.Generic;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Utility.Raii;
using Dnc.Util;

namespace Dnc.Windows;

public static class ClassJobSelector
{
    private const float IconSize = 32f;
    private const float RoleColumnWidth = 150f;

    public static void Draw(Configuration configuration)
    {
        if (configuration.SelectedClassJobIds.Count == 0)
            ImGui.TextDisabled("Notify all classes (none selected). Select classes to filter join notifications.");
        else
            ImGui.Text($"Filtering {configuration.SelectedClassJobIds.Count} class(es). Party full (8/8) always notifies.");

        ImGui.Spacing();

        using (var table = ImRaii.Table("ClassJobSelectorTable", 3,
                   ImGuiTableFlags.BordersInnerV | ImGuiTableFlags.SizingFixedFit))
        {
            if (!table)
                return;

            ImGui.TableSetupColumn("Role", ImGuiTableColumnFlags.WidthFixed, RoleColumnWidth);
            ImGui.TableSetupColumn("JOB", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("CLASS", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableHeadersRow();

            foreach (var row in ClassJobRegistry.Rows)
            {
                if (row.Jobs.Count == 0 && row.Classes.Count == 0)
                    continue;

                ImGui.TableNextRow();

                ImGui.TableNextColumn();
                DrawRoleHeader(configuration, row);

                ImGui.TableNextColumn();
                DrawClassJobIcons(configuration, row.Jobs);

                ImGui.TableNextColumn();
                DrawClassJobIcons(configuration, row.Classes);
            }
        }

        ImGui.Spacing();

        if (ImGui.Button("Clear selection"))
            configuration.ClearClassJobSelection();
    }

    private static void DrawRoleHeader(Configuration configuration, PfRoleRow row)
    {
        var allSelected = configuration.IsRoleGroupFullySelected(row.AllIds);
        var label = allSelected ? $"{row.Label} *" : row.Label;

        ImGui.PushStyleColor(ImGuiCol.Text, row.LabelColor);
        if (ImGui.Selectable(label, allSelected, ImGuiSelectableFlags.None, new Vector2(RoleColumnWidth - 8f, IconSize)))
            configuration.SetRoleGroup(row.AllIds, !allSelected);

        ImGui.PopStyleColor();
    }

    private static void DrawClassJobIcons(Configuration configuration, IReadOnlyList<ClassJobEntry> entries)
    {
        foreach (var entry in entries)
        {
            DrawClassJobIcon(configuration, entry);
            ImGui.SameLine(0f, 4f);
        }
    }

    private static void DrawClassJobIcon(Configuration configuration, ClassJobEntry entry)
    {
        if (entry.IconId == 0)
        {
            if (ImGui.Button($"{entry.Abbreviation}##missing-{entry.RowId}", new Vector2(IconSize, IconSize)))
                configuration.ToggleClassJob(entry.RowId);

            return;
        }

        var texture = Service.TextureProvider.GetFromGameIcon(new GameIconLookup(entry.IconId));
        if (!texture.TryGetWrap(out var wrap, out _))
        {
            if (ImGui.Button($"{entry.Abbreviation}##loading-{entry.RowId}", new Vector2(IconSize, IconSize)))
                configuration.ToggleClassJob(entry.RowId);

            return;
        }

        var isFiltering = configuration.SelectedClassJobIds.Count > 0;
        var isSelected = configuration.IsClassJobSelected(entry.RowId);
        var tint = isFiltering
            ? (isSelected ? Vector4.One : new Vector4(0.45f, 0.45f, 0.45f, 0.65f))
            : new Vector4(0.85f, 0.85f, 0.85f, 1.0f);

        ImGui.PushID((int)entry.RowId);
        ImGui.Image(wrap.Handle, new Vector2(IconSize, IconSize), Vector2.Zero, Vector2.One, tint);

        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.TextUnformatted(entry.Abbreviation);
            ImGui.EndTooltip();
        }

        ImGui.SetCursorScreenPos(ImGui.GetItemRectMin());
        if (ImGui.InvisibleButton("##classjob-btn", new Vector2(IconSize, IconSize)))
            configuration.ToggleClassJob(entry.RowId);

        if (isFiltering && isSelected)
        {
            var min = ImGui.GetItemRectMin();
            var max = ImGui.GetItemRectMax();
            var drawList = ImGui.GetWindowDrawList();
            drawList.AddRect(min, max, ImGui.ColorConvertFloat4ToU32(new Vector4(0.95f, 0.78f, 0.25f, 1.0f)), 0f, ImDrawFlags.None, 2f);
        }

        ImGui.PopID();
    }
}
