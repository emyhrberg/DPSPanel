using System.Collections.Generic;
using System.Linq;
using DPSPanel.Common.Configs;
using DPSPanel.DamageCalculation.Classes;
using DPSPanel.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;

namespace DPSPanel.UI
{
    public class MainPanel : UIPanel
    {
        private float BAR_SPACING = 10f;
        private readonly Color panelColor = new(49, 84, 141);

        private float bossHeaderHeight = 40f;

        // Player bars (MP or SP)
        private readonly Dictionary<int, PlayerBar> playerBars = new();
        public Dictionary<int, PlayerBar> PlayerBars => playerBars;

        // Singleplayer weapon bars
        private readonly Dictionary<string, WeaponBar> weaponBars = new();
        public Dictionary<string, WeaponBar> WeaponBars => weaponBars;

        public bool CurrentBossAlive = false;
        public int CurrentBossWhoAmI;
        public int CurrentBossID;
        public string CurrentBossName;

        // Boss icon is optional
        public BossHead bossIcon = new();
        public bool HideWhenInventoryOpen = true;

        #region Constructor
        public MainPanel()
        {
            float width = SizeHelper.GetWidthFromConfig();
            Width.Set(width, 0f);

            // Default height is just enough for the boss header
            Height.Set(bossHeaderHeight, 0f);
            BackgroundColor = panelColor;
            HAlign = 0.5f;
            SetPadding(5f);
        }
        #endregion

        #region Boss Setup
        public void SetBossTitle(string bossName, int bossWhoAmI, int bossID)
        {
            CurrentBossWhoAmI = bossWhoAmI;
            CurrentBossID = bossID;
            CurrentBossName = bossName;

            // Display the boss's name at the top
            UIText bossTitle = new(bossName, 1.0f)
            {
                HAlign = 0.52f,
                Top = { Pixels = 6f }
            };
            Append(bossTitle);

            // Optionally add boss icon
            Config c = ModContent.GetInstance<Config>();
            if (bossID != -1 && c.ShowBossIcon)
            {
                // Log.Info("Setting boss icon: " + bossID);
                SetBossIcon(bossID);
            }

            // Set panel height to 40
            Height.Set(bossHeaderHeight, 0f);
            Recalculate();
            if (Parent is MainContainer parent)
            {
                parent.Height.Set(bossHeaderHeight, 0f);
                parent.Recalculate(); // Recalculate the parent to ensure it fits the new height
            }
        }

        public void SetBossIcon(int bossHeadId)
        {
            bossIcon.SetBossHeadID(bossHeadId);
            Append(bossIcon);
        }
        #endregion

        #region Drawing & Visibility
        public override void Draw(SpriteBatch spriteBatch)
        {
            // Hide the panel if inventory is closed and HideWhenInventoryOpen = false
            // (meaning we only want to see it with the inventory open).
            if (!Main.playerInventory && !HideWhenInventoryOpen)
                return;

            base.Draw(spriteBatch);
        }
        #endregion

        #region Player Bars (used in SP or MP)
        public void CreatePlayerBar(string playerName, int playerWhoAmI)
        {
            if (!playerBars.ContainsKey(playerWhoAmI))
            {
                PlayerBar bar = new(playerName: playerName, playerWhoAmI: playerWhoAmI);
                Append(bar);
                playerBars[playerWhoAmI] = bar;
            }
        }

        /// <summary>
        /// Called in MP to update each player's damage & weapons. Also used in SP for the local player.
        /// </summary>
        public void UpdatePlayerBars(string playerName, int playerDamage, int playerWhoAmI, List<Weapon> weapons)
        {
            if (!playerBars.ContainsKey(playerWhoAmI))
                CreatePlayerBar(playerName, playerWhoAmI);

            playerBars[playerWhoAmI].PlayerDamage = playerDamage;

            // Sort players by descending damage
            var sortedPlayers = playerBars.OrderByDescending(p => p.Value.PlayerDamage).ToList();
            int highestDamage = sortedPlayers.First().Value.PlayerDamage;

            // Reposition each player bar in sorted order
            float yOffset = bossHeaderHeight;
            for (int i = 0; i < sortedPlayers.Count; i++)
            {
                PlayerBar pbar = sortedPlayers[i].Value;
                int percentage = (highestDamage > 0)
                    ? (int)((pbar.PlayerDamage / (float)highestDamage) * 100)
                    : 0;

                // Optionally pick a color
                Color barColor = ColorHelper.standardColors[i % ColorHelper.standardColors.Length];

                // Update bar text/position
                float ItemHeight = SizeHelper.HeightSizes["Medium"];
                pbar.UpdatePlayerBar(percentage, pbar.PlayerName, pbar.PlayerDamage, barColor);
                pbar.Top.Set(yOffset, 0f);
                yOffset += ItemHeight + BAR_SPACING;

                // If we track per‐player weapons in MP, you could update them here or skip.
                pbar.UpdateWeaponData(weapons);
            }

            // Finally, resize the panel to fit the new arrangement of player bars (MP or SP).
            ResizePanelHeight();
        }

