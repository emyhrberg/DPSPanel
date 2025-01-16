using System.Collections.Generic;
using System.Linq;
using DPSPanel.Content.DPS;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;

namespace DPSPanel.UI.DPS
{
    public class DPSPanel : UIPanel
    {
        // Variables for dragging the panel
        private Vector2 offset;
        private bool dragging;
        private bool clickStartedInsidePanel;

        // Panel items
        public Dictionary<string, List<UIText>> bossEntries = new();
        private int itemCount = 0; // Track the number of items added

        public DPSPanel()
        {
            // Set the panel size and background color
            Width.Set(300f, 0f);
            Height.Set(50f, 0f);
            Left.Set(400f, 0f); // distance from the left edge
            Top.Set(200f, 0f); // distance from the top edge
            BackgroundColor = new Color(73, 94, 171); // Light blue background

            // adjust padding for adding child elements
            SetPadding(10);

            // add initial text
            AddItem("DPS Panel (Type /help)");
        }

        public void AddItem(string text)
        {
            var label = new UIText(text);
            label.Top.Set(itemCount * 20f, 0f); // Position the new item below existing ones
            Append(label);

            itemCount++;
            RecalculateHeight();
        }

        public void AddItemForBoss(string bossKey, string text)
        {
            if (!bossEntries.ContainsKey(bossKey))
            {
                bossEntries[bossKey] = new List<UIText>();
            }

            var label = new UIText(text);
            label.Top.Set(itemCount * 20f, 0f); // Position below the current items
            Append(label);

            bossEntries[bossKey].Add(label);
            itemCount++;
            RecalculateHeight(); 
        }

        public void ClearItemsForBoss(string bossKey)
        {
            if (bossEntries.ContainsKey(bossKey))
            {
                foreach (var entry in bossEntries[bossKey])
                {
                    RemoveChild(entry);
                    itemCount--; 
                }
                bossEntries[bossKey].Clear();
                RecalculateHeight(); 
            }
        }

        public void ClearAllItems()
        {
            foreach (var boss in bossEntries)
            {
                foreach (var entry in boss.Value)
                {
                    RemoveChild(entry);
                }
            }

            bossEntries.Clear();
            RemoveAllChildren();
            itemCount = 0; // Reset item count
            AddItem("DPS Panel (Type /help)");
            // TODO dont remove the close button lol
        }

        private void RecalculateHeight()
        {
            // Calculate the total height based on the number of rows (boss headers and damage rows)
            int totalItems = bossEntries.Values.Sum(entries => entries.Count) + bossEntries.Count; // Include headers
            float newHeight = 50f + totalItems * 20f; // Base height + 20 pixels per item
            Height.Set(newHeight, 0f);
            Recalculate();
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
