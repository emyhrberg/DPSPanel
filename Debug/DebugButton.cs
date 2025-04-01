using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace DPSPanel.Debug
{
    public class DebugButton : UIText
    {
        private Action onClick;

        public DebugButton(string text, Action action = null) : base(text)
        {
            Width.Set(0, 1f);
            Height.Set(20, 0f);
            TextColor = Color.White;
            this.onClick = action;
            TextOriginX = 0;
            TextOriginY = 0;
        }

        public override void MouseOver(UIMouseEvent evt)
        {
            base.MouseOver(evt);
            TextColor = Color.Yellow;
        }

        public override void MouseOut(UIMouseEvent evt)
        {
            base.MouseOut(evt);
            TextColor = Color.White;
        }

        public override void LeftClick(UIMouseEvent evt)
        {
            // base.LeftClick(evt);
            onClick?.Invoke();
        }

        public override void Update(GameTime gameTime)
        {
            if (IsMouseHovering)
            {
                Main.LocalPlayer.mouseInterface = true; // Prevents other UI elements from being used
            }

            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            // Draw debug background color
            var rect = GetDimensions().ToRectangle();
            spriteBatch.Draw(TextureAssets.MagicPixel.Value, rect, Color.Black * 0.5f);

            base.Draw(spriteBatch);
        }
    }
}