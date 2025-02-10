using System.Linq;
using DPSPanel.Core.DamageCalculation;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

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
            // Do nothing if in single-player mode.
            if (Main.netMode == NetmodeID.SinglePlayer)
                return;

            // Get variables to send.
            string player = Main.LocalPlayer.name;
            int damageDone = fight.players.FirstOrDefault(p => p.playerName == Main.LocalPlayer.name)?.playerDamage ?? 0;

            // Retrieve your mod instance to create a packet.
            DPSPanel modInstance = ModContent.GetInstance<DPSPanel>();
            ModPacket packet = modInstance.GetPacket();

            // Write packet data.
            // Since you only have one packet type, there's no need to write a type identifier.
            packet.Write(player);
            packet.Write(damageDone);
            packet.Write(fight.whoAmI);
            packet.Write(fight.bossName);
            packet.Write(fight.bossHeadId);
            packet.Write(Main.LocalPlayer.whoAmI);

            modInstance.Logger.Info($"[Client] Sent player: {player} | dmg: {damageDone} | BossWhoAmI: {fight.whoAmI} | BossName: {fight.bossName} | BossHeadID: {fight.bossHeadId} | LocalPlayerWhoAmI: {Main.LocalPlayer.whoAmI}");

            // Send the packet to the server.
            packet.Send();
        }
    }
}
