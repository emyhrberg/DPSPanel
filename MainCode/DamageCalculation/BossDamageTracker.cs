using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace DPSPanel.MainCode.Panel
{
    public class BossDamageTracker : ModPlayer
    {
        public class BossFight
        {
            public string bossName;
            public int damageTaken;
            public Dictionary<string, Players> players;
        }

        public class Players
        {
            public Dictionary<string, Weapons> weapons;
        }

        public class Weapons
        {
            public int damage;
        }

        // Create new dictionary of bossfights.
        // Int: ID of the boss, BossFight contains all the data
        public Dictionary<int, BossFight> bossFights = new Dictionary<int, BossFight>();

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            // melee damage done to boss
            TrackBossDamage(item.Name, damageDone, target);
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            // projectile damage done to boss
            TrackBossDamage(proj.Name, damageDone, target);
        }

        private void TrackBossDamage(string weaponName, int damageDone, NPC npc)
        {
            // check if npc is friendly
            if (!npc.boss | npc.friendly)
            {
                return;
            }

            // create new entry if its a new boss id
            int id = npc.whoAmI; // BOSS ID 
            if (!bossFights.ContainsKey(id))
            {
                bossFights.Add(id, new BossFight()
                {
                    bossName = npc.FullName,
                    damageTaken = 0,
                    players = new Dictionary<string, Players>() // init empty players list
                });
            }

            // create new player entry
            string playerName = Main.LocalPlayer.name;
            if (!bossFights[id].players.ContainsKey(playerName))
            {
                bossFights[id].players.Add(playerName, new Players()
                {
                    weapons = new Dictionary<string, Weapons>() // init empty weapons list
                });
            }

            // create new weapon entry
            if (!bossFights[id].players[playerName].weapons.ContainsKey(weaponName))
            {
                bossFights[id].players[playerName].weapons.Add(weaponName, new Weapons()
                {
                    damage = 0 // init damage to 0 for the new weapon
                });
            }

            // add damage to the weapon
            bossFights[id].players[playerName].weapons[weaponName].damage += damageDone;

            // add damage to the boss
            bossFights[id].damageTaken += damageDone;

            // print the boss fight
            printBossFight();
        }

        private void printBossFight()
        {
            // Layout:
            // Boss1Name
            // Player1Name
            // Weapon1Name: Damage
            // Weapon2Name: Damage
            // Player2Name
            // Weapon1Name: Damage

            foreach (var boss in bossFights)
            {
                Main.NewText($"Boss: {boss.Value.bossName}");
                foreach (var player in boss.Value.players)
                {
                    Main.NewText($"Player: {player.Key}");
                    foreach (var weapon in player.Value.weapons)
                    {
                        Main.NewText($"{weapon.Key}: {weapon.Value.damage}");
                    }
                }
            }

        }
    }
}
