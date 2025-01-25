using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.GameContent;
using Terraria;
namespace DPSPanel.UI
{
    public class BossIconElement : UIElement
    {
        public int bossHeadID;

        public BossIconElement()
        {
            Width.Set(30f, 0f);
            Height.Set(30f, 0f);
            Top.Set(-10f, 0f);
            Left.Set(0, 0f);

            HAlign = 0.5f;
        }

        protected override void DrawSelf(SpriteBatch sb)
        {
            base.DrawSelf(sb);

            // ensure index is valid!
            if (bossHeadID >= 0 || bossHeadID <= TextureAssets.NpcHeadBoss.Length)
            {
                Texture2D bossHeadTexture = TextureAssets.NpcHeadBoss[bossHeadID]?.Value;
                CalculatedStyle dims = GetDimensions();
                Vector2 pos = new(dims.X, dims.Y);
                sb.Draw(bossHeadTexture, pos, Color.White);
            }
            else
            {
                ModContent.GetInstance<DPSPanel>().Logger.Info($"Invalid boss index {bossHeadID}");
            }
        }

        public void UpdateBossIcon(int _headID)
        {
            bossHeadID = _headID;
        }
    }
}
