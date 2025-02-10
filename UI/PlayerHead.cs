using DPSPanel.Core.Configs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI;

namespace DPSPanel.UI
{
    public class PlayerHead : UIElement
    {
        private readonly Player player;

        // make a get set for fillcolor
        public Color FillColor { get; set; }

        public PlayerHead(Player player)
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

            // make a clone and have it always face right
            Player rightFacingClone = (Player)player.Clone();

            // do NOT force direction as this messes up the minimap.
            // rightFacingClone.direction = 1;

            Main.MapPlayerRenderer.DrawPlayerHead(
                Main.Camera,
                rightFacingClone,
                drawPosition,
                1f, // alpha/transparency
                0.8f, // scale
                Color.White * 0.01f // border color
            );
        }
    }
}
