using DPSPanel.Configs;
using DPSPanel.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace DPSPanel.UI

{
    public class ToggleButtonElement : UIElement
    {
        private Texture2D ninjaTexture;
        private Texture2D ninjaHighlightedTexture;
        private Vector2 clickStartPosition; // Start position of a mouse click
        private bool isDragging;

        public ToggleButtonElement()
        {
            Width.Set(30f, 0f);
            Height.Set(30f, 0f);
            Top.Set(4f, 0f);
            Left.Set(4f, 0f);

            ninjaTexture = LoadAssets.NinjaTexture.Value;
            ninjaHighlightedTexture = LoadAssets.NinjaHighlightedTexture.Value;
        }

        protected override void DrawSelf(SpriteBatch sb)
        {
            Config c = ModContent.GetInstance<Config>();
            if (!Main.playerInventory && !c.AlwaysShowButton)
                return;

            base.DrawSelf(sb);

            // Get the dimensions of the element
            const float scale = 0.65f; // Scale factor
            CalculatedStyle dims = GetDimensions();
            Vector2 pos = new(dims.X + (dims.Width - ninjaTexture.Width * scale) / 2f,
                              dims.Y + (dims.Height - ninjaTexture.Height * scale) / 2f);

            var parentContainer = Parent as BossContainerElement;

            // Check hover behavior based on config
            bool isHoverValid = c.DisableValidHoverHighlight
                ? IsMouseHovering
                : IsMouseHovering && (!Main.mouseLeft || parentContainer.clickStartInsidePanel);

            if (isHoverValid)
            {
                sb.Draw(ninjaHighlightedTexture, pos, null, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
                if (parentContainer.panelVisible)
                    Main.instance.MouseText("Click to hide panel");
                else
                    Main.instance.MouseText("Click to show panel");
            }
            else
            {
                sb.Draw(ninjaTexture, pos, null, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
            }
        }

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
                var parentContainer = Parent as BossContainerElement;

                // Ensure the parent container is not null before accessing it
                if (parentContainer == null)
                {
                    ModContent.GetInstance<DPSPanel>().Logger.Warn("Parent container is null. TogglePanel() cannot be called.");
                    return;
                }

                parentContainer.TogglePanel();
            }
        }
        #endregion
    }
}
