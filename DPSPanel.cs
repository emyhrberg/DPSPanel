using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.GameContent;
using Terraria.ModLoader;
using System.IO;

namespace DPSPanel
{
    public class DPSPanel : Mod
    {
        public enum ModMessageType
        {
            PlayerDamage
        }

        public override void HandlePacket(BinaryReader reader, int whoAmI)
        {
            ModMessageType msgType = (ModMessageType)reader.ReadByte();
            switch (msgType)
            {
                case ModMessageType.PlayerDamage:
                    string playerName = reader.ReadString();
                    int damageDone = reader.ReadInt32();
                    Logger.Info($"Received damage data: Player {playerName} dealt {damageDone} damage.");
                    break;
            }
        }
    }
}
