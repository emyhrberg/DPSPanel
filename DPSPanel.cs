using System.IO;
using DPSPanel.Core.Networking;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace DPSPanel
{
    public class DPSPanel : Mod
    {
        public override void HandlePacket(BinaryReader reader, int whoAmI)
        {
            if (Main.netMode == NetmodeID.SinglePlayer)
                return;

            // Since you’re always sending one type of packet, no need for an enum or type byte.
            FightPacketHandler.Handle(reader, whoAmI);
        }
    }
}
