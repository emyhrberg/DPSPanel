using System.Collections.Generic;
using System.Linq;
using DPSPanel.Configs;
using DPSPanel.DamageCalculation;
using DPSPanel.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
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

        // singleplayer: weaponBars: Key = each individual weaponName, Value = WeaponBarElement
        private Dictionary<string, WeaponBarElement> damageBars = [];

        // bossTitle: Title of the boss
        public int CurrentBossID;
        public string CurrentBossName;
        private const float bossHeaderHeight = 28f;

        public Panel()
        {
            Width.Set(PANEL_WIDTH, 0f);
            Height.Set(PANEL_HEIGHT, 0f);
            BackgroundColor = panelColor;

            // set position relative to PARENT container.
            HAlign = 0.5f; // Center horizontally
            SetPadding(PANEL_PADDING);
        }

        public void SetBossTitle(string bossName, int bossID)
        {
            // Set the boss name and ID
            CurrentBossID = bossID;
            CurrentBossName = bossName;

            // Add the element to the panel
            UIText bossTitle = new(bossName, 1.0f);
            bossTitle.HAlign = 0.5f;
            bossTitle.Top.Set(6f, 0f);
            Append(bossTitle);

            // Resize panel height
            currentYOffset = bossHeaderHeight;
            ResizePanelHeight();
        }

        public void SetBossIcon(int bossHeadId)
        {
            // Create the boss icon
            BossIconElement bossIcon = new();
            bossIcon.UpdateBossIcon(bossHeadId);
            bossIcon.Left.Set(5f, 0f);
            bossIcon.Top.Set(5f, 0f);
            Append(bossIcon);
        }

        public override void Draw(SpriteBatch sb)
        {
            Config c = ModContent.GetInstance<Config>();

            if (!Main.playerInventory && !c.AlwaysShowButton)
                return;

            base.Draw(sb);
        }

        public void CreateDamageBar(string playerName, int playerWhoAmI)
        {
            // Check if the bar already exists
            if (players.ContainsKey(playerName))
                return;

            // Create a new bar
            DamageBarElement bar = new(currentYOffset, playerName, playerWhoAmI);

            Append(bar);
            players[playerName] = bar;

            ModContent.GetInstance<DPSPanel>().Logger.Info($"Creating damage bar for {playerName}");

            currentYOffset += ItemHeight + ITEM_PADDING * 2; // Adjust Y offset for the next element
            ResizePanelHeight();
        }

        public void UpdateDamageBars(string playerName, int playerDamage, int playerWhoAmI)
        {
            if (!players.ContainsKey(playerName))
            {
                CreateDamageBar(playerName, playerWhoAmI);
                return;
            }

            // Update the player's damage in the DamageBarElement
            var bar = players[playerName];
            bar.PlayerDamage = playerDamage;

            // Sort players by damage in descending order
            var sortedPlayers = players
                .OrderByDescending(p => p.Value.PlayerDamage)
                .ToList();

            // Reset Y offset for re-sorting
            currentYOffset = bossHeaderHeight;

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

                ModContent.GetInstance<DPSPanel>().Logger.Info($"[Client] Updating bar: {playerName}, Damage: {playerDamage}, Percentage: {percentageToFill}");

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
            damageBars.Clear(); // Clear the weapon bars dictionary
            currentYOffset = bossHeaderHeight; // Reset the Y offset
            ResizePanelHeight();
        }

        private void ResizePanelHeight()
        {
            // set parent container height
            if (Parent is not BossContainerElement parentContainer)
                return; // Exit if parentContainer is not set
            parentContainer.Height.Set(currentYOffset + ITEM_PADDING, 0f);
            parentContainer.Recalculate();

            // debug to logger the current height
            ModContent.GetInstance<DPSPanel>().Logger.Info($"Panel.cs: Setting Panel height: {currentYOffset + ITEM_PADDING}");

            Height.Set(currentYOffset + ITEM_PADDING, 0f);
            Recalculate();
        }

        #region Singleplayer
        public void CreateWeaponDamageBar(string barName)
        {
            if (!damageBars.ContainsKey(barName))
            {
                WeaponBarElement bar = new(currentYOffset);
                Append(bar);
                damageBars[barName] = bar;

                ModContent.GetInstance<DPSPanel>().Logger.Info($"Creating weapon bar for {barName}");

                // Removed the following lines to prevent double resizing and Y-offset changes
                // currentYOffset += ItemHeight + ITEM_PADDING * 2;
                // ResizePanelHeight();
            }
        }

        public void UpdateWeaponDamageBars(List<Weapon> weapons)
        {
            // Set currentYOffset to after all player damage bars
            currentYOffset = bossHeaderHeight + (players.Count * (ItemHeight + ITEM_PADDING * 2));

            if (weapons == null || weapons.Count == 0)
            {
                ResizePanelHeight();
                return;
            }

            // Sort weapons by descending damage.
            int highest = weapons.FirstOrDefault()?.damage ?? 1;

            for (int i = 0; i < weapons.Count; i++)
            {
                var wpn = weapons[i];

                // Check if the bar already exists
                if (!damageBars.ContainsKey(wpn.weaponName))
                {
                    CreateWeaponDamageBar(wpn.weaponName);
                    // Removed: return;
                }

                WeaponBarElement bar = damageBars[wpn.weaponName];

                // Update with the current data.
                int percentageToFill = (int)(wpn.damage / (float)highest * 100);
                Color color = PanelColors.colors[i % PanelColors.colors.Length];

                bar.UpdateDamageBar(percentageToFill, wpn.weaponName, wpn.damage, wpn.weaponItemID, color);
                bar.Top.Set(currentYOffset, 0f);
                currentYOffset += ItemHeight + ITEM_PADDING * 2;
            }

            // Call ResizePanelHeight once after all updates
            ResizePanelHeight();
        }
        #endregion
    }
}