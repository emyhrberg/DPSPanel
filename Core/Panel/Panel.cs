using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly float padding = 5f;
        private readonly float PANEL_WIDTH = 300f; // 300 width
        private readonly float PANEL_HEIGHT = 40f; // is reset anyways
        private readonly Color panelColor = new(49, 84, 141); 
        private float currentYOffset = 0;
        private const float ItemHeight = 40f;

        // Add event handlers for child changes
        public event Action OnSizeChanged;

        // bar items
        private Dictionary<string, DamageBarElement> damageBars = [];
        // private NPC currentBoss;
        private const float headerHeight = 16f;

        public Panel()
        {
            Width.Set(PANEL_WIDTH, 0f);
            Height.Set(PANEL_HEIGHT, 0f);
            BackgroundColor = panelColor;
            VAlign = 0.07f; // 7% from the top
            HAlign = 0.5f; // Center horizontally
            SetPadding(padding);
        }

        public void SetBossTitle(string bossName = "Boss Name", NPC npc = null)
        {
            // currentBoss = npc;
            UIText bossTitle = new(bossName, 1.0f);
            bossTitle.HAlign = 0.5f;
            Append(bossTitle);

            currentYOffset = headerHeight + padding * 2; // Adjust Y offset for the next element
            ResizePanelHeight();
        }

        public override void Draw(SpriteBatch sb)
        {
            base.Draw(sb);
        }

        public void CreateDamageBar(string barName = "Name")
        {
            // Check if the bar already exists
            if (!damageBars.ContainsKey(barName))
            {
                // Create a new bar
                DamageBarElement bar = new(currentYOffset);
                Append(bar);
                damageBars[barName] = bar;

                currentYOffset += ItemHeight + padding * 2; // Adjust Y offset for the next element
                ResizePanelHeight();
            }
        }

        public void UpdateDamageBars(List<Weapon> weapons)
        {
            // Reset vertical offset. needed?
            currentYOffset = headerHeight + padding * 2;

            // Sort weapons by descending damage.
            weapons = weapons.OrderByDescending(w => w.damage).ToList();
            int highest = weapons.FirstOrDefault()?.damage ?? 1;

            for (int i = 0; i < weapons.Count; i++)
            {
                var wpn = weapons[i];
                Color color = PanelColors.colors[i % PanelColors.colors.Length];

                // Get for this weapon.
                DamageBarElement bar = damageBars[wpn.weaponName];

                // Update  with the current data.
                bar.UpdateDamageBar(highest, wpn.weaponName, wpn.damage, color, wpn.itemID, wpn.itemType);
                bar.Top.Set(currentYOffset, 0f);
                currentYOffset += ItemHeight + padding * 2;
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
            Height.Set(currentYOffset + padding, 0f);
            Recalculate();
            OnSizeChanged?.Invoke();
        }
    }
}