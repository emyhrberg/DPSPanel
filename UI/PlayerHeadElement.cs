using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI;

namespace DPSPanel.UI
{
    public class PlayerHeadElement : UIElement
    {
        private readonly Player player;

        public PlayerHeadElement(Player player)
        {
            this.player = player;
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            base.DrawSelf(spriteBatch);

            if (player == null || !player.active)
                return;

            CalculatedStyle dims = GetDimensions();
            int xOffset = 10;
            int yOffset = 16;
            Vector2 drawPosition = new(dims.X + xOffset, dims.Y + yOffset);

            Main.MapPlayerRenderer.DrawPlayerHead(
                Main.Camera,
                player,
                drawPosition,
                1f, // alpha/transparency
                1.1f, // scale
                Color.White // border color
            );
        }
    }
}
