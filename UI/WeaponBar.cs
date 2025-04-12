using DPSPanel.Common.Configs;
using DPSPanel.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;
using static DPSPanel.Common.Configs.Config;

namespace DPSPanel.UI
{
    public class WeaponBar : UIElement
    {
        private Asset<Texture2D> emptyBar; // Background 
        private Asset<Texture2D> fullBar;  // Foreground fill texture
        private readonly UIText textElement; // For weapon name & damage

        private Color fillColor;
        private int percentage;

        // Weapon info
        private int weaponItemID;
        private string weaponName;

        public WeaponBar()
        {
            Config c = ModContent.GetInstance<Config>();
            string theme = c.Theme;

            // Default (medium) textures unless user config says large
            emptyBar = typeof(Ass).GetField(theme)?.GetValue(null) as Asset<Texture2D>;
            fullBar = Ass.BarFill;

            if (Conf.C.Width == "Large" || Conf.C.Width == "Medium")
            {
                emptyBar = typeof(Ass).GetField($"{theme}Large")?.GetValue(null) as Asset<Texture2D>;
                fullBar = Ass.BarFillLarge;
            }

            // Height from config (e.g. "Small", "Medium", "Large")
            float newHeight = SizeHelper.HeightSizes["Medium"];
            Height.Set(newHeight, 0f);

            Width.Set(0, 1.0f);
            HAlign = 0.5f;

            textElement = new UIText("", 0.8f)
            {
                HAlign = 0.5f,
                VAlign = 0.5f,
            };
            Append(textElement);
        }

        /// <summary>
        /// Updates the bar with new data (percentage fill, name, damage, etc.).
        /// </summary>
        public void UpdateWeaponBar(int percentage, string weaponName, int weaponDamage, int weaponID, Color fillColor)
        {
            this.percentage = percentage;
            this.fillColor = fillColor;
            this.weaponName = weaponName;
            weaponItemID = weaponID;

            textElement.SetText($"{weaponName} ({weaponDamage})");
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            base.DrawSelf(spriteBatch);
            DrawDamageBarFill(spriteBatch);
            DrawDamageBarOutline(spriteBatch);
            DrawWeaponIcon(spriteBatch);
        }

        public void UpdateTheme(Asset<Texture2D> emptyBar, string large)
        {
            this.emptyBar = emptyBar;
            if (large == "Large")
            {
                this.fullBar = Ass.BarFillLarge;
            }
        }

        private void DrawDamageBarFill(SpriteBatch spriteBatch)
        {
            CalculatedStyle dims = GetDimensions();
            Vector2 pos = new(dims.X, dims.Y);

            int fillWidth = (int)(dims.Width * (percentage / 100f));
            if (fillWidth <= 0) return;

            Rectangle sourceRect = new(
                0, 0,
                (int)(fullBar.Width() * (percentage / 100f)),
                fullBar.Height()
            );

            Rectangle destRect = new Rectangle(
                (int)pos.X,
                (int)pos.Y,
                fillWidth,
                (int)dims.Height
            );

            spriteBatch.Draw(fullBar.Value, destRect, sourceRect, fillColor);
        }

        private void DrawDamageBarOutline(SpriteBatch spriteBatch)
        {
            CalculatedStyle dims = GetDimensions();
            Vector2 pos = new(dims.X, dims.Y);

            Rectangle rect = new(
                (int)pos.X,
                (int)pos.Y,
                (int)dims.Width,
                (int)dims.Height
            );

            spriteBatch.Draw(emptyBar.Value, rect, Color.DarkGray);
        }

        private void DrawWeaponIcon(SpriteBatch spriteBatch)
        {
            // Draw either the known item icon or a placeholder
            Texture2D texture;
            if (weaponItemID < 0 || weaponItemID >= TextureAssets.Item.Length)
            {
                // Fallback: question mark or NPC head #0
                texture = TextureAssets.NpcHead[0].Value;
            }
            else
            {
                texture = TextureAssets.Item[weaponItemID].Value;
            }

            CalculatedStyle dims = GetDimensions();
            const int maxIconHeight = 32;
            const int paddingLeft = 5;

            int origW = texture.Width;
            int origH = texture.Height;
            float scale = (origH > maxIconHeight) ? (maxIconHeight / (float)origH) : 1f;

            int scaledW = (int)(origW * scale);
            int scaledH = (int)(origH * scale);

            int iconX = (int)dims.X + paddingLeft;
            int iconY = (int)(dims.Y + (dims.Height - scaledH) / 2f);

            Rectangle destRect = new(iconX, iconY, scaledW, scaledH);
            spriteBatch.Draw(texture, destRect, Color.White);
        }
    }
}
