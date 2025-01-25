using System.Collections.Generic;
using System.Linq;
using DPSPanel.Configs;
using DPSPanel.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;

namespace DPSPanel.UI

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

        // damageBars: Key = each individual playerName, Value = DamageBarElement
        private Dictionary<string, DamageBarElement> players = [];
        private const float headerHeight = 28f;

        public Panel()
        {
            Width.Set(PANEL_WIDTH, 0f);
            Height.Set(PANEL_HEIGHT, 0f);
            BackgroundColor = panelColor;

            // set position relative to PARENT container.
            HAlign = 0.5f; // Center horizontally
            SetPadding(PANEL_PADDING);
        }

        public void SetBossTitle(string bossName)
        {
            UIText bossTitle = new(bossName, 1.0f);
            bossTitle.HAlign = 0.5f;
            bossTitle.Top.Set(6f, 0f);
            Append(bossTitle);
            currentYOffset = headerHeight;
            ResizePanelHeight();
        }

        public override void Draw(SpriteBatch sb)
        {
            SimpleConfig c = ModContent.GetInstance<SimpleConfig>();
            if (!c.ShowToggleButton)
                return;
            base.Draw(sb);
        }

        public void CreateDamageBar(string playerName)
        {
            // Check if the bar already exists
            if (players.ContainsKey(playerName))
                return;

            // Create a new bar
            DamageBarElement bar = new(currentYOffset, playerName);

            Append(bar);
            players[playerName] = bar;

            ModContent.GetInstance<DPSPanel>().Logger.Info($"Creating damage bar for {playerName}");

            currentYOffset += ItemHeight + ITEM_PADDING * 2; // Adjust Y offset for the next element
            ResizePanelHeight();
        }

        public void UpdateDamageBars(string playerName, int playerDamage)
        {
            if (!players.ContainsKey(playerName))
                CreateDamageBar(playerName);

            // Update the player's damage in the DamageBarElement
            var bar = players[playerName];
            bar.PlayerDamage = playerDamage;

            // Sort players by damage in descending order
            var sortedPlayers = players
                .OrderByDescending(p => p.Value.PlayerDamage)
                .ToList();

            // Reset Y offset for re-sorting
            currentYOffset = headerHeight;

            // Calculate the highest damage once
            int highestDamage = sortedPlayers.First().Value.PlayerDamage;

            for (int i = 0; i < sortedPlayers.Count; i++)
            {
                string currentPlayerName = sortedPlayers[i].Key;
                DamageBarElement currentBar = sortedPlayers[i].Value;

                // Update bar position to match the sorted order
                currentBar.Top.Set(currentYOffset, 0f);
                currentYOffset += ItemHeight + ITEM_PADDING * 2;

                // Calculate fill percentage safely
                int percentageToFill = highestDamage > 0
                    ? (int)(currentBar.PlayerDamage / (float)highestDamage * 100)
                    : 0;

                ModContent.GetInstance<DPSPanel>().Logger.Info($"Updating bar: {playerName}, Damage: {playerDamage}, Percentage: {percentageToFill}");

                // Assign a color based on position in the sorted list
                Color barColor = PanelColors.colors[i % PanelColors.colors.Length];

                // Update the current bar
                currentBar.UpdateDamageBar(percentageToFill, currentPlayerName, currentBar.PlayerDamage, barColor);
            }

            ResizePanelHeight();
        }


        public void ClearPanelAndAllItems()
        {
            RemoveAllChildren();
            players.Clear(); // Clear the players dictionary
            currentYOffset = headerHeight; // Reset the Y offset
            ResizePanelHeight();
        }

        private void ResizePanelHeight()
        {
            // set parent container height
            if (Parent is not BossContainerElement parentContainer)
                return; // Exit if parentContainer is not set
            parentContainer.Height.Set(currentYOffset + ITEM_PADDING, 0f);
            parentContainer.Recalculate();

            Height.Set(currentYOffset + ITEM_PADDING, 0f);
            Recalculate();
        }
    }
}