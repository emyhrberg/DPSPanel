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
        private Dictionary<int, PlayerBar> playerBars = new Dictionary<int, PlayerBar>();

        // (For singleplayer weapon bars – not used in MP if each player has its own damage panel.)
        private Dictionary<string, WeaponBar> weaponBars = new Dictionary<string, WeaponBar>();

        // Dictionary for damage panels.
        private Dictionary<string, PlayerDamagePanel> damagePanels = new Dictionary<string, PlayerDamagePanel>();

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

            bossIcon.SetBossHeadID(bossHeadId);
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
            if (playerBars.ContainsKey(playerWhoAmI))
            {
                Log.Info($"[MainPanel.CreatePlayerBar] Skipping creation for player '{playerName}' (ID: {playerWhoAmI}) because a PlayerBar already exists. Total PlayerBars: {playerBars.Count}");
                return;
            }
            PlayerBar bar = new PlayerBar(currentYOffset, playerName, playerWhoAmI);
            Append(bar);
            playerBars[playerWhoAmI] = bar;
            currentYOffset += ItemHeight + ITEM_PADDING * 2;
            ResizePanelHeight();
        }

        // In MP, we update each player's bar and then update its damage panel with weapon data.
        public void UpdatePlayerBars(string playerName, int playerDamage, int playerWhoAmI, List<Weapon> weapons)
        {
            Log.Info($"[MainPanel.UpdatePlayerBars] Called for player '{playerName}' (ID: {playerWhoAmI}): Damage={playerDamage}, WeaponsCount={weapons?.Count ?? 0}");

            if (!playerBars.ContainsKey(playerWhoAmI))
            {
                Log.Info($"[MainPanel.UpdatePlayerBars] No PlayerBar for '{playerName}' (ID: {playerWhoAmI}) found. Creating one.");
                CreatePlayerBar(playerName, playerWhoAmI);
            }

            // Update damage
            playerBars[playerWhoAmI].PlayerDamage = playerDamage;

            // Sort by playerWhoAmI for consistent ordering (or sort by damage if you prefer)
            var sortedPlayers = playerBars.OrderBy(p => p.Key).ToList();
            Log.Info($"[MainPanel.UpdatePlayerBars] Sorted {sortedPlayers.Count} PlayerBars. Keys: {string.Join(", ", sortedPlayers.Select(p => p.Key))}");

            currentYOffset = bossHeaderHeight;
            int highestDamage = sortedPlayers.First().Value.PlayerDamage;
            for (int i = 0; i < sortedPlayers.Count; i++)
            {
                string currentPlayerName = sortedPlayers[i].Value.PlayerName;
                PlayerBar currentBar = sortedPlayers[i].Value;
                currentBar.Top.Set(currentYOffset, 0f);
                Log.Info($"[MainPanel.UpdatePlayerBars] Setting '{currentPlayerName}' (ID: {sortedPlayers[i].Key}) PlayerBar top at offset: {currentYOffset}. (PlayerDamage = {currentBar.PlayerDamage})");
                currentYOffset += ItemHeight + ITEM_PADDING * 2;

                int percentageToFill = highestDamage > 0 ? (int)(currentBar.PlayerDamage / (float)highestDamage * 100) : 0;
                Color barColor = PanelColors.colors[i % PanelColors.colors.Length];
                Log.Info($"[MainPanel.UpdatePlayerBars] For '{currentPlayerName}' (ID: {sortedPlayers[i].Key}): percentageToFill = {percentageToFill}%, Color = {barColor}, Index = {i}");
                currentBar.UpdatePlayerBar(percentageToFill, currentPlayerName, currentBar.PlayerDamage, barColor);
            }

            Log.Info($"[MainPanel.UpdatePlayerBars] Updating weapon data for '{playerName}' (ID: {playerWhoAmI})");
            playerBars[playerWhoAmI].UpdateWeaponData(weapons);
            ResizePanelHeight();
            Log.Info($"[MainPanel.UpdatePlayerBars] Finished updating. Total PlayerBars = {playerBars.Count}, Final currentYOffset = {currentYOffset}");
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
        public void CreateWeaponBarSP(string barName)
        {
            if (!weaponBars.ContainsKey(barName))
            {
                WeaponBar bar = new WeaponBar(currentYOffset);
                Append(bar);
                weaponBars[barName] = bar;
                Log.Info($"Creating weapon bar for {barName}");
            }
        }

        public void UpdateAllWeaponBarsSP(List<Weapon> weapons)
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
                    CreateWeaponBarSP(wpn.weaponName);
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
