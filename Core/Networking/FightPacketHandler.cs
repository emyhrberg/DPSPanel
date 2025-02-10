using System.IO;
using DPSPanel.Helpers;
using DPSPanel.UI;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace DPSPanel.Core.Networking
{
    public static class FightPacketHandler
    {
        public static void Handle(BinaryReader reader, int whoAmI)
        {
            // Since you’re only using one packet type, you don’t need to read a type byte.
            // Just read the expected data in the proper order.
            string playerName = reader.ReadString();
            int damageDone = reader.ReadInt32();
            int bossWhoAmI = reader.ReadInt32();
            string bossName = reader.ReadString();
            int bossHeadId = reader.ReadInt32();
            int playerWhoAmI = reader.ReadInt32();

            if (Main.netMode == NetmodeID.Server)
            {
                // Server processes the packet and broadcasts it to all clients
                Log.Info($"[Server] Received FightPacket from {playerName}: {damageDone} damage to {bossName} (bossWhoAmI {bossWhoAmI}) | bossHeadID: {bossHeadId} | playerWhoAmI: {playerWhoAmI}");

                // Create a new packet to broadcast to clients.
                // Note: We obtain the mod instance via ModContent.GetInstance<T>().
                ModPacket packet = ModContent.GetInstance<DPSPanel>().GetPacket();
                // No need to write a message type since this is the only packet
                packet.Write(playerName);
                packet.Write(damageDone);
                packet.Write(bossWhoAmI);
                packet.Write(bossName);
                packet.Write(bossHeadId);
                packet.Write(playerWhoAmI);
                packet.Send(); // Broadcast to all clients
            }
            else if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                // Client updates its UI with the received data
                Log.Info($"[Client] Updating UI for {playerName}: {damageDone} damage to {bossName} (whoAmI {bossWhoAmI} | headID: {bossHeadId}) | playerWHOAMI: {playerWhoAmI}");

                MainSystem sys = ModContent.GetInstance<MainSystem>();
                Panel panel = sys.state.container.panel;
                if (panel.CurrentBossID != bossWhoAmI)
                {
                    // New boss fight: clear panel and set title
                    Log.Info($"[Client] New boss fight detected: {bossName} (whoamI {bossWhoAmI}) | headID: {bossHeadId}");
                    panel.ClearPanelAndAllItems();
                    panel.SetBossTitle(bossName, bossHeadId);
                }

                panel.UpdateDamageBars(playerName, damageDone, playerWhoAmI);
            }
        }
    }
}
