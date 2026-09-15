using System;
using System.Collections.Generic;
using System.Linq;
using DPSPanel.Common.DamageCalculation.Classes;

namespace DPSPanel.Core.Debug;

/// <summary>Local UI fixtures, independent of Terraria's player slots and network encounter data.</summary>
public sealed class DebugPreviewState
{
    private sealed class TestPlayer(int id, string name)
    {
        public int Id = id;
        public string Name = name;
        public int Revision;
        public List<Weapon> Weapons = [];
        public int WeaponIndex;
    }

    private readonly List<TestPlayer> players = [];
    private int nextPlayerId = 1000;
    private int selectedIndex;
    private TestPlayer Selected => players.Count == 0 ? null : players[selectedIndex];
    public int? SelectedPlayerId => Selected?.Id;
    public string SelectedPlayerName => Selected?.Name ?? "No test player";
    public Weapon SelectedWeapon => Selected is { Weapons.Count: > 0 } p ? p.Weapons[p.WeaponIndex] : null;
    public int Count => players.Count;

    public int AddPlayer()
    {
        int id = nextPlayerId++;
        players.Add(new TestPlayer(id, $"Test player {id - 999}"));
        selectedIndex = players.Count - 1;
        return id;
    }

    public bool RemovePlayer()
    {
        if (Selected == null)
            return false;
        players.RemoveAt(selectedIndex);
        selectedIndex = Math.Clamp(selectedIndex, 0, Math.Max(0, players.Count - 1));
        return true;
    }

    public void NextPlayer()
    {
        if (players.Count > 0)
            selectedIndex = (selectedIndex + 1) % players.Count;
    }

    public bool AddWeapon(int itemId, string name, long damage = 100)
    {
        if (Selected is not { } p || p.Weapons.Any(w => w.weaponItemID == itemId))
            return false;
        p.Weapons.Add(new Weapon(itemId, name, Math.Clamp(damage, 0, 1_000_000_000_000L)));
        p.WeaponIndex = p.Weapons.Count - 1;
        p.Revision++;
        return true;
    }

    public bool RemoveWeapon()
    {
        if (Selected is not { Weapons.Count: > 0 } p)
            return false;
        p.Weapons.RemoveAt(p.WeaponIndex);
        p.WeaponIndex = Math.Clamp(p.WeaponIndex, 0, Math.Max(0, p.Weapons.Count - 1));
        p.Revision++;
        return true;
    }

    public void NextWeapon()
    {
        if (Selected is { Weapons.Count: > 0 } p)
            p.WeaponIndex = (p.WeaponIndex + 1) % p.Weapons.Count;
    }

    public void ChangeDamage(long delta)
    {
        if (Selected is not { Weapons.Count: > 0 } p)
            return;
        Weapon weapon = p.Weapons[p.WeaponIndex];
        decimal value = (decimal)weapon.damage + delta;
        p.Weapons[p.WeaponIndex] = weapon with { damage = (long)Math.Clamp(value, 0, 1_000_000_000_000m) };
        p.Revision++;
    }

    public void SimulateDamage(int tick)
    {
        for (int i = 0; i < players.Count; i++)
        {
            var p = players[i];
            for (int j = 0; j < p.Weapons.Count; j++)
            {
                var weapon = p.Weapons[j];
                long gain = (i == tick % players.Count ? 1500 : 10) * (j + 1);
                p.Weapons[j] = weapon with { damage = Math.Min(1_000_000_000_000L, weapon.damage + gain) };
            }
            p.Revision++;
        }
    }

    public IReadOnlyList<PlayerFightData> Snapshots() => players
        .Select(p => new PlayerFightData(p.Id, p.Name, p.Revision, p.Weapons)).ToArray();

    public void Clear()
    {
        players.Clear();
        selectedIndex = 0;
    }
}
