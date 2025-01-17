using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using System.Linq;

namespace DPSPanel.MainCode.Panel
{
    public class BossDamageTracker : ModPlayer
    {
        // Class for storing the details for a boss fight
        public class BossFight
        {
            public int bossId;            // incremental id
            public int initialLife;
            public string bossName;
            public int damageTaken;
            public List<MyPlayer> players = new List<MyPlayer>();
        }

        // Class for storing a player's data for a boss fight
        public class MyPlayer
        {
            public string playerName;
            public int totalDamage;
            public List<Weapons> weapons = new List<Weapons>();
        }

        // Class for storing weapon data
        public class Weapons
        {
            public string weaponName;
            public int damage;
        }

        // Client-side boss fight storage (which sends update packets to the server)
        public Dictionary<int, BossFight> clientBossFights = new Dictionary<int, BossFight>();

        private int bossIdCounter = 0;

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
                bossIdCounter++;
                return;
            }

            // Create the boss fight if not exists, otherwise merge update.
            if (!clientBossFights.ContainsKey(bossIdCounter))
            {
                clientBossFights[bossIdCounter] = new BossFight
                {
                    bossId = bossIdCounter,
                    bossName = npc.FullName,
                    initialLife = npc.lifeMax,
                    damageTaken = damageDone,
                    players = new List<MyPlayer>()
                };
            }
            else
            {
                // Update the damage value (in a cumulative snapshot this might just be overwritten)
                clientBossFights[bossIdCounter].damageTaken += damageDone;
            }

            var currentFight = clientBossFights[bossIdCounter];
            string playerName = Main.LocalPlayer.name;

            // Find or add the player for this fight
            var player = currentFight.players.FirstOrDefault(p => p.playerName == playerName);
            if (player == null)
            {
                player = new MyPlayer { 
                    playerName = playerName, 
                    weapons = new List<Weapons>(),
                    totalDamage = 0
                };
                currentFight.players.Add(player);
            }
            player.totalDamage += damageDone; // Always update the player damage

            // Find or add the weapon entry
            var weapon = player.weapons.FirstOrDefault(w => w.weaponName == weaponName);
            if (weapon == null)
            {
                weapon = new Weapons { weaponName = weaponName, damage = damageDone };
                player.weapons.Add(weapon);
            }
            else
            {
                weapon.damage += damageDone;
            }

            // Here you would call SendBossUpdateToServer if in multiplayer.
            SendBossUpdateToServer(currentFight);
        }

        private void SendBossUpdateToServer(BossFight fight)
        {
            // Only send if running as a MultiplayerClient.
            if (Main.netMode != NetmodeID.MultiplayerClient)
                return;

            ModPacket packet = Mod.GetPacket();
            packet.Write(fight.bossId);
            packet.Write(fight.bossName);
            packet.Write(fight.initialLife);
            packet.Write(fight.damageTaken);
            packet.Write(fight.players.Count);

            foreach (var p in fight.players)
            {
                packet.Write(p.playerName);
                packet.Write(p.totalDamage);
                packet.Write(p.weapons.Count);
                foreach (var weapon in p.weapons)
                {
                    packet.Write(weapon.weaponName);
                    packet.Write(weapon.damage);
                }
            }

            Mod.Logger.Info("Sending Boss Fight Data to Server");
            packet.Send();
        }
    }
}
