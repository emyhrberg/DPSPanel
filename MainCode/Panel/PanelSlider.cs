using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace DPSPanel.MainCode.Panel
{
    public class PanelSlider : UIElement
    {
        private readonly Asset<Texture2D> sliderEmpty; // Background slider texture
        private readonly Asset<Texture2D> sliderFull;  // Foreground fill texture
        private readonly Color fillColor;             // Color for the fill
        private readonly int value;                   // Fill value (0-100)
        private readonly UIText textElement;          // Text element for slider label

        public PanelSlider(Asset<Texture2D> sliderEmpty, Asset<Texture2D> sliderFull, string text, Color color, int value)
        {
            this.sliderEmpty = sliderEmpty;
            this.sliderFull = sliderFull;
            this.fillColor = color;
            this.value = (int)MathHelper.Clamp(value, 0, 100); // Clamp value between 0 and 100

            // Create and text element
            textElement = new UIText(text, 0.8f)
            {
                HAlign = 0.5f, // Center horizontally
                VAlign = 0.5f, // Center vertically
            };

            Append(textElement);
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            base.DrawSelf(spriteBatch);

            // Get the dimensions of the element
            CalculatedStyle dimensions = GetDimensions();
            Vector2 position = new(dimensions.X, dimensions.Y);

            // Draw the slider empty texture (full width, no distortion)
            spriteBatch.Draw(
                sliderEmpty.Value,
                new Rectangle((int)position.X, (int)position.Y, (int)dimensions.Width, (int)dimensions.Height),
                Color.DarkGray // Tint for the empty slider
            );

            // Calculate the width of the filled portion based on value (0-100)
            int fillWidth = (int)(dimensions.Width * (value / 100f));

            if (fillWidth > 0) // Only draw if there's some fill
            {
                // Define the source rectangle for the SliderFull texture to avoid stretching
                Rectangle sourceRect = new Rectangle(0, 0, (int)(sliderFull.Width() * (value / 100f)), sliderFull.Height());

                // Draw the filled portion of the slider
                spriteBatch.Draw(
                    sliderFull.Value,
                    new Rectangle((int)position.X, (int)position.Y, fillWidth, (int)dimensions.Height),
                    sourceRect,
                    fillColor // Tint for the full slider
                );
            }
        }
    }
}
