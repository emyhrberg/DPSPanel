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
using System;

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
            CalculatedStyle dims = GetDimensions();
            Vector2 pos = new(dims.X, dims.Y);
            Rectangle rectangleSize = new(0, 0, 48, 48); 

            // Check Item or Projectile
            if (itemType == "Item")
            {
                texture = TextureAssets.Item[itemId].Value;
            }
            else if (itemType == "Projectile")
            {
                texture = TextureAssets.Projectile[itemId].Value;
            }

            // fix a few hardcoded projectiles to items.
            texture = ResizeWeirdTextures(texture);

            // debug item info
            float w = texture.Width;
            float h = texture.Height;

            // if the texture is too big, scale it down until its at least 48 width or height
            float w2 = 0f;
            float h2 = 0f;
            if (w > 48 || h > 48)
            {
                float scale = 48 / Math.Max(w, h);
                w2 = w*scale;
                h2 = w*scale;
                rectangleSize = new Rectangle(0, 0, (int)w2, (int)h2);

            }
            ModContent.GetInstance<DPSPanel>().Logger.Info($"[{weaponName}] {itemType} ID: {itemId} WxH Before: {w}x{h} WxH After: {w2}x{h2}");

            // Draw texture
            if (texture != null)
            {
                spriteBatch.Draw(texture, pos, rectangleSize, Color.White);
            }
        }

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

        private static readonly Dictionary<int, int> projectileToItemMap = new()
        {
            // Map projectile ID 933 (e.g., Zenith projectile) to item ID 4956 (Zenith item)
            {933, 4956}, // zenith
            {409, 2622}, // typhoon
            {981,278}, // silver bullet
        };

    }
}
