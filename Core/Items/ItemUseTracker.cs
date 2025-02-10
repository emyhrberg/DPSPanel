using System.Collections.Generic;
using System.IO;
using DPSPanel.Helpers;
using log4net.Repository.Hierarchy;
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
            // pre-HM
            ItemID.SlimeCrown, // king slime
            ItemID.SuspiciousLookingEye, // eye of cthulhu
            ItemID.WormFood, // eater of worlds
            ItemID.BloodySpine, // brain of cthulhu
            ItemID.Abeemination, // queen bee
            ItemID.ClothierVoodooDoll, // skeletron
            ItemID.DeerThing, // deerclops

            // HM
            4988, // queen slime (gelatin crystal)

            ItemID.MechanicalEye, // the twins
            ItemID.MechanicalWorm, // the destroyer
            ItemID.MechanicalSkull, // skeletron prime
            ItemID.LihzahrdPowerCell, // golem

            // invasions
            ItemID.GoblinBattleStandard, // goblin army
            ItemID.SnowGlobe, // frost legion
            ItemID.PirateMap, // pirate invasion
            ItemID.PumpkinMoonMedallion, // pumpkin moon
            ItemID.NaughtyPresent, // frost moon
        ];

        // NOTE: missing:
        // pre-HM:
        // wof

        // HM:
        // plantera
        // duke fishron
        // lunatic cultist

        public override bool InstancePerEntity => true;

        public override void OnConsumeItem(Item item, Player player)
        {
            Log.Info("UseItem called. item.Name: " + item.Name);

            // add calamity boss summoning items
            Mod calamity = ModLoader.GetMod("CalamityMod");
            if (calamity == null)
            {
                Log.Info("no calamity");
            }
            else
            {
                Log.Info("calamity");
            }
            // add calamity boss summoning items

            // add thorium boss summoning items

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