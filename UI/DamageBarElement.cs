using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.UI;
using Terraria.GameContent.UI.Elements;
using Terraria;
using Terraria.GameContent;
using DPSPanel.Helpers;
using DPSPanel.Configs;
using Terraria.ModLoader;

namespace DPSPanel.UI
{
    public class DamageBarElement : UIElement
    {
        private readonly Asset<Texture2D> emptyBar; // Background 
        private readonly Asset<Texture2D> fullBar;  // Foreground fill texture
        private readonly UIText textElement;        // Text element for displaying player info
        private const float ItemHeight = 40f;       // Height of each damage bar

        private Color fillColor;     // Color for the fill
        private int percentage;      // Progress percentage (0-100)

        // Properties
        public string PlayerName { get; set; }
        public int PlayerDamage { get; set; }
        public int PlayerWhoAmI { get; set; }

        // Player head element
        private PlayerHeadElement playerHeadElement;

        public DamageBarElement(float currentYOffset, string playerName, int playerWhoAmI)
        {
            // Load bar textures
            emptyBar = LoadAssets.BarEmpty;
            fullBar = LoadAssets.BarFull;

            // Set dimensions and alignment
            Width = new StyleDimension(0, 1.0f); // Fill the width of the parent
            Height = new StyleDimension(ItemHeight, 0f); // Set fixed height
            Top = new StyleDimension(currentYOffset, 0f);
            HAlign = 0.5f; // Center horizontally

            // Initialize player properties
            PlayerName = playerName;
            PlayerDamage = 0;
            PlayerWhoAmI = playerWhoAmI;

            // Create and center the text element
            textElement = new UIText("", 0.8f) // 80% scale
            {
                HAlign = 0.5f,
                VAlign = 0.5f,
            };
            Append(textElement);

            // Add player head if the player is active
            Config c = ModContent.GetInstance<Config>();
            if (c.ShowPlayerIcon)
            {
                if (playerWhoAmI >= 0 && playerWhoAmI < Main.player.Length && Main.player[playerWhoAmI].active)
                {
                    Player player = Main.player[playerWhoAmI];
                    playerHeadElement = new PlayerHeadElement(player);
                    Append(playerHeadElement);
                }
            }
        }

        public void UpdateDamageBar(int percentage, string playerName, int playerDamage, Color fillColor)
        {
            this.percentage = percentage;
            this.fillColor = fillColor;

            PlayerName = playerName;
            PlayerDamage = playerDamage;

            // update fill color? config option?
            // playerHeadElement.FillColor = fillColor;

            textElement.SetText($"{playerName} ({playerDamage})");
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            base.DrawSelf(spriteBatch);
            DrawDamageBarFill(spriteBatch);
            DrawDamageBarOutline(spriteBatch);
        }

        private void DrawDamageBarFill(SpriteBatch spriteBatch)
        {
            CalculatedStyle dims = GetDimensions();
            Vector2 position = new Vector2(dims.X, dims.Y);

            // Calculate the width of the filled portion based on percentage
            int fillWidth = (int)(dims.Width * (percentage / 100f));
            if (fillWidth > 0)
            {
                Rectangle sourceRect = new Rectangle(0, 0, (int)(fullBar.Width() * (percentage / 100f)), fullBar.Height());
                Rectangle destRect = new Rectangle((int)position.X, (int)position.Y, fillWidth, (int)dims.Height);

                spriteBatch.Draw(fullBar.Value, destRect, sourceRect, fillColor);
            }
        }

        private void DrawDamageBarOutline(SpriteBatch spriteBatch)
        {
            CalculatedStyle dims = GetDimensions();
            Vector2 position = new Vector2(dims.X, dims.Y);
            Rectangle rect = new Rectangle((int)position.X, (int)position.Y, (int)dims.Width, (int)dims.Height);
            spriteBatch.Draw(emptyBar.Value, rect, Color.DarkGray);
        }
    }
}
