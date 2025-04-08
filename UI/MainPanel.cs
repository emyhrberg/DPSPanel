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
        private float PANEL_PADDING = 5f;
        private float BAR_SPACING = 10f;
        private readonly Color panelColor = new Color(49, 84, 141);

        // Adding new bars will increase the height of the panel.
        public float ItemHeight = 16f;
        private float bossHeaderHeight = 40f;
        public float currentYOffset = 40f;

        // Dictionary for player bars.
        private Dictionary<int, PlayerBar> playerBars = [];
        public Dictionary<int, PlayerBar> PlayerBars => playerBars;

        // (For singleplayer weapon bars – not used in MP if each player has its own damage panel.)
        private Dictionary<string, WeaponBar> weaponBars = [];
        public Dictionary<string, WeaponBar> WeaponBars => weaponBars;

        public bool CurrentBossAlive = false; // their health. updated in PacketHandler.
        public int CurrentBossWhoAmI;
        public int CurrentBossID;
        public string CurrentBossName;
        public BossHead bossIcon = new BossHead();

        // Show only when inventory open
        public bool HideWhenInventoryOpen = true;

        #region Constructor
        public MainPanel()
        {
            // Convert from string to float using the dictionary to set the width.
            float width = SizeHelper.GetWidthFromConfig();
            Width.Set(width, 0f);

            // Set other properties of the panel.
            Height.Set(bossHeaderHeight, 0f);
            BackgroundColor = panelColor;
            HAlign = 0.5f;
            SetPadding(PANEL_PADDING);
        }
        #endregion

        public void SetBossTitle(string bossName, int bossWhoAmI, int bossID)
        {
            CurrentBossWhoAmI = bossWhoAmI;
            CurrentBossID = bossID;
            CurrentBossName = bossName;

            // Change the text size based on the boss name length.
            // float textSize = bossName.Length > 20 ? 0.75f : 1.0f;
            float textSize = 1.0f;

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

        public void SetBossIcon(int bossHeadId)
        {
            bossIcon.SetBossHeadID(bossHeadId);
            Append(bossIcon);
        }

        public override void Draw(SpriteBatch sb)
        {
            // hot reload here
            //Height.Set(50, 0);
            BAR_SPACING = 10f;
            //Log.SlowInfo("height" + Height.Pixels);

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

            // Skip bar spacing here.
            // currentYOffset += ItemHeight + BAR_SPACING;
            // ResizePanelHeight();
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

            // Reset the current Y offset to the top of the panel.
            currentYOffset = bossHeaderHeight;

            int highestDamage = sortedPlayers.First().Value.PlayerDamage;
            for (int i = 0; i < sortedPlayers.Count; i++)
            {
                string currentPlayerName = sortedPlayers[i].Value.PlayerName;
                PlayerBar currentBar = sortedPlayers[i].Value;

                // Update the bar's position and height:
                currentBar.Top.Set(currentYOffset, 0f);
                currentYOffset += ItemHeight;

                int percentageToFill = highestDamage > 0 ? (int)(currentBar.PlayerDamage / (float)highestDamage * 100) : 0;

                // Use the sorted index to assign a color.
                Color barColor = Color.White; // Default color
                if (Conf.C.BarColors == "Rainbow")
                {
                    barColor = ColorHelper.rainbowColors()[i % ColorHelper.rainbowColors().Length];
                }
                else
                {
                    barColor = ColorHelper.standardColors[i % ColorHelper.standardColors.Length];
                }

                // Update the player bar with the new values.
                currentBar.UpdatePlayerBar(percentageToFill, currentPlayerName, currentBar.PlayerDamage, barColor);
            }

            if (sortedPlayers.Count > 0)
                currentYOffset += 5f;

            playerBars[playerWhoAmI].UpdateWeaponData(weapons);

            // Update the panels height after all bars are updated.
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

            // If no bars, currentYOffset is just bossHeaderHeight (40).
            float finalHeight = currentYOffset;

            // If somehow finalHeight < 40, clamp it to 40 so there's always at least 40 for the header
            if (finalHeight < bossHeaderHeight)
                finalHeight = bossHeaderHeight;

            // Now we do NOT add +PANEL_PADDING, because we already added 5 after the last bar above.
            // finalHeight is now the exact size we want.

            parentContainer.Height.Set(finalHeight, 0f);
            parentContainer.Recalculate();

            Height.Set(finalHeight, 0f);
            Recalculate();
        }

        #region Rebuild panel size
        public void RebuildAllBarsLayout()
        {
            // Start offset below the boss header:
            currentYOffset = bossHeaderHeight;

            // 1) Re-position and re-draw PlayerBars
            //    If you want to keep them in damage-sorted order, you can
            //    replicate the logic in UpdatePlayerBars. For a minimal example:
            var sortedPlayers = playerBars.OrderByDescending(p => p.Value.PlayerDamage).ToList();
            int highestDamage = sortedPlayers.Count > 0 ? sortedPlayers.First().Value.PlayerDamage : 1;

            for (int i = 0; i < sortedPlayers.Count; i++)
            {
                PlayerBar bar = sortedPlayers[i].Value;

                bar.Top.Set(currentYOffset, 0f);
                currentYOffset += ItemHeight + BAR_SPACING;

                // Re-calculate the fill percentage:
                int percent = highestDamage > 0 ?
                    (int)(bar.PlayerDamage / (float)highestDamage * 100) : 0;

                Color barColor = Color.White; // Default color
                if (Conf.C.BarColors == "Rainbow")
                {
                    barColor = ColorHelper.rainbowColors()[i % ColorHelper.rainbowColors().Length];
                }
                else
                {
                    barColor = ColorHelper.standardColors[i % ColorHelper.standardColors.Length];
                }
                bar.UpdatePlayerBar(percent, bar.PlayerName, bar.PlayerDamage, barColor);

                // Update leftoffset for the player damage panel to the width of the config
                Config c = ModContent.GetInstance<Config>();
                float width = SizeHelper.GetWidthFromConfig();
                float height = SizeHelper.HeightSizes[c.BarHeight];

                // Change left offset for player damage panel
                bar.playerDamagePanel.UpdateLeft(width);
                bar.playerDamagePanel.UpdatePanelWidth(width);
                bar.playerDamagePanel.UpdateBarHeight(height);
            }

            // 2) Re-position the singleplayer weapon bars (if you’re using them)
            //    We'll just re-stack them in the order they exist:
            foreach (WeaponBar wBar in weaponBars.Values)
            {
                wBar.Top.Set(currentYOffset, 0f);
                currentYOffset += ItemHeight;
                // We do NOT recalc fill percentages here unless you also want to.
            }

            // 3) Finally, adjust the panel's total height
            ResizePanelHeight();
        }
        #endregion

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
            currentYOffset = bossHeaderHeight + (playerBars.Count * (ItemHeight + BAR_SPACING));
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

                // Calculate the percentage to fill the bar based on the weapon's damage.
                int percentageToFill = (int)(wpn.damage / (float)highest * 100);
                Color color = Color.White; // Default color
                if (Conf.C.BarColors == "Rainbow")
                {
                    color = ColorHelper.rainbowColors()[i % ColorHelper.rainbowColors().Length];
                }
                else
                {
                    color = ColorHelper.standardColors[i % ColorHelper.standardColors.Length];
                }

                // Update the weapon bar with the new values.
                WeaponBar bar = weaponBars[wpn.weaponName];
                bar.UpdateWeaponBar(percentageToFill, wpn.weaponName, wpn.damage, wpn.weaponItemID, color);
                bar.Top.Set(currentYOffset, 0f);
                currentYOffset += ItemHeight + BAR_SPACING;
            }
            ResizePanelHeight();
        }
        #endregion
    }
}
