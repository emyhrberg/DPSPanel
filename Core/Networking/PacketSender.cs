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
            if (Main.netMode != NetmodeID.MultiplayerClient) // Return if not multiplayer
                return;

            // Get variables to send.
            string player = Main.LocalPlayer.name;
            int damageDone = fight.players.FirstOrDefault(p => p.playerName == Main.LocalPlayer.name)?.playerDamage ?? 0;

            // Write packet data.
            // Since you only have one packet type, there's no need to write a type identifier.
            ModPacket fightPacket = ModContent.GetInstance<DPSPanel>().GetPacket();
            fightPacket.Write((byte)PacketType.FightPacket);
            fightPacket.Write(player);
            fightPacket.Write(damageDone);
            fightPacket.Write(fight.whoAmI);
            fightPacket.Write(fight.bossName);
            fightPacket.Write(fight.bossHeadId);
            fightPacket.Write(Main.LocalPlayer.whoAmI);

            Log.Info($"[Client] Sent player: {player} | dmg: {damageDone} | BossWhoAmI: {fight.whoAmI} | BossName: {fight.bossName} | BossHeadID: {fight.bossHeadId} | LocalPlayerWhoAmI: {Main.LocalPlayer.whoAmI}");

            fightPacket.Send(); // Send the packet to the server.
        }
    }
}
