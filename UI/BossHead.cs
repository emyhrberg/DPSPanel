using DPSPanel.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI;
namespace DPSPanel.UI
{
    public class BossHead : UIElement
    {
        public int _bossHeadID;

        public BossHead()
        {
            Width.Set(30f, 0f);
            Height.Set(30f, 0f);
            HAlign = 0.5f;
        }

        protected override void DrawSelf(SpriteBatch sb)
        {
            base.DrawSelf(sb);

            if (_bossHeadID == -1)
                return;

            // ensure index is valid!
            if (_bossHeadID >= 0 && _bossHeadID <= TextureAssets.NpcHeadBoss.Length && TextureAssets.NpcHead[_bossHeadID]?.Value != null)
            {
                Texture2D bossHeadTexture = TextureAssets.NpcHeadBoss[_bossHeadID]?.Value;
                CalculatedStyle dims = GetDimensions();
                Vector2 pos = new(dims.X, dims.Y);
                sb.Draw(bossHeadTexture, pos, Color.White);
            }
            else
            {
                Log.Info($"Invalid boss index {_bossHeadID}");
            }
        }

        public void SetBossHeadID(int bossHeadID)
        {
            _bossHeadID = bossHeadID;
        }
    }
}
