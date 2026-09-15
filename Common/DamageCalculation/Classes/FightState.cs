using System;
using System.Collections.Generic;
using System.Linq;

namespace DPSPanel.Common.DamageCalculation.Classes;

/// <summary>Replaces each player's complete snapshot within one encounter.</summary>
public sealed class FightState
{
    private readonly Dictionary<int, PlayerFightData> players = [];
    private readonly Dictionary<int, Dictionary<int, long>> clearBaselines = [];
    public long Id { get; private set; }
    public IReadOnlyDictionary<int, PlayerFightData> Players => players;

    public bool Begin(long id)
    {
        if (id <= Id)
            return false;
        Reset();
        Id = id;
        return true;
    }

    public bool Apply(long fightId, PlayerFightData snapshot)
    {
        if (Id == 0 || fightId != Id || snapshot.Revision <= 0 ||
            (players.TryGetValue(snapshot.PlayerId, out var previous) && snapshot.Revision <= previous.Revision))
            return false;
        players[snapshot.PlayerId] = snapshot;
        return true;
    }

    public IEnumerable<PlayerFightData> VisiblePlayers()
    {
        foreach (var player in players.Values)
        {
            clearBaselines.TryGetValue(player.PlayerId, out var baseline);
            var weapons = player.Weapons.Select(w => w with
            {
                damage = Math.Max(0, w.damage - (baseline?.GetValueOrDefault(w.weaponItemID) ?? 0))
            }).Where(w => w.damage > 0).ToArray();
            if (weapons.Length > 0)
                yield return new PlayerFightData(player.PlayerId, player.Name, player.Revision, weapons);
        }
    }

    public void ClearVisible()
    {
        foreach (var player in players.Values)
            clearBaselines[player.PlayerId] = player.Weapons.ToDictionary(w => w.weaponItemID, w => w.damage);
    }

    public void RemovePlayer(int playerId)
    {
        players.Remove(playerId);
        clearBaselines.Remove(playerId);
    }

    public void Reset()
    {
        Id = 0;
        players.Clear();
        clearBaselines.Clear();
    }
}
