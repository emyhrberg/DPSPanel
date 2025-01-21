using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.GameContent;
using Terraria;
using DPSPanel.Core.Configs;
using Terraria.ModLoader.UI;

namespace DPSPanel.Core.Panel
{
    public class BossIconElement : UIElement
    {
        public NPC currentBoss; 

        public BossIconElement()
        {
            Width.Set(30f, 0f);
            Height.Set(30f, 0f);
            Top.Set(0, 0f);
            Left.Set(0, 0f);
            currentBoss = null;
        }

        public void UpdateBossIcon(NPC boss)
        {
            currentBoss = boss;
        }

        public override void LeftClick(UIMouseEvent evt)
        {
            // Toggle panel
            PanelSystem s = ModContent.GetInstance<PanelSystem>();
            s?.state?.container?.TogglePanel();
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            base.DrawSelf(spriteBatch);
            Config c = ModContent.GetInstance<Config>();
            if (!c.EnableButton)
                return;

            if (IsMouseHovering)
            {
                UICommon.TooltipMouseText("Toggle DPS Panel");
                // Main.hoverItemName = "Toggle DPS Panel 2";
            }

            if (currentBoss == null)
                DrawBossIconAtIndex(spriteBatch, 7);
            else
                DrawBossIconAtIndex(spriteBatch, currentBoss.type);
        }

        private void DrawBossIconAtIndex(SpriteBatch sb, int i)
        {
            Texture2D bossHeadTexture = TextureAssets.NpcHeadBoss[i]?.Value;
            CalculatedStyle dims = GetDimensions();
            Vector2 pos = new(dims.X, dims.Y);
            sb.Draw(bossHeadTexture, pos, Color.White);
        }
    }
}