using System.IO;
using DPSPanel.Networking;
using Terraria.ModLoader;

namespace DPSPanel
{
    [Autoload(Side = ModSide.Both)]
    public class DPSPanel : Mod
    {
        public override void HandlePacket(BinaryReader reader, int whoAmI)
        {
            PacketHandler.Handle(reader);
        }
    }
}
