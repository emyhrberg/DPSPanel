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
            // Receive boss fight context from the server.
            int bossWhoAmI = reader.ReadInt32();
            string bossName = reader.ReadString();
            int bossHeadId = reader.ReadInt32();
            int currentLife = reader.ReadInt32();
            int initialLife = reader.ReadInt32();
            int damageTaken = reader.ReadInt32();

            // Read players.
            int playerCount = reader.ReadInt32();
            List<PlayerFightData> players = [];
            for (int i = 0; i < playerCount; i++)
            {
                string pName = reader.ReadString();
                int pDamage = reader.ReadInt32();
                int pWhoAmI = reader.ReadInt32();
                players.Add(new PlayerFightData(pWhoAmI, pName, pDamage));
            }

            // Read weapons.
            int weaponCount = reader.ReadInt32();
            List<Weapon> weapons = [];
            for (int i = 0; i < weaponCount; i++)
            {
                string wpnName = reader.ReadString();
                int wpnItemID = reader.ReadInt32();
                int wpnDamage = reader.ReadInt32();
                weapons.Add(new Weapon(wpnItemID, wpnName, wpnDamage));
            }

            if (Main.netMode == NetmodeID.Server)
            {
                // Re-broadcast the packet to all clients by writing all the data again.
                ModPacket packet = ModContent.GetInstance<DPSPanel>().GetPacket();
                packet.Write((byte)PacketType.FightPacket);
                packet.Write(bossWhoAmI);
                packet.Write(bossName);
                packet.Write(bossHeadId);
                packet.Write(currentLife);
                packet.Write(initialLife);
                packet.Write(damageTaken);

                packet.Write(players.Count);
                foreach (var p in players)
                {
                    packet.Write(p.playerName);
                    packet.Write(p.playerDamage);
                    packet.Write(p.playerWhoAmI);
                }

                packet.Write(weapons.Count);
                foreach (var w in weapons)
                {
                    packet.Write(w.weaponName);
                    packet.Write(w.weaponItemID);
                    packet.Write(w.damage);
                }

                packet.Send();
            }
            else if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                // Relay the data from the server to the client's UI.
                MainSystem sys = ModContent.GetInstance<MainSystem>();
                MainPanel panel = sys.state.container.panel;

                // If a new boss fight is detected, clear & set a new title.
                if (panel.CurrentBossWhoAmI != bossWhoAmI)
                {
                    panel.ClearPanelAndAllItems();
                    panel.SetBossTitle(bossName, bossWhoAmI, bossHeadId);
                }

                // For each player in the packet, update or create that player's bar.
                foreach (var p in players)
                {
                    panel.UpdatePlayerBars(p.playerName, p.playerDamage, p.playerWhoAmI, weapons);
                }
            }
        }
    }
}
