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
        private int weaponItemID;
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

        public void UpdateDamageBar(int _percentage, string _weaponName, int weaponDamage, int weaponID, Color _fillColor)
        {
            percentage = _percentage;
            weaponItemID = weaponID;
            fillColor = _fillColor;
            textElement.SetText($"{_weaponName} ({weaponDamage})");
        }

        protected override void DrawSelf(SpriteBatch sb)
        {
            base.DrawSelf(sb);
            DrawDamageBarFill(sb);
            DrawDamageBarOutline(sb);
            DrawIcon(sb);
        }

        private void DrawDamageBarFill(SpriteBatch sb)
        {
            CalculatedStyle dims = GetDimensions();
            Vector2 pos = new Vector2(dims.X, dims.Y);
            // Rectangle rect = new Rectangle((int)pos.X, (int)pos.Y, (int)dims.Width, (int)dims.Height);

            // Draw filled portion based on percentage.
            int fillWidth = (int)(dims.Width * (percentage / 100f));
            if (fillWidth > 0)
            {
                Rectangle sourceRect = new Rectangle(0, 0, (int)(fullBar.Width() * (percentage / 100f)), fullBar.Height());
                sb.Draw(fullBar.Value, new Rectangle((int)pos.X, (int)pos.Y, fillWidth, (int)dims.Height), sourceRect, fillColor);
            }
        }

        private void DrawDamageBarOutline(SpriteBatch sb)
        {
            CalculatedStyle dims = GetDimensions();
            Vector2 pos = new Vector2(dims.X, dims.Y);
            Rectangle rect = new Rectangle((int)pos.X, (int)pos.Y, (int)dims.Width, (int)dims.Height);
            sb.Draw(emptyBar.Value, rect, Color.DarkGray);
        }

        private void DrawIcon(SpriteBatch sb)
        {
            // check if enabled
            Config c = ModContent.GetInstance<Config>();
            if (!c.ShowWeaponIcon)
                return;

            // Load texture (by default, zenith)
            Texture2D texture = TextureAssets.Item[weaponItemID].Value;

            // Scale and position
            CalculatedStyle dims = GetDimensions();
            Vector2 pos = new(dims.X, dims.Y);
            //Rectangle rectangleSize = new(0, 0, 48, 48); 

            // Check Item or Projectile
            // texture = TextureAssets.Item[itemId].Value;

            // debug item info
            float w = texture.Width;
            float h = texture.Height;

            // ModContent.GetInstance<DPSPanel>().Logger.Info($"[{weaponName}] {itemType} ID: {itemId} WxH: {w}x{h}");
                    // draw with scaling
            float scale = 0.8f;

            // custom scaling for small like yoyos and grenades are 16x16 and 20x20
            if (w <= 20 && h <= 20)
            {
                scale = 2f;
            }

            sb.Draw(texture, pos, null, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        }
    }
}
