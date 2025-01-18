using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent; // For TextureAssets

namespace DPSPanel
{
    public class DPSPanel : Mod
    {
        public override void Load()
        {
            Main.OnPostDraw += DrawItemID1;
        }

        public override void Unload()
        {
            Main.OnPostDraw -= DrawItemID1;
        }

        private void DrawItemID1(GameTime gameTime)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = TextureAssets.Item[ItemID.WoodenSword].Value; // Using TextureAssets.Item

            // Position where the icon will be drawn
            Vector2 position = new Vector2(100, 100);

            spriteBatch.Begin();
            spriteBatch.Draw(texture, position, Color.White);
            spriteBatch.End();
        }
    }
}
