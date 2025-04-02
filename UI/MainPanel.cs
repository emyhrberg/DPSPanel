using System.Collections.Generic;
using System.Linq;
using DPSPanel.Common.Configs;
using DPSPanel.Common.DamageCalculation;
using DPSPanel.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.ModLoader;
using static DPSPanel.Common.Configs.Config;

namespace DPSPanel.UI
{
    public class MainPanel : UIPanel
    {
        private readonly float PANEL_PADDING = 5f;
        private readonly float ITEM_PADDING = 10f;
        private readonly float PANEL_HEIGHT = 40f;
        private readonly Color panelColor = new Color(49, 84, 141);

        // Adding new bars will increase the height of the panel.
        public float currentYOffset = 0f;
        public float ItemHeight = 16f;

        // Dictionary for player bars.
        private Dictionary<int, PlayerBar> playerBars = [];

        // (For singleplayer weapon bars – not used in MP if each player has its own damage panel.)
        private Dictionary<string, WeaponBar> weaponBars = [];
        public Dictionary<string, WeaponBar> WeaponBars => weaponBars;

        public bool CurrentBossAlive = false; // their health. updated in PacketHandler.
        public int CurrentBossWhoAmI;
        public int CurrentBossID;
        public string CurrentBossName;
        private const float bossHeaderHeight = 28f;
        public BossHead bossIcon = new BossHead();

        // Show only when inventory open
        public bool HideWhenInventoryOpen = true;

        public MainPanel()
        {
            // Convert from string to float using the dictionary to set the width.
            Config c = ModContent.GetInstance<Config>();
            string widthSize = c.Width;
            float width = 150; // default
            if (SizeHelper.WidthSizes.ContainsKey(widthSize))
            {
                width = SizeHelper.WidthSizes[widthSize];
            }
            Width.Set(width, 0f);

            // Set other properties of the panel.
            Height.Set(PANEL_HEIGHT, 0f);
            BackgroundColor = panelColor;
            HAlign = 0.5f;
            SetPadding(PANEL_PADDING);
        }

        public void SetBossTitle(string bossName, int bossWhoAmI, int bossID)
        {
            CurrentBossWhoAmI = bossWhoAmI;
            CurrentBossID = bossID;
            CurrentBossName = bossName;

            // Change the text size based on the boss name length.
            float textSize = bossName.Length > 14 ? 0.75f : 1.0f;

            UIText bossTitle = new(bossName, textSize);
            bossTitle.HAlign = 0.52f;
            bossTitle.Top.Set(6f, 0f);
            Append(bossTitle);

            Config c = ModContent.GetInstance<Config>();

            if (bossID != -1 && c.ShowBossIcon)
                SetBossIcon(bossID);

            currentYOffset = bossHeaderHeight;
            ResizePanelHeight();
        }

        public void SetCurrentBossAliveFlag(bool isAlive)
        {
            CurrentBossAlive = isAlive;
        }

        public void SetBossIcon(int bossHeadId)
        {
            bossIcon.SetBossHeadID(bossHeadId);
            Append(bossIcon);
        }

        public override void Draw(SpriteBatch sb)
        {
            //Log.Info("Width: " + Width.Pixels);

            //Width.Set(400, 0);
            //Height.Set(300, 0);
            //Log.Info("currYOffset: " + currentYOffset);
            // Log.Info("height: " + Height.Pixels);

            Config c = ModContent.GetInstance<Config>();
            if (!Main.playerInventory && !HideWhenInventoryOpen)
                return;
            base.Draw(sb);
        }

        public void CreatePlayerBar(string playerName, int playerWhoAmI)
        {
            if (playerBars.ContainsKey(playerWhoAmI))
            {
                return;
            }
            PlayerBar bar = new(currentYOffset, playerName, playerWhoAmI);
            Append(bar);
            playerBars[playerWhoAmI] = bar;
            currentYOffset += ItemHeight + ITEM_PADDING * 2;
            ResizePanelHeight();
        }

        // In MP, we update each player's bar and then update its damage panel with weapon data.
        // Updated UpdatePlayerBars function in MainPanel.cs
        public void UpdatePlayerBars(string playerName, int playerDamage, int playerWhoAmI, List<Weapon> weapons)
        {
            // // Log.Info($"[MainPanel.UpdatePlayerBars] Called for player '{playerName}' (ID: {playerWhoAmI}): Damage={playerDamage}, WeaponsCount={weapons?.Count ?? 0}");

            if (!playerBars.ContainsKey(playerWhoAmI))
            {
                // // Log.Info($"[MainPanel.UpdatePlayerBars] No PlayerBar for '{playerName}' (ID: {playerWhoAmI}) found. Creating one.");
                CreatePlayerBar(playerName, playerWhoAmI);
            }

            // Update damage for the player
            playerBars[playerWhoAmI].PlayerDamage = playerDamage;

            // Sort players by descending damage so that the highest damage is first.
            var sortedPlayers = playerBars.OrderByDescending(p => p.Value.PlayerDamage).ToList();

            currentYOffset = bossHeaderHeight;
            int highestDamage = sortedPlayers.First().Value.PlayerDamage;
            for (int i = 0; i < sortedPlayers.Count; i++)
            {
                string currentPlayerName = sortedPlayers[i].Value.PlayerName;
                PlayerBar currentBar = sortedPlayers[i].Value;
                currentBar.Top.Set(currentYOffset, 0f);
                currentYOffset += ItemHeight + ITEM_PADDING * 2;

                int percentageToFill = highestDamage > 0 ? (int)(currentBar.PlayerDamage / (float)highestDamage * 100) : 0;
                Color barColor = PanelColors.colors[i % PanelColors.colors.Length];
                currentBar.UpdatePlayerBar(percentageToFill, currentPlayerName, currentBar.PlayerDamage, barColor);
            }

            playerBars[playerWhoAmI].UpdateWeaponData(weapons);
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

        #region (Singleplayer weapon bars - not used in MP)
        public void CreateWeaponBarSP(string barName)
        {
            if (!weaponBars.ContainsKey(barName))
            {
                WeaponBar bar = new WeaponBar(currentYOffset);
                Append(bar);
                weaponBars[barName] = bar;
                // Log.Info($"Creating weapon bar for {barName}");
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
