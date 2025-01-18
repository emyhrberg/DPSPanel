using Microsoft.Xna.Framework.Graphics;
using Terraria.UI;
using Terraria.GameContent.UI.Elements;
using Microsoft.Xna.Framework;
using ReLogic.Content;

namespace DPSPanel.MainCode.Panel
{
    internal class PanelSlider : UIElement
    {
        private Asset<Texture2D> sliderTexture;

        public PanelSlider(Asset<Texture2D> asset)
        {
            sliderTexture = asset;
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            base.DrawSelf(spriteBatch);

            // Get pos and dimensions of self
            CalculatedStyle dimensions = GetDimensions();
            Vector2 position = new(dimensions.X, dimensions.Y);
            Vector2 size = new(dimensions.Width, dimensions.Height);

            // Draw slider
            Rectangle sliderRect = new((int)position.X, (int)position.Y, (int)size.X, (int)size.Y);
            spriteBatch.Draw(sliderTexture.Value, sliderRect, Color.White);
        }
    }
}
