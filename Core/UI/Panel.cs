using System.Collections.Generic;
using System.Linq;
using DPSPanel.Core.Configs;
using DPSPanel.Core.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;

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
        private const float headerHeight = 28f;

        // list of players
        private Dictionary<string, int> players = [];

        public Panel()
        {
            Width.Set(PANEL_WIDTH, 0f);
            Height.Set(PANEL_HEIGHT, 0f);
            BackgroundColor = panelColor;

            // set position relative to PARENT container.
            HAlign = 0.5f; // Center horizontally
            SetPadding(PANEL_PADDING);
        }

        public void SetBossTitle(string bossName = "Boss Name")
        {
            // currentBoss = npc;
            UIText bossTitle = new(bossName, 1.0f);
            bossTitle.HAlign = 0.5f;
            bossTitle.Top.Set(6f, 0f);
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
            // Check if the bar already exists
            if (damageBars.ContainsKey(playerName))
                return;

            // Create a new bar
            DamageBarElement bar = new(currentYOffset);
            Append(bar);
            damageBars[playerName] = bar;

            ModContent.GetInstance<DPSPanel>().Logger.Info($"Creating damage bar for {playerName}");

            currentYOffset += ItemHeight + ITEM_PADDING * 2; // Adjust Y offset for the next element
            ResizePanelHeight();
        }

        public void UpdateDamageBars(string playerName, int playerDamage)
        {
            // If the player doesn't already have a bar, create one
            if (!damageBars.ContainsKey(playerName))
                CreateDamageBar(playerName);

            // Update the player's damage in the dictionary
            players[playerName] = playerDamage;

            // Sort players by damage in descending order
            var sortedPlayers = players.OrderByDescending(p => p.Value).ToList();

            // Reset Y offset for sorting
            currentYOffset = headerHeight;

            for (int i = 0; i < sortedPlayers.Count; i++)
            {
                string currentPlayerName = sortedPlayers[i].Key;
                int currentPlayerDamage = sortedPlayers[i].Value;

                DamageBarElement bar = damageBars[currentPlayerName];

                // Update bar position to match the sorted order
                bar.Top.Set(currentYOffset, 0f);
                currentYOffset += ItemHeight + ITEM_PADDING * 2;

                // Calculate fill percentage based on the highest damage
                int highest = sortedPlayers.First().Value;
                int percentageToFill = (int)(currentPlayerDamage / (float)highest * 100);

                // Assign a color based on the position in the sorted list
                Color barColor = PanelColors.colors[i % PanelColors.colors.Length];

                // Update the bar for the current player
                bar.UpdateDamageBar(percentageToFill, currentPlayerName, currentPlayerDamage, barColor);
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