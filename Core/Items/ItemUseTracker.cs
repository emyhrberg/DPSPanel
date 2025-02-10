using System.Collections.Generic;
using System.IO;
using DPSPanel.Helpers;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static DPSPanel.Core.Networking.PacketHandler;

namespace DPSPanel.Core.Items
{
    // This GlobalItem applies to every item in the game.
    public class ItemUseTracker : GlobalItem
    {
        private List<int> bossSummoningItems = [
            ItemID.SlimeCrown,
            ItemID.DeerThing,
            ItemID.SuspiciousLookingEye,
            ItemID.WormFood,
            ItemID.MechanicalEye,
            ItemID.MechanicalWorm,
            ItemID.MechanicalSkull,
            ItemID.LihzahrdPowerCell,
        ];

        public override bool InstancePerEntity => true;

        public override void OnConsumeItem(Item item, Player player)
        {
            Log.Info("UseItem called");

            if (Main.netMode != NetmodeID.MultiplayerClient)
                return;

            // Check if the item used is a boss summoning item.
            if (bossSummoningItems.Contains(item.type))
            {
                Main.NewText($"[Client] Used boss summoning item: {item.Name}");
                ModPacket onConsumeItemPacket = ModContent.GetInstance<DPSPanel>().GetPacket();
                onConsumeItemPacket.Write((byte)PacketType.OnConsumeItemPacket);
                onConsumeItemPacket.Write(player.name);
                onConsumeItemPacket.Write(item.Name);
                Log.Info($"Packet Bytes Written: {((MemoryStream)onConsumeItemPacket.BaseStream).Length}");
                onConsumeItemPacket.Send();
            }

            // Return base.UseItem to preserve the original behavior.
            base.OnConsumeItem(item, player);
        }
    }
}