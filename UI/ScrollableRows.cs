using System;
using Terraria.GameContent;
using Terraria.UI;

namespace DPSPanel.UI;

/// <summary>A clipped row area with wheel scrolling when a layout height limit is reached.</summary>
public sealed class ScrollableRows : UIElement
{
    public UIElement Content { get; } = new();
    public float ScrollOffset { get; private set; }
    private float contentHeight;
    private float viewportHeight;

    public ScrollableRows()
    {
        SetPadding(0);
        Width.Set(0, 1);
        MaxHeight.Set(float.MaxValue, 0);
        OverflowHidden = true;
        Content.SetPadding(0);
        Content.MaxHeight.Set(float.MaxValue, 0);
        Append(Content);
    }

    public void SetExtent(float content, float viewport)
    {
        contentHeight = content;
        viewportHeight = viewport;
        Height.Set(viewport, 0);
        Content.Height.Set(content, 0);
        Content.PaddingLeft = Math.Max(0, DPSPanelLayout.RowInsetLeft);
        Content.PaddingRight = Math.Max(0, DPSPanelLayout.RowInsetRight);
        float scrollbarSpace = content > viewport ? DPSPanelLayout.ScrollbarWidth + DPSPanelLayout.ScrollbarGap : 0;
        Content.Width.Set(-Math.Max(0, scrollbarSpace), 1);
        SetScroll(ScrollOffset);
    }

    public void SetScroll(float value)
    {
        ScrollOffset = PanelLayout.ClampScroll(value, contentHeight, viewportHeight);
        Content.Top.Set(-ScrollOffset, 0);
        Recalculate();
    }

    public override void ScrollWheel(UIScrollWheelEvent evt)
    {
        SetScroll(ScrollOffset - evt.ScrollWheelValue / 120f * Math.Max(1, DPSPanelLayout.ScrollStep));
        Main.LocalPlayer.mouseInterface = true;
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        bool visible = Parent switch
        {
            MainPanel panel => panel.IsDisplayed,
            PlayerDamagePanel popup => popup.IsVisible,
            _ => false
        };
        if (visible && ContainsPoint(MainSystem.PointerPosition))
        {
            Main.LocalPlayer.mouseInterface = true;
            Terraria.GameInput.PlayerInput.LockVanillaMouseScroll("DPSPanel");
        }
    }

    protected override void DrawSelf(SpriteBatch sb)
    {
        if (contentHeight <= viewportHeight || viewportHeight <= 0)
            return;
        var d = GetDimensions();
        float thumbHeight = Math.Min(d.Height, Math.Max(DPSPanelLayout.ScrollbarMinThumbHeight, d.Height * viewportHeight / contentHeight));
        float y = d.Y + (d.Height - thumbHeight) * ScrollOffset / (contentHeight - viewportHeight);
        var c = DPSPanelLayout.ScrollbarColor;
        sb.Draw(TextureAssets.MagicPixel.Value,
            new Rectangle((int)(d.X + d.Width - DPSPanelLayout.ScrollbarWidth), (int)y,
                (int)Math.Max(1, DPSPanelLayout.ScrollbarWidth), (int)thumbHeight),
            new Color(c.R, c.G, c.B, c.A));
    }
}
