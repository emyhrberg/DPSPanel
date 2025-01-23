using System;
using System.Collections.Generic;
using System.Linq;
using DPSPanel.Core.Configs;
using DPSPanel.Core.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace DPSPanel.Core.Panel
{
    public class Panel : UIPanel
    {
        // Panel
        private readonly float PANEL_PADDING = 5f;
        private readonly float ITEM_PADDING = 10f;
        private readonly float PANEL_WIDTH = 300f; // 300 width
        private readonly float PANEL_HEIGHT = 40f; // is reset anyways by parent
        private readonly Color panelColor = new(49, 84, 141);
        private float currentYOffset = 0;
        private const float ItemHeight = 16f; // size of each item

        // bar items
        private Dictionary<string, DamageBarElement> damageBars = [];
        private const float headerHeight = 20f;
        private const float firstItemOffset = 3f;

        public Panel()
        {
            Width.Set(PANEL_WIDTH, 0f);
            Height.Set(PANEL_HEIGHT, 0f);
            BackgroundColor = panelColor;

            // set position relative to PARENT container.
            HAlign = 0.5f; // Center horizontally
            SetPadding(PANEL_PADDING);
        }

        public void SetBossTitle(string bossName = "Boss Name", NPC npc = null)
        {
            // currentBoss = npc;
            UIText bossTitle = new(bossName, 1.0f);
            bossTitle.HAlign = 0.5f;
            Append(bossTitle);
            currentYOffset = headerHeight;
            ResizePanelHeight();
        }

        public override void Draw(SpriteBatch sb)
        {
            Config c = ModContent.GetInstance<Config>();
            if (!c.EnableButton)
                return;
            base.Draw(sb);
        }

        public void CreateDamageBar(string playerName = "PlayerName")
        {
            if (damageBars.Count == 0)
            {
                // var parentContainer = Parent as BossPanelContainer;
                // parentContainer.Height.Set(currentYOffset + firstItemOffset, 0f);
                // parentContainer.Recalculate();

                // Height.Set(currentYOffset + firstItemOffset, 0f);
                // Recalculate();
            }


            // Check if the bar already exists
            if (!damageBars.ContainsKey(playerName))
            {
                // Create a new bar
                DamageBarElement bar = new(currentYOffset);
                Append(bar);
                damageBars[playerName] = bar;

                ModContent.GetInstance<DPSPanel>().Logger.Info($"Creating damage bar for {playerName}");

                currentYOffset += ItemHeight + ITEM_PADDING * 2; // Adjust Y offset for the next element
                ResizePanelHeight();
            }
        }

        public void UpdateDamageBars(List<PlayerFightData> players)
        {
            currentYOffset = headerHeight + ITEM_PADDING * 2;

            int highest = players.FirstOrDefault()?.playerDamage ?? 1;

            foreach (var player in players)
            {
                if (!damageBars.ContainsKey(player.playerName))
                {
                    CreateDamageBar(player.playerName);
                }

                DamageBarElement bar = damageBars[player.playerName];
                int percentageToFill = (int)(player.playerDamage / (float)highest * 100);

                bar.UpdateDamageBar(percentageToFill, player.playerName, player.playerDamage, Color.White);
                bar.Top.Set(currentYOffset, 0f);
                currentYOffset += ItemHeight + ITEM_PADDING * 2;
            }

            ResizePanelHeight();
        }

        public void ClearPanelAndAllItems()
        {
            RemoveAllChildren();
            damageBars = []; // reset
        }

        private void ResizePanelHeight()
        {
            // set parent container height
            var parentContainer = Parent as BossPanelContainer;
            if (parentContainer == null)
                return; // Exit if parentContainer is not set
            parentContainer.Height.Set(currentYOffset + ITEM_PADDING, 0f);
            parentContainer.Recalculate();

            Height.Set(currentYOffset + ITEM_PADDING, 0f);
            Recalculate();
        }
    }
}