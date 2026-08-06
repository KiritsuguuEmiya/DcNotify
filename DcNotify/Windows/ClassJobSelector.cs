using System;
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
        switch (configuration.ClassFilterMode)
        {
            case ClassFilterMode.All:
                ImGui.TextDisabled("ALL — notify on every join.");
                break;
            case ClassFilterMode.None:
                ImGui.TextDisabled("NONE — notify only when party is full (8/8).");
                break;
            case ClassFilterMode.Selected:
                ImGui.Text($"Filtering {configuration.SelectedClassJobIds.Count} job(s). Party full (8/8) always notifies.");
                break;
        }

        ImGui.Spacing();
        DrawPresetOptions(configuration);
        ImGui.Spacing();

        using (var table = ImRaii.Table("ClassJobSelectorTable", 2,
                   ImGuiTableFlags.BordersInnerV | ImGuiTableFlags.SizingFixedFit))
        {
            if (!table)
                return;

            ImGui.TableSetupColumn("Role", ImGuiTableColumnFlags.WidthFixed, RoleColumnWidth);
            ImGui.TableSetupColumn("Jobs", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableHeadersRow();

            foreach (var row in ClassJobRegistry.Rows)
            {
                if (row.Jobs.Count == 0)
                    continue;

                ImGui.TableNextRow();

                ImGui.TableNextColumn();
                DrawRoleHeader(configuration, row);

                ImGui.TableNextColumn();
                DrawClassJobIcons(configuration, row);
            }
        }
    }

    private static void DrawPresetOptions(Configuration configuration)
    {
        var buttonWidth = (RoleColumnWidth - 12f) / 2f;
        var buttonSize = new Vector2(buttonWidth, IconSize);
        var notifyAll = configuration.ClassFilterMode == ClassFilterMode.All;
        var notifyNone = configuration.ClassFilterMode == ClassFilterMode.None;

        DrawPresetOption("ALL", notifyAll, buttonSize, new Vector4(0.35f, 0.35f, 0.35f, 1f), () => configuration.SetClassFilterAll());
        ImGui.SameLine(0f, 8f);
        DrawPresetOption("NONE", notifyNone, buttonSize, new Vector4(0.28f, 0.28f, 0.28f, 1f), () => configuration.SetClassFilterNone());
    }

    private static void DrawPresetOption(string label, bool selected, Vector2 size, Vector4 bgColor, Action onClick)
    {
        var pos = ImGui.GetCursorScreenPos();
        var drawList = ImGui.GetWindowDrawList();
        drawList.AddRectFilled(pos, pos + size, ToColorU32(bgColor), 4f);

        ImGui.PushStyleColor(ImGuiCol.Header, Vector4.Zero);
        ImGui.PushStyleColor(ImGuiCol.HeaderHovered, new Vector4(1f, 1f, 1f, 0.12f));
        ImGui.PushStyleColor(ImGuiCol.HeaderActive, new Vector4(1f, 1f, 1f, 0.2f));
        ImGui.PushStyleColor(ImGuiCol.Text, selected ? Vector4.One : new Vector4(0.7f, 0.7f, 0.7f, 1f));
        if (ImGui.Selectable(label, selected, ImGuiSelectableFlags.None, size))
            onClick();

        ImGui.PopStyleColor(4);
    }

    private static void DrawRoleHeader(Configuration configuration, PfRoleRow row)
    {
        var allSelected = configuration.IsRoleGroupFullySelected(row.AllIds);
        var label = allSelected ? $"{row.Label} *" : row.Label;
        var size = new Vector2(RoleColumnWidth - 8f, IconSize);
        var pos = ImGui.GetCursorScreenPos();

        var drawList = ImGui.GetWindowDrawList();
        drawList.AddRectFilled(pos, pos + size, ToColorU32(row.BackgroundColor), 4f);

        ImGui.PushStyleColor(ImGuiCol.Header, Vector4.Zero);
        ImGui.PushStyleColor(ImGuiCol.HeaderHovered, new Vector4(1f, 1f, 1f, 0.12f));
        ImGui.PushStyleColor(ImGuiCol.HeaderActive, new Vector4(1f, 1f, 1f, 0.2f));
        ImGui.PushStyleColor(ImGuiCol.Text, row.LabelColor);
        if (ImGui.Selectable(label, allSelected, ImGuiSelectableFlags.None, size))
            configuration.SetRoleGroup(row.AllIds, !allSelected);

        ImGui.PopStyleColor(4);
    }

    private static void DrawClassJobIcons(Configuration configuration, PfRoleRow row)
    {
        foreach (var entry in row.Jobs)
        {
            DrawClassJobIcon(configuration, row, entry);
            ImGui.SameLine(0f, 4f);
        }
    }

    private static void DrawClassJobIcon(Configuration configuration, PfRoleRow row, ClassJobEntry entry)
    {
        var slotMin = ImGui.GetCursorScreenPos();
        var slotSize = new Vector2(IconSize, IconSize);
        var slotMax = slotMin + slotSize;
        var drawList = ImGui.GetWindowDrawList();
        drawList.AddRectFilled(slotMin, slotMax, ToColorU32(row.BackgroundColor), 4f);

        if (entry.IconId == 0)
        {
            ImGui.SetCursorScreenPos(slotMin);
            if (ImGui.Button($"{entry.Abbreviation}##missing-{entry.RowId}", slotSize))
                configuration.ToggleClassJob(entry.RowId);

            return;
        }

        var texture = Service.TextureProvider.GetFromGameIcon(new GameIconLookup(entry.IconId));
        if (!texture.TryGetWrap(out var wrap, out _))
        {
            ImGui.SetCursorScreenPos(slotMin);
            if (ImGui.Button($"{entry.Abbreviation}##loading-{entry.RowId}", slotSize))
                configuration.ToggleClassJob(entry.RowId);

            return;
        }

        var isFiltering = configuration.ClassFilterMode == ClassFilterMode.Selected;
        var isSelected = configuration.IsClassJobSelected(entry.RowId);
        var tint = isFiltering
            ? (isSelected ? Vector4.One : new Vector4(0.55f, 0.55f, 0.55f, 0.75f))
            : Vector4.One;

        ImGui.PushID((int)entry.RowId);
        ImGui.SetCursorScreenPos(slotMin);
        ImGui.Image(wrap.Handle, slotSize, Vector2.Zero, Vector2.One, tint);

        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.TextUnformatted(entry.Abbreviation);
            ImGui.EndTooltip();
        }

        ImGui.SetCursorScreenPos(slotMin);
        if (ImGui.InvisibleButton("##classjob-btn", slotSize))
            configuration.ToggleClassJob(entry.RowId);

        if (isFiltering && isSelected)
        {
            drawList.AddRect(slotMin, slotMax, ToColorU32(new Vector4(0.95f, 0.78f, 0.25f, 1f)), 4f, ImDrawFlags.None, 2f);
        }

        ImGui.PopID();
    }

    private static uint ToColorU32(Vector4 color)
        => ImGui.ColorConvertFloat4ToU32(color);
}
