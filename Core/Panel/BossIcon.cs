using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.GameContent;
using Terraria;
using DPSPanel.Core.Configs;
using DPSPanel.Core.Helpers;
using ReLogic.Content;

namespace DPSPanel.Core.Panel
{
    public class BossIconElement : UIElement
    {
        public NPC currentBoss;

        private Texture2D t;
        private Vector2 clickStartPosition; // Start position of a mouse click
        private bool isDragging;

        public BossIconElement()
        {
            Width.Set(30f, 0f);
            Height.Set(30f, 0f);
            Top.Set(0, 0f);
            Left.Set(0, 0f);

            t = LoadResources.CoolDown?.Value;
            currentBoss = null;
        }

        private Texture2D XTexture
        {
            get
            {
                t ??= LoadResources.CoolDown?.Value;
                return t;
            }
        }

        protected override void DrawSelf(SpriteBatch sb)
        {
            base.DrawSelf(sb);
            Config c = ModContent.GetInstance<Config>();
            if (!c.EnableButton)
                return;

            if (IsMouseHovering)
            {
                // outline
                // Texture2D outlinedTexture;
                // if (currentBoss == null)
                // {
                //     Texture2D bossHeadTexture = TextureAssets.NpcHeadBoss[7]?.Value;
                //     outlinedTexture = PanelColors.AddYellowOutlineToTexture(Main.graphics.GraphicsDevice, bossHeadTexture, 2);
                // } else {
                //     Texture2D bossHeadTexture = TextureAssets.NpcHeadBoss[currentBoss.GetBossHeadTextureIndex()]?.Value;
                //     outlinedTexture = PanelColors.AddYellowOutlineToTexture(Main.graphics.GraphicsDevice, bossHeadTexture, 2);
                // }
                // CalculatedStyle dims = GetDimensions();
                // Vector2 pos = new(dims.X, dims.Y);
                // sb.Draw(outlinedTexture, pos, Color.Yellow);

                // hover tooltip
                Main.hoverItemName = "Show Boss Damage";
            }

            if (currentBoss != null)
                DrawBossIconAtIndex(sb, currentBoss.GetBossHeadTextureIndex());
            else
                DrawBossIconAtIndex(sb, 7); // Fallback if no boss

            // if (c.DrawXOnDead)
                // DrawBossXIfDead(sb);
        }

        public void UpdateBossIcon(NPC boss)
        {
            currentBoss = boss;
        }

        private void DrawBossIconAtIndex(SpriteBatch sb, int i)
        {
            Texture2D bossHeadTexture = TextureAssets.NpcHeadBoss[i]?.Value;
            CalculatedStyle dims = GetDimensions();
            Vector2 pos = new(dims.X, dims.Y);
            sb.Draw(bossHeadTexture, pos, Color.White);
        }

        private void DrawBossXIfDead(SpriteBatch sb)
        {
            if (currentBoss == null || currentBoss.life > 0)
                return;

            ModContent.GetInstance<DPSPanel>().Logger.Info("boss index" + currentBoss?.GetBossHeadTextureIndex());

            CalculatedStyle dims = GetDimensions();
            Vector2 pos = new(dims.X, dims.Y);

            if (t==null)
            {
                t = ModContent.Request<Texture2D>("DPSPanel/Core/Resources/CoolDown", AssetRequestMode.ImmediateLoad).Value;
            }

            if (t != null)
                sb.Draw(t, pos, Color.White);
            else
                ModContent.GetInstance<DPSPanel>().Logger.Info("error index" + currentBoss?.GetBossHeadTextureIndex());
        }

        #region ClickDragHotFix
        public override void LeftMouseDown(UIMouseEvent evt)
        {
            base.LeftMouseDown(evt);

            // Record the start position when the mouse is pressed
            clickStartPosition = evt.MousePosition;
            isDragging = false; // Reset dragging flag
        }

        public override void LeftMouseUp(UIMouseEvent evt)
        {
            base.LeftMouseUp(evt);

            // Check if the mouse moved significantly during the click
            if (Vector2.Distance(clickStartPosition, evt.MousePosition) > 5f) // Threshold for drag
            {
                isDragging = true;
            }

            // Only toggle the panel if it was not a drag
            if (!isDragging)
            {
                var parentContainer = Parent as BossPanelContainer;
                parentContainer?.TogglePanel();
            }
        }
        #endregion
    }
}
