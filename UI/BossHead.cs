using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using Terraria.UI;
namespace DPSPanel.UI
{
    public class BossHead : UIElement
    {
        public int _bossHeadID = -1; // -1 means no/invalid boss head

        public BossHead()
        {
            Width.Set(26f, 0f);
            Height.Set(26f, 0f);
            HAlign = 1.0f;
        }

        protected override void DrawSelf(SpriteBatch sb)
        {
            //HAlign = 1.0f;

            base.DrawSelf(sb);

            // TODO change icon size based on panel size maybe?

            if (_bossHeadID == -1)
                return;

            // ensure index is valid!
            if (_bossHeadID >= 0 && _bossHeadID <= TextureAssets.NpcHeadBoss.Length && TextureAssets.NpcHead[_bossHeadID]?.Value != null)
            {
                Texture2D bossHeadTexture = TextureAssets.NpcHeadBoss[_bossHeadID]?.Value;
                CalculatedStyle dims = GetDimensions();
                Rectangle pos = dims.ToRectangle();
                sb.Draw(bossHeadTexture, pos, Color.White);
            }
            else
            {
                // Log.Info($"Invalid boss index {_bossHeadID}");
            }
        }

        public void SetBossHeadID(int bossHeadID)
        {
            _bossHeadID = bossHeadID;
        }
    }
}
