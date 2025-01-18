using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.UI;
using Terraria.GameContent.UI.Elements;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace DPSPanel.MainCode.Panel
{
    public class PanelSlider : UIElement
    {
        private readonly Asset<Texture2D> sliderEmpty; // Background slider texture
        private readonly Asset<Texture2D> sliderFull;  // Foreground fill texture
        private readonly UIText textElement;          // Text element for slider label

        private Texture2D weaponIcon;          // Icon for the weapon
        private Color fillColor;             // Color for the fill
        private int percentage;              // Progress percentage (0-100)

        // Weapon icon
        private int itemId;
        private string itemType;

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

        public void UpdateSlider(int highestDamage, string weaponName, int weaponDamage, Color newColor, int _itemId, string _itemType)
        {
            percentage = (int)((weaponDamage / (float)highestDamage) * 100);
            fillColor = newColor;
            textElement.SetText($"{weaponName} ({weaponDamage})");
            itemId = _itemId;
            itemType = _itemType;
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            base.DrawSelf(spriteBatch);
            DrawSlider(spriteBatch);
            DrawIcon(spriteBatch);
        }

        private void DrawSlider(SpriteBatch spriteBatch)
        {
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

        private void DrawIcon(SpriteBatch spriteBatch)
        {
            CalculatedStyle dims = GetDimensions();
            // Load texture (by default, zenith)
            Texture2D texture = TextureAssets.Item[ItemID.Zenith].Value;
            float scale = 0.75f;
            Vector2 pos = new(dims.X, dims.Y);

            // Check Item or Projectile
            if (itemType == "Item")
            {
                ModContent.GetInstance<DPSPanel>().Logger.Info("Item ID: " + itemId);
                texture = TextureAssets.Item[itemId].Value;
            }
            else if (itemType == "Projectile")
            {
                ModContent.GetInstance<DPSPanel>().Logger.Info("Projectile ID: " + itemId);
                texture = TextureAssets.Projectile[itemId].Value;
            }

            // Draw texture
            if (texture != null)
            {
                spriteBatch.Draw(texture, pos, null, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
            }
        }
    }
}
