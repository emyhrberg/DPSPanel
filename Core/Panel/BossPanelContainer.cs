using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI;
using DPSPanel.Core.Helpers;
using DPSPanel.Core.Configs;
using System.Linq;

namespace DPSPanel.Core.Panel
{
    public class BossPanelContainer : UIElement
    {
        public BossIconElement bossIcon;
        public Panel panel;
        private NPC currentBoss;

        private bool dragging;
        private Vector2 offset;

        public bool panelVisible = true;

        private readonly float PANEL_WIDTH = 300f; // 300 width
        private float currentHeight = 40f; // Dynamic height

        public BossPanelContainer()
        {
            // set the size and position of the panel
            Width.Set(PANEL_WIDTH, 0f);
            Height.Set(currentHeight, 0f);
            VAlign = 0.07f;
            HAlign = 0.5f;

            // 1) add boss panel
            panel = new Panel();
            // panel.OnSizeChanged += Panel_OnChildChanged;
            Append(panel);

            // 2) add boss icon with initial king slime
            bossIcon = new BossIconElement();
            Append(bossIcon);

            // 3) add boss title
            panel.SetBossTitle("Fight a boss to display stats!", currentBoss);
        }

        public void TogglePanel()
        {
            panelVisible = !panelVisible;

            if (panelVisible)
            {
                if (!Children.Contains(panel))
                    Append(panel);
                    bossIcon.Remove();
                    Append(bossIcon);
                    RecalculateSize();
            }
            else
            {
                if (Children.Contains(panel))
                    panel.Remove();
                    currentHeight = bossIcon.GetDimensions().Height; // Set height to just icon height
                    Height.Set(currentHeight, 0f);
                    Recalculate();
            }
        }

        #region Recalculate Panel
        private void Panel_OnChildChanged(UIElement parent, UIElement child)
        {
            RecalculateSize();
        }

        public void RecalculateSize()
        {
            if (panel != null)
            {
                // Get panel's calculated dimensions
                CalculatedStyle panelDims = panel.GetDimensions();
                
                // Update container height to match panel
                currentHeight = panelDims.Height;
                Height.Set(currentHeight, 0f);
                
                // Recalculate container and children
                Recalculate();

                // Ensure boss icon stays on top
                if (bossIcon != null && Children.Contains(bossIcon))
                {
                    bossIcon.Remove();
                    Append(bossIcon);
                }
            }
        }

        public override void Recalculate()
        {
            base.Recalculate();

            // After recalculating, update children positions if needed
            if (panel != null && Children.Contains(panel))
            {
                panel.Width.Set(PANEL_WIDTH, 0f);
                panel.Recalculate();
            }

            if (bossIcon != null && Children.Contains(bossIcon))
            {
                // Position boss icon relative to panel
                CalculatedStyle containerDims = GetDimensions();
                bossIcon.Left.Set(containerDims.Width - bossIcon.Width.Pixels, 0f);
                bossIcon.Top.Set(0f, 0f);
                bossIcon.Recalculate();
            }
        }
        #endregion

        #region Draggable Container
        public override void LeftMouseDown(UIMouseEvent evt)
        {
            base.LeftMouseDown(evt);

            if (ContainsPoint(evt.MousePosition))
            {
                dragging = true;
                offset = new Vector2(evt.MousePosition.X - Left.Pixels, evt.MousePosition.Y - Top.Pixels);
            }
        }

        public override void LeftMouseUp(UIMouseEvent evt)
        {
            base.LeftMouseUp(evt);
            dragging = false;
        }
        
        // right mouse also
        public override void RightMouseDown(UIMouseEvent evt)
        {
            base.RightMouseDown(evt);

            if (ContainsPoint(evt.MousePosition))
            {
                dragging = true;
                offset = new Vector2(evt.MousePosition.X - Left.Pixels, evt.MousePosition.Y - Top.Pixels);
            }
        }

        public override void RightMouseUp(UIMouseEvent evt)
        {
            base.RightMouseUp(evt);
            dragging = false;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (dragging)
            {
                Left.Set(Main.mouseX - offset.X, 0f);
                Top.Set(Main.mouseY - offset.Y, 0f);
                Recalculate();
            }
        }
        #endregion
    }
}
