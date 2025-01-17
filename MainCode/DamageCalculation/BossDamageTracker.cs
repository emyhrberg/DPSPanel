using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.ID;
using log4net.Repository.Hierarchy; // We'll use Microsoft.Xna.Framework.Color
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
            public List<MyPlayer> players = [];
        }

        public class MyPlayer
        {
            public string playerName;         // ID = playerName
            public List<Weapons> weapons = [];
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
        public Dictionary<int, BossFight> clientBossFights = new Dictionary<int, BossFight>();

        // increment this each time we start a new fight
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

            // Create BossFight if it doesn't exist
            if (!clientBossFights.ContainsKey(bossIdCounter))
            {
                clientBossFights[bossIdCounter] = new BossFight
                {
                    bossId = bossIdCounter,
                    bossName = npc.FullName,
                    initialLife = npc.lifeMax,
                    damageTaken = 0
                };
            }

            // Add damage to the current fight
            var currentFight = clientBossFights[bossIdCounter];
            currentFight.damageTaken += damageDone;

            // Create Player if it doesn't exist
            string playerName = Main.LocalPlayer.name;
            var player = currentFight.players.FirstOrDefault(p => p.playerName == playerName) ?? new MyPlayer { playerName = playerName };
            if (!currentFight.players.Contains(player)) 
                currentFight.players.Add(player);

            // Create Weapon if it doesnt exist
            var weapon = player.weapons.FirstOrDefault(w => w.weaponName == weaponName) ?? new Weapons { weaponName = weaponName };
            if (!player.weapons.Contains(weapon)) 
                player.weapons.Add(weapon);

            // Update the weapon damage
            weapon.damage += damageDone;
            SendBossUpdateToServer(currentFight);
        }

        // -------------------------------------------------
        // Fix the final blow discrepancy
        // -------------------------------------------------
        private void FixFinalBlowDiscrepancy()
        {
            if (!clientBossFights.ContainsKey(bossIdCounter))
                return; // check if there even exists a fight to fix

            var currFight = clientBossFights[bossIdCounter];

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
        }

        // -------------------------------------------------
        // Send to DPS Panel
        // -------------------------------------------------

        private void SendBossUpdateToServer(BossFight fight)
        {
            // Disable if not in multiplayer
            if (Main.netMode != NetmodeID.MultiplayerClient) 
                return;

            ModPacket packet = Mod.GetPacket();
            packet.Write(fight.bossId);
            packet.Write(fight.bossName);
            packet.Write(fight.initialLife);
            packet.Write(fight.damageTaken);
            packet.Write(fight.players.Count);

            // Add player and weapon data
            foreach (var p in fight.players)
            {
                packet.Write(p.playerName);
                packet.Write(p.weapons.Count);
                foreach (var weapon in p.weapons)
                {
                    packet.Write(weapon.weaponName);
                    packet.Write(weapon.damage);
                }
            }
            // get logger
            Mod.Logger.Info("Sending Boss Fight Data to Server");
            packet.Send(); // Send to the server
        }

        private void printBossFights()
        {
            foreach (var kvp in clientBossFights)
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
