using System.Collections.Generic;
using System.Linq;
using DPSPanel.Core.Configs;
using DPSPanel.Core.DamageCalculation;
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
        private readonly float PANEL_HEIGHT = 40f; // is reset anyways by parent
        private readonly Color panelColor = new(49, 84, 141);
        private float currentYOffset = 0;
        private const float ItemHeight = 16f; // size of each item

        // multiplayer: damageBars: Key = each individual playerName, Value = DamageBarElement
        private Dictionary<string, PlayerBar> players = [];

        // singleplayer: weaponBars: Key = each individual weaponName, Value = WeaponBarElement
        private Dictionary<string, WeaponBar> damageBars = [];

        // bossTitle: Title of the boss
        public int CurrentBossID;
        public string CurrentBossName;
        private const float bossHeaderHeight = 28f;
        public BossHead bossIcon = new();
        public static Panel Instance;

        public Panel()
        {
            Instance = this;
            Config c = ModContent.GetInstance<Config>();
            if (c.BarWidth == 150)
                Width.Set(150, 0f);
            else if (c.BarWidth == 300)
                Width.Set(300, 0f);

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

            // Add the text to the panel
            UIText bossTitle = new(bossName, 1.0f);
            bossTitle.HAlign = 0.5f;
            bossTitle.Top.Set(6f, 0f);
            Append(bossTitle);

            // Add the boss icon
            Config c = ModContent.GetInstance<Config>();
            if (bossID != -1 && c.ShowBossIcon)
                SetBossIcon(bossID);

            // Resize panel height
            currentYOffset = bossHeaderHeight;
            ResizePanelHeight();
        }

        public void SetBossIcon(int bossHeadId)
        {
            // Create the boss icon
            // left offset based on slider width
            Config c = ModContent.GetInstance<Config>();

            if (c.BarWidth == 150)
                bossIcon.Left.Set(60f, 0f);
            else if (c.BarWidth == 300)
                bossIcon.Left.Set(100f, 0f);

            bossIcon.UpdateBossIcon(bossHeadId);
            Append(bossIcon);
        }

        public static void UpdateBarWidth(Config c)
        {
            if (Instance == null)
                return;
            if (c.BarWidth == 150)
                Instance.bossIcon.Left.Set(60f, 0f);
            else if (c.BarWidth == 300)
                Instance.bossIcon.Left.Set(60f, 0f);
        }

        public override void Draw(SpriteBatch sb)
        {
            Config c = ModContent.GetInstance<Config>();

            if (!Main.playerInventory && !c.ShowOnlyWhenInventoryOpen)
                return;

            base.Draw(sb);
        }

        public void CreateDamageBar(string playerName, int playerWhoAmI)
        {
            // Check if the bar already exists
            if (players.ContainsKey(playerName))
                return;

            // Create a new bar
            PlayerBar bar = new(currentYOffset, playerName, playerWhoAmI);

            Append(bar);
            players[playerName] = bar;

            Log.Info($"Creating damage bar for {playerName}");

            currentYOffset += ItemHeight + ITEM_PADDING * 2; // Adjust Y offset for the next element
            ResizePanelHeight();
        }

        public void UpdateDamageBars(string playerName, int playerDamage, int playerWhoAmI)
        {
            if (!players.ContainsKey(playerName))
                CreateDamageBar(playerName, playerWhoAmI);

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
                PlayerBar currentBar = sortedPlayers[i].Value;

                // Update bar position to match the sorted order
                currentBar.Top.Set(currentYOffset, 0f);
                currentYOffset += ItemHeight + ITEM_PADDING * 2;

                // Calculate fill percentage safely
                int percentageToFill = highestDamage > 0
                    ? (int)(currentBar.PlayerDamage / (float)highestDamage * 100)
                    : 0;

                Log.Info($"[Client] Updating bar: {playerName}, Damage: {playerDamage}, Percentage: {percentageToFill}");

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
            if (Parent is not MainContainer parentContainer)
                return; // Exit if parentContainer is not set
            parentContainer.Height.Set(currentYOffset + ITEM_PADDING, 0f);
            parentContainer.Recalculate();

            // debug to logger the current height
            Log.Info($"Panel.cs: Setting Panel height: {currentYOffset + ITEM_PADDING}");

            Height.Set(currentYOffset + ITEM_PADDING, 0f);
            Recalculate();
        }

        #region Singleplayer
        public void CreateWeaponDamageBar(string barName)
        {
            if (!damageBars.ContainsKey(barName))
            {
                WeaponBar bar = new(currentYOffset);
                Append(bar);
                damageBars[barName] = bar;

                Log.Info($"Creating weapon bar for {barName}");

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

                WeaponBar bar = damageBars[wpn.weaponName];

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