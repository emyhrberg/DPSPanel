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

namespace DPSPanel.UI
{
    public class WeaponBar : UIElement
    {
        private Asset<Texture2D> emptyBar; // Background 
        private Asset<Texture2D> fullBar;  // Foreground fill texture
        private readonly UIText textElement; // Text element for display
        private const float ItemHeight = 40f; // Height of the bar

        private Color fillColor; // Fill color
        private int percentage;  // Fill percentage

        // Weapon info
        private int weaponItemID;
        private string weaponName;

        public WeaponBar(float currentYOffset)
        {
            emptyBar = Assets.BarEmpty150;
            fullBar = Assets.BarFull150;

            Width = new StyleDimension(0, 1.0f);
            Height = new StyleDimension(ItemHeight, 0f);
            Top = new StyleDimension(currentYOffset, 0f);
            HAlign = 0.5f;

            textElement = new UIText("", 0.8f)
            {
                HAlign = 0.5f,
                VAlign = 0.5f,
            };
            Append(textElement);
        }

        public void UpdateWeaponBar(int _percentage, string _weaponName, int weaponDamage, int weaponID, Color _fillColor)
        {
            percentage = _percentage;
            weaponItemID = weaponID;
            fillColor = _fillColor;
            weaponName = _weaponName;
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
            Vector2 pos = new(dims.X, dims.Y);
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
            Texture2D texture;
            if (weaponItemID == -1)
                texture = TextureAssets.NpcHead[0].Value; // Placeholder texture (a question mark)
            else
                texture = TextureAssets.Item[weaponItemID].Value;

            CalculatedStyle dims = GetDimensions();
            const int maxIconHeight = 32;
            const int paddingLeft = 5;
            int originalWidth = texture.Width;
            int originalHeight = texture.Height;
            float scale = originalHeight > maxIconHeight ? (float)maxIconHeight / originalHeight : 1f;
            int scaledWidth = (int)(originalWidth * scale);
            int scaledHeight = (int)(originalHeight * scale);
            int iconX = (int)dims.X + paddingLeft;
            int iconY = (int)(dims.Y + (dims.Height - scaledHeight) / 2f);
            Rectangle destRect = new Rectangle(iconX, iconY, scaledWidth, scaledHeight);
            sb.Draw(texture, destRect, Color.White);
        }
    }
}
