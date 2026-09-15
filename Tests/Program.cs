using DPSPanel.Common.DamageCalculation.Classes;
using DPSPanel.Core.Utilities;
using DPSPanel.Networking;
using DPSPanel.UI;
using DPSPanel.Core.Debug;

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
Check("Unchanged layout defaults do not repeatedly rebuild the UI", () =>
{
    DPSPanelLayout.Update();
    int version = DPSPanelLayout.Version;
    for (int i = 0; i < 100; i++)
        Assert(!DPSPanelLayout.Update(), "An unchanged frame requested rebuilding.");
    Assert(DPSPanelLayout.Version == version, "Layout version changed without an edit.");
});
Check("Applying changed layout values advances the version once", () =>
{
    int version = DPSPanelLayout.Version;
    DPSPanelLayout.SmallWidth = 999;
    DPSPanelLayout.PanelPaddingTop = 999;
    Assert(DPSPanelLayout.Update(), "Changed layout was not detected.");
    Assert(DPSPanelLayout.Version == version + 1 && DPSPanelLayout.SmallWidth == 150,
        "Layout was not reapplied as one revision.");
});
Check("Width and theme changes invalidate the appearance independently", () =>
{
    var appearance = new PanelAppearance("Default", "Small", "Damage", true, true, true);
    Assert(appearance != appearance with { Theme = "Fancy" }, "Theme does not trigger a rebuild.");
    Assert(appearance != appearance with { Width = "Large" }, "Width does not trigger a rebuild.");
    Assert(appearance != appearance with { ShowPlayerIcons = false }, "Icon toggle does not trigger a rebuild.");
    Assert(DPSPanelLayout.WidthFor("Large") == 450 && DPSPanelLayout.WidthFor("Medium") == 300 &&
        DPSPanelLayout.WidthFor("invalid") == 150, "Config width mapping is incorrect.");
});
Check("Custom bar heights and gaps include exactly the last row", () =>
{
    foreach (int count in new[] { 1, 3, 30 })
        foreach (float height in new[] { 12f, 64f, 100f })
            foreach (float gap in new[] { 0f, 7f, 30f })
                Assert(PanelLayout.RowTop(count - 1, height, gap) + height == PanelLayout.RowsHeight(count, height, gap),
                    "Custom row layout clips or adds an extra trailing gap.");
});
Check("Height limits create a viewport without discarding content", () =>
{
    var metrics = PanelLayout.Measure(1000, 65, 0, 300, 800);
    Assert(metrics.Height == 300 && metrics.ViewportHeight == 235 && metrics.ContentHeight == 1000,
        "Panel limits lost rows or ignored chrome/padding.");
    Assert(PanelLayout.Measure(1000, 65, 0, 0, 200).Height == 200, "Screen height is not respected.");
});
Check("Automatic and fixed panel heights respect padding and minimums", () =>
{
    Assert(PanelLayout.Measure(140, 24, 0, 0, 1000).Height == 164, "Automatic popup height is wrong.");
    Assert(PanelLayout.Measure(40, 50, 300, 300, 1000).Height == 300, "Fixed height is not supported.");
    Assert(PanelLayout.Measure(0, 50, 0, 0, 1000).ViewportHeight == 0, "Empty panels retain row space.");
    Assert(PanelLayout.Measure(1000, 50, 500, 200, 1000).Height == 200, "Minimum overrode the maximum.");
});
Check("Scrolling clamps after deletion or a viewport resize", () =>
{
    Assert(PanelLayout.ClampScroll(900, 1000, 200) == 800, "Scroll exceeded the last row.");
    Assert(PanelLayout.ClampScroll(800, 100, 200) == 0, "Deleted rows left an empty scrolled panel.");
    Assert(PanelLayout.ClampScroll(-40, 1000, 200) == 0, "Negative scroll was accepted.");
    Assert(PanelLayout.ClampScroll(800, 1000, 600) == 400, "Larger viewport did not clamp scrolling.");
});
Check("Debug players have isolated weapon lists and identities", () =>
{
    var preview = new DebugPreviewState();
    int a = preview.AddPlayer();
    preview.AddWeapon(10, "A weapon", 100);
    int b = preview.AddPlayer();
    preview.AddWeapon(20, "B weapon", 200);
    preview.ChangeDamage(50);
    var snapshots = preview.Snapshots();
    Assert(a >= 1000 && b != a, "Preview used a real player slot.");
    Assert(snapshots.Single(p => p.PlayerId == a).Damage == 100, "Editing B changed A.");
    Assert(snapshots.Single(p => p.PlayerId == b).Weapons.Single().weaponItemID == 20, "B inherited A's weapon.");
});
Check("Removing a debug player removes its weapons and preserves remaining identity", () =>
{
    var preview = new DebugPreviewState();
    int a = preview.AddPlayer();
    preview.AddWeapon(10, "A", 100);
    int b = preview.AddPlayer();
    preview.AddWeapon(20, "B", 200);
    preview.RemovePlayer();
    Assert(preview.SelectedPlayerId == a && preview.Snapshots().Single().Weapons.Single().weaponItemID == 10,
        "Removed player's selection or weapons survived.");
    Assert(preview.AddPlayer() != b, "New preview player reused stale identity.");
});
Check("Debug weapon deletion and selection handle the final entry", () =>
{
    var preview = new DebugPreviewState();
    preview.AddPlayer();
    preview.AddWeapon(10, "A", 100);
    Assert(!preview.AddWeapon(10, "Duplicate", 200), "Duplicate weapon ID was added.");
    preview.AddWeapon(20, "B", 200);
    preview.RemoveWeapon();
    Assert(preview.SelectedWeapon.weaponItemID == 10, "Weapon selection did not clamp after deletion.");
    preview.RemoveWeapon();
    preview.NextWeapon();
    preview.ChangeDamage(100);
    Assert(preview.SelectedWeapon == null && preview.Snapshots().Single().Weapons.Count == 0,
        "Final weapon cannot be removed cleanly.");
});
Check("Debug damage snapshots stay immutable and clamp extreme edits", () =>
{
    var preview = new DebugPreviewState();
    preview.AddPlayer();
    preview.AddWeapon(10, "A", 100);
    var before = preview.Snapshots().Single();
    preview.ChangeDamage(long.MinValue);
    Assert(preview.SelectedWeapon.damage == 0, "Damage became negative.");
    preview.ChangeDamage(long.MaxValue);
    Assert(preview.SelectedWeapon.damage == 1_000_000_000_000L, "Damage overflowed.");
    Assert(before.Damage == 100 && before.Revision < preview.Snapshots().Single().Revision,
        "An existing snapshot changed in place.");
});
Check("Simulation and clearing work on empty and populated previews", () =>
{
    var preview = new DebugPreviewState();
    preview.SimulateDamage(0);
    preview.NextPlayer();
    preview.NextWeapon();
    Assert(!preview.RemovePlayer() && !preview.RemoveWeapon(), "Empty removals reported success.");
    preview.AddPlayer(); preview.AddWeapon(10, "A");
    preview.AddPlayer(); preview.AddWeapon(20, "B");
    preview.SimulateDamage(0);
    Assert(preview.Snapshots().All(p => p.Damage > 100), "Simulation skipped a player.");
    preview.Clear();
    Assert(preview.Count == 0 && preview.SelectedPlayerId == null && preview.SelectedWeapon == null,
        "Clear left stale selection or data.");
});
Console.WriteLine($"All {passed} regression checks passed.");

