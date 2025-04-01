using DPSPanel.Common.Configs;
using DPSPanel.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace DPSPanel.UI

{
    public class ToggleButton : UIElement
    {
        private Texture2D img;
        private Texture2D imgHighlighted;
        private Vector2 clickStartPosition; // Start position of a mouse click
        private bool isDragging;

        public ToggleButton()
        {
            Width.Set(30f, 0f);
            Height.Set(30f, 0f);
            Top.Set(4f, 0f);
            Left.Set(4f, 0f);

            img = Assets.ToggleButton.Value;
            imgHighlighted = Assets.ToggleButtonHighlighted.Value;
        }

        protected override void DrawSelf(SpriteBatch sb)
        {
            Config c = ModContent.GetInstance<Config>();
            if (!Main.playerInventory && !c.ShowOnlyWhenInventoryOpen)
                return;

            base.DrawSelf(sb);

            // Get the dimensions of the element
            const float scale = 0.8f; // Scale factor
            CalculatedStyle dims = GetDimensions();
            Vector2 pos = new(dims.X + (dims.Width - img.Width * scale) / 2f,
                              dims.Y + (dims.Height - img.Height * scale) / 2f);


            // Draw either the button or highlighted button based on hover state
            if (IsMouseHovering && c.ShowHighlightButtonWhenHovering)
            {
                sb.Draw(imgHighlighted, pos, null, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
                Main.instance.MouseText("Left click to toggle panel \nRight click to only show when inventory is open");
            }
            else
            {
                sb.Draw(img, pos, null, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
            }
        }

        #region RightClick
        public override void RightMouseDown(UIMouseEvent evt)
        {
            base.RightMouseDown(evt);

            // on right click we toggle the config setting to only show in inventory.
            ModContent.GetInstance<Config>().ShowOnlyWhenInventoryOpen = !ModContent.GetInstance<Config>().ShowOnlyWhenInventoryOpen;

            string text = ModContent.GetInstance<Config>().ShowOnlyWhenInventoryOpen ? "Always show DPSPanel" : "Show DPSPanel only when inventory is open";
            Main.NewText(text, Color.White);
        }
        #endregion

        #region ClickDragHotFix
        public override void LeftMouseDown(UIMouseEvent evt)
        {
            base.LeftMouseDown(evt);

            // Record the start position when the mouse is pressed
            clickStartPosition = evt.MousePosition;
            isDragging = false; // Reset dragging flag
        }

        public override void LeftMouseUp(UIMouseEvent evt)
        {
            base.LeftMouseUp(evt);

            // Check if the mouse moved significantly during the click
            if (Vector2.Distance(clickStartPosition, evt.MousePosition) > 5f) // Threshold for drag
            {
                isDragging = true;
            }

            // Only toggle the panel if it was not a drag
            if (!isDragging)
            {
                var parentContainer = Parent as MainContainer;

                // Ensure the parent container is not null before accessing it
                if (parentContainer == null)
                {
                    Log.Warn("Parent container is null. TogglePanel() cannot be called.");
                    return;
                }

                parentContainer.TogglePanel();
            }
        }
        #endregion
    }
}
