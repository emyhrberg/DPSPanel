using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.UI;
using Terraria.GameContent.UI.Elements;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using DPSPanel.Core.Helpers;
using DPSPanel.Core.Configs;

namespace DPSPanel.Core.Panel
{
    public class DamageBarElement : UIElement
    {
        private readonly Asset<Texture2D> emptyBar; // Background 
        private readonly Asset<Texture2D> fullBar;  // Foreground fill texture
        private readonly UIText textElement;          // Text element for 
        private const float ItemHeight = 40f; // size of each item

        private Color fillColor;             // Color for the fill
        private int percentage;              // Progress percentage (0-100)

        // Weapon icon
        private int weaponItemID;
        private string weaponName;           // Weapon name

        public DamageBarElement(float currentYOffset)
        {
            // check config settings for theme
            Config c = ModContent.GetInstance<Config>();
            if (c.Theme == "Generic")
            {
                emptyBar = LoadResources.BarGenericEmpty;
                fullBar = LoadResources.BarGenericFull;
            }
            else if (c.Theme == "Fancy")
            {
                emptyBar = LoadResources.BarFancyEmpty;
                fullBar = LoadResources.BarFancyFull;
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

        // public void UpdateDamageBar(int _percentage, string _weaponName, int weaponDamage, int weaponID, Color _fillColor)
        // {
        //     percentage = _percentage;
        //     // weaponItemID = weaponID;
        //     fillColor = _fillColor;
        //     // weaponName = _weaponName; // used for debugging only
        //     textElement.SetText($"{_weaponName} ({weaponDamage})");
        // }

        public void UpdateDamageBar(int percentage, string playerName, int playerDamage, Color fillColor)
        {
            this.percentage = percentage;
            this.fillColor = fillColor;
            textElement.SetText($"{playerName} ({playerDamage})");
        }


        protected override void DrawSelf(SpriteBatch sb)
        {
            base.DrawSelf(sb);
            DrawDamageBarFill(sb);
            DrawDamageBarOutline(sb);

            // check if singleplayer or multiplayer
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {

            }
            else if (Main.netMode == NetmodeID.SinglePlayer)
            {
                DrawWeaponIcon(sb);
            }

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
            Config c = ModContent.GetInstance<Config>();
            if (!c.ShowWeaponIcon)
                return;

            // Load the appropriate texture based on the weaponItemID
            Texture2D texture;
            if (weaponItemID == -1) // Invalid item ID, use a default or placeholder icon
                texture = TextureAssets.Buff[BuffID.Confused].Value; // Example: Confused debuff as placeholder
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
