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
    public class DPSPanel : Mod
    {
        private Dictionary<int, BossDamageTracker.BossFight> serverBossFights
            = new Dictionary<int, BossDamageTracker.BossFight>();

        public override void HandlePacket(BinaryReader reader, int whoAmI)
        {
            if (Main.netMode == NetmodeID.Server)
                HandleServerPacket(reader, whoAmI);
            else if (Main.netMode == NetmodeID.MultiplayerClient)
                HandleClientPacket(reader, whoAmI);
        }

        private void HandleServerPacket(BinaryReader reader, int whoAmI)
        {
            // Read the boss fight data from the packet
            int bossId = reader.ReadInt32();
            string bossName = reader.ReadString();
            int initialLife = reader.ReadInt32();
            int damageTaken = reader.ReadInt32();
            int playerCount = reader.ReadInt32();

            // Try to get an existing boss fight; if not, create one.
            BossDamageTracker.BossFight fight;
            if (!serverBossFights.TryGetValue(bossId, out fight))
            {
                // Create a new boss fight structure
                fight = new BossDamageTracker.BossFight
                {
                    bossId = bossId,
                    bossName = bossName,
                    initialLife = initialLife,
                    damageTaken = damageTaken,
                    // Initialize the list of players
                    players = new List<BossDamageTracker.MyPlayer>()
                };
                serverBossFights[bossId] = fight;
            }
            else
            {
                // If the fight already exists, update its basic fields.
                fight.bossName = bossName;
                fight.initialLife = initialLife;
                fight.damageTaken = damageTaken;
            }

            // Process each player entry from the packet
            for (int i = 0; i < playerCount; i++)
            {
                string playerName = reader.ReadString();
                int weaponCount = reader.ReadInt32();

                // See if this player already exists for the fight
                var existingPlayer = fight.players.FirstOrDefault(p => p.playerName == playerName);
                if (existingPlayer == null)
                {
                    // Create and add the player if not found.
                    existingPlayer = new BossDamageTracker.MyPlayer
                    {
                        playerName = playerName,
                        weapons = new List<BossDamageTracker.Weapons>()
                    };
                    fight.players.Add(existingPlayer);
                }

                // Process each weapon for that player
                for (int j = 0; j < weaponCount; j++)
                {
                    string weaponName = reader.ReadString();
                    int weaponDamage = reader.ReadInt32();

                    // Look for an existing weapon entry
                    var existingWeapon = existingPlayer.weapons.FirstOrDefault(w => w.weaponName == weaponName);
                    if (existingWeapon == null)
                    {
                        // Add a new weapon if not found.
                        existingWeapon = new BossDamageTracker.Weapons
                        {
                            weaponName = weaponName,
                            damage = weaponDamage
                        };
                        existingPlayer.weapons.Add(existingWeapon);
                    }
                    else
                    {
                        // Update the damage value if it already exists (assuming the incoming value
                        // is cumulative).
                        existingWeapon.damage = weaponDamage;
                    }
                }
            }

            // Print out the currently stored boss fight data.
            PrintBossFights();
        }

        private void HandleClientPacket(BinaryReader reader, int whoAmI)
        {
            // Clientside processing if necessary.
        }

        /// <summary>
        /// Prints every boss fight in a formatted way:
        /// Boss Name (ID)
        ///      Player1
        ///          Weapon1 - Damage: X
        ///          Weapon2 - Damage: Y
        ///      Player2
        ///          Weapon1 - Damage: Z
        /// Total Boss Fights: N
        /// </summary>
        private void PrintBossFights()
        {
            foreach (var kvp in serverBossFights)
            {
                var boss = kvp.Value;
                Logger.Info($"{boss.bossName} (ID: {boss.bossId})");

                foreach (var player in boss.players)
                {
                    Logger.Info($"\t{player.playerName}");
                    foreach (var weapon in player.weapons)
                    {
                        Logger.Info($"\t\t{weapon.weaponName} - Damage: {weapon.damage}");
                    }
                }
            }

            Logger.Info($"Total Boss Fights: {serverBossFights.Count}");
            Logger.Info($"---------------------------------------------");
        }
    }
}
