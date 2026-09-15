using System;
using DPSPanel.Common.Configs;
using Terraria.UI;

namespace DPSPanel.UI;

public sealed class MainContainer : UIElement
{
    public bool dragging;
    private Vector2 dragOffset;
    public ToggleButton toggleButton;
    public MainPanel panel;
    public bool panelVisible = true;

    public MainContainer()
    {
        SetPadding(0);
        MaxHeight.Set(float.MaxValue, 0);
        MaxWidth.Set(float.MaxValue, 0);
        Width.Set(SizeHelper.GetWidthFromConfig(), 0);
        Height.Set(PanelLayout.Height(0, PanelLayout.HeaderHeight), 0);
        Vector2 position = PanelPositionJsonHelper.ReadPanelPosition();
        HAlign = Math.Clamp(position.X, 0, 1);
        // Anchor the top edge. VAlign would move every row when the panel grows.
        Top.Set(0, Math.Clamp(position.Y, 0, 1));
        panel = new MainPanel();
        Append(panel);
        toggleButton = new ToggleButton();
        Append(toggleButton);
    }

    public override void LeftMouseDown(UIMouseEvent evt)
    {
        base.LeftMouseDown(evt);
        if (!Config.Conf.C.MakePanelDraggable || !ContainsPoint(evt.MousePosition))
            return;
        dragging = true;
        var dims = GetDimensions();
        var parent = Parent.GetInnerDimensions();
        dragOffset = evt.MousePosition - new Vector2(dims.X, dims.Y);
        HAlign = 0;
        Left.Set(dims.X - parent.X, 0);
        Top.Set(dims.Y - parent.Y, 0);
        panel.HidePopup();
        Main.LocalPlayer.mouseInterface = true;
    }

    public override void LeftMouseUp(UIMouseEvent evt)
    {
        base.LeftMouseUp(evt);
        if (!dragging)
            return;
        dragging = false;
        var parent = Parent.GetInnerDimensions();
        float horizontal = Left.Pixels / Math.Max(1, parent.Width - Width.Pixels);
        float vertical = Top.Pixels / Math.Max(1, parent.Height);
        HAlign = Math.Clamp(horizontal, 0, 1);
        Left.Set(0, 0);
        Top.Set(0, Math.Clamp(vertical, 0, 1));
        PanelPositionJsonHelper.WritePanelPosition(new Vector2(HAlign, Top.Percent));
        Recalculate();
    }

    public override void Update(GameTime gameTime)
    {
        if (Parent == null)
            return;
        if (dragging)
        {
            var parent = Parent.GetInnerDimensions();
            Vector2 mouse = Main.MouseScreen / Main.UIScale;
            Left.Set(Math.Clamp(mouse.X - dragOffset.X - parent.X, 0, Math.Max(0, parent.Width - Width.Pixels)), 0);
            Top.Set(Math.Clamp(mouse.Y - dragOffset.Y - parent.Y, 0, Math.Max(0, parent.Height - Height.Pixels)), 0);
            Recalculate();
            Main.LocalPlayer.mouseInterface = true;
        }
        base.Update(gameTime);
    }

    public override bool ContainsPoint(Vector2 point)
    {
        if (!panel.HideWhenInventoryOpen && !Main.playerInventory)
            return false;
        return panelVisible ? base.ContainsPoint(point) : toggleButton.ContainsPoint(point);
    }

    public void TogglePanel()
    {
        panelVisible = !panelVisible;
        panel.HidePopup();
        if (panelVisible)
        {
            Append(panel);
            Append(toggleButton);
        }
        else
            panel.Remove();
    }
}
