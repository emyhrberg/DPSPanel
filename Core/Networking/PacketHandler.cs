using System.IO;
using DPSPanel.Helpers;
using DPSPanel.UI;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Chat;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace DPSPanel.Core.Networking
{
    /// <summary>
    /// This class handles receiving packets from the server and clients.
    /// The server receives the packet and broadcasts it to all clients.
    /// The client receives the packet and updates its UI.
    /// </summary>
    public static class PacketHandler
    {
        public enum PacketType
        {
            FightPacket,
            OnConsumeItemPacket
        }

        public static void Handle(BinaryReader reader)
        {
            if (Main.netMode == NetmodeID.SinglePlayer)
                return;

            PacketType packetType = (PacketType)reader.ReadByte();
            switch (packetType)
            {
                case PacketType.FightPacket:
                    HandleFightPacket(reader);
                    break;
                case PacketType.OnConsumeItemPacket:
                    HandleOnConsumeItemPacket(reader);
                    break;
            }
        }

        // In PacketHandler.cs - in HandleOnConsumeItemPacket:
        private static void HandleOnConsumeItemPacket(BinaryReader reader)
        {
            long availableBefore = reader.BaseStream.Length - reader.BaseStream.Position;
            Log.Info($"Available bytes before reading: {availableBefore}");
            string playerName = reader.ReadString();
            string itemName = reader.ReadString();
            long availableAfter = reader.BaseStream.Length - reader.BaseStream.Position;
            Log.Info($"Available bytes after reading: {availableAfter}");

            if (Main.netMode == NetmodeID.Server)
            {
                ChatHelper.BroadcastChatMessage(
                    NetworkText.FromLiteral($"{playerName} used {itemName}!"),
                    Color.White
                );
                Log.Info($"[Server] Received OnConsumeItemPacket from {playerName}: {itemName}");
            }
        }

        private static void HandleFightPacket(BinaryReader reader)
        {
            // Read the data from the packet
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
                ModPacket fightPacket = ModContent.GetInstance<DPSPanel>().GetPacket();
                fightPacket.Write((byte)PacketType.FightPacket);
                fightPacket.Write(playerName);
                fightPacket.Write(damageDone);
                fightPacket.Write(bossWhoAmI);
                fightPacket.Write(bossName);
                fightPacket.Write(bossHeadId);
                fightPacket.Write(playerWhoAmI);
                fightPacket.Send(); // Broadcast to all clients
            }
            else if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                // Client receives the packet and processes it
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
