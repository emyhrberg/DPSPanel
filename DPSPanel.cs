using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.GameContent;
using Terraria.ModLoader;
using System.IO;
using DPSPanel.Core.Panel;
using System.Collections.Generic;

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
            Logger.Info($"Received packet with length: {reader.BaseStream.Length} | from: {whoAmI} | netmode: {Main.netMode}");

            ModMessageType msgType = (ModMessageType)reader.ReadByte();
            switch (msgType)
            {
                case ModMessageType.FightPacket:
                    string playerName = reader.ReadString();
                    int damageDone = reader.ReadInt32();
                    int bossId = reader.ReadInt32();
                    string bossName = reader.ReadString();

                    Logger.Info($"Player: {playerName} | Damage: {damageDone} | Boss: {bossName} | BossID ({bossId})");

                    // update UI
                    PanelSystem sys = ModContent.GetInstance<PanelSystem>();
                    if (sys != null)
                        sys.state.container.panel.UpdateDamageBars(playerName, damageDone);
                    else
                        Logger.Warn("PanelSystem is null");
                    break;
            }
        }
    }
}
