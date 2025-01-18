using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using System.Linq;

namespace DPSPanel.MainCode.Panel
{
    public class BossDamageTrackerMP : ModPlayer
    {

        // --------------------------------------------------------------------------------
        // Classes
        // --------------------------------------------------------------------------------

        // Class for storing the details for a boss fight
        public class BossFight
        {
            public required int bossId;            // incremental id 0, 1, 2, ...
            public int initialLife;
            public string bossName;
            public int damageTaken;
            public HashSet<CustomPlayer> players = [];
        }

        // Class for storing a player's data for a boss fight
        public class CustomPlayer
        {
            public required string playerName;
            public int totalDamage;
            public HashSet<Weapons> weapons = [];
        }

        // Class for storing weapon data
        public class Weapons
        {
            public required string weaponName;
            public int damage;
        }

        // Local storage for the boss fights.
        // Key: bossId, Value: BossFight
        public Dictionary<int, BossFight> fights = [];

        // Incremental counter for each fight (0, 1, 2, ...)
        private int fightId = 0;

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            TrackBossDamage(item.Name, damageDone, target);
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            TrackBossDamage(proj.Name, damageDone, target);
        }

        private void TrackBossDamage(string weaponName, int damageDone, NPC npc)
        {
            // Only track real boss fights
            if (!npc.boss || npc.friendly)
                return;

            if (npc.life <= 0)
            {
                fightId++;
                return;
            }

            // Create the boss fight if it does not exist
            BossFight fight = null;

            if (!fights.ContainsKey(fightId)) // dictionary key indexer
            {
                // Add the weapon to the player
                Weapons weapon = new()
                {
                    weaponName = weaponName,
                    damage = damageDone
                };

                // Add the player to the fight
                CustomPlayer player = new()
                {
                    playerName = Main.LocalPlayer.name,
                    totalDamage = damageDone,
                    weapons = [weapon]
                };

                // Create the fight object
                fight = new BossFight
                {
                    bossId = fightId,
                    bossName = npc.FullName,
                    initialLife = npc.lifeMax,
                    damageTaken = damageDone,
                    players = [player]
                };
            }
            else
            {
                fight = fights[fightId];
                fight.damageTaken += damageDone;

                // Check if the player is already in the fight. use one liner
                CustomPlayer player = fight.players.FirstOrDefault(p => p.playerName == Main.LocalPlayer.name);
                if (player != null)
                {

                }

            }


        }
    }
}
