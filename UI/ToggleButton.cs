using System;
using DPSPanel.Common.Configs;
using Microsoft.Xna.Framework.Input;
using Terraria.UI;

namespace DPSPanel.UI;

public sealed class ToggleButton : UIElement
{
    private readonly Texture2D img;
    private readonly Texture2D imgHighlighted;
    private bool pressed;

    public ToggleButton()
    {
        ApplyLayout();
        img = Ass.ToggleButton.Value;
        imgHighlighted = Ass.ToggleButtonHighlighted.Value;
    }

    public void ApplyLayout()
    {
        Width.Set(Math.Max(1, DPSPanelLayout.ToggleWidth), 0);
        Height.Set(Math.Max(1, DPSPanelLayout.ToggleHeight), 0);
        MaxHeight.Set(float.MaxValue, 0);
        Top.Set(DPSPanelLayout.ToggleTop, 0);
        Left.Set(DPSPanelLayout.ToggleLeft, 0);
        Recalculate();
    }

    protected override void DrawSelf(SpriteBatch sb)
    {
        if (Parent is not MainContainer container || !Main.playerInventory && !container.panel.HideWhenInventoryOpen)
            return;
        base.DrawSelf(sb);
        float scale = Math.Max(0.01f, DPSPanelLayout.ToggleIconScale);
        CalculatedStyle dims = GetDimensions();
        Vector2 pos = new(dims.X + (dims.Width - img.Width * scale) / 2f,
                          dims.Y + (dims.Height - img.Height * scale) / 2f);
        sb.Draw(IsMouseHovering ? imgHighlighted : img, pos, null, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        if (IsMouseHovering && Config.Conf.C.ShowTooltips)
            Main.instance.MouseText("Left click to toggle panel \nRight click to only show when inventory is open\nAlt click to open config\nCtrl click to clear panel");
    }

    public override void RightMouseDown(UIMouseEvent evt)
    {
        base.RightMouseDown(evt);
        if (Parent is not MainContainer container)
            return;
        container.panel.HideWhenInventoryOpen = !container.panel.HideWhenInventoryOpen;
        Main.NewText(container.panel.HideWhenInventoryOpen ? "Always show DPSPanel" : "Show DPSPanel only when inventory is open", Color.White);
    }

    public override void LeftMouseDown(UIMouseEvent evt)
    {
        pressed = true;
        base.LeftMouseDown(evt);
    }

    public override void LeftMouseUp(UIMouseEvent evt)
    {
        bool wasPressed = pressed;
        pressed = false;
        // The parent finishes the captured gesture first and keeps its drag/cancel verdict.
        base.LeftMouseUp(evt);
        if (!wasPressed || !Main.hasFocus || Parent is not MainContainer container ||
            container.SuppressToggleClick || !ContainsPoint(evt.MousePosition))
            return;
        if (Main.keyState.IsKeyDown(Keys.LeftControl) || Main.keyState.IsKeyDown(Keys.RightControl))
        {
            ModContent.GetInstance<MainSystem>().ClearDisplay();
            return;
        }
        if (Main.keyState.IsKeyDown(Keys.LeftAlt) || Main.keyState.IsKeyDown(Keys.RightAlt))
        {
            Config.Conf.C.Open();
            return;
        }
        container.TogglePanel();
    }
}
