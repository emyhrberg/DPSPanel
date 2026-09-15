using System;
using System.Collections.Generic;
using System.Linq;
using DPSPanel.Common.Configs;
using DPSPanel.Common.DamageCalculation.Classes;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using Terraria.UI.Chat;

namespace DPSPanel.UI;

public sealed class MainPanel : UIPanel
{
    private readonly Dictionary<int, PlayerBar> playerBars = [];
    private readonly Dictionary<int, WeaponBar> weaponBars = [];
    private List<int> playerOrder = [];
    private int? hoveredPlayer;
    private bool singleplayer;
    private FightContext fight;
    public PlayerDamagePanel Popup { get; set; }
    // Kept as the existing toggle's "always visible" setting.
    public bool HideWhenInventoryOpen = true;
    public bool IsDisplayed => Parent is MainContainer { panelVisible: true } &&
        (HideWhenInventoryOpen || Main.playerInventory);

    public MainPanel()
    {
        SetPadding(PanelLayout.Padding);
        MaxHeight.Set(float.MaxValue, 0);
        MaxWidth.Set(float.MaxValue, 0);
        BackgroundColor = new Color(49, 84, 141);
        Width.Set(SizeHelper.GetWidthFromConfig(), 0);
        Height.Set(PanelLayout.Height(0, PanelLayout.HeaderHeight), 0);
    }

    public void SetFight(FightContext context) => fight = context;

    public void Reset()
    {
        RemoveAllChildren();
        playerBars.Clear();
        weaponBars.Clear();
        playerOrder.Clear();
        hoveredPlayer = null;
        fight = null;
        Popup?.Reset();
        ApplyLayout();
    }

    public void SetPlayers(IReadOnlyList<PlayerFightData> players, bool isSingleplayer)
    {
        if (singleplayer != isSingleplayer)
            Reset();
        singleplayer = isSingleplayer;

        if (singleplayer)
        {
            var weapons = players.FirstOrDefault()?.Weapons ?? Array.Empty<Weapon>();
            SetWeapons(weapons);
        }
        else
        {
            var ids = players.Select(p => p.PlayerId).ToHashSet();
            foreach (int id in playerBars.Keys.Where(id => !ids.Contains(id)).ToArray())
            {
                playerBars[id].Remove();
                playerBars.Remove(id);
                if (hoveredPlayer == id)
                    HidePopup();
            }
            long highest = players.Count > 0 ? players.Max(p => p.Damage) : 0;
            foreach (var player in players)
            {
                if (!playerBars.TryGetValue(player.PlayerId, out var bar))
                {
                    bar = new PlayerBar(player.PlayerId);
                    playerBars.Add(player.PlayerId, bar);
                    Append(bar);
                }
                // Each row receives only its owner's complete snapshot.
                bar.SetPlayer(player, highest);
            }
            ArrangePlayers(hoveredPlayer.HasValue || PointerOverRow());
            if (hoveredPlayer is int owner && playerBars.TryGetValue(owner, out var selected))
                Popup?.SetPlayer(selected.Data);
        }
        ApplyLayout();
    }

    private void SetWeapons(IReadOnlyList<Weapon> weapons)
    {
        var sorted = weapons.OrderByDescending(w => w.damage).ThenBy(w => w.weaponItemID).ToArray();
        var ids = sorted.Select(w => w.weaponItemID).ToHashSet();
        foreach (int id in weaponBars.Keys.Where(id => !ids.Contains(id)).ToArray())
        {
            weaponBars[id].Remove();
            weaponBars.Remove(id);
        }
        long highest = sorted.FirstOrDefault()?.damage ?? 0;
        for (int i = 0; i < sorted.Length; i++)
        {
            var weapon = sorted[i];
            if (!weaponBars.TryGetValue(weapon.weaponItemID, out var bar))
            {
                bar = new WeaponBar();
                weaponBars.Add(weapon.weaponItemID, bar);
                Append(bar);
            }
            bar.SetWeapon(weapon, highest, ColorHelper.standardColors[i % ColorHelper.standardColors.Length]);
            bar.Top.Set(PanelLayout.RowTop(i, PanelLayout.HeaderHeight), 0);
        }
    }

    private bool PointerOverRow() => IsDisplayed &&
        playerBars.Values.Any(bar => bar.ContainsPoint(Main.MouseScreen / Main.UIScale));

