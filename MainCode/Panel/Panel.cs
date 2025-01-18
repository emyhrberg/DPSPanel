using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace DPSPanel.MainCode.Panel
{
    public class Panel : UIPanel
    {
        // Variables for dragging the panel
        private Vector2 offset;
        private bool dragging;
        private bool clickStartedInsidePanel;
        private readonly bool IS_DRAGGABLE = false; // Debug option: Set to true to enable dragging

        // Panel items
        private readonly float padding;
        private readonly float headerHeight = 16f;
        private float currentYOffset = 0;
        private const float ItemHeight = 40f;

        public Panel(float padding)
        {
            this.padding = padding;
            SetPadding(padding);
        }

        // Define predefined colors for each row
        private Color[] colorsToUse =
        [
            new Color(85, 115, 240), // Cool Blue
            new Color(255, 140, 0),  // Vivid Orange
            new Color(60, 180, 170), // Teal
            new Color(255, 215, 70),  // Gold
            new Color(240, 85, 85)   // Warm Red
        ];
        private int colorIndex;

        // Panel item structure
        public struct DPSPanelItem
        {
            public string Name;
            public float DPS;
            public Color Color;
            public int progress; // 0-100 damage dealt
        }

        /* -------------------------------------------------------------
         * Panel content
         * -------------------------------------------------------------
         */

        public void AddPanelHeader()
        {
            // Header text
            UIText header = new("Damage dealt", 1.0f);
            header.HAlign = 0.5f;
            Append(header);
            currentYOffset += headerHeight + padding*2;
            ResizePanelHeight();
        }

        public void AddPanelItem(string itemName)
        {
            // Cycle through colors
            Color selectedColor = colorsToUse[colorIndex++ % colorsToUse.Length];

            // Create uitextpanel
            UITextPanel<string> text = new(itemName, 1.0f, large: false)
            {
                Width = new StyleDimension(0, 1.0f),
                Height = new StyleDimension(ItemHeight, 0f),
                Top = new StyleDimension(currentYOffset, 0f),
                HAlign = 1.0f, // Horizontal align to left of panel
                BackgroundColor = selectedColor
            };

            Append(text);
            currentYOffset += ItemHeight;
            ResizePanelHeight();
        }

        private void ResizePanelHeight()
        {
            Height.Set(currentYOffset, 0f);
            Recalculate();
        }

        /* -------------------------------------------------------------
         * Dragging functionality
         * -------------------------------------------------------------
         */

        public override void LeftMouseDown(UIMouseEvent evt)
        {

            if (!IS_DRAGGABLE) return; 

            // When the mouse is pressed, start dragging the panel
            base.LeftMouseDown(evt);
            if (evt.Target == this)
            {
                DragStart(evt);
                clickStartedInsidePanel = true;

                // Prevent other UI elements from interacting
                Main.LocalPlayer.mouseInterface = true;
            }
        }

        public override void LeftMouseUp(UIMouseEvent evt)
        {
            if (!IS_DRAGGABLE) return;

            // When the mouse is released, stop dragging the panel
            base.LeftMouseUp(evt);
            if (clickStartedInsidePanel)
            {
                DragEnd(evt);
            }
            clickStartedInsidePanel = false; // default to false
        }

        private void DragStart(UIMouseEvent evt)
        {
            if (!IS_DRAGGABLE) return;

            // Start dragging the panel
            offset = new Vector2(evt.MousePosition.X - Left.Pixels, evt.MousePosition.Y - Top.Pixels);
            dragging = true;
        }

        private void DragEnd(UIMouseEvent evt)
        {
            if (!IS_DRAGGABLE) return;

            // Stop dragging the panel
            dragging = false;
            Vector2 endMousePosition = evt.MousePosition;
            Left.Set(endMousePosition.X - offset.X, 0f);
            Top.Set(endMousePosition.Y - offset.Y, 0f);
            Recalculate();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (!IS_DRAGGABLE) return;

            // If the mouse is inside the panel, set the mouse interface to true to prevent other UI elements from interacting
            if (dragging || clickStartedInsidePanel && ContainsPoint(Main.MouseScreen))
            {
                Main.LocalPlayer.mouseInterface = true;
            }

            // Drag the panel
            if (dragging)
            {
                Left.Set(Main.mouseX - offset.X, 0f);
                Top.Set(Main.mouseY - offset.Y, 0f);
                Recalculate();
            }

            // Keep the panel within bounds
            var parentSpace = Parent.GetDimensions().ToRectangle();
            if (!GetDimensions().ToRectangle().Intersects(parentSpace))
            {
                Left.Pixels = Utils.Clamp(Left.Pixels, 0, parentSpace.Right - Width.Pixels);
                Top.Pixels = Utils.Clamp(Top.Pixels, 0, parentSpace.Bottom - Height.Pixels);
                Recalculate();
            }
        }
    }
}
