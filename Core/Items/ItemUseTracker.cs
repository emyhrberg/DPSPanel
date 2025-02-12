using System.Collections.Generic;
using System.IO;
using DPSPanel.Helpers;
using log4net.Repository.Hierarchy;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Chat;
using Terraria.ID;
using Terraria.Localization;
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

            base.OnConsumeItem(item, player);
            return;

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
                Main.NewText($"[ItemUseTracker.cs] Used boss summoning item: {item.Name}");
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

        // In PacketHandler.cs - in HandleOnConsumeItemPacket:
        private static void HandleOnConsumeItemPacket(BinaryReader reader)
        {
            long availableBefore = reader.BaseStream.Length - reader.BaseStream.Position;
            Log.Info($"Available bytes before reading: {availableBefore}");
            string playerName = reader.ReadString();
            string itemName = reader.ReadString();
            long availableAfter = reader.BaseStream.Length - reader.BaseStream.Position;
            Log.Info($"Available bytes after reading: {availableAfter}");

            if (Main.netMode == NetmodeID.Server)
            {
                ChatHelper.BroadcastChatMessage(
                    NetworkText.FromLiteral($"{playerName} used {itemName}!"),
                    Color.White
                );
                Log.Info($"[Server] Received OnConsumeItemPacket from {playerName}: {itemName}");
            }
        }
    }
}