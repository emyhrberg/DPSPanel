using System;
using DPSPanel.Common.Configs;
using Microsoft.Xna.Framework.Input;
using Terraria.UI;

namespace DPSPanel.UI;

public sealed class MainContainer : UIElement
{
    private readonly PanelDragState drag = new();
    private bool canMovePanel;
    public bool dragging => drag.IsDragging && canMovePanel;
    public bool IsPointerCaptured => drag.PointerDown;
    public bool SuppressToggleClick => drag.SuppressClick;
    public ToggleButton toggleButton;
    public MainPanel panel;
    public bool panelVisible = true;
    private bool IsAvailable => panel.HideWhenInventoryOpen || Main.playerInventory;

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
        if (Parent == null || !Main.hasFocus || !ContainsPoint(evt.MousePosition))
            return;
        var dims = GetDimensions();
        drag.Begin(evt.MousePosition.X, evt.MousePosition.Y, dims.X, dims.Y);
        canMovePanel = Config.Conf.C.MakePanelDraggable &&
            !Main.keyState.IsKeyDown(Keys.LeftControl) && !Main.keyState.IsKeyDown(Keys.RightControl) &&
            !Main.keyState.IsKeyDown(Keys.LeftAlt) && !Main.keyState.IsKeyDown(Keys.RightAlt);
        // A press alone must not change alignment, position, popup state or the saved position.
        Main.LocalPlayer.mouseInterface = true;
    }

    private void MovePointer(Vector2 mouse)
    {
        var position = drag.Move(mouse.X, mouse.Y, DPSPanelLayout.DragThreshold);
        if (!canMovePanel || position is not { } point)
            return;
        var parent = Parent.GetInnerDimensions();
        HAlign = 0;
        Left.Set(Math.Clamp(point.X - parent.X, 0, Math.Max(0, parent.Width - Width.Pixels)), 0);
        Top.Set(Math.Clamp(point.Y - parent.Y, 0, Math.Max(0, parent.Height - Height.Pixels)), 0);
        panel.HidePopup();
        Recalculate();
        panel.ApplyLayout();
    }

    public void ClampToScreen()
    {
        if (Parent == null || IsPointerCaptured)
            return;
        var screen = Parent.GetInnerDimensions();
        float margin = Math.Max(0, DPSPanelLayout.ScreenMargin);
        float minimumHeight = DPSPanelLayout.HeaderHeight + DPSPanelLayout.HeaderGap +
            DPSPanelLayout.PanelPaddingTop + DPSPanelLayout.PanelPaddingBottom +
            Math.Max(1, Math.Max(DPSPanelLayout.PlayerBarHeight, DPSPanelLayout.WeaponBarHeight));
        float y = Math.Clamp(GetDimensions().Y - screen.Y, margin, Math.Max(margin, screen.Height - minimumHeight - margin));
        Top.Set(0, y / Math.Max(1, screen.Height));
        Recalculate();
    }

    public override void LeftMouseUp(UIMouseEvent evt)
    {
        base.LeftMouseUp(evt);
        if (!IsPointerCaptured)
            return;
        if (Main.hasFocus && IsAvailable)
            MovePointer(evt.MousePosition);
        FinishPointer(!Main.hasFocus || !IsAvailable);
    }

    private void FinishPointer(bool cancelled)
    {
        bool moved = dragging;
        drag.End(cancelled);
        if (!moved || Parent == null)
            return;
        var parent = Parent.GetInnerDimensions();
        float horizontal = Left.Pixels / Math.Max(1, parent.Width - Width.Pixels);
        float vertical = Top.Pixels / Math.Max(1, parent.Height);
        HAlign = Math.Clamp(horizontal, 0, 1);
        Left.Set(0, 0);
        Top.Set(0, Math.Clamp(vertical, 0, 1));
        PanelPositionJsonHelper.WritePanelPosition(new Vector2(HAlign, Top.Percent));
        Recalculate();
        ModContent.GetInstance<MainSystem>().RequestRebuild();
    }

    public void CancelPointer()
    {
        if (IsPointerCaptured)
            FinishPointer(true);
    }

    public override void Update(GameTime gameTime)
    {
        if (Parent == null)
            return;
        if (IsPointerCaptured)
        {
            if (!Main.hasFocus || !IsAvailable || canMovePanel && !Config.Conf.C.MakePanelDraggable)
                CancelPointer();
            else
            {
                // UserInterface supplied the same coordinates to LeftMouseDown. Do not scale again.
                MovePointer(MainSystem.PointerPosition);
                Main.LocalPlayer.mouseInterface = true;
                // A row can be removed during a drag, preventing its mouse-up from bubbling here.
                if (!Main.mouseLeft)
                    FinishPointer(false);
            }
        }
        if (IsAvailable && ContainsPoint(MainSystem.PointerPosition))
            Main.LocalPlayer.mouseInterface = true;
        base.Update(gameTime);
    }

    public override bool ContainsPoint(Vector2 point)
    {
        if (!IsAvailable)
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
            panel.ApplyLayout();
        }
        else
            panel.Remove();
    }
}
