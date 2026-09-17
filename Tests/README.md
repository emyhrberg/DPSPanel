# DPSPanel regression checks

Run the dependency-free checks with:

```powershell
dotnet run --project Tests/DPSPanel.RegressionTests.csproj
```

These checks compile the production snapshot, packet-codec and layout files directly.
They cover player ownership, complete weapon-list replacement, stale/repeated packets,
new encounters, clear baselines, reused player slots, large damage totals, malformed
packets, bar/panel heights and stable ordering during hovering. They also cover live-layout
versioning, appearance changes, custom row geometry, height limits, scroll clamping, and
debug-fixture ownership, selection, deletion and immutable snapshots. Drag checks cover
click jitter, grab offsets at five resolutions and five UI scales, dragging outside the
panel, quick release, returning to the press point, and cancelled capture (44 checks total).

See [DEBUGGING.md](../DEBUGGING.md) for all numpad controls and the C# Hot Reload workflow.

## In-game verification

The checks do not create a graphics device or simulate a multiplayer connection.
After closing tModLoader, build DPSPanel in Visual Studio and launch the Terraria
profile. Use the updated 0.6.2 mod on the server and every client.

1. In singleplayer, hit a target dummy with three weapons. The first hit should count;
   each row should fit inside the panel. Ctrl-click the toggle to clear, then hit once:
   only that new damage should appear.
2. Fight a normal boss, Eater of Worlds and another segmented boss. End each encounter
   and summon another boss, including another of the same type. The next encounter
   should start with empty totals, while the completed encounter remains readable.
3. In multiplayer, have two players use different weapons. Hover each row and verify
   that its popup shows only that player's weapons.
4. Keep the pointer on one row or its popup while the other player overtakes it in
   damage. The inspected player should stay selected. Move away to resume sorting.
5. Change all three widths, themes, damage/percent display, and icon toggles while
   results are visible. Check both the main panel and the weapon popup.
6. Hide the panel and try inventory-only mode. An invisible popup should not capture
   the mouse. Drag the panel at a non-default UI scale and verify its saved position.
7. Leave/rejoin the world and disconnect/reconnect a multiplayer player. Old weapons
   must not carry over into the new world, encounter, or player slot.
8. Repeat at 100%, 125%, 150% and 200% UI scale on different resolutions. Click the
   header without moving: its position must remain unchanged. Drag from a corner,
   the middle of a bar, and the toggle: the original grab point must follow the cursor.
9. Drag outside the original panel and release; drag away and back to the starting
   point before releasing. Neither action should accidentally toggle the panel.
   Alt-tab or resize while holding the mouse, then release and return: dragging must stop.
10. Disable panel dragging and verify clicks still toggle. Ctrl/Alt-click should clear
    or open settings without moving the panel. Drag a test-player row, remove it with
    Num2 while still holding the mouse, then release: the panel must stop following it.

## Encounter behavior

- Bosses alive at the same time share one encounter.
- The server assigns encounter IDs and relays complete per-player snapshots.
- Ctrl-click clears the local display using damage baselines. Subsequent hits count
  from zero without restoring the previous totals or clearing other players' screens.
- Target-dummy tracking is singleplayer-only and is paused during boss encounters.
- Percentages retain the original comparison against the highest-damage row.
