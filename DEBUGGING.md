# Live panel layout and debug controls

## Start a development session

1. Close any running tModLoader instance so its mod file can be replaced.
2. In Visual Studio, select **Debug** and build DPSPanel. Start the **Terraria** launch profile with **F5**.
   DPSPanel is a class library loaded by tModLoader; launching the DLL directly is not supported.
3. Enter a world and enable **Num Lock**. These shortcuts are compiled only when `DEBUG` is defined.
   A Release build excludes the keybinds and debug NPC hooks.

## Hot reload the layout

Edit the literal values in `UI/DPSPanelLayout.cs`, **inside `Update()`**, then use Visual Studio's
**Apply Code Changes / Hot Reload** button while the debugger is attached. For example:

```csharp
Set(ref MediumWidth, 360f, ref changed);
Set(ref PlayerBarHeight, 48f, ref changed);
Set(ref WeaponBarHeight, 36f, ref changed);
Set(ref PanelPaddingLeft, 12f, ref changed);
Set(ref PopupPaddingTop, 10f, ref changed);
Set(ref MainMaxHeight, 400f, ref changed);
Set(ref BarTextOffsetY, -2f, ref changed);
```

`MainSystem` calls `Update()` each frame. Changed values increment `Version`, triggering a full
rebuild of the existing main rows and popup rows, including their theme textures. Data, hovered
player identity, row ordering and scroll offsets are preserved. Rebuilding waits until dragging
ends. Unchanged frames do not rebuild the controls.

The layout file contains widths, independent player/weapon heights, main/popup min/max heights,
all four padding values, row insets and gaps, popup positioning, header spacing, text scale and
offsets, icon size/position, toggle geometry, colors, and scroll settings. Values are UI pixels,
before Terraria's UI scale.

- Height `0` means automatic. Panels fit their rows until the available screen height is reached.
- Set minimum and maximum to the same positive value for a fixed panel height.
- Extra rows remain accessible with the mouse wheel. The scroll indicator shows the visible range.
- Width names **Small / Medium / Large** select the corresponding values in this layout file.
- Normal config changes also rebuild both panels, including completed encounters and hidden panels.
- Edit method-body values rather than field initializers: initializers do not run again during Hot Reload.
- This is C#/.NET Hot Reload; editing the file alone without applying code changes does not update a running game.

Official tModLoader guidance: [Edit and Continue](https://github.com/tModLoader/tModLoader/wiki/Why-Use-an-IDE#edit-and-continue).

## Keypad controls

| Key | Action | With Shift |
| --- | --- | --- |
| Num1 | Add and select a test player, with one weapon | Same |
| Num2 | Remove selected test player and its weapons | Same |
| Num3 | Select next test player | Same |
| Num4 | Add and select the next sample weapon | Same |
| Num5 | Remove selected test weapon | Same |
| Num6 | Select next test weapon | Same |
| Num7 | Toggle simulated damage and ranking changes | Switch main panel between player rows and selected player's weapons |
| Num8 | Spawn selected boss in singleplayer; show real encounter data | Select next boss: King Slime, Eye, Eater, Destroyer |
| Num9 | Remove debug-created NPCs, without loot or kill progression | Same |
| Num0 | Clear test preview | Show keybind help |
| Num+ | Add 100 damage to selected weapon | Add 10,000 |
| Num- | Subtract 100 damage, down to zero | Subtract 10,000 |
| Num* | Next theme | Previous theme |
| Num/ | Next width | Previous width |
| Num. | Toggle test preview / real encounter | Create 12 players with 13 weapons each |

Num Lock remains the keypad mode switch. Terraria/XNA exposes keypad Enter as the same `Keys.Enter`
as normal Enter, so it is left available for chat. Shortcuts pause while typing, in menus/config,
or while the game is unfocused. Chat reports the selected player and weapon after edits.

Test players are local UI fixtures, with IDs outside Terraria's player slots. They do not create
real world players or send damage packets. Real combat tracking continues underneath the preview;
Num. restores its current results. Returning to a world clears all fixtures. Ctrl-clicking the
panel toggle clears whichever data source is currently displayed.

Theme/width shortcuts change the current config in memory for quick comparison. They do not write
a config file; make and save changes through the normal mod config for persistence.

Boss controls are singleplayer-only and spawn real NPCs. Num9 targets NPCs marked as created by
these controls (including NPCs spawned by them and linked worm segments). Normal gameplay rules
still apply if you fight them: defeating them can grant loot/progression, and night bosses can
leave during daytime.

## Suggested visual checks

1. Press **Shift+Num.** for the large fixture and **Num7** to animate it. Scroll both panels and
   verify every row is reachable. Hover a player while rankings change; ownership should remain stable.
2. Add/delete weapons and players while a popup is open; removed entries should disappear immediately.
3. Cycle **Num*** and **Num/** with populated rows. Open a different player's popup after changing them.
4. Change theme/width in the actual mod config, including while the panel is collapsed.
5. Apply layout edits above with Hot Reload; verify the main panel, toggle and popup all resize together.
6. Use **Shift+Num7** to exercise the singleplayer weapon view, and **Num8** to verify a real encounter.

Automated checks: `dotnet run --project Tests/DPSPanel.RegressionTests.csproj`.
These exercise data and layout calculations; actual rendering, key input and applying Hot Reload
still need an in-game development session.