    private void ArrangePlayers(bool freeze)
    {
        playerOrder = PanelLayout.OrderPlayers(playerOrder,
            playerBars.Values.Select(p => (p.PlayerId, p.Data.Damage)), freeze);
        for (int i = 0; i < playerOrder.Count; i++)
            playerBars[playerOrder[i]].Top.Set(PanelLayout.RowTop(i, PanelLayout.HeaderHeight), 0);
    }

    public void ApplyLayout()
    {
        float width = SizeHelper.GetWidthFromConfig();
        float height = PanelLayout.Height(playerBars.Count + weaponBars.Count, PanelLayout.HeaderHeight);
        Width.Set(width, 0);
        Height.Set(height, 0);
        if (Parent is MainContainer container)
        {
            // Update the parent before recalculating its children.
            container.Width.Set(width, 0);
            container.Height.Set(height, 0);
            container.Recalculate();
        }
        else
            Recalculate();
        Popup?.ApplyLayout();
    }

    public void UpdateHover(Vector2 mouse)
    {
        if (Popup == null)
            return;
        if (!IsDisplayed || singleplayer || Parent is MainContainer { dragging: true })
        {
            HidePopup();
            return;
        }

        int? selected = null;
        if (Popup.IsVisible && Popup.ContainsPoint(mouse) && hoveredPlayer.HasValue)
            selected = hoveredPlayer;
        else
            foreach (int id in playerOrder)
                if (playerBars[id].ContainsPoint(mouse))
                {
                    selected = id;
                    break;
                }

        if (selected is not int owner || !playerBars.TryGetValue(owner, out var bar))
        {
            bool wasHovered = hoveredPlayer.HasValue;
            HidePopup();
            if (wasHovered)
            {
                ArrangePlayers(false);
                Recalculate();
            }
            return;
        }

        if (hoveredPlayer != owner)
            Popup.SetPlayer(bar.Data);
        hoveredPlayer = owner;
        Popup.IsVisible = Popup.WeaponCount > 0;
        Main.LocalPlayer.mouseInterface = true;

        var row = bar.GetDimensions();
        var panel = GetDimensions();
        var screen = Parent.Parent.GetInnerDimensions();
        float x = panel.X + panel.Width;
        if (x + Popup.Width.Pixels > screen.X + screen.Width)
            x = panel.X - Popup.Width.Pixels;
        x = Math.Clamp(x, screen.X, Math.Max(screen.X, screen.X + screen.Width - Popup.Width.Pixels));
        float y = Math.Clamp(row.Y, screen.Y, Math.Max(screen.Y, screen.Y + screen.Height - Popup.Height.Pixels));
        Popup.Left.Set(x - screen.X, 0);
        Popup.Top.Set(y - screen.Y, 0);
        Popup.Recalculate();
    }

    public void HidePopup()
    {
        hoveredPlayer = null;
        if (Popup != null)
            Popup.IsVisible = false;
    }

    public override bool ContainsPoint(Vector2 point) => IsDisplayed && base.ContainsPoint(point);

    public override void Draw(SpriteBatch sb)
    {
        if (IsDisplayed)
            base.Draw(sb);
    }

    protected override void DrawSelf(SpriteBatch sb)
    {
        base.DrawSelf(sb);
        var dims = GetInnerDimensions();
        bool showIcon = Config.Conf.C.ShowBossIcon && fight != null &&
            fight.HeadIcon >= 0 && fight.HeadIcon < TextureAssets.NpcHeadBoss.Length;
        float available = Math.Max(1f, dims.Width - 34 - (showIcon ? 28 : 0));
        string name = fight?.Name ?? "DPSPanel";
        Vector2 size = FontAssets.MouseText.Value.MeasureString(name);
        float scale = Math.Min(0.9f, available / Math.Max(1f, size.X));
        ChatManager.DrawColorCodedStringWithShadow(sb, FontAssets.MouseText.Value, name,
            new Vector2(dims.X + 34 + (available - size.X * scale) / 2, dims.Y + (30 - size.Y * scale) / 2),
            Color.White, 0f, Vector2.Zero, new Vector2(scale));
        if (showIcon)
            sb.Draw(TextureAssets.NpcHeadBoss[fight.HeadIcon].Value,
                new Rectangle((int)(dims.X + dims.Width - 26), (int)dims.Y + 2, 26, 26), Color.White);
    }
}
