using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ModLoader;
using Terraria.UI;
using DPSPanel.Core.Helpers;
using Terraria.GameContent;
using Terraria.Localization;
using System.Text;
using Terraria;
using ReLogic.Text;
using ReLogic.Graphics;
using DPSPanel.Core.Configs;

namespace DPSPanel.Core.Panel
{
    public class KingSlimeButton : UIElement
    {

        // textures
        private readonly Asset<Texture2D> ksButton;
        private readonly Asset<Texture2D> ksButtonHighlight;

        public static bool ShowButton = true; // Default to true

        public KingSlimeButton()
        {
            // Load textures
            ksButton = LoadResources.KingSlimeButton;
            ksButtonHighlight = LoadResources.KingSlimeButtonHighlight;

            // Set the element’s size to that of the texture
            if (ksButton != null && ksButton.Value != null)
            {
                Width.Set(ksButton.Value.Width, 0f); // 35x35 w x h
                Height.Set(ksButton.Value.Height, 0f);
                HAlign = 0.5f;
                VAlign = 0.03f;
            }
        }

        public override void LeftClick(UIMouseEvent evt)
        {
            ModContent.GetInstance<DPSPanel>().Logger.Info("King Slime button clicked!");

            // Toggle panel
            PanelSystem s = ModContent.GetInstance<PanelSystem>();

            if (s == null)
            {
                ModContent.GetInstance<DPSPanel>().Logger.Info("Error: PanelSystem is null");
                return;
            }
            //s.state.ToggleDPSPanel();
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            base.DrawSelf(spriteBatch);
            // first draw or not
            Config c = ModContent.GetInstance<Config>();
            if (!c.EnableButton || !ShowButton)
            {
                return;
            }

            //then, always null check
            if (ksButton == null || ksButtonHighlight == null)
            {
                ModContent.GetInstance<DPSPanel>().Logger.Info("Error: no texture king slime found failed to drawself");
                return;
            }

            CalculatedStyle dimensions = GetInnerDimensions();

            if (IsMouseHovering)
            {
                // Draw king slime highlighted
                spriteBatch.Draw(ksButtonHighlight.Value, dimensions.Position(), Color.White);

                // Draw Boss Damage text
                var font = FontAssets.MouseText.Value;
                StringBuilder text = new("Show Boss Damage");
                int w = ksButton.Value.Width;
                Vector2 pos = dimensions.Position() + new Vector2(-52f, -w + 10f);
                spriteBatch.DrawString(font, text, pos, Color.White);
            }
            else
            {
                // Draw king slime
                spriteBatch.Draw(ksButton.Value, dimensions.Position(), Color.White);
            }
        }
    }
}