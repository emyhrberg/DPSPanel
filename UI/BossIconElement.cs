using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.GameContent;
using Terraria;
using ReLogic.Content;

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

        protected override void DrawSelf(SpriteBatch sb)
        {
            base.DrawSelf(sb);
            int bossIndex = currentBoss.GetBossHeadTextureIndex();
            DrawBossIconAtIndex(sb, bossIndex);
        }

        public void UpdateBossIcon(NPC boss)
        {
            currentBoss = boss;
        }

        private void DrawBossIconAtIndex(SpriteBatch sb, int i)
        {
            // ensure index is valid!
            if (i >= 0 || i <= TextureAssets.NpcHeadBoss.Length)
            {
                Texture2D bossHeadTexture = TextureAssets.NpcHeadBoss[i]?.Value;
                CalculatedStyle dims = GetDimensions();
                Vector2 pos = new(dims.X, dims.Y);
                sb.Draw(bossHeadTexture, pos, Color.White);
            }
            else
            {
                ModContent.GetInstance<DPSPanel>().Logger.Info($"Invalid boss index {i}");
            }
        }
    }
}
