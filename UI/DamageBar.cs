using System;
using DPSPanel.Common.Configs;
using ReLogic.Content;
using Terraria.GameContent;
using Terraria.UI;
using Terraria.UI.Chat;

namespace DPSPanel.UI;

/// <summary>Common bar rendering keeps text, textures and sizing consistent.</summary>
public abstract class DamageBar : UIElement
{
    private Asset<Texture2D> outline;
    private Asset<Texture2D> fill;
    private string theme;
    private string width;
    protected string Label = "";
    protected long Damage;
    protected int Percentage;
    protected Color FillColor;
    protected virtual bool HasIcon => true;

    protected DamageBar()
    {
        Width.Set(0, 1f);
        Height.Set(PanelLayout.BarHeight, 0f);
        MinHeight.Set(PanelLayout.BarHeight, 0f);
        MaxHeight.Set(PanelLayout.BarHeight, 0f);
        SetPadding(0);
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
        if (outline == null || theme != config.Theme || width != config.Width)
        {
            theme = config.Theme;
            width = config.Width;
            outline = BarAssets.Outline(theme, width);
            fill = BarAssets.Fill(width);
        }

        var dims = GetDimensions();
        var bounds = dims.ToRectangle();
        int fillWidth = (int)(bounds.Width * Percentage / 100f);
        if (fillWidth > 0)
            sb.Draw(fill.Value, new Rectangle(bounds.X, bounds.Y, fillWidth, bounds.Height),
                new Rectangle(0, 0, (int)(fill.Width() * Percentage / 100f), fill.Height()), FillColor);
        sb.Draw(outline.Value, bounds, Color.DarkGray);
        DrawIcon(sb, dims);

        string text = config.DamageDisplay == "Percent" ? $"{Label} ({Percentage}%)" : $"{Label} ({Damage})";
        float left = HasIcon ? 38f : 7f;
        float available = Math.Max(1f, dims.Width - left - 7f);
        Vector2 measured = FontAssets.MouseText.Value.MeasureString(text);
        float scale = Math.Min(0.8f, available / Math.Max(1f, measured.X));
        var position = new Vector2(dims.X + left + (available - measured.X * scale) / 2,
            dims.Y + (dims.Height - measured.Y * scale) / 2);
        ChatManager.DrawColorCodedStringWithShadow(sb, FontAssets.MouseText.Value, text, position,
            Color.White, 0f, Vector2.Zero, new Vector2(scale));
    }

    protected abstract void DrawIcon(SpriteBatch sb, CalculatedStyle dimensions);
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
