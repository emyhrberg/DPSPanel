#if DEBUG
using System;
using System.Collections.Generic;
using System.Linq;
using DPSPanel.Common.Configs;
using DPSPanel.Common.DamageCalculation.Classes;
using DPSPanel.UI;
using Microsoft.Xna.Framework.Input;
using Terraria.DataStructures;
using Terraria.ID;

namespace DPSPanel.Core.Debug;

[Autoload(Side = ModSide.Client)]
public sealed class DebugKeybindPlayer : ModPlayer
{
    public override void OnEnterWorld()
    {
        if (Player.whoAmI == Main.myPlayer)
            Main.NewText("DPSPanel debug: Num1 adds a test player; Num. toggles preview; Shift+Num0 shows all keys. Enable Num Lock.", DebugKeybinds.MessageColor);
    }
}

[Autoload(Side = ModSide.Client)]
public sealed class DebugKeybinds : ModSystem
{
    internal static readonly Color MessageColor = new(255, 170, 60);
    private static readonly string[] Themes = ["Default", "Fancy", "Golden", "Leaf", "Retro", "Sticks", "StoneGold", "Tribute", "TwigLeaf", "Valkyrie"];
    private static readonly string[] Widths = ["Small", "Medium", "Large"];
    private static readonly int[] Weapons = [ItemID.CopperShortsword, ItemID.WoodenBow, ItemID.TacticalShotgun,
        ItemID.FlowerPow, ItemID.Megashark, ItemID.TerraBlade, ItemID.LastPrism, ItemID.BeeGun,
        ItemID.StarWrath, ItemID.RainbowRod, ItemID.Flamethrower, ItemID.Minishark, -1];
    private static readonly int[] Bosses = [NPCID.KingSlime, NPCID.EyeofCthulhu, NPCID.EaterofWorldsHead, NPCID.TheDestroyer];
    private DebugPreviewState preview = new();
    private readonly Dictionary<int, Player> portraits = [];
    private bool simulate;
    private bool weaponView;
    private int bossIndex;
    private double nextTick;
    private int tick;
    public bool PreviewActive { get; private set; }
    private MainSystem UI => ModContent.GetInstance<MainSystem>();

    public override void OnWorldLoad() => Reset();
    public override void OnWorldUnload() => Reset();

    private void Reset()
    {
        PreviewActive = simulate = weaponView = false;
        preview = new();
        portraits.Clear();
        tick = bossIndex = 0;
        nextTick = 0;
        UI?.PresentEncounter(true);
    }

    public override void UpdateUI(GameTime gameTime)
    {
        if (Main.gameMenu || Main.dedServ || !Main.hasFocus || Main.drawingPlayerChat || Main.editSign ||
            Main.editChest || Main.blockInput || Main.inFancyUI || UI?.state == null)
            return;

        bool shift = Main.keyState.IsKeyDown(Keys.LeftShift) || Main.keyState.IsKeyDown(Keys.RightShift);
        if (KeyboardHelper.Pressed(Keys.NumPad1)) { Activate(); AddPlayer(); Publish(); Describe(); }
        else if (KeyboardHelper.Pressed(Keys.NumPad2))
        {
            Activate();
            if (preview.SelectedPlayerId is int id) portraits.Remove(id);
            preview.RemovePlayer(); Publish(); Describe();
        }
        else if (KeyboardHelper.Pressed(Keys.NumPad3)) { Activate(); preview.NextPlayer(); Publish(); Describe(); }
        else if (KeyboardHelper.Pressed(Keys.NumPad4)) { Activate(); AddWeapon(); Publish(); Describe(); }
        else if (KeyboardHelper.Pressed(Keys.NumPad5)) { Activate(); preview.RemoveWeapon(); Publish(); Describe(); }
        else if (KeyboardHelper.Pressed(Keys.NumPad6)) { Activate(); preview.NextWeapon(); Publish(); Describe(); }
        else if (KeyboardHelper.Pressed(Keys.NumPad7))
        {
            Activate();
            if (shift) { weaponView = !weaponView; Publish(true); Say(weaponView ? "Selected player's weapons in main panel." : "Multiplayer player rows and hover popups."); }
            else { simulate = !simulate; Say($"Simulated damage {(simulate ? "on" : "off")}."); }
        }
        else if (KeyboardHelper.Pressed(Keys.NumPad8))
        {
            if (shift) { bossIndex = (bossIndex + 1) % Bosses.Length; Say($"Selected boss: {Lang.GetNPCNameValue(Bosses[bossIndex])}"); }
            else SpawnBoss();
        }
        else if (KeyboardHelper.Pressed(Keys.NumPad9)) RemoveDebugBosses();
        else if (KeyboardHelper.Pressed(Keys.NumPad0))
        {
            if (shift) Help();
            else { Activate(); ClearPreview(); Say("Preview cleared."); }
        }
        else if (KeyboardHelper.Pressed(Keys.Add)) { Activate(); preview.ChangeDamage(shift ? 10000 : 100); Publish(); Describe(); }
        else if (KeyboardHelper.Pressed(Keys.Subtract)) { Activate(); preview.ChangeDamage(shift ? -10000 : -100); Publish(); Describe(); }
        else if (KeyboardHelper.Pressed(Keys.Multiply))
        {
            var c = Config.Conf.C;
            c.Theme = Cycle(Themes, c.Theme, shift);
            UI.RequestRebuild(); Say($"Theme: {c.Theme} (session preview).");
        }
        else if (KeyboardHelper.Pressed(Keys.Divide))
        {
            var c = Config.Conf.C;
            c.Width = Cycle(Widths, c.Width, shift);
            UI.RequestRebuild(); Say($"Width: {c.Width} (session preview).");
        }
        else if (KeyboardHelper.Pressed(Keys.Decimal))
        {
            if (shift) StressPreview();
            else if (PreviewActive) { PreviewActive = false; UI.PresentEncounter(true); Say("Showing real encounter data."); }
            else { Activate(); if (preview.Count == 0) AddPlayer(); Publish(true); Say("Showing local test data."); }
        }

        double now = gameTime.TotalGameTime.TotalSeconds;
        if (PreviewActive && simulate && now >= nextTick)
        {
            nextTick = now + 0.25;
            preview.SimulateDamage(tick++);
            Publish();
        }
    }

