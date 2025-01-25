using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.IO;
using DPSPanel.UI;

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
                    int bossId = reader.ReadInt32();
                    string bossName = reader.ReadString();

                    if (Main.netMode == NetmodeID.Server)
                    {
                        // Server processes the packet and broadcasts it to all clients
                        Logger.Info($"[Server] Received FightPacket from {playerName}: {damageDone} damage to {bossName} (ID {bossId})");

                        // Send data to all clients
                        ModPacket packet = GetPacket();
                        packet.Write((byte)ModMessageType.FightPacket);
                        packet.Write(playerName);
                        packet.Write(damageDone);
                        packet.Write(bossId);
                        packet.Write(bossName);
                        packet.Send(); // Broadcast to all clients
                    }
                    else if (Main.netMode == NetmodeID.MultiplayerClient)
                    {
                        // Client updates its UI with the received data
                        Logger.Info($"[Client] Updating UI for {playerName}: {damageDone} damage to {bossName} (ID {bossId})");

                        PanelSystem sys = ModContent.GetInstance<PanelSystem>();
                        Panel panel = sys.state.container.panel;

                        if (panel.CurrentBossID != bossId)
                        {
                            // new boss fight, clear panel and set title
                            Logger.Info($"[Client] New boss fight detected: {bossName} (ID {bossId})");
                            panel.ClearPanelAndAllItems();
                            panel.SetBossTitle(bossName, bossId);
                        }

                        panel.UpdateDamageBars(playerName, damageDone);
                    }
                    break;
            }
        }
    }
}
