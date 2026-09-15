using DPSPanel.Common.DamageCalculation.Classes;
using Terraria.ID;

namespace DPSPanel.Networking;

public enum PacketType : byte
{
    Context,
    Snapshot,
    RequestSync,
    RemovePlayer
}

public static class PacketSender
{
    private static ModPacket Create(PacketType type)
    {
        var packet = ModContent.GetInstance<DPSPanel>().GetPacket();
        packet.Write(FightPacketCodec.Version);
        packet.Write((byte)type);
        return packet;
    }

    public static void Context(FightContext fight, int toClient = -1)
    {
        if (Main.netMode != NetmodeID.Server)
            return;
        var packet = Create(PacketType.Context);
        packet.Write(fight.Id);
        packet.Write(fight.Name);
        packet.Write(fight.HeadIcon);
        packet.Write(fight.IsDummy);
        packet.Write(fight.IsAlive);
        packet.Send(toClient);
    }

    public static void Snapshot(long fightId, PlayerFightData player, int toClient = -1)
    {
        if (Main.netMode == NetmodeID.SinglePlayer)
            return;
        var packet = Create(PacketType.Snapshot);
        FightPacketCodec.WriteSnapshot(packet, fightId, player);
        packet.Send(toClient);
    }

    public static void RequestSync()
    {
        if (Main.netMode == NetmodeID.MultiplayerClient)
            Create(PacketType.RequestSync).Send();
    }

    public static void RemovePlayer(long fightId, int playerId)
    {
        if (Main.netMode != NetmodeID.Server)
            return;
        var packet = Create(PacketType.RemovePlayer);
        packet.Write(fightId);
        packet.Write(playerId);
        packet.Send();
    }
}
