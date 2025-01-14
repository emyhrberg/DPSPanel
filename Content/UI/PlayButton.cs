using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.UI;
using Terraria;
using Terraria.ModLoader;

namespace BetterDPS.Content.UI
{
    class PlayButton : UIElement
    {
        Color color = new Color(50, 255, 153);
        Vector2 pos = new Vector2(Main.screenWidth + 20, Main.screenHeight - 20) / 2f;
        Texture2D playButtonTexture = (Texture2D) ModContent.Request<Texture2D>("Terraria/Images/UI/ButtonPlay");

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(playButtonTexture, pos, color);
        }
    }
}