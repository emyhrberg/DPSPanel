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
using DPSPanel.Core.Helpers;
using DPSPanel.Core.Configs;

namespace DPSPanel.Core.Panel
{
    public class DamageBarElement : UIElement
    {
        private readonly Asset<Texture2D> emptyBar; // Background 
        private readonly Asset<Texture2D> fullBar;  // Foreground fill texture
        private readonly UIText textElement;          // Text element for 
        private const float ItemHeight = 40f;

        private Color fillColor;             // Color for the fill
        private int percentage;              // Progress percentage (0-100)

        // Weapon icon
        private int itemId;
        private string itemType;
        // private string weaponName;           // Weapon name

        public DamageBarElement(float currentYOffset)
        {
            // check config settings for theme
            Config c = ModContent.GetInstance<Config>();
            if (c.Theme == "Generic")
            {
                emptyBar = LoadResources.BarGenericEmpty;
                fullBar = LoadResources.BarGenericFull;
            }
            else
            {
                emptyBar = LoadResources.BarFancyEmpty;
                fullBar = LoadResources.BarFancyFull;
            }

            Width = new StyleDimension(0, 1.0f); // Fill the width of the panel
            Height = new StyleDimension(ItemHeight, 0f); // Set height
            Top = new StyleDimension(currentYOffset, 0f);
            HAlign = 0.5f; // Center horizontally

            // Create the text element centered on the bar.
            textElement = new UIText("", 0.8f)
            {
                HAlign = 0.5f,
                VAlign = 0.5f,
            };
            Append(textElement);
        }

        public void UpdateDamageBar(int highestDamage, string _weaponName, int weaponDamage, Color newColor, int _itemId, string _itemType)
        {
            percentage = (int)(weaponDamage / (float)highestDamage * 100);
            fillColor = newColor;
            textElement.SetText($"{_weaponName} ({weaponDamage})");
            itemId = _itemId;
            itemType = _itemType;
            // weaponName = _weaponName;
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            base.DrawSelf(spriteBatch);
            DrawDamageBar(spriteBatch);
            DrawIcon(spriteBatch);
        }

        private void DrawDamageBar(SpriteBatch spriteBatch)
        {
            CalculatedStyle dims = GetDimensions();
            Vector2 pos = new Vector2(dims.X, dims.Y);
            Rectangle rect = new Rectangle((int)pos.X, (int)pos.Y, (int)dims.Width, (int)dims.Height);

            // Draw background.
            spriteBatch.Draw(emptyBar.Value, rect, Color.DarkGray);

            // Draw filled portion based on percentage.
            int fillWidth = (int)(dims.Width * (percentage / 100f));
            if (fillWidth > 0)
            {
                Rectangle sourceRect = new Rectangle(0, 0, (int)(fullBar.Width() * (percentage / 100f)), fullBar.Height());
                spriteBatch.Draw(fullBar.Value, new Rectangle((int)pos.X, (int)pos.Y, fillWidth, (int)dims.Height), sourceRect, fillColor);
            }
        }

        private void DrawIcon(SpriteBatch spriteBatch)
        {
            // Load texture (by default, zenith)
            Texture2D texture = TextureAssets.Item[ItemID.Zenith].Value;

            // Scale and position
            CalculatedStyle dims = GetDimensions();
            Vector2 pos = new(dims.X, dims.Y);
            //Rectangle rectangleSize = new(0, 0, 48, 48); 

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

            //ModContent.GetInstance<DPSPanel>().Logger.Info($"[{weaponName}] {itemType} ID: {itemId} WxH: {w}x{h}");

            // Draw texture
            if (texture != null)
            {
                // if height is more than 70, its too damn big. try to crop it to only show top 70 pixels
                if (h > 70)
                {
                    Rectangle sourceRect = new Rectangle(0, 0, 70, 70); // crop area
                    float scaleFactor = 0.6f;
                    int destWidth = (int)(sourceRect.Width * scaleFactor);
                    int destHeight = (int)(sourceRect.Height * scaleFactor);
                    CalculatedStyle dims2 = GetDimensions();
                    Vector2 pos2 = new Vector2(dims2.X, dims2.Y);
                    Rectangle destinationRect = new Rectangle((int)pos.X, (int)pos.Y, destWidth, destHeight);

                    //ModContent.GetInstance<DPSPanel>().Logger.Info($"[{weaponName}] {itemType} ID: {itemId} Cropped h > 70 scaled to: {destWidth}x{destHeight}");

                    spriteBatch.Draw(texture, destinationRect, sourceRect, Color.White);
                }
                else
                {
                    // draw with scaling
                    float scale = 0.8f;

                    // custom scaling for small like yoyos and grenades are 16x16 and 20x20
                    if (w <= 20 && h <= 20)
                    {
                        scale = 2f;
                    }

                    spriteBatch.Draw(texture, pos, null, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
                }
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
            {981,278}, // silver bulleta2
        };

    }
}
