using DPSPanel.Common.DamageCalculation.Classes;
using DPSPanel.Core.Utilities;
using DPSPanel.Networking;

int passed = 0;
void Check(string name, Action action)
{
    action();
    Console.WriteLine($"PASS {name}");
    passed++;
}
void Assert(bool condition, string message)
{
    if (!condition) throw new Exception(message);
}
void Reject(Action read)
{
    try { read(); } catch (InvalidDataException) { return; }
    throw new Exception("Malformed snapshot was accepted.");
}
PlayerFightData Snapshot(int id, int revision, params Weapon[] weapons) => new(id, $"Player {id}", revision, weapons);
FightState NewFight()
{
    var state = new FightState();
    state.Begin(1);
    return state;
}
byte[] Encode(long id, PlayerFightData player)
{
    using var stream = new MemoryStream();
    using var writer = new BinaryWriter(stream);
    FightPacketCodec.WriteSnapshot(writer, id, player);
    return stream.ToArray();
}
(long FightId, PlayerFightData Player) Decode(byte[] bytes)
{
    using var stream = new MemoryStream(bytes);
    using var reader = new BinaryReader(stream);
    return FightPacketCodec.ReadSnapshot(reader);
}

Check("Interleaved player packets preserve separate weapon ownership", () =>
{
    var state = NewFight();
    state.Apply(1, Snapshot(1, 1, new Weapon(10, "Sword", 100)));
    state.Apply(1, Snapshot(2, 1, new Weapon(20, "Shotgun", 200)));
    state.Apply(1, Snapshot(1, 2, new Weapon(10, "Sword", 300)));
    Assert(state.Players[2].Weapons.Single().weaponItemID == 20, "Player B inherited A's weapons.");
    Assert(state.Players[2].Damage == 200, "Player B's damage changed.");
});
Check("A replacement snapshot removes stale weapon entries", () =>
{
    var state = NewFight();
    state.Apply(1, Snapshot(1, 1, new Weapon(10, "Sword", 100), new Weapon(20, "Old gun", 200)));
    state.Apply(1, Snapshot(1, 2, new Weapon(10, "Sword", 120)));
    Assert(state.VisiblePlayers().Single().Weapons.Count == 1, "Old gun survived replacement.");
});
Check("Empty weapon lists remove the visible player row", () =>
{
    var state = NewFight();
    state.Apply(1, Snapshot(1, 1, new Weapon(10, "Sword", 100)));
    state.Apply(1, Snapshot(1, 2));
    Assert(!state.VisiblePlayers().Any(), "An empty player is still visible.");
});
Check("Duplicate and out-of-order snapshots cannot restore stale data", () =>
{
    var state = NewFight();
    state.Apply(1, Snapshot(1, 3, new Weapon(10, "Sword", 300)));
    Assert(!state.Apply(1, Snapshot(1, 2, new Weapon(20, "Old gun", 200))), "Old revision accepted.");
    Assert(!state.Apply(1, Snapshot(1, 3, new Weapon(20, "Old gun", 200))), "Duplicate revision accepted.");
    Assert(state.Players[1].Weapons.Single().weaponItemID == 10, "Stale weapon reappeared.");
});
Check("A new encounter clears every player's old weapons", () =>
{
    var state = NewFight();
    state.Apply(1, Snapshot(1, 1, new Weapon(10, "Sword", 100)));
    state.Apply(1, Snapshot(2, 1, new Weapon(20, "Gun", 100)));
    Assert(state.Begin(2) && state.Players.Count == 0, "New fight kept old entries.");
    Assert(!state.Apply(1, Snapshot(1, 20, new Weapon(20, "Old gun", 1000))), "Late previous-fight packet accepted.");
    Assert(state.Apply(2, Snapshot(1, 1, new Weapon(30, "New weapon", 5))), "New fight's first hit was lost.");
});
Check("Repeated start packets do not clear an active encounter", () =>
{
    var state = NewFight();
    state.Apply(1, Snapshot(1, 1, new Weapon(10, "Sword", 100)));
    Assert(!state.Begin(1) && state.Players.Count == 1, "Duplicate context cleared the fight.");
    Assert(!state.Begin(0), "Earlier context accepted.");
});
Check("Clearing removes old totals and counts only subsequent damage", () =>
{
    var state = NewFight();
    state.Apply(1, Snapshot(1, 1, new Weapon(10, "Sword", 100), new Weapon(20, "Gun", 200)));
    state.ClearVisible();
    Assert(!state.VisiblePlayers().Any(), "Clear left visible rows.");
    state.Apply(1, Snapshot(1, 2, new Weapon(10, "Sword", 150), new Weapon(20, "Gun", 200), new Weapon(30, "Bow", 75)));
    var player = state.VisiblePlayers().Single();
    Assert(player.Weapons.Count == 2 && player.Damage == 125, "Clear restored old weapons/totals.");
    state.ClearVisible();
    Assert(!state.VisiblePlayers().Any(), "Second clear failed.");
});
Check("Clear baselines are independent for each player", () =>
{
    var state = NewFight();
    state.Apply(1, Snapshot(1, 1, new Weapon(10, "Sword", 100)));
    state.Apply(1, Snapshot(2, 1, new Weapon(10, "Sword", 300)));
    state.ClearVisible();
    state.Apply(1, Snapshot(1, 2, new Weapon(10, "Sword", 110)));
    state.Apply(1, Snapshot(2, 2, new Weapon(10, "Sword", 340)));
    Assert(state.VisiblePlayers().Single(p => p.PlayerId == 1).Damage == 10, "Player A baseline wrong.");
    Assert(state.VisiblePlayers().Single(p => p.PlayerId == 2).Damage == 40, "Player B baseline wrong.");
});
Check("Reused player slots can start a fresh revision sequence", () =>
{
    var state = NewFight();
    state.Apply(1, Snapshot(1, 40, new Weapon(10, "Old player", 100)));
    state.ClearVisible();
    state.RemovePlayer(1);
    Assert(state.Apply(1, Snapshot(1, 1, new Weapon(20, "New player", 20))), "Reconnected player rejected.");
    Assert(state.VisiblePlayers().Single().Damage == 20, "Reconnected player inherited a baseline.");
});
Check("Snapshots are independent of mutable collector lists", () =>
{
    var weapons = new List<Weapon> { new Weapon(10, "Sword", 100) };
    var player = new PlayerFightData(1, "A", 1, weapons);
    weapons.Clear();
    Assert(player.Weapons.Count == 1, "Snapshot referenced the collector's list.");
});
Check("Identical weapon names remain distinct by item ID", () =>
{
    var state = NewFight();
    state.Apply(1, Snapshot(1, 1, new Weapon(10, "Sword", 100), new Weapon(20, "Sword", 200)));
    Assert(state.VisiblePlayers().Single().Weapons.Count == 2, "Distinct item IDs were merged.");
});
Check("World reset clears encounter IDs, players and clear baselines", () =>
{
    var state = NewFight();
    state.Apply(1, Snapshot(1, 1, new Weapon(10, "Sword", 100)));
    state.ClearVisible();
    state.Reset();
    Assert(state.Id == 0 && state.Players.Count == 0, "World reset retained state.");
    state.Begin(1);
    state.Apply(1, Snapshot(1, 1, new Weapon(10, "Sword", 5)));
    Assert(state.VisiblePlayers().Single().Damage == 5, "New world inherited a baseline.");
});
Check("Network round trip preserves owner, revision, weapon IDs and large totals", () =>
{
    var result = Decode(Encode(42, new PlayerFightData(4, "Two players can share this name", 7,
        [new Weapon(10, "Weapon", (long)int.MaxValue + 100), new Weapon(-1, "Unknown", 25)])));
    Assert(result.FightId == 42 && result.Player.PlayerId == 4 && result.Player.Revision == 7, "Packet identity changed.");
    Assert(result.Player.Damage == (long)int.MaxValue + 125 && result.Player.Weapons[1].weaponItemID == -1, "Packet data changed.");
});
Check("Network rejects duplicate weapon IDs", () =>
    Reject(() => Decode(Encode(1, Snapshot(1, 1, new Weapon(10, "A", 10), new Weapon(10, "B", 20))))));
