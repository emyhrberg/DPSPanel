
using Terraria.ModLoader;
using System.IO;
using Terraria.ID;
using Terraria;
using DPSPanel.MainCode.Panel;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System;
using System.Linq;

namespace DPSPanel
{
    // Please read https://github.com/tModLoader/tModLoader/wiki/Basic-tModLoader-Modding-Guide#mod-skeleton-contents for more information about the various files in a mod.

    public class DPSPanel : Mod
	{
        private Dictionary<int, BossDamageTracker.BossFight> serverBossFights = new Dictionary<int, BossDamageTracker.BossFight>();

        public override void HandlePacket(BinaryReader reader, int whoAmI)
        {
            if (Main.netMode == NetmodeID.Server)
                HandleServerPacket(reader, whoAmI);
            else if (Main.netMode == NetmodeID.MultiplayerClient)
                HandleClientPacket(reader, whoAmI);
        }

        private void HandleServerPacket(BinaryReader reader, int whoAmI)
        {
            // Read the boss fight data
            int bossId = reader.ReadInt32();
            string bossName = reader.ReadString();
            int initialLife = reader.ReadInt32();
            int damageTaken = reader.ReadInt32();
            int playerCount = reader.ReadInt32();

            // Create or update the boss fight struct
            BossDamageTracker.BossFight fight;
            if (!serverBossFights.TryGetValue(bossId, out fight))
            {
                fight = new BossDamageTracker.BossFight
                {
                    bossId = bossId,
                    bossName = bossName,
                    initialLife = initialLife,
                    damageTaken = damageTaken,
                    // Create a new list for players.
                    players = new List<BossDamageTracker.MyPlayer>()
                };
                serverBossFights[bossId] = fight;
            }
            else
            {
                // If the fight already exists, update its basic values.
                fight.bossName = bossName;
                fight.initialLife = initialLife;
                fight.damageTaken = damageTaken;
            }

            // Iterate through player data
            fight.players.Clear(); // Clear previous list if necessary, or merge if you prefer.
            for (int i = 0; i < playerCount; i++)
            {
                string playerName = reader.ReadString();
                int weaponCount = reader.ReadInt32();

                var player = new BossDamageTracker.MyPlayer
                {
                    playerName = playerName,
                    weapons = new List<BossDamageTracker.Weapons>()
                };

                // Iterate through weapon data for this player
                for (int j = 0; j < weaponCount; j++)
                {
                    string weaponName = reader.ReadString();
                    int weaponDamage = reader.ReadInt32();

                    player.weapons.Add(new BossDamageTracker.Weapons
                    {
                        weaponName = weaponName,
                        damage = weaponDamage
                    });
                }
                fight.players.Add(player);
            }

            // Print out formatted information
            PrintBossFights();
        }

        private void PrintBossFights()
        {
            // Iterate over all boss fights
            foreach (var kvp in serverBossFights)
            {
                var boss = kvp.Value;
                Logger.Info($"{boss.bossName} (ID: {boss.bossId})");

                // Print each player for this boss fight
                foreach (var player in boss.players)
                {
                    Logger.Info($"\t{player.playerName}");
                    // Print each weapon of the player
                    foreach (var weapon in player.weapons)
                    {
                        Logger.Info($"\t\t{weapon.weaponName} - Damage: {weapon.damage}");
                    }
                }
            }

            // Finally print the count of boss fights
            Logger.Info($"Total Boss Fights: {serverBossFights.Count}");
        }

        private void HandleClientPacket(BinaryReader reader, int whoAmI)
        {
            // chill for now
        }
    }
}
