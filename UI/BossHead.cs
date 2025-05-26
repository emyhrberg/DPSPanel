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
            base.DrawSelf(sb);

            if (_bossHeadID == -1)
                return;

            // Make sure we stay within array bounds and match the array we’re about to use:
            if (_bossHeadID >= 0
                && _bossHeadID < TextureAssets.NpcHeadBoss.Length
                && TextureAssets.NpcHeadBoss[_bossHeadID]?.Value != null)
            {
                Texture2D bossHeadTexture = TextureAssets.NpcHeadBoss[_bossHeadID].Value;
                CalculatedStyle dims = GetDimensions();
                Rectangle pos = dims.ToRectangle();
                sb.Draw(bossHeadTexture, pos, Color.White);
            }
            else
            {
                // Optional logging or fallback
                // Log.Info($"Invalid boss index {_bossHeadID}");
            }
        }

        public void SetBossHeadID(int bossHeadID)
        {
            _bossHeadID = bossHeadID;
        }
    }
}
