using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.UI;
using Terraria.GameContent.UI.Elements;
using Terraria;

namespace DPSPanel.MainCode.Panel
{
    public class PanelSlider : UIElement
    {
        private readonly Asset<Texture2D> sliderEmpty; // Background slider texture
        private readonly Asset<Texture2D> sliderFull;  // Foreground fill texture
        private readonly UIText textElement;          // Text element for slider label
        private Color fillColor;             // Color for the fill
        private int percentage;              // Progress percentage (0-100)

        public PanelSlider(Asset<Texture2D> sliderEmpty, Asset<Texture2D> sliderFull)
        {
            this.sliderEmpty = sliderEmpty;
            this.sliderFull = sliderFull;
            this.percentage = 0; // initial progress

            // Create the text element centered on the slider.
            textElement = new UIText("", 0.8f)
            {
                HAlign = 0.5f,
                VAlign = 0.5f,
            };
            Append(textElement);
        }

        public void UpdateSlider(int highestDamage, string weaponName, int weaponDamage, Color newColor)
        {
            percentage = (int)((weaponDamage / (float)highestDamage) * 100);
            fillColor = newColor;
            textElement.SetText($"{weaponName} ({weaponDamage})");
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            base.DrawSelf(spriteBatch);

            CalculatedStyle dims = GetDimensions();
            Vector2 pos = new Vector2(dims.X, dims.Y);
            Rectangle rect = new Rectangle((int)pos.X, (int)pos.Y, (int)dims.Width, (int)dims.Height);

            // Draw background.
            spriteBatch.Draw(sliderEmpty.Value, rect, Color.DarkGray);

            // Draw filled portion based on percentage.
            int fillWidth = (int)(dims.Width * (percentage / 100f));
            if (fillWidth > 0)
            {
                Rectangle sourceRect = new Rectangle(0, 0, (int)(sliderFull.Width() * (percentage / 100f)), sliderFull.Height());
                spriteBatch.Draw(sliderFull.Value, new Rectangle((int)pos.X, (int)pos.Y, fillWidth, (int)dims.Height), sourceRect, fillColor);
            }
        }
    }
}