Check("Network rejects negative damage", () =>
    Reject(() => Decode(Encode(1, Snapshot(1, 1, new Weapon(10, "A", -1))))));
Check("Network rejects overflowing aggregate damage", () =>
    Reject(() => Decode(Encode(1, Snapshot(1, 1, new Weapon(10, "A", long.MaxValue), new Weapon(20, "B", 1))))));
Check("Network rejects invalid player IDs and encounter IDs", () =>
{
    Reject(() => Decode(Encode(1, Snapshot(255, 1, new Weapon(10, "A", 10)))));
    Reject(() => Decode(Encode(0, Snapshot(1, 1, new Weapon(10, "A", 10)))));
});
Check("Network rejects oversized weapon collections", () =>
    Reject(() => Decode(Encode(1, new PlayerFightData(1, "A", 1,
        Enumerable.Range(0, FightPacketCodec.MaxWeapons + 1).Select(i => new Weapon(i, "A", 1)))))));
Check("The popup's last bar fits inside its bottom padding", () =>
{
    foreach (int count in new[] { 1, 2, 3, 8, 30 })
    {
        float bottom = PanelLayout.Padding + PanelLayout.RowTop(count - 1) + PanelLayout.BarHeight;
        Assert(bottom + PanelLayout.Padding == PanelLayout.Height(count), $"Popup clipped row {count}.");
    }
    Assert(PanelLayout.Height(3) == 150, "Three 40-pixel bars with padding/spacing must occupy 150 pixels.");
});
Check("The main panel includes header, every bar, spacing and padding", () =>
{
    foreach (int count in new[] { 1, 2, 8, 30 })
    {
        float bottom = PanelLayout.Padding + PanelLayout.RowTop(count - 1, PanelLayout.HeaderHeight) + PanelLayout.BarHeight;
        Assert(bottom + PanelLayout.Padding == PanelLayout.Height(count, PanelLayout.HeaderHeight), "Main panel clips the last row.");
    }
});
Check("A cleared panel returns to header height", () =>
    Assert(PanelLayout.Height(0, PanelLayout.HeaderHeight) == 50, "Empty panel retains stale height."));
Check("Hover freezes row identity when damage rankings change", () =>
{
    var order = PanelLayout.OrderPlayers([1, 2], [(1, 100L), (2, 500L)], true);
    Assert(order.SequenceEqual([1, 2]), "Rows swapped under the pointer.");
    var sorted = PanelLayout.OrderPlayers(order, [(1, 100L), (2, 500L)], false);
    Assert(sorted.SequenceEqual([2, 1]), "Ranking did not resume after hovering.");
});
Check("New and disconnected players do not displace a hovered row unnecessarily", () =>
{
    var order = PanelLayout.OrderPlayers([1, 2], [(1, 100L), (2, 500L), (3, 1000L)], true);
    Assert(order.SequenceEqual([1, 2, 3]), "New player displaced the hovered row.");
    order = PanelLayout.OrderPlayers(order, [(1, 100L), (3, 1000L)], true);
    Assert(order.SequenceEqual([1, 3]), "Disconnected player left a stale row.");
});
Check("Equal damage uses stable player-ID ordering", () =>
    Assert(PanelLayout.OrderPlayers([], [(5, 100L), (2, 100L)], false).SequenceEqual([2, 5]), "Ties reorder unpredictably."));
Console.WriteLine($"All {passed} regression checks passed.");

