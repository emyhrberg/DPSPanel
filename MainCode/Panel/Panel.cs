using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
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
        private float currentYOffset = 0;
        private const float ItemHeight = 16f;

        public Panel()
        {
            // Empty constructor for no reason
        }

        // Define predefined colors for each row
        private Color[] colorsToUse =
        [
            new Color(240, 85, 85),   // Warm Red
            new Color(85, 115, 240), // Cool Blue
            new Color(255, 140, 0),  // Vivid Orange
            new Color(60, 180, 170), // Teal
            new Color(255, 215, 70)  // Gold
        ];

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
            UIText headerText = new("Damage dealt", 1.0f);
            headerText.Top.Set(currentYOffset, 0f); // = 0
            headerText.Left.Set(0, 0f);
            headerText.SetPadding(0);
            Append(headerText);
            ResizePanelHeight();
        }

        public void AddPanelItem(string itemName)
        {
            UIText text = new(itemName, 1.0f);
            text.Top.Set(currentYOffset, 0f);
            Append(text);

            // Adjust offset for the next item
            ResizePanelHeight();
        }

        private void ResizePanelHeight()
        {
            currentYOffset += ItemHeight;
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
