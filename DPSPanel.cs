using Terraria.ModLoader;
using System.IO;
using Terraria.ID;
using Terraria;
using DPSPanel.MainCode.Panel;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System;
using System.Linq;
using static DPSPanel.MainCode.Panel.BossDamageTracker;

namespace DPSPanel
{
    public class DPSPanel : Mod
    {
        private Dictionary<int, BossFight> serverBossFights
            = new Dictionary<int, BossFight>();

        public override void HandlePacket(BinaryReader reader, int whoAmI)
        {
            if (Main.netMode == NetmodeID.Server)
                HandleServerPacket(reader, whoAmI);
            else if (Main.netMode == NetmodeID.MultiplayerClient)
                HandleClientPacket(reader, whoAmI);
        }

        private void HandleServerPacket(BinaryReader reader, int whoAmI)
        {
            // We receive the boss fights from the client and
            // update the server's boss fight data.
            var fight = ReceiveBossFightPacket(reader);

            if (serverBossFights.TryGetValue(fight.bossId, out BossFight value))
            {
                // If the boss fight already exists, merge the data.
                MergeBossFight(value, fight);
            }
            else
            {
                // Otherwise, add the new boss fight.
                serverBossFights[fight.bossId] = fight;
            }

            // Then, we send the updated boss fight data to all clients.
            ModPacket packet = CreateBossFightPacket(fight);
            packet.Send(-1); // -1 meaning send to all clients only
            PrintBossFights(); // just debug print
        }

        private void HandleClientPacket(BinaryReader reader, int whoAmI)
        {
            // We receive the boss fights from the server and
            // update the client's boss fight data.
            var fight = ReceiveBossFightPacket(reader);
            DPSPanelSystem system = ModContent.GetInstance<DPSPanelSystem>();
            system.state.dpsPanel.UpdateDPSPanel(fight);
        }

        private BossFight ReceiveBossFightPacket(BinaryReader reader)
        {
            // Read the boss fight data from the packet and return it.
            var fight = new BossFight
            {
                bossId = reader.ReadInt32(),
                bossName = reader.ReadString(),
                initialLife = reader.ReadInt32(),
                damageTaken = reader.ReadInt32(),
                players = new List<MyPlayer>()
            };

            int playerCount = reader.ReadInt32();
            for (int i = 0; i < playerCount; i++)
            {
                var player = new MyPlayer
                {
                    playerName = reader.ReadString(),
                    totalDamage = reader.ReadInt32(),
                    weapons = new List<Weapons>()
                };

                int weaponCount = reader.ReadInt32();
                for (int j = 0; j < weaponCount; j++)
                {
                    player.weapons.Add(new Weapons
                    {
                        weaponName = reader.ReadString(),
                        damage = reader.ReadInt32()
                    });
                }
                fight.players.Add(player);
            }

            return fight;
        }

        private void MergeBossFight(BossFight existing, BossFight incoming)
        {
            // Add cumulative damage
            existing.damageTaken += incoming.damageTaken;

            // For each incoming player, add or update in the existing fight.
            foreach (var incomingPlayer in incoming.players)
            {
                var existingPlayer = existing.players.FirstOrDefault(p => p.playerName == incomingPlayer.playerName);
                if (existingPlayer == null)
                {
                    // Add new player entry
                    existing.players.Add(incomingPlayer);
                }
                else
                {
                    // Update total damage
                    existingPlayer.totalDamage += incomingPlayer.totalDamage;

                    // For each weapon from the incoming player
                    foreach (var incomingWeapon in incomingPlayer.weapons)
                    {
                        var existingWeapon = existingPlayer.weapons.FirstOrDefault(w => w.weaponName == incomingWeapon.weaponName);
                        if (existingWeapon == null)
                        {
                            // Add new weapon
                            existingPlayer.weapons.Add(incomingWeapon);
                        }
                        else
                        {
                            // Update the weapon's damage
                            existingWeapon.damage += incomingWeapon.damage;
                        }
                    }
                }
            }
        }

        public ModPacket CreateBossFightPacket(BossFight fight)
        {
            // Write the boss fight data to a packet and return the packet.
            ModPacket packet = GetPacket();
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

            Logger.Info("Sending Boss Fight Data to Server");
            return packet;
        }

        private void PrintBossFights()
        {
            // Debug print the boss fight data.
            foreach (var kvp in serverBossFights)
            {
                var boss = kvp.Value;
                Logger.Info($"{boss.bossName} (ID: {boss.bossId})");

                foreach (var player in boss.players)
                {
                    Logger.Info($"\t\t{player.playerName} - {player.totalDamage} damage");
                    foreach (var weapon in player.weapons)
                    {
                        Logger.Info($"\t\t{weapon.weaponName} - {weapon.damage} damage");
                    }
                }
            }

            Logger.Info($"Total Boss Fights: {serverBossFights.Count}");
            Logger.Info($"---------------------------------------------");
        }
    }
}
