using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;

namespace DPSPanel.MainCode.Panel
{
    public class DPSPanel : UIPanel
    {
        // Variables for dragging the panel
        private Vector2 offset;
        private bool dragging;
        private bool clickStartedInsidePanel;

        // Panel items
        private const float ItemHeight = 30f; // Height of each item (label)
        private const float Padding = 10f; // Padding for the panel
        private int itemCount = 0; // Track the number of items added
        private float currentYOffset = 10f; // Initial offset for the first row
        private readonly Dictionary<string, UIText> bossLabels = new();

        public DPSPanel()
        {
            // Set the panel size and background color
            Width.Set(300f, 0f);
            Height.Set(150f, 0f);
            Left.Set(400f, 0f); // distance from the left edge
            Top.Set(200f, 0f); // distance from the top edge
            BackgroundColor = new Color(73, 94, 171); // Light blue background

            // adjust padding for adding child elements
            SetPadding(10);

            // add initial text
            AddItem("DPS Panel");
        }

        public void AddItem(string text)
        {
            var label = new UIText(text)
            {
                Top = new StyleDimension(currentYOffset, 0f),
                Left = new StyleDimension(10f, 0f)
            };
            Append(label);

            currentYOffset += 20f; // Increment Y offset for the next item
            ResizeToFitItems();
        }

        private void ResizeToFitItems()
        {
            // Calculate the required height based on the number of items
            float requiredHeight = Padding * 2 + itemCount * ItemHeight;

            // Resize the panel if needed
            if (Height.Pixels < requiredHeight)
            {
                Height.Set(requiredHeight, 0f);
                Recalculate(); // Recalculate UI layout
            }
        }

        /*
         * DRAGGING FUNCTIONALITY
         */////////////////////////////////////////////////////////////

        public override void LeftMouseDown(UIMouseEvent evt)
        {
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
            // Start dragging the panel
            offset = new Vector2(evt.MousePosition.X - Left.Pixels, evt.MousePosition.Y - Top.Pixels);
            dragging = true;
        }

        private void DragEnd(UIMouseEvent evt)
        {
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
