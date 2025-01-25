using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.IO;
using DPSPanel.UI;
using DPSPanel.Configs;

namespace DPSPanel
{
    public class DPSPanel : Mod
    {
        public enum ModMessageType
        {
            FightPacket
        }

        public override void HandlePacket(BinaryReader reader, int whoAmI)
        {
            ModMessageType msgType = (ModMessageType)reader.ReadByte();
            switch (msgType)
            {
                case ModMessageType.FightPacket:
                    string playerName = reader.ReadString();
                    int damageDone = reader.ReadInt32();
                    int bossWhoAmI = reader.ReadInt32();
                    string bossName = reader.ReadString();
                    int bossHeadId = reader.ReadInt32();
                    // int playerHeadIndex = reader.ReadInt32();
                    int playerHeadIndex = -1;

                    if (Main.netMode == NetmodeID.Server)
                    {
                        // Server processes the packet and broadcasts it to all clients
                        Logger.Info($"[Server] Received FightPacket from {playerName}: {damageDone} damage to {bossName} (whoAmI {bossWhoAmI})");

                        // Send data to all clients
                        ModPacket packet = GetPacket();
                        packet.Write((byte)ModMessageType.FightPacket);
                        packet.Write(playerName);
                        packet.Write(damageDone);
                        packet.Write(bossWhoAmI);
                        packet.Write(bossName);
                        packet.Write(bossHeadId);
                        // packet.Write(Main.player[whoAmI].head); // for drawing players heads
                        packet.Send(); // Broadcast to all clients
                    }
                    else if (Main.netMode == NetmodeID.MultiplayerClient)
                    {
                        // Client updates its UI with the received data
                        Logger.Info($"[Client] Updating UI for {playerName}: {damageDone} damage to {bossName} (whoAmI {bossWhoAmI} | headID: {bossHeadId}) | playerHeadIndex: {playerHeadIndex}");

                        PanelSystem sys = ModContent.GetInstance<PanelSystem>();
                        Panel panel = sys.state.container.panel;

                        if (panel.CurrentBossID != bossWhoAmI)
                        {
                            // new boss fight, clear panel and set title
                            Logger.Info($"[Client] New boss fight detected: {bossName} (whoamI {bossWhoAmI}) | headID: {bossHeadId}");
                            panel.ClearPanelAndAllItems();
                            panel.SetBossTitle(bossName, bossWhoAmI);
                        }

                        panel.UpdateDamageBars(playerName, damageDone, playerHeadIndex);
                    }
                    break;
            }
        }
    }
}
