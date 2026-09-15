using System;
using System.Collections.Generic;
using System.IO;
using DPSPanel.Common.DamageCalculation.Classes;

namespace DPSPanel.Networking;

/// <summary>One wire format for all boss types, with weapon ownership inside the player snapshot.</summary>
public static class FightPacketCodec
{
    public const byte Version = 1;
    public const int MaxWeapons = 512;

    public static void WriteSnapshot(BinaryWriter writer, long fightId, PlayerFightData player)
    {
        writer.Write(fightId);
        writer.Write(player.PlayerId);
        writer.Write(player.Name);
        writer.Write(player.Revision);
        writer.Write(player.Weapons.Count);
        foreach (var weapon in player.Weapons)
        {
            writer.Write(weapon.weaponItemID);
            writer.Write(weapon.weaponName);
            writer.Write(weapon.damage);
        }
    }

    public static (long FightId, PlayerFightData Player) ReadSnapshot(BinaryReader reader)
    {
        long fightId = reader.ReadInt64();
        int playerId = reader.ReadInt32();
        string name = reader.ReadString();
        int revision = reader.ReadInt32();
        int count = reader.ReadInt32();
        if (fightId <= 0 || playerId < 0 || playerId >= 255 || revision <= 0 ||
            name.Length > 256 || count < 0 || count > MaxWeapons)
            throw new InvalidDataException("Invalid damage snapshot.");

        List<Weapon> weapons = new(count);
        HashSet<int> ids = [];
        long total = 0;
        for (int i = 0; i < count; i++)
        {
            int itemId = reader.ReadInt32();
            string weaponName = reader.ReadString();
            long damage = reader.ReadInt64();
            if (itemId < -1 || weaponName.Length > 256 || damage < 0 ||
                damage > long.MaxValue - total || !ids.Add(itemId))
                throw new InvalidDataException("Invalid weapon entry.");
            total += damage;
            weapons.Add(new Weapon(itemId, weaponName, damage));
        }
        return (fightId, new PlayerFightData(playerId, name, revision, weapons));
    }
}
