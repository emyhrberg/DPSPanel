using System.Collections.Generic;
using DPSPanel.Core.Configs;
using DPSPanel.Core.DamageCalculation;
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
        private Asset<Texture2D> emptyBar; // Background 
        private Asset<Texture2D> fullBar;  // Foreground fill texture
        private readonly UIText textElement; // Text element for displaying player info
        private const float ItemHeight = 40f; // Height of the player bar

        private Color fillColor; // Fill color
        private int percentage;  // Fill percentage (0-100)

        // Properties
        public string PlayerName { get; set; }
        public int PlayerDamage { get; set; }
        public int PlayerWhoAmI { get; set; }

        // The damage panel is attached as a child of the PlayerBar.
        private PlayerDamagePanel damagePanel;

        // Player head element
        private PlayerHead playerHeadElement;

        public static PlayerBar Instance;

        public PlayerBar(float currentYOffset, string playerName, int playerWhoAmI)
        {
            Instance = this;
            Config c = ModContent.GetInstance<Config>();
            if (c.BarWidth == 150)
            {
                emptyBar = Assets.BarEmpty150;
                fullBar = Assets.BarFull150;
            }
            else if (c.BarWidth == 300)
            {
                emptyBar = Assets.BarEmpty300;
                fullBar = Assets.BarFull300;
            }

            // Set dimensions and alignment.
            Width = new StyleDimension(0, 1.0f); // Fill parent's width.
            Height = new StyleDimension(ItemHeight, 0f); // Fixed height.
            Top = new StyleDimension(currentYOffset, 0f);
            HAlign = 0.5f; // Center horizontally.
            this.OverflowHidden = false; // Allow overflow from e.g weapon bars.

            // Initialize player properties.
            PlayerName = playerName;
            PlayerDamage = 0;
            PlayerWhoAmI = playerWhoAmI;

            // Create and center the text element.
            textElement = new UIText("", 0.8f)
            {
                HAlign = 0.5f,
                VAlign = 0.5f,
            };
            Append(textElement);

            // Optionally add the player head if enabled.
            if (c.ShowPlayerIcons)
            {
                if (playerWhoAmI >= 0 && playerWhoAmI < Main.player.Length && Main.player[playerWhoAmI].active)
                {
                    Player player = Main.player[playerWhoAmI];
                    playerHeadElement = new PlayerHead(player);
                    Append(playerHeadElement);
                }
            }

            // Create the damage panel once.
            damagePanel = new PlayerDamagePanel();
            MainSystem sys = ModContent.GetInstance<MainSystem>();
            CalculatedStyle dims = sys.state.container.panel.GetOuterDimensions();
            damagePanel.Left.Set(100f, 0f);
            Append(damagePanel);
            Recalculate();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            // Optionally toggle visibility on hover.
            if (IsMouseHovering)
                damagePanel.IsVisible = true;
            else
                damagePanel.IsVisible = false;
        }

        /// <summary>
        /// Called (from MainPanel) when new weapon data is received.
        /// </summary>
        /// 
        public void UpdateWeaponData(List<Weapon> weapons)
        {
            damagePanel?.UpdateWeaponBars(weapons);
        }

        public static void UpdateBarWidth(Config config)
        {
            if (Instance == null)
                return;
            if (config.BarWidth == 150)
            {
                Instance.emptyBar = Assets.BarEmpty150;
                Instance.fullBar = Assets.BarFull150;
            }
            else if (config.BarWidth == 300)
            {
                Instance.emptyBar = Assets.BarEmpty300;
                Instance.fullBar = Assets.BarFull300;
            }
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
