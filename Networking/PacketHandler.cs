using System.Collections.Generic;
using System.IO;
using DPSPanel.Common.DamageCalculation;
using DPSPanel.UI;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace DPSPanel.Networking
{
    /// <summary>
    /// This class handles receiving packets from the server and clients.
    /// The server re-broadcasts the packet to all clients.
    /// The client updates its UI.
    /// </summary>
    public static class PacketHandler
    {
        public enum PacketType
        {
            FightPacket,       // Normal single‐NPC boss
            FightPacketWorm,   // Other worm‐like bosses
            FightPacketEoW,    // Eater of Worlds
        }

        public static void Handle(BinaryReader reader)
        {
            if (Main.netMode == NetmodeID.SinglePlayer)
                return; // No need to handle in single player

            PacketType packetType = (PacketType)reader.ReadByte();
            switch (packetType)
            {
                case PacketType.FightPacket:
                    HandleFightPacket(reader);
                    break;
                case PacketType.FightPacketWorm:
                    HandleWormFightPacket(reader);
                    break;
                case PacketType.FightPacketEoW:
                    HandleEoWFightPacket(reader);
                    break;
            }
        }

        /// <summary>
        /// Handle normal single‐NPC boss fight packets.
        /// </summary>
        private static void HandleFightPacket(BinaryReader reader)
        {
            int bossWhoAmI = reader.ReadInt32();
            string bossName = reader.ReadString();
            int bossHeadId = reader.ReadInt32();
            int currentLife = reader.ReadInt32();
            int initialLife = reader.ReadInt32();
            int damageTaken = reader.ReadInt32();

            // Players
            int playerCount = reader.ReadInt32();
            List<PlayerFightData> players = new List<PlayerFightData>(playerCount);
            for (int i = 0; i < playerCount; i++)
            {
                string pName = reader.ReadString();
                int pDamage = reader.ReadInt32();
                int pWhoAmI = reader.ReadInt32();
                players.Add(new PlayerFightData(pWhoAmI, pName, pDamage));
            }

            // Weapons
            int weaponCount = reader.ReadInt32();
            List<Weapon> weapons = new List<Weapon>(weaponCount);
            for (int i = 0; i < weaponCount; i++)
            {
                string wpnName = reader.ReadString();
                int wpnItemID = reader.ReadInt32();
                int wpnDamage = reader.ReadInt32();
                weapons.Add(new Weapon(wpnItemID, wpnName, wpnDamage));
            }

            if (Main.netMode == NetmodeID.Server)
            {
                // Re-broadcast
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
                // Update local UI
                MainSystem sys = ModContent.GetInstance<MainSystem>();
                MainPanel panel = sys.state.container.panel;

                // If new boss or changed, re-init panel
                if (panel.CurrentBossWhoAmI != bossWhoAmI)
                {
                    panel.ClearPanelAndAllItems();
                    panel.SetBossTitle(bossName, bossWhoAmI, bossHeadId);
                }

                // Update each player's bar
                foreach (var p in players)
                {
                    panel.UpdatePlayerBars(p.playerName, p.playerDamage, p.playerWhoAmI, weapons);
                }
            }
        }

        /// <summary>
        /// Handle worm‐like boss fights (e.g., The Destroyer).
        /// You’d likely read (headIndex, totalLife, totalLifeMax, etc.).
        /// </summary>
        private static void HandleWormFightPacket(BinaryReader reader)
        {
            // 1) Read data from packet
            bool isAlive = reader.ReadBoolean();
            int headIndex = reader.ReadInt32(); // npc.realLife of the worm's head
            string bossName = reader.ReadString();
            int bossHeadId = reader.ReadInt32();
            int damageTaken = reader.ReadInt32();
            int totalLife = reader.ReadInt32();
            int totalLifeMax = reader.ReadInt32();

            int playerCount = reader.ReadInt32();
            List<PlayerFightData> players = new List<PlayerFightData>(playerCount);
            for (int i = 0; i < playerCount; i++)
            {
                string pName = reader.ReadString();
                int pDamage = reader.ReadInt32();
                int pWhoAmI = reader.ReadInt32();
                players.Add(new PlayerFightData(pWhoAmI, pName, pDamage));
            }

            int weaponCount = reader.ReadInt32();
            List<Weapon> weapons = new List<Weapon>(weaponCount);
            for (int i = 0; i < weaponCount; i++)
            {
                string wpnName = reader.ReadString();
                int wpnItemID = reader.ReadInt32();
                int wpnDamage = reader.ReadInt32();
                weapons.Add(new Weapon(wpnItemID, wpnName, wpnDamage));
            }

            // 2) Server re-broadcast if needed
            if (Main.netMode == NetmodeID.Server)
            {
                ModPacket packet = ModContent.GetInstance<DPSPanel>().GetPacket();
                packet.Write((byte)PacketType.FightPacketWorm);

                packet.Write(isAlive);
                packet.Write(headIndex);
                packet.Write(bossName);
                packet.Write(bossHeadId);
                packet.Write(damageTaken);
                packet.Write(totalLife);
                packet.Write(totalLifeMax);

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
            // 3) Client update
            else if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                MainSystem sys = ModContent.GetInstance<MainSystem>();
                MainPanel panel = sys.state.container.panel;

                // For worm fights, you might store the worm’s "bossWhoAmI" as -1 or use the headIndex
                // We'll do a simple check: if the panel doesn't match, reset it
                if (panel.CurrentBossWhoAmI != headIndex)
                {
                    panel.ClearPanelAndAllItems();
                    // Typically we pass -1 for whoAmI for worm fights
                    panel.SetBossTitle(bossName, -1, bossHeadId);
                }

                // Then update all players
                foreach (var p in players)
                {
                    panel.UpdatePlayerBars(p.playerName, p.playerDamage, p.playerWhoAmI, weapons);
                }
            }
        }

        /// <summary>
        /// Handle the Eater of Worlds specifically (Head/Body/Tail).
        /// Similar structure to the worm fight, but possibly different fields or logic.
        /// </summary>
        private static void HandleEoWFightPacket(BinaryReader reader)
        {
            bool isAlive = reader.ReadBoolean();
            string bossName = reader.ReadString(); // "Eater of Worlds"
            int bossHeadId = reader.ReadInt32();
            int damageTaken = reader.ReadInt32();
            int totalLife = reader.ReadInt32();
            int totalLifeMax = reader.ReadInt32();

            int playerCount = reader.ReadInt32();
            List<PlayerFightData> players = new List<PlayerFightData>(playerCount);
            for (int i = 0; i < playerCount; i++)
            {
                string pName = reader.ReadString();
                int pDamage = reader.ReadInt32();
                int pWhoAmI = reader.ReadInt32();
                players.Add(new PlayerFightData(pWhoAmI, pName, pDamage));
            }

            int weaponCount = reader.ReadInt32();
            List<Weapon> weapons = new List<Weapon>(weaponCount);
            for (int i = 0; i < weaponCount; i++)
            {
                string wpnName = reader.ReadString();
                int wpnItemID = reader.ReadInt32();
                int wpnDamage = reader.ReadInt32();
                weapons.Add(new Weapon(wpnItemID, wpnName, wpnDamage));
            }

            // Server re-broadcast
            if (Main.netMode == NetmodeID.Server)
            {
                ModPacket packet = ModContent.GetInstance<DPSPanel>().GetPacket();
                packet.Write((byte)PacketType.FightPacketEoW);

                packet.Write(isAlive);
                packet.Write(bossName);
                packet.Write(bossHeadId);
                packet.Write(damageTaken);
                packet.Write(totalLife);
                packet.Write(totalLifeMax);

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
            // Client update
            else if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                MainSystem sys = ModContent.GetInstance<MainSystem>();
                MainPanel panel = sys.state.container.panel;

                // EoW typically doesn't have a single whoAmI for the entire worm, so we can do e.g. -1
                if (panel.CurrentBossWhoAmI != -1)
                {
                    panel.ClearPanelAndAllItems();
                    panel.SetBossTitle(bossName, -1, bossHeadId);
                }

                foreach (var p in players)
                {
                    panel.UpdatePlayerBars(p.playerName, p.playerDamage, p.playerWhoAmI, weapons);
                }
            }
        }
    }
}
