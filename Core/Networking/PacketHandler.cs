using System.Collections.Generic;
using System.IO;
using DPSPanel.Core.DamageCalculation;
using DPSPanel.Helpers;
using DPSPanel.UI;
using Terraria;
using Terraria.ID;
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

            // Read the weapons
            int weaponCount = reader.ReadInt32();
            List<Weapon> weapons = new List<Weapon>();
            for (int i = 0; i < weaponCount; i++)
            {
                string wpnName = reader.ReadString();
                int wpnItemID = reader.ReadInt32();
                int wpnDamage = reader.ReadInt32();
                weapons.Add(new Weapon(wpnItemID, wpnName, wpnDamage));
            }

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

                // Write the weapons to the packet
                fightPacket.Write(weapons.Count);
                foreach (Weapon weapon in weapons)
                {
                    fightPacket.Write(weapon.weaponName);
                    fightPacket.Write(weapon.weaponItemID);
                    fightPacket.Write(weapon.damage);
                }

                fightPacket.Send(); // Broadcast to all clients
            }
            else if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                // Client receives the packet and processes it
                // Client updates its UI with the received data
                Log.Info($"[PacketHandler.cs] Updating UI for {playerName}: {damageDone} damage to {bossName} (whoAmI {bossWhoAmI} | headID: {bossHeadId}) | playerWHOAMI: {playerWhoAmI}");

                MainSystem sys = ModContent.GetInstance<MainSystem>();
                MainPanel panel = sys.state.container.panel;
                if (panel.CurrentBossID != bossWhoAmI)
                {
                    // New boss fight: clear panel and set title
                    // Log.Info($"[PacketHandler.cs] New boss fight detected: {bossName} (whoamI {bossWhoAmI}) | headID: {bossHeadId}");

                    // TODO neccessary to reset panel EVERY time we find a new boss?
                    // This is called quite frequently.
                    // Investigate this if statement later.
                    panel.ClearPanelAndAllItems();
                    panel.SetBossTitle(bossName, bossHeadId);
                }

                panel.UpdatePlayerBars(playerName, damageDone, playerWhoAmI, weapons);
            }
        }
    }
}