        /// <summary>
        /// Clears all bars and resets the panel to the basic boss header only.
        /// </summary>
        public void ClearPanelAndAllItems()
        {
            RemoveAllChildren();
            playerBars.Clear();
            weaponBars.Clear();
            Append(bossIcon); // re‐append bossIcon if you still want it shown
        }
        #endregion

        #region Singleplayer Weapon Bars
        /// <summary>
        /// Creates a singleplayer weapon bar if it doesn’t already exist.
        /// </summary>
        public void CreateWeaponBarSP(string barName)
        {
            if (!weaponBars.ContainsKey(barName))
            {
                WeaponBar bar = new();
                Append(bar);
                weaponBars[barName] = bar;
            }
        }

        /// <summary>
        /// Updates (or creates) the weapon bars in singleplayer, then resizes the panel.
        /// </summary>
        public void UpdateAllWeaponBarsSP(List<Weapon> weapons)
        {
            // Optionally remove old bars from the UI so we can completely refresh their layout.
            // If you want to keep them in memory instead, you can just reposition them instead of clearing.
            foreach (var kvp in weaponBars)
            {
                var oldBar = kvp.Value;
                oldBar.Remove();
            }
            weaponBars.Clear();

            // If no weapons, just resize (to show nothing).
            if (weapons == null || weapons.Count == 0)
            {
                ResizePanelHeight();
                return;
            }

            // Sort them by damage descending so highest is first.
            weapons = weapons.OrderByDescending(w => w.damage).ToList();

            // Figure out the highest damage for percentage calculation.
            int highest = (weapons[0].damage > 0) ? weapons[0].damage : 1;

            // Start layout below any player bars already in place.
            float ItemHeight = SizeHelper.HeightSizes["Medium"];
            float yOffset = bossHeaderHeight + (playerBars.Count * (ItemHeight + BAR_SPACING));

            for (int i = 0; i < weapons.Count; i++)
            {
                var w = weapons[i];
                int percentage = (int)((w.damage / (float)highest) * 100);
                Color barColor = ColorHelper.standardColors[i % ColorHelper.standardColors.Length];

                // Create a new bar (or reuse existing if you prefer).
                WeaponBar bar = new WeaponBar();
                bar.UpdateWeaponBar(percentage, w.weaponName, w.damage, w.weaponItemID, barColor);

                // Position it.
                bar.Top.Set(yOffset, 0f);
                bar.Left.Set(0f, 0f);
                Append(bar);

                // Track it in the dictionary so you can find it later.
                weaponBars[w.weaponName] = bar;

                // Move yOffset down for the next bar.
                yOffset += ItemHeight + BAR_SPACING;
            }

            // Finally, recalc panel height to fit the new bars.
            ResizePanelHeight();
        }
        #endregion

        #region Resizing Logic
        /// <summary>
        /// Recalculates the final panel height based on the number of bars (players + weapons),
        /// with separate singleplayer vs. multiplayer logic if desired.
        /// </summary>
        public void ResizePanelHeight()
        {
            // Base minimum: boss header area
            float finalHeight = bossHeaderHeight;

            float ItemHeight = SizeHelper.HeightSizes["Medium"];

            if (Main.netMode == NetmodeID.SinglePlayer)
            {
                // Add space for each player bar
                finalHeight += playerBars.Count * (ItemHeight + BAR_SPACING);

                // Add space for each weapon bar
                finalHeight += weaponBars.Count * (ItemHeight + BAR_SPACING);

                // Some bottom padding
                finalHeight += 5f;
            }
            else
            {
                // Multiplayer logic (if you want it different):
                // Just add space for each player bar (e.g. ignoring weapon bars in MP)
                finalHeight += playerBars.Count * (ItemHeight + BAR_SPACING);
                finalHeight += 5f;
            }

            // Don’t let finalHeight be less than the bossHeaderHeight
            if (finalHeight < bossHeaderHeight)
                finalHeight = bossHeaderHeight;

            Height.Set(finalHeight, 0f);
            Recalculate();
            if (Parent is MainContainer parent)
            {
                parent.Height.Set(finalHeight, 0f);
                parent.Recalculate(); // Recalculate the parent to ensure it fits the new height
            }
            // Log.Info("mainpanel height: " + Height.Pixels);
        }
        #endregion
    }
}
