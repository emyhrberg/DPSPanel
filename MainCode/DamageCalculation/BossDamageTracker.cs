using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework; // We'll use Microsoft.Xna.Framework.Color
// Remove or comment out using System.Drawing; to avoid conflicts

namespace DPSPanel.MainCode.Panel
{
    public class BossDamageTracker : ModPlayer
    {
        // -------------------------------------------------
        // Classes to store boss, player, and weapons
        // -------------------------------------------------
        public class BossFight
        {
            public int bossId;                // incremental 0, 1, 2, ...
            public int initialLife;
            public string bossName;
            public int damageTaken;
            public List<MyPlayer> players = new List<MyPlayer>();
        }

        public class MyPlayer
        {
            public string playerName;         // ID = playerName
            public List<Weapons> weapons = new List<Weapons>();
        }

        public class Weapons
        {
            public string weaponName;         // ID = weaponName
            public int damage;
        }

        // -------------------------------------------------
        // Fields for Tracking
        // -------------------------------------------------

        // Dictionary of boss fights, keyed by bossId
        public Dictionary<int, BossFight> bossFights = new Dictionary<int, BossFight>();

        // We'll increment this each time we start a new fight
        private int bossIdCounter = 0;

        // Remember who dealt the last hit (so we can fix the final blow)
        private Weapons lastHitWeapon = null;
        private MyPlayer lastHitPlayer = null;

        // -------------------------------------------------
        // Terraria Hooks
        // -------------------------------------------------
        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            TrackBossDamage(item.Name, damageDone, target);
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            TrackBossDamage(proj.Name, damageDone, target);
        }

        // -------------------------------------------------
        // Main Logic
        // -------------------------------------------------
        private void TrackBossDamage(string weaponName, int damageDone, NPC npc)
        {
            // Ignore if it's not a boss or is friendly
            if (!npc.boss || npc.friendly)
                return;

            // If the boss has just died (life <= 0), fix final blow, increment counter, and return
            if (npc.life <= 0)
            {
                FixFinalBlowDiscrepancy();
                bossIdCounter++;
                //printBossFights(); // if you want to debug
                return;
            }

            // 1) If we don't already have a BossFight for bossIdCounter, create one.
            if (!bossFights.ContainsKey(bossIdCounter))
            {
                var newBossFight = new BossFight
                {
                    bossId = bossIdCounter,
                    initialLife = npc.lifeMax,
                    bossName = npc.FullName,
                    damageTaken = 0
                };
                bossFights.Add(bossIdCounter, newBossFight);
            }

            // 2) Get our current fight
            var currentFight = bossFights[bossIdCounter];

            // 3) Update total damage for the boss
            currentFight.damageTaken += damageDone;

            // 4) Find or create player entry
            string playerName = Main.LocalPlayer.name;
            var existingPlayer = currentFight.players
                .FirstOrDefault(p => p.playerName == playerName);

            if (existingPlayer == null)
            {
                existingPlayer = new MyPlayer
                {
                    playerName = playerName
                };
                currentFight.players.Add(existingPlayer);
            }

            // 5) Find or create weapon entry
            var existingWeapon = existingPlayer.weapons
                .FirstOrDefault(w => w.weaponName == weaponName);

            if (existingWeapon == null)
            {
                existingWeapon = new Weapons
                {
                    weaponName = weaponName,
                    damage = 0
                };
                existingPlayer.weapons.Add(existingWeapon);
            }

            // 6) Add damage to the weapon
            existingWeapon.damage += damageDone;

            // 7) Store references for last hit
            lastHitWeapon = existingWeapon;
            lastHitPlayer = existingPlayer;

            // 8) Update the panel each time
            UpdateDPSPanel(currentFight);
        }

        // -------------------------------------------------
        // Fix the final blow discrepancy
        // -------------------------------------------------
        private void FixFinalBlowDiscrepancy()
        {
            if (!bossFights.ContainsKey(bossIdCounter))
                return; // no fight to fix

            var currFight = bossFights[bossIdCounter];

            // If total damage != initial boss life, fix it
            int discrepancy = currFight.damageTaken - currFight.initialLife;
            if (discrepancy == 0)
                return; // no fix needed

            // overshoot if > 0, undershoot if < 0
            currFight.damageTaken -= discrepancy;

            // Adjust the last hitter's weapon
            if (lastHitWeapon != null)
            {
                lastHitWeapon.damage -= discrepancy;
                if (lastHitWeapon.damage < 0)
                    lastHitWeapon.damage = 0;
            }

            // Get players and damage and summarize like Player: Damage
            string playersDamage = string.Join(", ", currFight.players
                .Select(p => $"{p.playerName}: {p.weapons.Sum(w => w.damage)}"));

            Main.NewText(
                $"[Boss Fight {bossIdCounter} - {currFight.bossName}] Damage: " +
                $"{playersDamage}"
            );

            // Update the panel after correction
            UpdateDPSPanel(currFight);
        }

        // -------------------------------------------------
        // Send to DPS Panel
        // -------------------------------------------------
        private void UpdateDPSPanel(BossFight fight)
        {
            // If no fights or we somehow lost the current
            if (bossFights.Count == 0)
                return;

            // Max 5 bosses, then reset
            if (bossFights.Count > 5)
            {
                // Clear the dictionary
                bossFights.Clear();
                var uiSystemClear = ModContent.GetInstance<DPSPanelSystem>();
                uiSystemClear.state.dpsPanel.ClearItems();
                return;
            }

            // Access the UISystem to manage the panels
            var uiSystem = ModContent.GetInstance<DPSPanelSystem>();

            // 1) Boss line
            string bossKey = $"Boss:{fight.bossId}";
            string bossText = $"{fight.bossName} - {fight.damageTaken} damage";
            Color bossColor = new Color(255, 225, 0);
            uiSystem.state.dpsPanel.UpdateItem(bossKey, bossText, bossColor);

            // 2) Player lines
            foreach (var plr in fight.players)
            {
                string playerKey = $"{fight.bossId}|Player:{plr.playerName}";
                int playerTotal = plr.weapons.Sum(w => w.damage);
                string playerText = $"  {plr.playerName} - {playerTotal} damage";
                Color playerColor = new Color(85, 255, 85);
                uiSystem.state.dpsPanel.UpdateItem(playerKey, playerText, playerColor);

                // 3) Weapon lines
                foreach (var wpn in plr.weapons)
                {
                    string weaponKey = $"Boss:{fight.bossId}|Player:{plr.playerName}|Weapon:{wpn.weaponName}";
                    string weaponText = $"    {wpn.weaponName} - {wpn.damage} damage";
                    Color weaponColor = new Color(115, 195, 255);
                    uiSystem.state.dpsPanel.UpdateItem(weaponKey, weaponText, weaponColor);
                }
            }
        }

        private void printBossFights()
        {
            foreach (var kvp in bossFights)
            {
                var bossFight = kvp.Value;
                Main.NewText($"Boss ID: {bossFight.bossId} - {bossFight.bossName} - Damage: {bossFight.damageTaken}");

                foreach (var player in bossFight.players)
                {
                    Main.NewText($"  Player: {player.playerName}");
                    foreach (var weapon in player.weapons)
                    {
                        Main.NewText($"    Weapon: {weapon.weaponName} - Damage: {weapon.damage}");
                    }
                }
            }
        }
    }
}
