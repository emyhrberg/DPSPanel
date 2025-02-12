using System.Linq;
using DPSPanel.Core.DamageCalculation;
using DPSPanel.Helpers;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static DPSPanel.Core.Networking.PacketHandler;

namespace DPSPanel.Core.Networking
{
    public static class PacketSender
    {
        /// <summary>
        /// Sends a packet containing the player’s damage info to the server.
        /// </summary>
        /// <param name="fight">The fight context containing damage and boss info.</param>
        public static void SendPlayerDamagePacket(BossDamageTrackerMP.BossFight fight)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient) // Only send from clients
                return;

            ModPacket fightPacket = ModContent.GetInstance<DPSPanel>().GetPacket();
            fightPacket.Write((byte)PacketType.FightPacket);

            // Write boss fight context
            fightPacket.Write(fight.whoAmI);
            fightPacket.Write(fight.bossName);
            fightPacket.Write(fight.bossHeadId);
            fightPacket.Write(fight.currentLife);
            fightPacket.Write(fight.initialLife);
            fightPacket.Write(fight.damageTaken);

            // Write player list
            fightPacket.Write(fight.players.Count);
            foreach (PlayerFightData player in fight.players)
            {
                fightPacket.Write(player.playerName);
                fightPacket.Write(player.playerDamage);
                fightPacket.Write(player.playerWhoAmI);
            }

            // Write weapons
            fightPacket.Write(fight.weapons.Count);
            foreach (Weapon weapon in fight.weapons)
            {
                fightPacket.Write(weapon.weaponName);
                fightPacket.Write(weapon.weaponItemID);
                fightPacket.Write(weapon.damage);
            }

            // Log.Info($"[PacketSender.cs] Sent boss fight packet with {fight.players.Count} players. Boss: {fight.bossName}, DamageTaken: {fight.damageTaken}");
            fightPacket.Send(); // Broadcast the packet
        }
    }
}
