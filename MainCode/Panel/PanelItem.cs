using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace DPSPanel.MainCode.Panel
{
    public class CustomUITextPanel<T> : UITextPanel<T>
    {
        public float FillPercentage { get; set; } = 1.0f; // Default to fully filled
        public Color FillColor { get; set; } = Color.White; // Default fill color

        public CustomUITextPanel(T text, float textScale = 1.0f, bool large = false) : base(text, textScale, large)
        {
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            base.DrawSelf(spriteBatch);

            // Calculate the dimensions of the fill rectangle
            CalculatedStyle dimensions = GetInnerDimensions();
            int fillWidth = (int)(dimensions.Width * FillPercentage);

            // Draw the fill rectangle
            Rectangle fillRect = new Rectangle((int)dimensions.X, (int)dimensions.Y, fillWidth, (int)dimensions.Height);
            spriteBatch.Draw(TextureAssets.MagicPixel.Value, fillRect, FillColor);
        }
    }
}
