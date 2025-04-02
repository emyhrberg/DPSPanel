using System.Collections.Generic;
using DPSPanel.Common.Configs;
using DPSPanel.Common.DamageCalculation;
using DPSPanel.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace DPSPanel.UI
{
    public class PlayerBar : UIElement
    {
        private readonly Asset<Texture2D> emptyBar; // Background 
        private readonly Asset<Texture2D> fullBar;  // Foreground fill texture
        private readonly UIText textElement; // Text element for displaying player info
        private const float ItemHeight = 40f; // Height of the player bar

        private Color fillColor; // Fill color
        private int percentage;  // Fill percentage (0-100)

        // Properties
        public string PlayerName { get; set; }
        public int PlayerDamage { get; set; }
        public int PlayerWhoAmI { get; set; }

        // The damage panel is attached as a child of the PlayerBar.
        private PlayerDamagePanel playerDamagePanel;

        // Player head element
        private PlayerHead playerHeadElement;

        public PlayerBar(float currentYOffset, string playerName, int playerWhoAmI)
        {
            Config c = ModContent.GetInstance<Config>();
            emptyBar = Ass.Default;
            fullBar = Ass.BarFill;

            Width = new StyleDimension(0, 1.0f);
            Height = new StyleDimension(ItemHeight, 0f);
            Top = new StyleDimension(currentYOffset, 0f);
            HAlign = 0.5f;

            PlayerName = playerName;
            PlayerDamage = 0;
            PlayerWhoAmI = playerWhoAmI;

            textElement = new UIText("", 0.8f);
            textElement.HAlign = 0.5f;
            textElement.VAlign = 0.5f;
            Append(textElement);

            if (c.ShowPlayerIcons)
            {
                if (playerWhoAmI >= 0 && playerWhoAmI < Main.player.Length && Main.player[playerWhoAmI].active)
                {
                    Player player = Main.player[playerWhoAmI];
                    playerHeadElement = new PlayerHead(player);
                    Append(playerHeadElement);
                }
            }

            // Create the damage panel.
            playerDamagePanel = new PlayerDamagePanel();
            playerDamagePanel.MaxHeight = new StyleDimension(500f, 0f);

            // Offset by one panel size to the right.
            playerDamagePanel.Left.Set(150f, 0f);
            Append(playerDamagePanel);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // Only update the damage panel if it has been created.
            if (playerDamagePanel != null)
            {
                // Also check if we are dragging the main container
                MainSystem sys = ModContent.GetInstance<MainSystem>();
                // bool dragging = sys.state.container.dragging;

                // Check if config option and if boss is alive
                Config c = ModContent.GetInstance<Config>();
                bool showOnHover = c.ShowWeaponsDuringBossFight;
                bool isBossAlive = sys.state.container.panel.CurrentBossAlive;
                // // Log.Info("ShowOnHover: " + showOnHover + " IsBossAlive: " + isBossAlive);

                if (!showOnHover && isBossAlive)
                {
                    playerDamagePanel.IsVisible = false;
                    return;
                }

                // If either this PlayerBar or its damage panel is hovered, make the damage panel visible.
                if (IsMouseHovering || playerDamagePanel.IsMouseHovering)
                    playerDamagePanel.IsVisible = true;
                else
                    playerDamagePanel.IsVisible = false;
            }
        }

        public void UpdateWeaponData(List<Weapon> weapons)
        {
            playerDamagePanel?.UpdateWeaponBars(weapons);
        }

        public void UpdatePlayerBar(int percentage, string playerName, int playerDamage, Color fillColor)
        {
            this.percentage = percentage;
            this.fillColor = fillColor;
            PlayerName = playerName;
            PlayerDamage = playerDamage;
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
            int fillWidth = (int)(dims.Width * (percentage / 100f));
            if (fillWidth > 0)
            {
                Rectangle sourceRect = new(0, 0, (int)(fullBar.Width() * (percentage / 100f)), fullBar.Height());
                Rectangle destRect = new((int)position.X, (int)position.Y, fillWidth, (int)dims.Height);
                spriteBatch.Draw(fullBar.Value, destRect, sourceRect, fillColor);
            }
        }

        private void DrawDamageBarOutline(SpriteBatch spriteBatch)
        {
            CalculatedStyle dims = GetDimensions();
            Vector2 position = new(dims.X, dims.Y);
            Rectangle rect = new((int)position.X, (int)position.Y, (int)dims.Width, (int)dims.Height);
            spriteBatch.Draw(emptyBar.Value, rect, Color.DarkGray);
        }
    }
}
