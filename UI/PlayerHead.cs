using DPSPanel.Common.Configs;
using DPSPanel.Common.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace DPSPanel.UI
{
    public class PlayerHead : UIElement
    {
        private readonly Player player;

        public PlayerHead(Player player)
        {
            this.player = player;
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            Config c = ModContent.GetInstance<Config>();
            if (!c.ShowPlayerIcons)
                return;

            base.DrawSelf(spriteBatch);

            if (player == null || !player.active)
                return;

            CalculatedStyle dims = GetDimensions();
            int xOffset = 1;
            int yOffset = 16;
            Vector2 drawPosition = new(dims.X + xOffset, dims.Y + yOffset);

            if (player.dead)
            {
                // you can do something here, but when dead the player disappears and is undrawable so you have to make a clone before death or something
            }

            // assign the shouldFlipHeadDraw flag to the direction of the player,
            // meaning the head will be flipped if the player is facing left
            PlayerHeadFlipSystem.shouldFlipHeadDraw = player.direction == -1;

            // Small, Medium, Large Bar Height equates to 0.9, 1.1 and 1.3 respectively
            float headScale = 0.9f;
            // if (c.BarHeight == "Small")
            // {
            //     headScale = 0.9f;
            //     drawPosition -= new Vector2(0, 1); // Adjust position for small head size
            // }
            // else if (c.BarHeight == "Medium")
            // {
            //     headScale = 1.1f;
            //     drawPosition += new Vector2(0, 2); // Adjust position for medium head size
            // }
            // else if (c.BarHeight == "Large")
            // {
            //     headScale = 1.3f;
            //     drawPosition += new Vector2(0, 4); // Adjust position for larger head size
            // }

            Main.MapPlayerRenderer.DrawPlayerHead(
                Main.Camera,
                player, // player to draw
                drawPosition,
                1f, // alpha/transparency
                headScale, // scale
                Color.White // border color
            );
            PlayerHeadFlipSystem.shouldFlipHeadDraw = false;
        }
    }
}
