using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.UI;
using Terraria.GameContent;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader.UI;
using DPSPanel.Configs;
using Terraria.ModLoader;

namespace DPSPanel.UI
{
    public class CustomButtonElement : UITextPanel<string>
    {

        private readonly Action onClick;

        public CustomButtonElement(string label, string tooltip, Action onClick)
            : base(label, 0.8f)
        {
            this.onClick = onClick;

            // show tooltip when hovering over the button
            UICommon.TooltipMouseText(tooltip);

            // set the size of the button
            Width.Set(10, 0);
            Height.Set(10, 0);

            // invoke the onClick action when the button is clicked
            // this is later implemented in the base class
            OnLeftClick += (evt, element) => onClick();
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            Config c = ModContent.GetInstance<Config>();
            if (!c.AlwaysShowButton)
                return;

            base.DrawSelf(spriteBatch);
            // Customize appearance on hover
            if (IsMouseHovering && !(Main.mouseLeft && !(Parent as BossContainerElement)?.clickStartInsidePanel == true))
            {
                spriteBatch.Draw(TextureAssets.MagicPixel.Value, GetDimensions().ToRectangle(), Color.White * 0.2f);
            }
        }
    }
}
