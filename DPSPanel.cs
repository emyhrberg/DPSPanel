
using Terraria.ModLoader;
using System.IO;

namespace DPSPanel
{
    // Please read https://github.com/tModLoader/tModLoader/wiki/Basic-tModLoader-Modding-Guide#mod-skeleton-contents for more information about the various files in a mod.

    public class DPSPanel : Mod
	{

        public override void HandlePacket(BinaryReader reader, int whoAmI)
        {
            Logger.Info("Packet received HandlePacket() called!");

            string message = reader.ReadString();
            Logger.Info("Packet received Message: " + message);
        }
    }
}
