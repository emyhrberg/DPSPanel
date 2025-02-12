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
    public class MainPanel : UIPanel
    {
        private readonly float PANEL_PADDING = 5f;
        private readonly float ITEM_PADDING = 10f;
        private readonly float PANEL_HEIGHT = 40f;
        private readonly Color panelColor = new Color(49, 84, 141);
        private float currentYOffset = 0f;
        private const float ItemHeight = 16f;

        // Dictionary for player bars.
        private Dictionary<string, PlayerBar> playerBars = new Dictionary<string, PlayerBar>();

        // (For singleplayer weapon bars – not used in MP if each player has its own damage panel.)
        private Dictionary<string, WeaponBar> weaponBars = new Dictionary<string, WeaponBar>();

        public int CurrentBossID;
        public string CurrentBossName;
        private const float bossHeaderHeight = 28f;
        public BossHead bossIcon = new BossHead();
        public static MainPanel Instance;

        public MainPanel()
        {
            Instance = this;
            Config c = ModContent.GetInstance<Config>();
            if (c.BarWidth == 150)
                Width.Set(150, 0f);
            else if (c.BarWidth == 300)
                Width.Set(300, 0f);

            Height.Set(PANEL_HEIGHT, 0f);
            BackgroundColor = panelColor;
            HAlign = 0.5f;
            SetPadding(PANEL_PADDING);
        }

        public void SetBossTitle(string bossName, int bossID)
        {
            CurrentBossID = bossID;
            CurrentBossName = bossName;
            UIText bossTitle = new UIText(bossName, 1.0f)
            {
                HAlign = 0.5f
            };
            bossTitle.Top.Set(6f, 0f);
            Append(bossTitle);

            Config c = ModContent.GetInstance<Config>();
            if (bossID != -1 && c.ShowBossIcon)
                SetBossIcon(bossID);

            currentYOffset = bossHeaderHeight;
            ResizePanelHeight();
        }

        public void SetBossIcon(int bossHeadId)
        {
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

        public void CreatePlayerBar(string playerName, int playerWhoAmI)
        {
            if (playerBars.ContainsKey(playerName))
                return;
            PlayerBar bar = new PlayerBar(currentYOffset, playerName, playerWhoAmI);
            Append(bar);
            playerBars[playerName] = bar;
            // Log.Info($"[MainPanel] Creating player bar for {playerName}");
            currentYOffset += ItemHeight + ITEM_PADDING * 2;
            ResizePanelHeight();
        }

        // In MP, we update each player's bar and then update its damage panel with weapon data.
        public void UpdatePlayerBars(string playerName, int playerDamage, int playerWhoAmI, List<Weapon> weapons)
        {
            // Create a new player bar if needed.
            if (!playerBars.ContainsKey(playerName))
                CreatePlayerBar(playerName, playerWhoAmI);

            // Update the player's damage.
            playerBars[playerName].PlayerDamage = playerDamage;

            // Re-sort all player bars by descending damage.
            var sortedPlayers = playerBars.OrderByDescending(p => p.Value.PlayerDamage).ToList();

            // Reset Y offset for all bars.
            currentYOffset = bossHeaderHeight;
            // Get the highest damage for fill percentage calculations.
            int highestDamage = sortedPlayers.First().Value.PlayerDamage;

            // Update each player's bar: reposition and update fill percentage/color.
            for (int i = 0; i < sortedPlayers.Count; i++)
            {
                string currentPlayerName = sortedPlayers[i].Key;
                PlayerBar currentBar = sortedPlayers[i].Value;

                // Set the vertical position.
                currentBar.Top.Set(currentYOffset, 0f);
                currentYOffset += ItemHeight + ITEM_PADDING * 2;

                // Calculate fill percentage (safely, using highest damage).
                int percentageToFill = highestDamage > 0
                    ? (int)(currentBar.PlayerDamage / (float)highestDamage * 100)
                    : 0;

                // Assign a color based on sorted order.
                Color barColor = PanelColors.colors[i % PanelColors.colors.Length];

                // Update the player bar's own drawing (text and fill).
                currentBar.UpdatePlayerBar(percentageToFill, currentPlayerName, currentBar.PlayerDamage, barColor);
            }

            // Finally, update the weapon data for the specified player's damage panel.
            // (Each PlayerBar has its own damage panel; that panel will update all its weapon bars.)
            playerBars[playerName].UpdateWeaponData(weapons);

            ResizePanelHeight();
        }

        public void ClearPanelAndAllItems()
        {
            RemoveAllChildren();
            playerBars.Clear();
            weaponBars.Clear();
            currentYOffset = bossHeaderHeight;
            ResizePanelHeight();
        }

        private void ResizePanelHeight()
        {
            if (Parent is not MainContainer parentContainer)
                return;
            parentContainer.Height.Set(currentYOffset + ITEM_PADDING, 0f);
            parentContainer.Recalculate();
            Height.Set(currentYOffset + ITEM_PADDING, 0f);
            Recalculate();
        }

        #region (Singleplayer weapon bars – not used in MP)
        public void CreateWeaponBar(string barName)
        {
            if (!weaponBars.ContainsKey(barName))
            {
                WeaponBar bar = new WeaponBar(currentYOffset);
                Append(bar);
                weaponBars[barName] = bar;
                Log.Info($"Creating weapon bar for {barName}");
            }
        }

        public void UpdateAllWeaponBars(List<Weapon> weapons)
        {
            currentYOffset = bossHeaderHeight + (playerBars.Count * (ItemHeight + ITEM_PADDING * 2));
            if (weapons == null || weapons.Count == 0)
            {
                ResizePanelHeight();
                return;
            }
            int highest = weapons.FirstOrDefault()?.damage ?? 1;
            for (int i = 0; i < weapons.Count; i++)
            {
                var wpn = weapons[i];
                if (!weaponBars.ContainsKey(wpn.weaponName))
                {
                    CreateWeaponBar(wpn.weaponName);
                }
                WeaponBar bar = weaponBars[wpn.weaponName];
                int percentageToFill = (int)(wpn.damage / (float)highest * 100);
                Color color = PanelColors.colors[i % PanelColors.colors.Length];
                bar.UpdateWeaponBar(percentageToFill, wpn.weaponName, wpn.damage, wpn.weaponItemID, color);
                bar.Top.Set(currentYOffset, 0f);
                currentYOffset += ItemHeight + ITEM_PADDING * 2;
            }
            ResizePanelHeight();
        }
        #endregion
    }
}
