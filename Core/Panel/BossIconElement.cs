using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.GameContent;
using Terraria;
using DPSPanel.Core.Configs;

namespace DPSPanel.Core.Panel
{
    public class BossIconElement : UIElement
    {

        NPC currentBoss; 

        public BossIconElement()
        {

        }

        public void UpdateBossIcon(NPC boss)
        {
            currentBoss = boss;
        }

        public override void LeftClick(UIMouseEvent evt)
        {
            // Toggle panel
            PanelSystem s = ModContent.GetInstance<PanelSystem>();
            s?.state?.ToggleDPSPanel();
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            base.DrawSelf(spriteBatch);
            Config c = ModContent.GetInstance<Config>();
            if (!c.EnableButton)
                return;

            if (currentBoss == null)
                DrawBossIcon(spriteBatch, 7);
        }

        private void DrawBossIcon(SpriteBatch sb, int i)
        {
            Texture2D bossHeadTexture = TextureAssets.NpcHeadBoss[i]?.Value;
            CalculatedStyle dims = GetInnerDimensions();
            Vector2 pos = new Vector2(dims.X, dims.Y);
            sb.Draw(bossHeadTexture, pos, Color.White);
        }
    }
}