using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;

namespace Dnc.Util;

public static class PartyCompositionRenderer
{
    private const int SlotSize = 40;
    private const int SlotPadding = 4;
    private const int BorderWidth = 2;

    public static async Task<byte[]?> RenderAsync(PartySlot[] slots)
    {
        if (slots.Length == 0)
            return null;

        var visibleSlots = 0;
        foreach (var slot in slots)
        {
            if (slot.Kind != PartySlotKind.Omitted)
                visibleSlots++;
        }

        if (visibleSlots == 0)
            return null;

        var width = slots.Length * (SlotSize + SlotPadding) - SlotPadding;
        var height = SlotSize;

        try
        {
            using var bitmap = new Bitmap(width, height);
            using var graphics = Graphics.FromImage(bitmap);
            graphics.Clear(Color.FromArgb(255, 45, 45, 48));
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

            var x = 0;
            foreach (var slot in slots)
            {
                if (slot.Kind == PartySlotKind.Omitted)
                {
                    x += SlotSize + SlotPadding;
                    continue;
                }

                var borderColor = GetRoleBorderColor(slot.Role);
                var rect = new Rectangle(x, 0, SlotSize, SlotSize);

                using (var borderBrush = new SolidBrush(borderColor))
                    graphics.FillRoundedRectangle(borderBrush, rect, 6);

                var inner = Rectangle.Inflate(rect, -BorderWidth, -BorderWidth);
                using (var innerBrush = new SolidBrush(Color.FromArgb(255, 30, 30, 32)))
                    graphics.FillRoundedRectangle(innerBrush, inner, 4);

                var iconRect = Rectangle.Inflate(inner, -3, -3);
                var icon = await GameIconLoader.LoadAsync(slot.IconId);
                if (icon != null)
                {
                    using (icon)
                    {
                        var opacity = slot.Kind == PartySlotKind.Empty ? 0.55f : 1f;
                        DrawIcon(graphics, icon, iconRect, opacity);
                    }
                }

                if (slot.Kind == PartySlotKind.Filled)
                    DrawCheckmark(graphics, rect);

                x += SlotSize + SlotPadding;
            }

            using var stream = new MemoryStream();
            bitmap.Save(stream, ImageFormat.Png);
            return stream.ToArray();
        }
        catch (Exception ex)
        {
            Service.PluginLog.Warning(ex, "Failed to render party composition image.");
            return null;
        }
    }

    private static void DrawIcon(Graphics graphics, Bitmap icon, Rectangle dest, float opacity)
    {
        if (opacity >= 0.99f)
        {
            graphics.DrawImage(icon, dest);
            return;
        }

        var colorMatrix = new ColorMatrix
        {
            Matrix33 = opacity,
        };

        using var attributes = new ImageAttributes();
        attributes.SetColorMatrix(colorMatrix);
        graphics.DrawImage(icon, dest, 0, 0, icon.Width, icon.Height, GraphicsUnit.Pixel, attributes);
    }

    private static void DrawCheckmark(Graphics graphics, Rectangle slotRect)
    {
        const int size = 12;
        var checkRect = new Rectangle(slotRect.Right - size - 2, slotRect.Top + 2, size, size);

        using var fill = new SolidBrush(Color.FromArgb(255, 240, 190, 40));
        graphics.FillEllipse(fill, checkRect);

        using var pen = new Pen(Color.FromArgb(255, 35, 35, 35), 2f)
        {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round,
        };

        var cx = checkRect.X + checkRect.Width / 2f;
        var cy = checkRect.Y + checkRect.Height / 2f;
        graphics.DrawLines(pen,
        [
            new PointF(cx - 3f, cy),
            new PointF(cx - 1f, cy + 2.5f),
            new PointF(cx + 3.5f, cy - 2.5f),
        ]);
    }

    private static Color GetRoleBorderColor(PfRoleGroup? role)
    {
        return role switch
        {
            PfRoleGroup.Tank => Color.FromArgb(255, 70, 115, 195),
            PfRoleGroup.Healer => Color.FromArgb(255, 70, 175, 90),
            PfRoleGroup.MeleeDps or PfRoleGroup.PhysicalRangedDps or PfRoleGroup.MagicalRangedDps
                => Color.FromArgb(255, 195, 70, 70),
            PfRoleGroup.Free => Color.FromArgb(255, 140, 140, 140),
            _ => Color.FromArgb(255, 100, 100, 100),
        };
    }
}

internal static class GraphicsExtensions
{
    public static void FillRoundedRectangle(this Graphics graphics, Brush brush, Rectangle bounds, int radius)
    {
        using var path = CreateRoundedRectangle(bounds, radius);
        graphics.FillPath(brush, path);
    }

    private static GraphicsPath CreateRoundedRectangle(Rectangle bounds, int radius)
    {
        var path = new GraphicsPath();
        var diameter = radius * 2;
        var arc = new Rectangle(bounds.Location, new Size(diameter, diameter));

        path.AddArc(arc, 180, 90);
        arc.X = bounds.Right - diameter;
        path.AddArc(arc, 270, 90);
        arc.Y = bounds.Bottom - diameter;
        path.AddArc(arc, 0, 90);
        arc.X = bounds.Left;
        path.AddArc(arc, 90, 90);
        path.CloseFigure();
        return path;
    }
}
