using System;
using System.Linq;
using DPSPanel.Configs;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace DPSPanel.UI
{
    // the entire container for the boss damage system:
    // 1) Panel
    // 2) Toggle button
    // 3) Damage bars
    // 4) Weapon bars
    // So this is the entirety of what we drag around the screen.
    public class BossContainerElement : UIElement
    {
        // dragging
        public bool dragging;
        public bool clickStartInsidePanel;
        private Vector2 offset;

        // elements
        public ToggleButtonElement toggleButton;
        public Panel panel;
        public CustomButtonElement clearButton;
        public bool panelVisible = true;

        private const float MIN_WIDTH = 300f;  // Minimum container width
        private const float MIN_HEIGHT = 40f;  // Minimum container height

        public BossContainerElement()
        {
            // Container defaults
            Width.Set(MIN_WIDTH, 0f);
            Height.Set(MIN_HEIGHT, 0f);
            VAlign = 0.07f; // 7% down from top
            HAlign = 0.5f;  // center horizontally

            // 1) Create the panel
            panel = new Panel();
            Append(panel);

            // 2) Create the button
            toggleButton = new ToggleButtonElement();
            // Append it last, so it draws on top
            Append(toggleButton);

            // 3) Add custom buttons
            // AddCustomButtons();

            int invalidBossIdTemp = -1;
            panel.SetBossTitle("Boss Damage System", invalidBossIdTemp);
        }

        #region Custom Buttons
        private void AddCustomButtons()
        {
            // Config c = ModContent.GetInstance<Config>();
            // if (!c.ShowClearButton)
            //     return;

            // Clear Button
            // clearButton = new CustomButtonElement("Clear", "Clear all damage data", () =>
            // {
            //     panel.ClearPanelAndAllItems();
            //     panel.SetBossTitle("Boss Damage System", -1);
            //     Main.NewText("Damage data cleared.", Color.Green);
            // });
            // clearButton.HAlign = 1.0f; // Right-aligned
            // clearButton.Top.Set(0f, 0f); // Top-aligned
            // clearButton.Width.Set(10f, 0f); // Set width
            // Append(clearButton);

            // // Lock Draggable Button
            // // var lockButton = new CustomButtonElement("Lock", "Toggle dragging", () =>
            // {
            //     isDraggable = !isDraggable;
            //     Main.NewText($"Dragging {(isDraggable ? "enabled" : "disabled")}.", isDraggable ? Color.Green : Color.Red);
            // });
            // lockButton.Left.Set(115f, 0f); // Positioned next to Clear Button
            // lockButton.Top.Set(5f, 0f);
            // Append(lockButton);
        }
        #endregion

        #region Dragging
        public override void LeftMouseDown(UIMouseEvent evt)
        {
            base.LeftMouseDown(evt);
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

        public override void LeftMouseUp(UIMouseEvent evt)
        {
            base.LeftMouseUp(evt);
            if (dragging)
            {
                DragEnd(evt);
                dragging = false;
                clickStartInsidePanel = false;
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

        private void DragEnd(UIMouseEvent evt)
        {
            Vector2 endMousePosition = evt.MousePosition;
            dragging = false;

            Left.Set(endMousePosition.X - offset.X, 0f);
            Top.Set(endMousePosition.Y - offset.Y, 0f);

            Main.LocalPlayer.mouseInterface = false; // Allow other UI elements to be used

            Recalculate();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // This ensures we do not use other items while dragging
            // if (ContainsPoint(Main.MouseScreen))
            // Main.LocalPlayer.mouseInterface = true;

            // log state of clickstartinsidepanel
            // ModContent.GetInstance<DPSPanel>().Logger.Info($"clickStartInsidePanel: {clickStartInsidePanel}");

            if (dragging)
            {
                Left.Set(Main.mouseX - offset.X, 0f);
                Top.Set(Main.mouseY - offset.Y, 0f);
                Recalculate();
            }

            // Check if the container is out of bounds
            var parentSpace = Parent.GetDimensions().ToRectangle();
            if (!GetDimensions().ToRectangle().Intersects(parentSpace))
            {
                Left.Pixels = Utils.Clamp(Left.Pixels, 0, parentSpace.Right - Width.Pixels);
                Top.Pixels = Utils.Clamp(Top.Pixels, 0, parentSpace.Bottom - Height.Pixels);
                // Recalculate forces the UI system to do the positioning math again.
                Recalculate();
            }

            if (dragging)
            {
                Main.LocalPlayer.mouseInterface = true;
            }
        }
        #endregion

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

                    // Config c = ModContent.GetInstance<Config>();
                    // if (c.ShowClearButton && clearButton != null)
                    // Append(clearButton);
                }
            }
            else
            {
                if (Children.Contains(panel))
                {
                    panel.Remove();
                    clearButton?.Remove();
                    // Main.NewText("Panel hidden. In chat, you can type /dps <toggle> <item> <clear> ", Color.SteelBlue);
                }
            }
        }
        #endregion
    }
}
