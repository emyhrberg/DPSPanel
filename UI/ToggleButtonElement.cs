using DPSPanel.Configs;
using DPSPanel.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
            Top.Set(0, 0f);
            Left.Set(0, 0f);

            ninjaTexture = LoadResources.NinjaTexture.Value;
            ninjaHighlightedTexture = LoadResources.NinjaHighlightedTexture.Value;
        }

        protected override void DrawSelf(SpriteBatch sb)
        {
            SimpleConfig c = ModContent.GetInstance<SimpleConfig>();

            if (c != null && !c.ShowToggleButton)
                return;

            base.DrawSelf(sb);

            // get the dimensions of the element
            const float scale = 0.7f; // Scale factor
            CalculatedStyle dims = GetDimensions();
            Vector2 pos = new(dims.X + (dims.Width - ninjaTexture.Width * scale) / 2f,
                            dims.Y + (dims.Height - ninjaTexture.Height * scale) / 2f);

            if (IsMouseHovering)
            {
                sb.Draw(ninjaHighlightedTexture, pos, null, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
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
                parentContainer?.TogglePanel();
            }
        }
        #endregion
    }
}
