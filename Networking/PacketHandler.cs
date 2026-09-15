using System;
using System.IO;
using System.Linq;
using DPSPanel.Common.DamageCalculation;
using DPSPanel.Common.DamageCalculation.Classes;
using Terraria.ID;

namespace DPSPanel.Networking;

public static class PacketHandler
{
    public static void Handle(BinaryReader reader, int sender)
    {
        if (Main.netMode == NetmodeID.SinglePlayer)
            return;

        try
        {
            if (reader.ReadByte() != FightPacketCodec.Version)
                return;
            var type = (PacketType)reader.ReadByte();
            var system = ModContent.GetInstance<EncounterSystem>();
            switch (type)
            {
                case PacketType.Context when Main.netMode == NetmodeID.MultiplayerClient:
                    var context = new FightContext(reader.ReadInt64(), reader.ReadString(), reader.ReadInt32(),
                        reader.ReadBoolean(), reader.ReadBoolean());
                    if (context.Id > 0 && context.Name.Length <= 256)
                        system.SetContext(context);
                    break;

                case PacketType.Snapshot:
                    var (fightId, player) = FightPacketCodec.ReadSnapshot(reader);
                    if (player.Weapons.Any(w => w.weaponItemID >= ItemLoader.ItemCount))
                        return;
                    if (Main.netMode == NetmodeID.Server)
                    {
                        if (sender < 0 || sender >= Main.maxPlayers || !Main.player[sender].active ||
                            player.PlayerId != sender)
                            return;
                        // Never let a client assign another player's identity or name.
                        player = new PlayerFightData(sender, Main.player[sender].name, player.Revision, player.Weapons);
                    }
                    system.ReceiveSnapshot(fightId, player);
                    break;

                case PacketType.RequestSync when Main.netMode == NetmodeID.Server:
                    if (sender >= 0 && sender < Main.maxPlayers)
                        system.SendSync(sender);
                    break;

                case PacketType.RemovePlayer when Main.netMode == NetmodeID.MultiplayerClient:
                    system.RemovePlayer(reader.ReadInt64(), reader.ReadInt32());
                    break;
            }
        }
        catch (Exception ex) when (ex is IOException or ArgumentException or OverflowException)
        {
            Log.Warn($"Ignored an invalid damage packet: {ex.Message}");
        }
    }
}
