using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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

            // Config c = ModContent.GetInstance<Config>();
            // if (c.BossIconSide == "Left")
            // {
                Top.Set(0, 0f);
                Left.Set(0, 0f);
            // }
            // else // right
            // {
                // HAlign = 1f;
            // }
            
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
                // UICommon.TooltipMouseText("Toggle DPS Panel");
                Main.hoverItemName = "Show Boss Damage";
            }

            if (currentBoss != null) 
            {
                int headIndex = currentBoss.GetBossHeadTextureIndex();
                if (headIndex < 0)
                    headIndex = 7; // fallback
                DrawBossIconAtIndex(spriteBatch, headIndex);
            }
            else
                DrawBossIconAtIndex(spriteBatch, 7); // fallback if no boss
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