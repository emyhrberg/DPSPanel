﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.UI;
using Terraria.GameContent.UI.Elements;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using DPSPanel.Configs;
using DPSPanel.Helpers;
using Terraria.ModLoader.UI;

namespace DPSPanel.UI
{
    public class WeaponDamageBarElement : UIElement
    {
        private Asset<Texture2D> emptyBar; // Background 
        private Asset<Texture2D> fullBar;  // Foreground fill texture
        private readonly UIText textElement;          // Text element for 
        private const float ItemHeight = 40f; // size of each item

        private Color fillColor;             // Color for the fill
        private int percentage;              // Progress percentage (0-100)

        // Weapon icon
        private int weaponItemID;
        private string weaponName;           // Weapon name

        public static WeaponDamageBarElement Instance;

        public WeaponDamageBarElement(float currentYOffset)
        {
            Instance = this;
            // check config settings for theme
            Config c = ModContent.GetInstance<Config>();

            if (c.BarWidth == "150")
            {
                emptyBar = LoadAssets.BarEmpty150;
                fullBar = LoadAssets.BarFull150;
            }
            else if (c.BarWidth == "300")
            {
                emptyBar = LoadAssets.BarEmpty300;
                fullBar = LoadAssets.BarFull300;
            }

            Width = new StyleDimension(0, 1.0f); // Fill the width of the panel
            Height = new StyleDimension(ItemHeight, 0f); // Set height
            Top = new StyleDimension(currentYOffset, 0f);
            HAlign = 0.5f; // Center horizontally

            // Create the text element centered on the bar.
            textElement = new UIText("", 0.8f) // 80% size
            {
                HAlign = 0.5f,
                VAlign = 0.5f,
            };
            Append(textElement);
        }

        public static void UpdateBarWidth(Config c)
        {
            if (Instance == null)
            {
                ModContent.GetInstance<DPSPanel>().Logger.Warn("Instance is null in WeaponDamageBarElement");
                return;
            }
            if (c.BarWidth == "150")
            {
                Instance.emptyBar = LoadAssets.BarEmpty150;
                Instance.fullBar = LoadAssets.BarFull150;
            }
            else if (c.BarWidth == "300")
            {
                Instance.emptyBar = LoadAssets.BarEmpty300;
                Instance.fullBar = LoadAssets.BarFull300;
            }
        }

        public void UpdateDamageBar(int _percentage, string _weaponName, int weaponDamage, int weaponID, Color _fillColor)
        {
            percentage = _percentage;
            weaponItemID = weaponID;
            fillColor = _fillColor;
            weaponName = _weaponName; // used for debugging only
            textElement.SetText($"{_weaponName} ({weaponDamage})");
        }

        protected override void DrawSelf(SpriteBatch sb)
        {
            base.DrawSelf(sb);
            DrawDamageBarFill(sb);
            DrawDamageBarOutline(sb);
            DrawWeaponIcon(sb);
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

        private void DrawWeaponIcon(SpriteBatch sb)
        {
            // Check if the weapon icon display is enabled in the config
            // Config c = ModContent.GetInstance<Config>();
            // if (!c.ShowWeaponIcon)
            // return;

            // Load the appropriate texture based on the weaponItemID
            Texture2D texture;
            if (weaponItemID == -1 && TextureAssets.NpcHead[0].Value != null)
                // texture = TextureAssets.Buff[BuffID.Confused].Value; // Example: Confused debuff as placeholder
                // Use NPCHEAD[0] as placeholder (a question mark)
                texture = TextureAssets.NpcHead[0].Value;
            else
                texture = TextureAssets.Item[weaponItemID].Value;

            // Get the dimensions of the current UI element
            CalculatedStyle dims = GetDimensions();

            // Define the desired maximum icon height
            const int maxIconHeight = 32;
            const int paddingLeft = 5; // Padding from the left edge

            // Get original texture size
            int originalWidth = texture.Width;
            int originalHeight = texture.Height;

            float scale;

            // Determine scaling factor based on original height
            if (originalHeight > maxIconHeight)
            {
                // Scale down to have a height of 32 pixels
                scale = (float)maxIconHeight / originalHeight;
            }
            else
            {
                // Use original size (no scaling)
                scale = 1f;
            }

            // Calculate scaled width and height while maintaining aspect ratio
            int scaledWidth = (int)(originalWidth * scale);
            int scaledHeight = (int)(originalHeight * scale);

            // Calculate position:
            // - X: padding from the left
            // - Y: vertically centered within the DamageBarElement
            int iconX = (int)dims.X + paddingLeft;
            int iconY = (int)(dims.Y + (dims.Height - scaledHeight) / 2f);

            // Define the destination rectangle with the calculated size and position
            Rectangle destRect = new Rectangle(iconX, iconY, scaledWidth, scaledHeight);

            // Draw the texture scaled to fit the destination rectangle
            sb.Draw(texture, destRect, Color.White);
        }

    }
}