    private static string Cycle(string[] values, string current, bool backwards) =>
        values[(Math.Max(0, Array.IndexOf(values, current)) + (backwards ? values.Length - 1 : 1)) % values.Length];

    private void Activate()
    {
        if (PreviewActive)
            return;
        PreviewActive = true;
        UI.state.container.panel.Reset();
        Say("Local test preview active (Num. returns to the real encounter).");
    }

    private void AddPlayer()
    {
        int id = preview.AddPlayer();
        Color color = ColorHelper.standardColors[id % ColorHelper.standardColors.Length];
        portraits[id] = new Player { active = true, name = preview.SelectedPlayerName, hair = (id - 1000) % 20,
            hairColor = color, shirtColor = color, underShirtColor = color, direction = id % 2 == 0 ? 1 : -1 };
        AddWeapon();
    }

    private void AddWeapon()
    {
        if (preview.Count == 0)
        {
            AddPlayer();
            return;
        }
        foreach (int id in Weapons)
            if (preview.AddWeapon(id, id < 0 ? "Unknown" : Lang.GetItemNameValue(id), 100 + tick++ * 25L))
                return;
        Say("All sample weapons added to this player.");
    }

    private void Publish(bool reset = false)
    {
        var panel = UI.state.container.panel;
        if (reset) panel.Reset();
        var players = preview.Snapshots();
        if (weaponView)
            players = players.Where(p => p.PlayerId == preview.SelectedPlayerId).ToArray();
        panel.SetPlayers(players, weaponView, portraits);
        panel.SetFight(new FightContext(long.MaxValue, "Debug preview", -1, false, true));
    }

    public void ClearPreview()
    {
        preview.Clear();
        portraits.Clear();
        simulate = false;
        Publish(true);
    }

    private void StressPreview()
    {
        Activate();
        ClearPreview();
        weaponView = false;
        for (int i = 0; i < 12; i++)
        {
            AddPlayer();
            for (int j = 1; j < Weapons.Length; j++) AddWeapon();
        }
        Publish(true);
        Say("12 players with 13 weapons each. Hover and scroll either panel; Num7 animates rankings.");
    }

    private void SpawnBoss()
    {
        if (Main.netMode != NetmodeID.SinglePlayer)
        {
            Say("Boss debug spawning is available in singleplayer.");
            return;
        }
        // Switch back to real tracking so this also exercises encounter boundaries.
        PreviewActive = false;
        UI.PresentEncounter(true);
        var player = Main.LocalPlayer;
        int slot = NPC.NewNPC(new EntitySource_Misc(DebugSpawnedNPC.SourceContext),
            (int)player.Center.X + 400, (int)player.Center.Y - 250, Bosses[bossIndex], Target: player.whoAmI);
        if (slot >= 0 && slot < Main.maxNPCs)
        {
            Main.npc[slot].GetGlobalNPC<DebugSpawnedNPC>().IsDebug = true;
            Say($"Spawned {Main.npc[slot].FullName}. Num9 removes debug NPCs without loot.");
        }
        else Say("No free NPC slot.");
    }

    private static void RemoveDebugBosses()
    {
        if (Main.netMode != NetmodeID.SinglePlayer)
        {
            Say("Boss debug removal is available in singleplayer.");
            return;
        }
        var debugIds = Main.npc.Where(n => n.active && n.GetGlobalNPC<DebugSpawnedNPC>().IsDebug)
            .Select(n => n.whoAmI).ToHashSet();
        int count = 0;
        foreach (NPC npc in Main.npc)
            if (npc.active && (debugIds.Contains(npc.whoAmI) || debugIds.Contains(npc.realLife)))
            {
                npc.active = false;
                count++;
            }
        Say($"Removed {count} debug NPCs.");
    }

    private void Describe() => Say($"{preview.SelectedPlayerName} | {preview.SelectedWeapon?.weaponName ?? "No weapon"} | {preview.SelectedWeapon?.damage ?? 0:N0} damage");
    private static void Say(string message) => Main.NewText($"[DPSPanel] {message}", MessageColor);
    private static void Help()
    {
        Say("Num1/2: add/remove player; Num3: next player. Num4/5: add/remove weapon; Num6: next weapon.");
        Say("Num7: animate damage; Shift+Num7: player/weapon view. Num8: spawn boss; Shift+Num8: choose boss; Num9: remove debug bosses.");
        Say("Num0: clear preview; Num +/-: +/-100 damage (Shift: 10,000). Num*: theme; Num/: width (Shift: reverse).");
        Say("Num.: test/real data; Shift+Num.: stress fixture; Shift+Num0: this help. Theme/width keys are session previews; save via mod config to persist.");
    }
}

[Autoload(Side = ModSide.Client)]
public sealed class DebugSpawnedNPC : GlobalNPC
{
    internal const string SourceContext = "DPSPanelDebug";
    public override bool InstancePerEntity => true;
    internal bool IsDebug;

    public override void OnSpawn(NPC npc, IEntitySource source)
    {
        IsDebug = source.Context == SourceContext ||
            source is EntitySource_Parent { Entity: NPC parent } && parent.GetGlobalNPC<DebugSpawnedNPC>().IsDebug;
    }
}
#endif
