using DPSPanel.Configs;
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

        // make a get set for fillcolor
        public Color FillColor { get; set; }

        public PlayerHeadElement(Player player)
        {
            this.player = player;

            // default fill color white
            FillColor = Color.White;
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            Config c = ModContent.GetInstance<Config>();
            if (!c.ShowPlayerIcon)
                return;

            base.DrawSelf(spriteBatch);

            if (player == null || !player.active)
                return;

            CalculatedStyle dims = GetDimensions();
            int xOffset = 10;
            int yOffset = 16;
            Vector2 drawPosition = new(dims.X + xOffset, dims.Y + yOffset);

            if (player.dead)
            {
                // you can do something here, but when dead the player disappears and is undrawable so you have to make a clone before death or something
            }

            Main.MapPlayerRenderer.DrawPlayerHead(
                Main.Camera,
                player,
                drawPosition,
                1f, // alpha/transparency
                1.1f, // scale
                FillColor // border color
            );
        }
    }
}
