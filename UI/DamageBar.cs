using System;
using DPSPanel.Common.Configs;
using ReLogic.Content;
using Terraria.GameContent;
using Terraria.UI;
using Terraria.UI.Chat;

namespace DPSPanel.UI;

public abstract class DamageBar : UIElement
{
    private Asset<Texture2D> outline;
    private Asset<Texture2D> fill;
    protected string Label = "";
    protected long Damage;
    protected int Percentage;
    protected Color FillColor;
    protected virtual bool HasIcon => true;
    protected abstract float RowHeight { get; }
    protected virtual float TextStart => DPSPanelLayout.WeaponIconLeft + DPSPanelLayout.WeaponIconSize + DPSPanelLayout.IconTextGap;

    protected DamageBar()
    {
        SetPadding(0);
        ApplyAppearance();
    }

    public void ApplyAppearance()
    {
        Width.Set(0, 1f);
        float height = Math.Max(1, RowHeight);
        Height.Set(height, 0);
        MinHeight.Set(height, 0);
        MaxHeight.Set(height, 0);
        var config = Config.Conf.C;
        outline = BarAssets.Outline(config.Theme, config.Width);
        fill = BarAssets.Fill(config.Width);
    }

    protected void SetData(string label, long damage, int percentage, Color color)
    {
        Label = label;
        Damage = damage;
        Percentage = Math.Clamp(percentage, 0, 100);
        FillColor = color;
    }

    protected override void DrawSelf(SpriteBatch sb)
    {
        var config = Config.Conf.C;
        var dims = GetDimensions();
        var bounds = dims.ToRectangle();
        int fillWidth = (int)(bounds.Width * Percentage / 100f);
        if (fillWidth > 0)
            sb.Draw(fill.Value, new Rectangle(bounds.X, bounds.Y, fillWidth, bounds.Height),
                new Rectangle(0, 0, (int)(fill.Width() * Percentage / 100f), fill.Height()), FillColor);
        sb.Draw(outline.Value, bounds, LayoutColors.From(DPSPanelLayout.BarOutline));
        DrawIcon(sb, dims);

        string text = config.DamageDisplay == "Percent" ? $"{Label} ({Percentage}%)" : $"{Label} ({Damage})";
        float left = HasIcon ? Math.Max(DPSPanelLayout.BarTextPaddingLeft, TextStart) : DPSPanelLayout.BarTextPaddingLeft;
        float available = Math.Max(1f, dims.Width - left - DPSPanelLayout.BarTextPaddingRight);
        Vector2 measured = FontAssets.MouseText.Value.MeasureString(text);
        float scale = Math.Min(Math.Max(0.01f, DPSPanelLayout.BarTextScale), available / Math.Max(1f, measured.X));
        var position = new Vector2(dims.X + left + (available - measured.X * scale) / 2 + DPSPanelLayout.BarTextOffsetX,
            dims.Y + (dims.Height - measured.Y * scale) / 2 + DPSPanelLayout.BarTextOffsetY);
        ChatManager.DrawColorCodedStringWithShadow(sb, FontAssets.MouseText.Value, text, position,
            LayoutColors.From(DPSPanelLayout.TextColor), 0f, Vector2.Zero, new Vector2(scale));
    }

    protected abstract void DrawIcon(SpriteBatch sb, CalculatedStyle dimensions);
}

internal static class LayoutColors
{
    public static Color From(LayoutColor c) => new(c.R, c.G, c.B, c.A);
}

internal static class BarAssets
{
    public static Asset<Texture2D> Outline(string theme, string width)
    {
        string name = (theme ?? "Default") + (width is "Medium" or "Large" ? "Large" : "");
        return typeof(Ass).GetField(name)?.GetValue(null) as Asset<Texture2D> ??
            (width is "Medium" or "Large" ? Ass.DefaultLarge : Ass.Default);
    }

    public static Asset<Texture2D> Fill(string width) => width is "Medium" or "Large" ? Ass.BarFillLarge : Ass.BarFill;
}
