using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace BetterDPS.UI.DPS
{
    public class DPSPanel : UIPanel
    {
        // Variables for dragging the panel
        private Vector2 offset;
        private bool dragging;
        private bool clickStartedInsidePanel;

        // For resizing items
        private const float ItemHeight = 20f; // Height of each item (label)
        private const float Padding = 10f; // Padding for the panel
        private const float InitialOffset = 30f; // Initial offset for the first item
        private int itemCount = 0; // Track the number of items added

        // constructor
        public DPSPanel()
        {
            // Set the panel size and background color
            Width.Set(300f, 0f);
            Height.Set(150f, 0f);
            Left.Set(400f, 0f); // distance from the left edge
            Top.Set(200f, 0f); // distance from the top edge
            BackgroundColor = new Color(73, 94, 171); // Light blue background

            // adjust padding for adding child elements
            PaddingTop = 10;
            PaddingLeft = 10;
            PaddingRight = 10;
            PaddingBottom = 10;
        }

        public void AddItem(string text)
        {
            // Create a new label for the item
            var label = new UIText(text);
            // Position
            float top = InitialOffset + itemCount * ItemHeight;
            label.Left.Set(Padding, 0f);
            label.Top.Set(top, 0f);
            // Append the label to the panel
            Append(label);

            // Increase item count and check if the panel needs resizing
            itemCount++;
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
