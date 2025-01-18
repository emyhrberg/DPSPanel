using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.UI;
using Terraria.GameContent.UI.Elements;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;

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
        private string weaponName;           // Weapon name

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

        public void UpdateSlider(int highestDamage, string _weaponName, int weaponDamage, Color newColor, int _itemId, string _itemType)
        {
            percentage = (int)((weaponDamage / (float)highestDamage) * 100);
            fillColor = newColor;
            textElement.SetText($"{_weaponName} ({weaponDamage})");
            itemId = _itemId;
            itemType = _itemType;
            weaponName = _weaponName;
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
            // Load texture (by default, zenith)
            Texture2D texture = TextureAssets.Item[ItemID.Zenith].Value;

            // Scale and position
            float scale = 0.75f;
            CalculatedStyle dims = GetDimensions();
            Vector2 pos = new(dims.X, dims.Y);

            // Check Item or Projectile
            if (itemType == "Item")
            {
                texture = TextureAssets.Item[itemId].Value;
            }
            else if (itemType == "Projectile")
            {
                texture = TextureAssets.Projectile[itemId].Value;
            }

            // Check size of texture
            // get WxH
            int w = texture.Width;
            int h = texture.Height;
            ModContent.GetInstance<DPSPanel>().Logger.Info($"[{weaponName}] {itemType} ID: {itemId} WxH: {w}x{h}");

            texture = ResizeWeirdTextures(texture);

            // Draw texture
            if (texture != null)
            {
                spriteBatch.Draw(texture, pos, null, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
            }
        }

        private static readonly Dictionary<int, int> projectileToItemMap = new()
        {
            // Map projectile ID 933 (e.g., Zenith projectile) to item ID 4956 (Zenith item)
            {933, 4956}, // zenith
            {409, 2622} // typhoon
        };

        private Texture2D ResizeWeirdTextures(Texture2D originalTexture)
        {
            // If the source is a projectile and there is a mapping,
            // return the texture of the corresponding item.
            if (itemType == "Projectile" && projectileToItemMap.TryGetValue(itemId, out int mappedItemId))
            {
                return TextureAssets.Item[mappedItemId].Value;
            }
            return originalTexture;
        }

    }
}
