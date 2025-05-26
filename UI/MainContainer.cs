using System;
using System.Linq;
using DPSPanel.Common.Configs;
using DPSPanel.Helpers;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;
using static DPSPanel.Common.Configs.Config;

namespace DPSPanel.UI
{
    // the entire container for the boss damage system:
    // 1) Panel
    // 2) Toggle button
    // 3) Damage bars
    // 4) Weapon bars
    // So this is the entirety of what we drag around the screen.
    public class MainContainer : UIElement
    {
        // dragging
        public bool dragging;
        public bool clickStartInsidePanel;
        private Vector2 offset;

        // elements
        public ToggleButton toggleButton;
        public MainPanel panel;
        public bool panelVisible = true;

        public MainContainer()
        {
            // Convert from string to float using the dictionary to set the width.
            float width = SizeHelper.GetWidthFromConfig();
            Width.Set(width, 0f);
            Height.Set(40, 0);

            // VAlign = 0.07f; // 7% down from top
            // HAlign = 0.5f;  // center horizontally

            // Read the file and set the position
            Vector2 position = PanelPositionJsonHelper.ReadPanelPosition();
            VAlign = position.Y; // Vertical alignment
            HAlign = position.X; // Horizontal alignment
            Log.Info("MainContainer: Read position from JSON x and y: " + position.X + ", " + position.Y);

            // 1) Create the panel
            panel = new MainPanel();
            Append(panel);
            Recalculate();

            // 2) Create the button
            toggleButton = new ToggleButton();
            // Append it last, so it draws on top
            Append(toggleButton);

            int invalidBossIdEqualsNoBossIconShowing = -1;
            panel.SetBossTitle("DPSPanel", invalidBossIdEqualsNoBossIconShowing, invalidBossIdEqualsNoBossIconShowing);
        }

        #region Dragging
        public override void LeftMouseDown(UIMouseEvent evt)
        {
            base.LeftMouseDown(evt);

            if (!Conf.C.MakePanelDraggable)
            {
                return;
            }

            if (ContainsPoint(evt.MousePosition))
            {
                clickStartInsidePanel = true;
                dragging = true;
                Main.LocalPlayer.mouseInterface = true; // Prevents other UI elements from being used
                DragStart(evt);
            }
            else
            {
                clickStartInsidePanel = false;
            }
        }

        private void DragEnd(UIMouseEvent evt)
        {
            Vector2 endMousePosition = evt.MousePosition;
            // dragging = false; // This is handled by LeftMouseUp

            Left.Set(endMousePosition.X - offset.X, 0f);
            Top.Set(endMousePosition.Y - offset.Y, 0f);

            // Recalculate to apply the new Left.Pixels and Top.Pixels
            Recalculate();

            // Now, convert the final pixel position to HAlign and VAlign
            if (Parent != null)
            {
                CalculatedStyle parentDims = Parent.GetDimensions();
                if (parentDims.Width > 0 && parentDims.Height > 0) // Avoid division by zero
                {
                    // The new HAlign is the original HAlign plus the ratio of Left.Pixels to parent width.
                    // The new VAlign is the original VAlign plus the ratio of Top.Pixels to parent height.
                    float newHAlign = this.HAlign + (this.Left.Pixels / parentDims.Width);
                    float newVAlign = this.VAlign + (this.Top.Pixels / parentDims.Height);

                    // Clamp values to be within [0, 1] for alignment
                    newHAlign = MathHelper.Clamp(newHAlign, 0f, 1f);
                    newVAlign = MathHelper.Clamp(newVAlign, 0f, 1f);

                    this.HAlign = newHAlign;
                    this.VAlign = newVAlign;

                    // Since HAlign and VAlign now define the position, reset pixel offsets
                    this.Left.Set(0f, 0f);
                    this.Top.Set(0f, 0f);

                    // Recalculate again with new HAlign/VAlign and zeroed pixel offsets
                    // This ensures the UI element stays visually in the same place after conversion.
                    Recalculate();

                    // Save the new HAlign and VAlign
                    PanelPositionJsonHelper.WritePanelPosition(new Vector2(this.HAlign, this.VAlign));
                    Log.Info($"DragEnd: Saved new HAlign: {this.HAlign}, VAlign: {this.VAlign}");
                }
            }
            Main.LocalPlayer.mouseInterface = false;
        }

        public override void LeftMouseUp(UIMouseEvent evt)
        {
            base.LeftMouseUp(evt);
            if (dragging)
            {
                DragEnd(evt); // DragEnd now handles the saving
                dragging = false;
                clickStartInsidePanel = false;
                // Main.LocalPlayer.mouseInterface = false; // Moved to DragEnd
            }
        }

        private void DragStart(UIMouseEvent evt)
        {
            offset = new Vector2(evt.MousePosition.X - Left.Pixels, evt.MousePosition.Y - Top.Pixels);
            // offset = evt.MousePosition - new Vector2(Left.Pixels, Top.Pixels);
            dragging = true;
            clickStartInsidePanel = true;
            Main.LocalPlayer.mouseInterface = true; // Prevents other UI elements from being used
        }


        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (dragging)
            {
                Left.Set(Main.mouseX - offset.X, 0f);
                Top.Set(Main.mouseY - offset.Y, 0f);
                // REMOVE THE FOLLOWING LINE:
                // PanelPositionJsonHelper.WritePanelPosition(new Vector2(HAlign, VAlign));
                // Log.Info("Pos: " + HAlign.ToString()); // This would also show the old HAlign
                Recalculate();
            }

            var parentSpace = Parent.GetDimensions().ToRectangle();
            if (!GetDimensions().ToRectangle().Intersects(parentSpace))
            {
                Left.Pixels = Utils.Clamp(Left.Pixels, 0, parentSpace.Right - Width.Pixels);
                Top.Pixels = Utils.Clamp(Top.Pixels, 0, parentSpace.Bottom - Height.Pixels);
                Recalculate();
            }

            if (dragging) // This check is redundant here as mouseInterface is set in DragStart/LeftMouseDown
            {
                Main.LocalPlayer.mouseInterface = true;
            }
        }
        #endregion

        public override bool ContainsPoint(Vector2 point)
        {
            // If panel is hidden, only consider the toggle button clickable/draggable
            if (!panelVisible)
            {
                return toggleButton.ContainsPoint(point);
            }
            // Otherwise, use the default logic
            return base.ContainsPoint(point);
        }

        #region Show/Hide Panel
        public void TogglePanel()
        {
            panelVisible = !panelVisible;

            if (panelVisible)
            {
                if (!Children.Contains(panel))
                {
                    // SHOW PANEL
                    Append(panel);

                    // remove & re-append the icon so it draws on top
                    toggleButton.Remove();
                    Append(toggleButton);
                }
            }
            else
            {
                if (Children.Contains(panel))
                {
                    panel.Remove();
                }
            }
        }
        #endregion
    }
}
