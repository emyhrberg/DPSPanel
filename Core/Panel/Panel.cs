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
using static DPSPanel.Core.Panel.BossDamageTrackerSP;

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

        public void CreateDamageBar(string barName = "Name")
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
            if (!damageBars.ContainsKey(barName))
            {
                // Create a new bar
                DamageBarElement bar = new(currentYOffset);
                Append(bar);
                damageBars[barName] = bar;

                currentYOffset += ItemHeight + ITEM_PADDING * 2; // Adjust Y offset for the next element
                ResizePanelHeight();
            }
        }

        public void UpdateDamageBars(List<Weapon> weapons)
        {
            // Reset vertical offset. needed for ensuring that an updated panel does not get added height)
            currentYOffset = headerHeight + ITEM_PADDING * 2;

            // Sort weapons by descending damage.
            // weapons = weapons.OrderByDescending(w => w.damage).ToList();
            int highest = weapons.FirstOrDefault()?.damage ?? 1;

            for (int i = 0; i < weapons.Count; i++)
            {
                var wpn = weapons[i];
                DamageBarElement bar = damageBars[wpn.weaponName];

                // Update  with the current data.
                int percentageToFill = (int)(wpn.damage / (float)highest * 100);
                Color color = PanelColors.colors[i % PanelColors.colors.Length];

                bar.UpdateDamageBar(percentageToFill, wpn.weaponName, wpn.damage, wpn.weaponItemID, color);
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
            parentContainer.Height.Set(currentYOffset + ITEM_PADDING, 0f);
            parentContainer.Recalculate();

            Height.Set(currentYOffset + ITEM_PADDING, 0f);
            Recalculate();
        }
    }
}